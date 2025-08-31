namespace Ballerina.DSL.Next.Types

module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model

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

  type Kind with
    static member AsStar(kind: Kind) =
      match kind with
      | Kind.Star -> sum.Return()
      | _ -> sum.Throw(Errors.Singleton $"Expected star kind, got {kind}")

    static member AsArrow(kind: Kind) =
      match kind with
      | Kind.Arrow(input, output) -> sum.Return(input, output)
      | _ -> sum.Throw(Errors.Singleton $"Expected arrow kind, got {kind}")

    static member AsSymbol(kind: Kind) =
      match kind with
      | Kind.Symbol -> sum.Return()
      | _ -> sum.Throw(Errors.Singleton $"Expected symbol kind, got {kind}")

  type TypeExpr with
    static member AsLookup(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Lookup id -> return id
        | _ -> return! $"Error: expected type lookup, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsValue
      (tryFind: Identifier -> Sum<TypeValue, Errors>)
      (tryFindSymbol: Identifier -> Sum<TypeSymbol, Errors>)
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

    static member AsUnion(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Union(cases) -> return cases
        | _ -> return! $"Error: expected union type, got {t}" |> Errors.Singleton |> sum.Throw
      }


    static member AsTuple(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Tuple(fields) -> return fields
        | _ -> return! $"Error: expected tuple type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsPrimitive(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Primitive p -> return p
        | _ -> return! $"Error: expected primitive type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsKeyOf(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.KeyOf id -> return id
        | _ -> return! $"Error: expected key of type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsRecord(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Record(fields) -> return fields
        | _ -> return! $"Error: expected record type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsArrow(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Arrow(input, output) -> return (input, output)
        | _ -> return! $"Error: expected arrow type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsMap(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Map(key, value) -> return (key, value)
        | _ -> return! $"Error: expected map type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsList(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.List(element) -> return element
        | _ -> return! $"Error: expected list type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsLambda(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Lambda(param, body) -> return (param, body)
        | _ -> return! $"Error: expected type lambda, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsSet(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Set(element) -> return element
        | _ -> return! $"Error: expected set type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsExclude(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Exclude(a, b) -> return (a, b)
        | _ -> return! $"Error: expected type exclude, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsApply(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Apply(id, args) -> return (id, args)
        | _ -> return! $"Error: expected type application, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsFlatten(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Flatten(id, args) -> return (id, args)
        | _ -> return! $"Error: expected type flatten, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsRotate(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Rotate t -> return t
        | _ -> return! $"Error: expected type rotate, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsSum(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Sum(variants) -> return variants
        | _ -> return! $"Error: expected sum type, got {t}" |> Errors.Singleton |> sum.Throw
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

    static member AsTuple(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Tuple(fields) -> return fields
        | _ ->
          return!
            $"Error: expected tuple type (ie generic), got {t}"
            |> Errors.Singleton
            |> sum.Throw
      }

    static member AsSum(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Sum(variants) -> return variants
        | _ ->
          return!
            $"Error: expected sum type (ie generic), got {t}"
            |> Errors.Singleton
            |> sum.Throw
      }

    static member AsArrow(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Arrow(input, output) -> return (input, output)
        | _ ->
          return!
            $"Error: expected arrow type (ie generic), got {t}"
            |> Errors.Singleton
            |> sum.Throw
      }

    static member AsMap(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Map(key, value) -> return (key, value)
        | _ ->
          return!
            $"Error: expected map type (ie generic), got {t}"
            |> Errors.Singleton
            |> sum.Throw
      }

    static member AsList(t: TypeValue) =
      sum {
        match t with
        | TypeValue.List(element) -> return element
        | _ ->
          return!
            $"Error: expected list type (ie generic), got {t}"
            |> Errors.Singleton
            |> sum.Throw
      }

    static member AsSet(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Set(element) -> return element
        | _ ->
          return!
            $"Error: expected set type (ie generic), got {t}"
            |> Errors.Singleton
            |> sum.Throw
      }

    static member AsLookup(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Lookup id -> return id
        | _ -> return! $"Error: expected type lookup, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsVar(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Var id -> return id
        | _ -> return! $"Error: expected type variable, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsPrimitive(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Primitive p -> return p
        | _ -> return! $"Error: expected primitive type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsApply(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Apply(v, a) -> return v, a
        | _ -> return! $"Error: expected apply type, got {t}" |> Errors.Singleton |> sum.Throw
      }

  type TypeValue with
    member t.AsExpr: TypeExpr =
      match t with
      | Primitive p -> TypeExpr.Primitive p
      | Var id -> TypeExpr.Lookup(Identifier.LocalScope id.Name)
      | Apply(v, a) -> TypeExpr.Apply((v |> TypeValue.Var).AsExpr, a.AsExpr)
      | Lookup id -> TypeExpr.Lookup id
      | Lambda(param, body) -> TypeExpr.Lambda(param, body)
      | Arrow(input, output) -> TypeExpr.Arrow(input.AsExpr, output.AsExpr)
      | Record(fields) ->
        TypeExpr.Record(
          fields
          |> Map.toList
          |> List.map (fun (k, v) -> k.Name |> Identifier.LocalScope |> TypeExpr.Lookup, v.AsExpr)
        )
      | Tuple(elements) -> TypeExpr.Tuple(elements |> List.map (fun e -> e.AsExpr))
      | Union(cases) ->
        TypeExpr.Union(
          cases
          |> Map.toList
          |> List.map (fun (k, v) -> k.Name |> Identifier.LocalScope |> TypeExpr.Lookup, v.AsExpr)
        )
      | Sum(elements) -> TypeExpr.Sum(elements |> List.map (fun e -> e.AsExpr))
      | List(element) -> TypeExpr.List(element.AsExpr)
      | Set(element) -> TypeExpr.Set(element.AsExpr)
      | Map(key, value) -> TypeExpr.Map(key.AsExpr, value.AsExpr)


  type Identifier with
    static member AsLocalScope(i: Identifier) =
      sum {
        match i with
        | Identifier.LocalScope s -> s
        | FullyQualified _ -> return! $"Error: expected local scope, got {i}" |> Errors.Singleton |> sum.Throw
      }

    static member AsFullyQualified(i: Identifier) =
      sum {
        match i with
        | Identifier.FullyQualified(s, x) -> s, x
        | Identifier.LocalScope _ ->
          return!
            $"Error: expected fully qualified identifier, got {i}"
            |> Errors.Singleton
            |> sum.Throw
      }
