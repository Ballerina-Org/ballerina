namespace Ballerina.DSL.Next.Types

module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model

  type TypeIdentifier with
    static member Create(name: string) : TypeIdentifier = { Name = name }

  type TypeVar with
    static member Create(name: string) : TypeVar =
      { Name = name
        Guid = Guid.CreateVersion7() }

  type TypeSymbol with
    static member Create(name: string) : TypeSymbol =
      { Name = name
        Guid = Guid.CreateVersion7() }

  type TypeVarIdentifier with
    static member Create(name: string) : TypeVarIdentifier = { Name = name }

  type TypeParameter with
    static member Create(name: string, kind: Kind) : TypeParameter = { Name = name; Kind = kind }

  type TypeExpr with
    static member AsLookup(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Lookup id -> return id
        | _ -> return! $"Error: expected type lookup, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsValue
      (tryFind: string -> Sum<TypeValue, Errors>)
      (tryFindSymbol: string -> Sum<TypeSymbol, Errors>)
      (t: TypeExpr)
      : Sum<TypeValue, Errors> =
      let (!) = TypeExpr.AsValue tryFind tryFindSymbol

      sum {
        match t with
        | TypeExpr.Primitive p -> return TypeValue.Primitive p
        | TypeExpr.Lookup v -> return! tryFind v
        | TypeExpr.Lambda(param, body) -> return TypeValue.Lambda(param, body)
        | TypeExpr.Arrow(input, output) ->
          let! input = !input
          let! output = !output
          return TypeValue.Arrow(input, output)

        | TypeExpr.Record(fields) ->
          let! fields =
            fields
            |> Seq.map (fun (k, v) ->
              sum {
                let! k = TypeExpr.AsLookup k
                let! k = tryFindSymbol k
                let! v = !v
                return (k, v)
              })
            |> sum.All
            |> sum.Map(Map.ofSeq)

          return TypeValue.Record(fields)
        | TypeExpr.Tuple(fields) ->
          let! items = fields |> List.map (!) |> sum.All
          return TypeValue.Tuple(items)
        | TypeExpr.Union(cases) ->
          let! cases =
            cases
            |> Seq.map (fun (k, v) ->
              sum {
                let! k = TypeExpr.AsLookup k
                let! k = tryFindSymbol k
                let! v = !v
                return (k, v)
              })
            |> sum.All
            |> sum.Map(Map.ofSeq)

          return TypeValue.Union(cases)
        | TypeExpr.Sum(fields) ->
          let! variants = fields |> List.map (!) |> sum.All
          return TypeValue.Sum(variants)
        | TypeExpr.Map(key, value) ->
          let! key = !key
          let! value = !value
          return TypeValue.Map(key, value)
        | TypeExpr.List(element) ->
          let! element = !element
          return TypeValue.List(element)
        | TypeExpr.Set(element) ->
          let! element = !element
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
