namespace Ballerina.DSL.Expr.Types

open System
open System.Linq
open System.Collections.Generic
open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Expr.Types.Model
open Ballerina.Collections.Sum
open FSharp.Data

module Faker =
  
  open Bogus
  
  type CollectionReference = {
    Id: Guid
    DisplayValue: string
  }

  let f = Faker()
  let rng = Random()
  
  type CollectionReference with
    static member fakeJson (suggestion: string) =
      JsonValue.Record [| 
        "Id", f.Random.Guid() |> string |> JsonValue.String
        "DisplayValue", 
          match suggestion with
          | "City" -> f.Address.City() |> JsonValue.String
          | "Department" -> f.Commerce.Department() |> JsonValue.String
          | _ -> f.Hacker.Abbreviation() |> JsonValue.String
      |]
      
  let fakePrimitive =
    function
    | PrimitiveType.StringType -> f.Name.FullName() |> JsonValue.String  
    | PrimitiveType.IntType -> rng.Next(1, 1000) |> decimal |> JsonValue.Number                 
    | PrimitiveType.FloatType -> rng.NextDouble() |> JsonValue.Float        
    | PrimitiveType.BoolType -> rng.Next(0, 2) = 1 |> JsonValue.Boolean             
    | PrimitiveType.GuidType -> Guid.NewGuid() |> string |> JsonValue.String                
    | PrimitiveType.DateTimeType -> DateTime.Now.AddDays(rng.Next(-365, 365) |> float) |> string |> JsonValue.String
    | PrimitiveType.DateOnlyType -> DateTime.Now.AddDays(rng.Next(-365, 365) |> float).Date |> string |> JsonValue.String
      
module RandomSeeder =
  
  open Faker
  
  let rec mergeJson (a: JsonValue) (b: JsonValue): JsonValue =
    match a, b with
    | JsonValue.Record propsA, JsonValue.Record propsB ->
      Array.append propsA propsB
      |> Array.groupBy fst
      |> Array.map (fun (key, values) ->
        let mergedValue =
          values
          |> Array.map snd
          |> Array.toList
          |> function
            | [v] -> v
            | [v1; v2] -> mergeJson v1 v2
            | more -> more |> List.last
        key, mergedValue
      )
      |> JsonValue.Record

    | JsonValue.Array arrA, JsonValue.Array arrB ->
      JsonValue.Array (Array.append arrA arrB)
    | _, b -> b

  let mergeJsonList (values: JsonValue list): JsonValue =
      match values with
      | [] -> JsonValue.Null
      | head :: tail -> List.fold mergeJson head tail
      
  let (|CollectionReference|_|) (e: ExprType) =
      let suggestion = "City" //TODO
      match e with
      | ExprType.RecordType m when m.Count = 2 ->
          match m |> Map.toList with
          | [(n1,x);(n2, y)] ->
            match x,y with
            | PrimitiveType PrimitiveType.StringType , PrimitiveType PrimitiveType.GuidType when
                n1 = "DisplayValue" && n2 = "Id" -> Some suggestion
            | _ -> None
          | _ -> failwith "never"
      | _ -> None
      
  let rec private lookup (expr: ExprTypeId) (context: TypeContext) =

    match Map.tryFind expr.TypeName context with
    | Some typeBinding -> Some typeBinding
    | None -> 
      let next: TypeContext = 
          context
          |> Map.values
          |> _.ToList()
          |> Seq.map (fun typeBinding -> typeBinding.TypeId.TypeName, typeBinding)
          |> Map.ofSeq
      next |> Map.tryPick ( fun name _t ->  lookup { TypeName = name } next)
      
  //todo: move this to fakers
  let pickOne (items: 'a list) : 'a =
      items[rng.Next(items.Length)]
      
  let cond traverse name tc acc expr1 expr2 =
    let b = traverse tc acc name (PrimitiveType PrimitiveType.BoolType)
    match b with
    | JsonValue.Boolean true -> 
        let v = traverse tc acc name expr1
        JsonValue.Record [| name, b; "Value", v |]
    | JsonValue.Boolean false -> 
        let v = traverse tc acc name expr2
        JsonValue.Record [| name, b; "Value", v |]
    | _ -> failwith "never"
    
  let rec traverse (typeContext: TypeContext) (acc: JsonValue) name (exprType: ExprType): JsonValue =
      match exprType with
      | UnitType 
      | CustomType _
      | VarType _ -> JsonValue.Record [||]
      | PrimitiveType pt -> fakePrimitive pt
      | CollectionReference suggestion -> CollectionReference.fakeJson suggestion
      | RecordType exprMap -> exprMap |> Map.toList |> List.map (fun (k, v) -> JsonValue.Record [| k, traverse typeContext acc k v |]) |> mergeJsonList
      | UnionType exprMap-> 
        exprMap 
        |> Map.toList 
        |> pickOne
        |> (fun (case, union) -> 
          match union.Fields with
          | UnitType -> JsonValue.String case.CaseName
          | _ -> JsonValue.Record [| case.CaseName, traverse typeContext acc case.CaseName union.Fields |])
      | ArrowType (expr1, expr2) 
      | MapType (expr1, expr2)
      | GenericApplicationType (expr1, expr2) 
      | SumType (expr1, expr2) -> cond traverse "IsRight" typeContext acc expr1 expr2
      | TupleType expr -> 
          expr
          |> List.indexed
          |> List.map (fun (i,e) -> $"Item{i+1}", e)
          |> Map.ofList
          |> RecordType
          |> traverse typeContext acc name
      | OptionType expr -> cond traverse "IsSome" typeContext acc expr UnitType
      | OneType expr 
      | ManyType expr 
      | ListType expr
      | TableType expr
      | SetType expr -> traverse typeContext acc name expr
      | GenericType (_exprTypeId, _exprTypeKind, exprType) -> traverse typeContext acc name exprType //TODO: generics
      | LookupType exprTypeId -> 
        lookup exprTypeId typeContext 
        |> Option.get 
        |> fun typeBinding -> 
            match exprTypeId with
            // todo
            | _-> (traverse typeContext acc typeBinding.TypeId.TypeName typeBinding.Type)
      |> mergeJson acc