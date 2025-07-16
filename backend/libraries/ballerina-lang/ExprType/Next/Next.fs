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
    | Symbol
    | Star
    | Arrow of Kind * Kind

  and TypeIdentifier = { Name: string }

  and TypeSymbol = { Name: string; Guid: Guid }

  and TypeVar =
    { Name: string
      Guid: Guid }

    override v.ToString() = v.Name

  and TypeVarIdentifier =
    { Name: string }

    override v.ToString() = v.Name

  and TypeExpr =
    | Primitive of PrimitiveType
    | Lookup of string
    | Apply of TypeExpr * TypeExpr
    | Lambda of TypeParameter * TypeExpr
    | Arrow of TypeExpr * TypeExpr
    | Record of List<TypeExpr * TypeExpr>
    | Tuple of List<TypeExpr>
    | Union of List<TypeExpr * TypeExpr>
    | List of TypeExpr
    | Set of TypeExpr
    | Map of TypeExpr * TypeExpr
    | KeyOf of TypeExpr
    | Sum of List<TypeExpr>
    | Flatten of TypeExpr * TypeExpr
    | Exclude of TypeExpr * TypeExpr
    | Rotate of TypeExpr

  and TypeBinding =
    { Identifier: TypeIdentifier
      Type: TypeExpr }

  and TypeValue =
    | Primitive of PrimitiveType
    | Var of TypeVar
    | Lookup of TypeIdentifier
    | Lambda of TypeParameter * TypeExpr
    | Arrow of TypeValue * TypeValue
    | Record of Map<TypeSymbol, TypeValue>
    | Tuple of List<TypeValue>
    | Union of Map<TypeSymbol, TypeValue>
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

        | Record(fields) ->
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
        | Tuple(fields) ->
          let! items = fields |> List.map (!) |> sum.All
          return TypeValue.Tuple(items)
        | Union(cases) ->
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
        | Sum(fields) ->
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


  // Eval.fs
  type TypeBindings = Map<string, TypeValue>
  type TypeSymbols = Map<string, TypeSymbol>

  type TypeExprEvalContext =
    { Bindings: TypeBindings
      Symbols: TypeSymbols }

  type TypeExprEvalResult = ReaderWithError<TypeExprEvalContext, TypeValue, Errors>
  type TypeExprEval = TypeExpr -> TypeExprEvalResult
  type TypeExprSymbolEvalResult = ReaderWithError<TypeExprEvalContext, TypeSymbol, Errors>
  type TypeExprSymbolEval = TypeExpr -> TypeExprSymbolEvalResult

  type TypeExprEvalContext with
    static member Empty: TypeExprEvalContext =
      { Bindings = Map.empty
        Symbols = Map.empty }

    static member Create(bindings: TypeBindings, symbols: TypeSymbols) : TypeExprEvalContext =
      { TypeExprEvalContext.Empty with
          Bindings = bindings
          Symbols = symbols }

    static member CreateFromSymbols(symbols: TypeSymbols) : TypeExprEvalContext =
      { TypeExprEvalContext.Empty with
          Symbols = symbols }

    static member tryFindType(v: string) : ReaderWithError<TypeExprEvalContext, TypeValue, Errors> =
      ReaderWithError(fun ctx -> ctx.Bindings |> Map.tryFindWithError v "bindings" v)

    static member tryFindSymbol(v: string) : ReaderWithError<TypeExprEvalContext, TypeSymbol, Errors> =
      ReaderWithError(fun ctx -> ctx.Symbols |> Map.tryFindWithError v "symbols" v)

    static member Updaters =
      {| Bindings = fun u (c: TypeExprEvalContext) -> { c with Bindings = c.Bindings |> u }
         Symbols = fun u (c: TypeExprEvalContext) -> { c with Symbols = c.Symbols |> u } |}

  type TypeExpr with
    static member EvalAsSymbol: TypeExprSymbolEval =
      fun t ->
        reader {
          let (!) = TypeExpr.EvalAsSymbol
          let (!!) = TypeExpr.Eval

          match t with
          | TypeExpr.Lookup v -> return! TypeExprEvalContext.tryFindSymbol v
          | TypeExpr.Apply(f, a) ->
            let! f = !!f
            let! a = !!a
            let! (param, body) = f |> TypeValue.AsLambda |> reader.OfSum

            return!
              !body
              |> reader.MapContext(Map.add param.Name a |> TypeExprEvalContext.Updaters.Bindings)
          | _ ->
            return!
              $"Error: invalid type expression when evaluating for symbol, got {t}"
              |> Errors.Singleton
              |> reader.Throw
        }

    static member Eval: TypeExprEval =
      fun t ->
        reader {
          let (!) = TypeExpr.Eval
          let (!!) = TypeExpr.EvalAsSymbol

          match t with
          | TypeExpr.Primitive p -> return TypeValue.Primitive p
          | TypeExpr.Lookup v -> return! TypeExprEvalContext.tryFindType v
          | TypeExpr.Apply(f, a) ->
            let! f = !f
            let! (param, body) = f |> TypeValue.AsLambda |> reader.OfSum

            match param.Kind with
            | Kind.Symbol ->
              let! a = !!a

              return!
                !body
                |> reader.MapContext(Map.add param.Name a |> TypeExprEvalContext.Updaters.Symbols)
            | _ ->
              let! a = !a

              return!
                !body
                |> reader.MapContext(Map.add param.Name a |> TypeExprEvalContext.Updaters.Bindings)
          | TypeExpr.Lambda(param, body) -> return TypeValue.Lambda(param, body)
          | TypeExpr.Arrow(input, output) ->
            let! input = !input
            let! output = !output
            return TypeValue.Arrow(input, output)
          | TypeExpr.Record(fields) ->
            let! fields =
              fields
              |> Seq.map (fun (k, v) ->
                reader {
                  let! k = !!k
                  let! v = !v
                  return (k, v)
                })
              |> reader.All
              |> reader.Map(Map.ofSeq)

            return TypeValue.Record(fields)
          | TypeExpr.Tuple(items) ->
            let! items = items |> List.map (!) |> reader.All
            return TypeValue.Tuple(items)
          | TypeExpr.Union(cases) ->
            let! cases =
              cases
              |> Seq.map (fun (k, v) ->
                reader {
                  let! k = !!k
                  let! v = !v
                  return (k, v)
                })
              |> reader.All
              |> reader.Map(Map.ofSeq)

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
          | TypeExpr.Flatten(type1, type2) ->
            let! type1 = !type1
            let! type2 = !type2

            return!
              reader.Any2
                (reader {
                  let! cases1 = type1 |> TypeValue.AsUnion |> reader.OfSum
                  let! cases2 = type2 |> TypeValue.AsUnion |> reader.OfSum

                  let cases1 = cases1 |> Map.toSeq
                  let keys1 = cases1 |> Seq.map fst |> Set.ofSeq

                  let cases2 = cases2 |> Map.toSeq
                  let keys2 = cases2 |> Seq.map fst |> Set.ofSeq

                  if keys1 |> Set.intersect keys2 |> Set.isEmpty then
                    return cases1 |> Seq.append cases2 |> Map.ofSeq |> TypeValue.Union
                  else
                    return!
                      $"Error: cannot flatten types with overlapping keys: {keys1} and {keys2}"
                      |> Errors.Singleton
                      |> reader.Throw
                })
                (reader {
                  let! fields1 = type1 |> TypeValue.AsRecord |> reader.OfSum
                  let! fields2 = type2 |> TypeValue.AsRecord |> reader.OfSum

                  let fields1 = fields1 |> Map.toSeq
                  let keys1 = fields1 |> Seq.map fst |> Set.ofSeq

                  let fields2 = fields2 |> Map.toSeq
                  let keys2 = fields2 |> Seq.map fst |> Set.ofSeq

                  if keys1 |> Set.intersect keys2 |> Set.isEmpty then
                    return fields1 |> Seq.append fields2 |> Map.ofSeq |> TypeValue.Record
                  else
                    return!
                      $"Error: cannot flatten types with overlapping keys: {keys1} and {keys2}"
                      |> Errors.Singleton
                      |> reader.Throw
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

  type KindCheckContext = { Kinds: Map<string, Kind> }
  type KindChecker = TypeExpr -> ReaderWithError<KindCheckContext, Kind, Errors>

  type KindCheckContext with
    static member Empty: KindCheckContext = { Kinds = Map.empty }

    static member Updaters =
      {| Kinds = fun u (c: KindCheckContext) -> { c with Kinds = c.Kinds |> u } |}

    static member Create(kinds: Map<string, Kind>) : KindCheckContext =
      { KindCheckContext.Empty with
          Kinds = kinds }

    static member tryFindKind(name: string) : ReaderWithError<KindCheckContext, Kind, Errors> =
      ReaderWithError(fun ctx -> ctx.Kinds |> Map.tryFindWithError name "kinds" name)

  type TypeExpr with
    static member KindCheck: KindChecker =
      fun t ->
        let (!) = TypeExpr.KindCheck

        reader {
          match t with
          | TypeExpr.Lookup id -> return! KindCheckContext.tryFindKind id
          | TypeExpr.Lambda(p, body) ->
            let! body =
              !body
              |> reader.MapContext(Map.add p.Name p.Kind |> KindCheckContext.Updaters.Kinds)

            return Kind.Arrow(p.Kind, body)
          | TypeExpr.Apply(f, a) ->
            let! f = TypeExpr.KindCheck f
            let! a = TypeExpr.KindCheck a

            match f with
            | Kind.Arrow(input, output) ->
              if input = a then
                return output
              else
                return!
                  $"Error: type mismatch in application, expected {input}, got {a}"
                  |> Errors.Singleton
                  |> reader.Throw
            | _ -> return! $"Error: expected function type, got {f}" |> Errors.Singleton |> reader.Throw
          | TypeExpr.Arrow(t1, t2)
          | TypeExpr.Exclude(t1, t2)
          | TypeExpr.Flatten(t1, t2)
          | TypeExpr.Map(t1, t2) ->
            let! t1 = !t1
            let! t2 = !t2

            if t1 <> Kind.Star || t2 <> Kind.Star then
              return!
                $"Error: expected star type, got {t1} and {t2}"
                |> Errors.Singleton
                |> reader.Throw
            else
              return Kind.Star
          | TypeExpr.Primitive _ -> return Kind.Star
          | TypeExpr.Sum args
          | TypeExpr.Tuple args ->
            do!
              args
              |> Seq.map (fun a ->
                reader {
                  let! a = !a

                  if a = Kind.Star then
                    return Kind.Star
                  else
                    return! $"Error: expected star type, got {a}" |> Errors.Singleton |> reader.Throw
                })
              |> reader.All
              |> reader.Ignore

            return Kind.Star
          | TypeExpr.Union args
          | TypeExpr.Record args ->

            do!
              args
              |> Seq.map (fun (f, a) ->
                reader {
                  let! f = !f
                  let! a = !a

                  if f = Kind.Symbol && a = Kind.Star then
                    return ()
                  else
                    return!
                      $"Error: expected symbol -> star for each record field, got {f} -> {a}"
                      |> Errors.Singleton
                      |> reader.Throw
                })
              |> reader.All
              |> reader.Ignore

            return Kind.Star
          | TypeExpr.List arg
          | TypeExpr.Set arg
          | TypeExpr.KeyOf arg
          | TypeExpr.Rotate arg ->
            let! arg = !arg

            if arg = Kind.Star then
              return Kind.Star
            else
              return! $"Error: expected star type, got {arg}" |> Errors.Singleton |> reader.Throw
        }
