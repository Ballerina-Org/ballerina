namespace Ballerina.DSL.Next

module Model =
  open Ballerina.Collections.Sum
  open Ballerina.Collections.NonEmptyList
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.Fun
  open Ballerina.StdLib.Object

  type TypeParameter = { Name: string; Kind: Kind }

  and Kind =
    | Star
    | Arrow of Kind * Kind

  and TypeIdentifier = { Name: string }

  and TypeVar =
    { Name: string }

    override v.ToString() = v.Name

  and TypeExpr =
    | Primitive of PrimitiveType
    | Var of TypeVar
    | Lookup of TypeIdentifier
    | Apply of TypeExpr * TypeExpr
    | Lambda of TypeParameter * TypeExpr
    | Arrow of TypeExpr * TypeExpr
    | Record of Map<string, TypeExpr>
    | Tuple of List<TypeExpr>
    | Union of Map<string, TypeExpr>
    | List of TypeExpr
    | Set of TypeExpr
    | Map of TypeExpr * TypeExpr
    | KeyOf of TypeExpr
    | Sum of List<TypeExpr>
    | Flatten of FlattenArgs
    | Exclude of TypeExpr * TypeExpr
    | Rotate of TypeExpr

  and FlattenArgs =
    { Left: TypeBinding
      Right: TypeBinding }

  and TypeBinding =
    { Identifier: TypeIdentifier
      Type: TypeExpr }

  and TypeValue =
    | Primitive of PrimitiveType
    | Var of TypeVar
    | Lookup of TypeIdentifier
    | Lambda of TypeParameter * TypeExpr
    | Arrow of TypeValue * TypeValue
    | Record of Map<string, TypeValue>
    | Tuple of List<TypeValue>
    | Union of Map<string, TypeValue>
    | Sum of List<TypeValue>
    | List of TypeValue
    | Set of TypeValue
    | Map of TypeValue * TypeValue

  and PrimitiveType =
    | Unit
    | Guid
    | Int
    | Decimal
    | Bool
    | String
  // add more

  // Patterns.fs
  type TypeIdentifier with
    static member Create(name: string) : TypeIdentifier = { Name = name }

  type TypeVar with
    static member Create(name: string) : TypeVar = { Name = name }

  type TypeParameter with
    static member Create(name: string, kind: Kind) : TypeParameter = { Name = name; Kind = kind }

  type TypeExpr with
    static member AsLookup(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Lookup id -> return id
        | _ -> return! $"Error: expected type lookup, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsValue(t: TypeExpr) : Sum<TypeValue, Errors> =
      sum {
        match t with
        | TypeExpr.Primitive p -> return TypeValue.Primitive p
        | TypeExpr.Var v -> return TypeValue.Var v
        | TypeExpr.Lookup id -> return TypeValue.Lookup id
        | TypeExpr.Lambda(param, body) -> return TypeValue.Lambda(param, body)
        | TypeExpr.Arrow(input, output) ->
          let! input = input |> TypeExpr.AsValue
          let! output = output |> TypeExpr.AsValue
          return TypeValue.Arrow(input, output)

        | Record(fields) ->
          let! fields = fields |> Map.map (fun _k -> TypeExpr.AsValue) |> sum.AllMap
          return TypeValue.Record(fields)
        | Tuple(fields) ->
          let! items = fields |> List.map (TypeExpr.AsValue) |> sum.All
          return TypeValue.Tuple(items)
        | Union(fields) ->
          let! cases = fields |> Map.map (fun _k -> TypeExpr.AsValue) |> sum.AllMap
          return TypeValue.Union(cases)
        | Sum(fields) ->
          let! variants = fields |> List.map (TypeExpr.AsValue) |> sum.All
          return TypeValue.Sum(variants)
        | TypeExpr.Map(key, value) ->
          let! key = key |> TypeExpr.AsValue
          let! value = value |> TypeExpr.AsValue
          return TypeValue.Map(key, value)
        | TypeExpr.List(element) ->
          let! element = element |> TypeExpr.AsValue
          return TypeValue.List(element)
        | TypeExpr.Set(element) ->
          let! element = element |> TypeExpr.AsValue
          return TypeValue.Set(element)
        | _ -> return! $"Error: expected type value, got {t}" |> Errors.Singleton |> sum.Throw
      }

  type TypeValue with
    static member AsLambda(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Lambda(param, body) -> return (param, body)
        | _ ->
          return!
            $"Error: expected type lambda (ie generic), got {t}"
            |> Errors.Singleton
            |> sum.Throw
      }

    static member AsUnion(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Union(cases) -> return cases
        | _ ->
          return!
            $"Error: expected union type (ie generic), got {t}"
            |> Errors.Singleton
            |> sum.Throw
      }

    static member AsRecord(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Record(fields) -> return fields
        | _ ->
          return!
            $"Error: expectedrecord type (ie generic), got {t}"
            |> Errors.Singleton
            |> sum.Throw
      }


  // Eval.fs
  type TypeBindings = Map<TypeIdentifier, TypeValue>
  type VarBindings = Map<TypeVar, TypeValue>

  type TypeExprEvalContext =
    { Types: TypeBindings
      Vars: VarBindings }

  type TypeExprEvalResult = ReaderWithError<TypeExprEvalContext, TypeValue, Errors>
  type TypeExprEval = TypeExpr -> TypeExprEvalResult

  type TypeExprEvalContext with
    static member Empty: TypeExprEvalContext = { Types = Map.empty; Vars = Map.empty }
    static member Create(types: TypeBindings, vars: VarBindings) : TypeExprEvalContext = { Types = types; Vars = vars }

    static member CreateFromTypes(types: TypeBindings) : TypeExprEvalContext =
      { TypeExprEvalContext.Empty with
          Types = types }

    static member CreateFromVars(vars: VarBindings) : TypeExprEvalContext =
      { TypeExprEvalContext.Empty with
          Vars = vars }

    static member Updaters =
      {| Types = fun u (c: TypeExprEvalContext) -> { c with Types = c.Types |> u }
         Vars = fun u (c: TypeExprEvalContext) -> { c with Vars = c.Vars |> u } |}

    static member LookupType(id: TypeIdentifier) : ReaderWithError<TypeExprEvalContext, TypeValue, Errors> =
      reader {
        let! context = reader.GetContext()
        return! context.Types |> Map.tryFindWithError id "types" id.Name |> reader.OfSum
      }

    static member LookupVar(id: TypeVar) : ReaderWithError<TypeExprEvalContext, TypeValue, Errors> =
      reader {
        let! context = reader.GetContext()
        return! context.Vars |> Map.tryFindWithError id "vars" id.Name |> reader.OfSum
      }

  type TypeExpr with
    static member Eval: TypeExprEval =
      fun t ->
        reader {
          let (!) = TypeExpr.Eval

          match t with
          | TypeExpr.Primitive p -> return TypeValue.Primitive p
          | TypeExpr.Var v -> return! TypeExprEvalContext.LookupVar(v)
          | TypeExpr.Lookup id -> return! TypeExprEvalContext.LookupType(id)
          | TypeExpr.Apply(f, a) ->
            let! f = !f
            let! a = !a
            let! (param, body) = f |> TypeValue.AsLambda |> reader.OfSum

            return!
              !body
              |> reader.MapContext(Map.add (TypeVar.Create(param.Name)) a |> TypeExprEvalContext.Updaters.Vars)
          | TypeExpr.Lambda(param, body) -> return TypeValue.Lambda(param, body)
          | TypeExpr.Arrow(input, output) ->
            let! input = !input
            let! output = !output
            return TypeValue.Arrow(input, output)
          | TypeExpr.Record(fields) ->
            let! fields = fields |> Map.map (fun _k -> (!)) |> reader.AllMap
            return TypeValue.Record(fields)
          | TypeExpr.Tuple(items) ->
            let! items = items |> List.map (!) |> reader.All
            return TypeValue.Tuple(items)
          | TypeExpr.Union(cases) ->
            let! cases = cases |> Map.map (fun _k -> (!)) |> reader.AllMap
            return TypeValue.Union(cases)
          | TypeExpr.List(element) ->
            let! element = !element
            return TypeValue.List(element)
          | TypeExpr.Set(element) ->
            let! element = !element
            return TypeValue.Set(element)
          | TypeExpr.Map(key, value) ->
            let! key = !key
            let! value = !value
            return TypeValue.Map(key, value)
          | TypeExpr.KeyOf(arg) ->
            let! arg = !arg
            let! cases = arg |> TypeValue.AsRecord |> reader.OfSum

            return
              cases
              |> Map.map (fun _ _ -> TypeValue.Primitive(PrimitiveType.Unit))
              |> TypeValue.Union
          | TypeExpr.Sum(variants) ->
            let! variants = variants |> List.map (!) |> reader.All

            return TypeValue.Sum(variants)
          | TypeExpr.Flatten({ Left = { Identifier = { Name = name1 }
                                        Type = type1 }
                               Right = { Identifier = { Name = name2 }
                                         Type = type2 } }) ->
            if name1 = name2 || name1 |> String.IsNullOrEmpty || name2 |> String.IsNullOrEmpty then
              return!
                $"Error: cannot flatten types with the same name '{name1}'"
                |> Errors.Singleton
                |> reader.Throw
            else
              let! type1 = !type1
              let! type2 = !type2

              return!
                reader.Any2
                  (reader {
                    let! cases1 = type1 |> TypeValue.AsUnion |> reader.OfSum
                    let! cases2 = type2 |> TypeValue.AsUnion |> reader.OfSum

                    let cases1 = cases1 |> Map.toSeq |> Seq.map (fun (k, v) -> (name1 + "." + k, v))
                    let cases2 = cases2 |> Map.toSeq |> Seq.map (fun (k, v) -> (name2 + "." + k, v))

                    return cases1 |> Seq.append cases2 |> Map.ofSeq |> TypeValue.Union
                  })
                  (reader {
                    let! fields1 = type1 |> TypeValue.AsRecord |> reader.OfSum
                    let! fields2 = type2 |> TypeValue.AsRecord |> reader.OfSum

                    let fields1 = fields1 |> Map.toSeq |> Seq.map (fun (k, v) -> (name1 + "." + k, v))
                    let fields2 = fields2 |> Map.toSeq |> Seq.map (fun (k, v) -> (name2 + "." + k, v))

                    return fields1 |> Seq.append fields2 |> Map.ofSeq |> TypeValue.Record
                  })
          | TypeExpr.Exclude(type1, type2) ->
            let! type1 = !type1
            let! type2 = !type2

            return!
              reader.Any2
                (reader {
                  let! cases1 = type1 |> TypeValue.AsUnion |> reader.OfSum
                  let! cases2 = type2 |> TypeValue.AsUnion |> reader.OfSum
                  let keys2 = cases2 |> Map.keys |> Set.ofSeq

                  return
                    cases1
                    |> Map.filter (fun k _ -> keys2 |> Set.contains k |> not)
                    |> TypeValue.Union
                })
                (reader {
                  let! fields1 = type1 |> TypeValue.AsRecord |> reader.OfSum
                  let! fields2 = type2 |> TypeValue.AsRecord |> reader.OfSum
                  let keys2 = fields2 |> Map.keys |> Set.ofSeq

                  return
                    fields1
                    |> Map.filter (fun k _ -> keys2 |> Set.contains k |> not)
                    |> TypeValue.Record
                })
          | TypeExpr.Rotate(t) ->
            let! t = !t

            return!
              reader.Any2
                (reader {
                  let! cases = t |> TypeValue.AsUnion |> reader.OfSum

                  return cases |> TypeValue.Record
                })
                (reader {
                  let! fields = t |> TypeValue.AsRecord |> reader.OfSum

                  return fields |> TypeValue.Union
                })
        }
