namespace Ballerina.DSL.Expr.Types

module Model =
  open System
  open Ballerina.Collections.Map
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Expr.Model
  open FSharp.Data

  type TypeVarBindings = Map<VarName, ExprType>

  and TypeBinding =
    { TypeId: ExprTypeId
      Type: ExprType
      Const: bool }

  and TypeBindingId = { TypeId: ExprTypeId }

  and TypeContext = Map<string, TypeBinding>

  and TypeBindings = Map<ExprTypeId, ExprType>

  type TypeBinding with
    static member Id(t: TypeBinding) = { TypeId = t.TypeId }

    static member Create(name, exprType) =
      { TypeBinding.TypeId = name
        TypeBinding.Type = exprType
        Const = false }

  type ExprType with
    static member Extend t1 t2 =
      match t1, t2 with
      | RecordType fields1, RecordType fields2 when
        fields1
        |> Map.keys
        |> Set.ofSeq
        |> Set.intersect (fields2 |> Map.keys |> Set.ofSeq)
        |> Set.isEmpty
        ->
        Map.merge (fun a _ -> a) fields1 fields2 |> ExprType.RecordType |> Left
      | _ -> Right(Errors.Singleton $$"""Error: cannot merge types {{t1}} and {{t2}}.""")

    static member GetTypesFreeVars(t: ExprType) : Set<ExprTypeId> =
      let (!) = ExprType.GetTypesFreeVars

      match t with
      | ExprType.UnitType
      | ExprType.CustomType _
      | ExprType.VarType _ -> Set.empty
      | ExprType.TupleType ts -> ts |> Seq.map (!) |> Seq.fold (+) Set.empty
      | ExprType.KeyOf t
      | ExprType.ListType t
      | ExprType.TableType t
      | ExprType.SetType t
      | ExprType.OptionType t
      | ExprType.OneType t
      | ExprType.ReadOnlyType t
      | ExprType.ManyType t -> !t
      | ExprType.LookupType t -> Set.singleton t
      | ExprType.MapType(k, v) -> !k + !v
      | ExprType.SumType(l, r) -> !l + !r
      | ExprType.PrimitiveType _ -> Set.empty
      | ExprType.UnionType cs ->
        let cs = cs |> Map.values |> List.ofSeq
        cs |> Seq.map (fun c -> !c.Fields) |> Seq.fold (+) Set.empty
      | ExprType.RecordType fs -> fs |> Map.values |> Seq.map (!) |> Seq.fold (+) Set.empty
      | ExprType.ArrowType(l, r) -> !l + !r
      | ExprType.GenericApplicationType(l, r) -> !l + !r
      | ExprType.GenericType(_, _, e) -> !e

    static member Substitute (tvars: TypeVarBindings) (t: ExprType) : ExprType =
      let (!) = ExprType.Substitute tvars
      let (!!) = List.map (!)

      match t with
      | ExprType.CustomType _
      | ExprType.LookupType _
      | ExprType.PrimitiveType _
      | ExprType.UnitType -> t
      | ExprType.VarType v ->
        match tvars |> Map.tryFind v with
        | None -> t
        | Some t -> t
      | ExprType.KeyOf t -> ExprType.KeyOf(!t)
      | ExprType.ListType t -> ExprType.ListType(!t)
      | ExprType.TableType t -> ExprType.TableType(!t)
      | ExprType.SetType t -> ExprType.SetType(!t)
      | ExprType.OptionType t -> ExprType.OptionType(!t)
      | ExprType.OneType t -> ExprType.OneType(!t)
      | ExprType.ReadOnlyType t -> ExprType.ReadOnlyType(!t)
      | ExprType.ManyType t -> ExprType.ManyType(!t)
      | ExprType.MapType(k, v) -> ExprType.MapType(!k, !v)
      | ExprType.SumType(l, r) -> ExprType.SumType(!l, !r)
      | ExprType.TupleType ts -> ExprType.TupleType(!!ts)
      | ExprType.UnionType cs -> ExprType.UnionType(cs |> Map.map (fun _ c -> { c with Fields = !c.Fields }))
      | ExprType.RecordType fs -> ExprType.RecordType(fs |> Map.map (fun _ -> (!)))
      | ExprType.ArrowType(l, r) -> ExprType.ArrowType(!l, !r)
      | ExprType.GenericApplicationType(l, r) -> ExprType.GenericApplicationType(!l, !r)
      | ExprType.GenericType(v, k, e) -> ExprType.GenericType(v, k, !e)

    static member ToJson(t: ExprType) : JsonValue =
      let rec (!) (t: ExprType) = ExprType.ToJson t

      match t with
      | ExprType.UnitType -> JsonValue.String "unit"
      | ExprType.PrimitiveType p ->
        match p with
        | PrimitiveType.GuidType -> JsonValue.String "guid"
        | PrimitiveType.StringType -> JsonValue.String "string"
        | PrimitiveType.IntType -> JsonValue.String "int"
        | PrimitiveType.FloatType -> JsonValue.String "float"
        | PrimitiveType.BoolType -> JsonValue.String "boolean"
        | PrimitiveType.DateOnlyType -> JsonValue.String "Date"
        | PrimitiveType.DateTimeType -> JsonValue.String "DateTime"
      | ExprType.VarType v -> JsonValue.String v.VarName
      | ExprType.CustomType s -> JsonValue.String s
      | ExprType.LookupType l -> JsonValue.String l.VarName
      | ExprType.OptionType t ->
        JsonValue.Record [| "fun", JsonValue.String "Option"; "args", JsonValue.Array [| !t |] |]
      | ExprType.OneType t -> JsonValue.Record [| "fun", JsonValue.String "One"; "args", JsonValue.Array [| !t |] |]
      | ExprType.ReadOnlyType t ->
        JsonValue.Record [| "fun", JsonValue.String "ReadOnly"; "args", JsonValue.Array [| !t |] |]
      | ExprType.ManyType t -> JsonValue.Record [| "fun", JsonValue.String "Many"; "args", JsonValue.Array [| !t |] |]
      | ExprType.SetType t ->
        JsonValue.Record [| "fun", JsonValue.String "MultiSelection"; "args", JsonValue.Array [| !t |] |]
      | ExprType.ListType t -> JsonValue.Record [| "fun", JsonValue.String "List"; "args", JsonValue.Array [| !t |] |]
      | ExprType.TableType t -> JsonValue.Record [| "fun", JsonValue.String "Table"; "args", JsonValue.Array [| !t |] |]
      | ExprType.TupleType ts ->
        let jsonTypes = ts |> List.map (!) |> List.toArray
        JsonValue.Record [| "fun", JsonValue.String "Tuple"; "args", JsonValue.Array jsonTypes |]
      | ExprType.MapType(k, v) ->
        JsonValue.Record [| "fun", JsonValue.String "Map"; "args", JsonValue.Array [| !k; !v |] |]
      | ExprType.SumType(l, r) ->
        JsonValue.Record [| "fun", JsonValue.String "Sum"; "args", JsonValue.Array [| !l; !r |] |]
      | ExprType.UnionType cases ->
        let jsonCases =
          cases
          |> Map.values
          |> Seq.map (fun case ->
            JsonValue.Record [| "caseName", JsonValue.String case.CaseName; "fields", !case.Fields |])
          |> Seq.toArray

        JsonValue.Record [| "fun", JsonValue.String "Union"; "args", JsonValue.Array jsonCases |]
      | ExprType.KeyOf(ExprType.LookupType l) ->
        JsonValue.Record
          [| "fun", JsonValue.String "KeyOf"
             "args", JsonValue.Array [| JsonValue.String l.VarName |] |]
      | ExprType.KeyOf t -> JsonValue.Record [| "fun", JsonValue.String "KeyOf"; "args", JsonValue.Array [| !t |] |]
      | ExprType.RecordType fields ->
        let jsonFields =
          fields
          |> Map.toArray
          |> Array.map (fun (fieldName, fieldType) -> fieldName, !fieldType)

        JsonValue.Record [| "fields", JsonValue.Record jsonFields |]
      | ExprType.ArrowType(l, r) ->
        JsonValue.Record [| "fun", JsonValue.String "Arrow"; "args", JsonValue.Array [| !l; !r |] |]
      | ExprType.GenericType(v, k, e) ->
        JsonValue.Record
          [| "fun", JsonValue.String "Generic"
             "varName", JsonValue.String v.VarName
             "kind", JsonValue.String(k.ToString())
             "expr", !e |]
      | ExprType.GenericApplicationType(f, a) ->
        JsonValue.Record
          [| "fun", JsonValue.String "GenericApplication"
             "args", JsonValue.Array [| !f; !a |] |]
