namespace Ballerina.DSL.Expr

module Next =
  open Ballerina.Collections.Sum
  open Ballerina.Collections.NonEmptyList
  open Ballerina.State.WithError
  open Ballerina.Errors
  open System
  open Ballerina.Fun
  open Ballerina.Core.Object

  type ReaderWithError<'c, 'a, 'e> =
    | ReaderWithError of ('c -> Sum<'a, 'e>)

    static member ofSum(r: Sum<'a, 'e>) : ReaderWithError<'c, 'a, 'e> = ReaderWithError(fun _ -> r)

    static member Run (c: 'c) (ReaderWithError r: ReaderWithError<'c, 'a, 'e>) = c |> r

    static member map<'b>(f: 'a -> 'b) : (ReaderWithError<'c, 'a, 'e> -> ReaderWithError<'c, 'b, 'e>) =
      fun (ReaderWithError r) -> ReaderWithError(r >> Sum.map f)

    static member mapContext<'c1>(f: 'c1 -> 'c) : (ReaderWithError<'c, 'a, 'e> -> ReaderWithError<'c1, 'a, 'e>) =
      fun (ReaderWithError r) -> ReaderWithError(f >> r)

    static member join: ReaderWithError<'c, ReaderWithError<'c, 'a, 'e>, 'e> -> ReaderWithError<'c, 'a, 'e> =
      fun (ReaderWithError r) ->
        ReaderWithError(fun c ->
          sum {
            let! ReaderWithError r_r = r c
            return! r_r c
          })

    static member cons: 'a -> ReaderWithError<'c, 'a, 'e> =
      fun v -> ReaderWithError(fun _c -> sum { return v })

    static member throw: 'e -> ReaderWithError<'c, 'a, 'e> =
      fun e -> ReaderWithError(fun _c -> sum.Throw(e))

  type ReaderWithErrorBuilder() =
    member _.Return<'c, 'a, 'e> v = ReaderWithError.cons v
    member _.ReturnFrom<'c, 'a, 'e>(r: ReaderWithError<'c, 'a, 'e>) = r

    member _.Bind<'c, 'a, 'b, 'e>(r: ReaderWithError<'c, 'a, 'e>, f: 'a -> ReaderWithError<'c, 'b, 'e>) =
      r |> ReaderWithError.map f |> ReaderWithError.join

    member reader.Combine<'c, 'a, 'b, 'e>(r1: ReaderWithError<'c, 'a, 'e>, r2: ReaderWithError<'c, 'b, 'e>) =
      reader {
        let! _ = r1
        return! r2
      }

    member reader.Throw<'c, 'a, 'e>(error: 'e) =
      ReaderWithError(fun _ -> sum.Throw(error))

    member inline _.All2<'c, 'a, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (ReaderWithError r1: ReaderWithError<'c, 'a, 'e>)
      : ReaderWithError<'c, 'a, 'e> -> ReaderWithError<'c, 'a * 'a, 'e> =
      fun (ReaderWithError r2) -> ReaderWithError(fun (c: 'c) -> sum.All2 (r1 c) (r2 c))

    member inline _.Any2<'c, 'a, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (ReaderWithError r1: ReaderWithError<'c, 'a, 'e>)
      : ReaderWithError<'c, 'a, 'e> -> ReaderWithError<'c, 'a, 'e> =
      fun (ReaderWithError r2) -> ReaderWithError(fun (c: 'c) -> sum.Any2 (r1 c) (r2 c))

    member inline _.All<'c, 'a, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (readers: seq<ReaderWithError<'c, 'a, 'e>>)
      : ReaderWithError<'c, List<'a>, 'e> =
      ReaderWithError(fun (c: 'c) -> sum.All(readers |> Seq.map (fun (ReaderWithError r) -> r c)))

    member inline _.Any<'c, 'a, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (readers: NonEmptyList<ReaderWithError<'c, 'a, 'e>>)
      : ReaderWithError<'c, 'a, 'e> =
      ReaderWithError(fun (c: 'c) -> sum.Any(readers |> NonEmptyList.map (fun (ReaderWithError r) -> r c)))

    member inline reader.AllMap<'c, 'a, 'e, 'k when 'k: comparison and 'e: (static member Concat: 'e * 'e -> 'e)>
      (readers: Map<'k, ReaderWithError<'c, 'a, 'e>>)
      : ReaderWithError<'c, Map<'k, 'a>, 'e> =
      ReaderWithError(fun (c: 'c) ->
        sum {
          let! (results: Map<'k, 'a>) = readers |> Map.map (fun _k (ReaderWithError p) -> p c) |> sum.AllMap
          return results
        })

    member _.OfSum(s: Sum<'a, 'e>) : ReaderWithError<'c, 'a, 'e> = ReaderWithError.ofSum s

    member _.MapContext<'c1, 'c, 'a, 'e>(f: 'c1 -> 'c) : ReaderWithError<'c, 'a, 'e> -> ReaderWithError<'c1, 'a, 'e> =
      ReaderWithError.mapContext f

    member _.Map<'c, 'a, 'b, 'e>(f: 'a -> 'b) : ReaderWithError<'c, 'a, 'e> -> ReaderWithError<'c, 'b, 'e> =
      ReaderWithError.map f

    member _.GetContext() : ReaderWithError<'c, 'c, 'e> =
      ReaderWithError(fun c -> sum { return c })


  let reader = ReaderWithErrorBuilder()

  type TypeParameter = { Name: string; Kind: Kind }

  and Kind =
    | Star
    | Arrow of Kind * Kind

  and TypeIdentifier = { Name: string }
  and TypeVar = { Name: string }

  and TypeExpr =
    | Primitive of PrimitiveType
    | Lookup of TypeIdentifier
    | Apply of TypeExpr * TypeExpr
    | Lambda of TypeParameter * TypeExpr
    | Arrow of TypeExpr * TypeExpr
    | Record of Map<string, TypeExpr>
    | Tuple of List<TypeExpr>
    | Union of Map<string, TypeExpr>
    | Sum of List<TypeExpr>
    | List of TypeExpr
    | Set of TypeExpr
    | Map of TypeExpr * TypeExpr
    | KeyOf of TypeExpr
    | Minus of TypeExpr * TypeExpr

  and TypeValue =
    | Primitive of PrimitiveType
    | Var of TypeVar
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
  type TypeExprEvalResult = ReaderWithError<TypeBindings, TypeValue, Errors>
  type TypeExprEval = TypeExpr -> TypeExprEvalResult

  type TypeExpr with
    static member private MergeUnionCases(variants: List<TypeValue>) : TypeExprEvalResult =
      reader {

        match variants with
        | [] -> return TypeValue.Primitive PrimitiveType.Unit
        | [ t ] -> return t
        | t1 :: t2 :: ts ->
          let! t1 = t1 |> TypeValue.AsUnion |> reader.OfSum
          let! t2 = t2 |> TypeValue.AsUnion |> reader.OfSum
          let keys1 = t1 |> Map.keys |> Set.ofSeq
          let keys2 = t2 |> Map.keys |> Set.ofSeq

          if keys1 |> Set.intersect keys2 |> Set.isEmpty then
            let t12 = Map.toSeq t1 |> Seq.append (Map.toSeq t2) |> Map.ofSeq |> TypeValue.Union

            return! TypeExpr.MergeUnionCases(t12 :: ts)
          else
            return!
              $"Error: cannot collapse unions {t1} and {t2} because they have overlapping keys"
              |> Errors.Singleton
              |> reader.Throw

      }

    static member Eval: TypeExprEval =
      fun t ->
        reader {
          let (!) = TypeExpr.Eval

          match t with
          | TypeExpr.Primitive p -> return TypeValue.Primitive p
          | TypeExpr.Lookup id ->
            let! vars = reader.GetContext()
            return! vars |> Map.tryFindWithError id "types" id.Name |> reader.OfSum
          | TypeExpr.Apply(f, a) ->
            let! f = !f
            let! a = !a
            let! (param, body) = f |> TypeValue.AsLambda |> reader.OfSum
            return! !body |> reader.MapContext(Map.add { Name = param.Name } a)
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
            return! reader.Any2 (TypeExpr.MergeUnionCases variants) (reader { return TypeValue.Sum variants })
          | TypeExpr.Minus(type1, type2) ->
            let! type1 = !type1
            let! type2 = !type2
            let! cases1 = type1 |> TypeValue.AsUnion |> reader.OfSum
            let! cases2 = type2 |> TypeValue.AsUnion |> reader.OfSum
            let keys2 = cases2 |> Map.keys |> Set.ofSeq

            return
              cases1
              |> Map.filter (fun k _ -> keys2 |> Set.contains k |> not)
              |> TypeValue.Union
        }

  type EquivalenceClass<'value when 'value: comparison> = Set<'value>

  and EquivalenceClassValueOperations<'var, 'value when 'var: comparison and 'value: comparison> =
    { equalize: 'value * 'value -> State<unit, unit, EquivalenceClasses<'var, 'value>, Errors>
      asVar: 'value -> Sum<'var, Errors>
      toValue: 'var -> 'value }

  and EquivalenceClasses<'var, 'value when 'var: comparison and 'value: comparison> =
    { Classes: Map<string, EquivalenceClass<'value>>
      Variables: Map<'var, string> }

    static member Updaters =
      {| Classes = fun u (c: EquivalenceClasses<'var, 'value>) -> { c with Classes = c.Classes |> u }
         Variables = fun u (c: EquivalenceClasses<'var, 'value>) -> { c with Variables = c.Variables |> u } |}

    static member Empty: EquivalenceClasses<'var, 'value> =
      { Classes = Map.empty
        Variables = Map.empty }

    static member private tryGetKey(var: 'var) =
      state {
        let! (classes: EquivalenceClasses<'var, 'value>) = state.GetState()

        return!
          classes.Variables
          |> Map.tryFindWithError var "var" (var.ToString())
          |> state.OfSum
      }

    static member private getKey(var: 'var) =
      state {
        let! key = EquivalenceClasses<'var, 'value>.tryGetKey var |> state.Catch

        match key with
        | Left key -> return key
        | Right _ ->
          do! state.SetState(EquivalenceClasses.Updaters.Variables(Map.add var $"{var}"))
          return $"{var}"
      }

    static member private bindKeyVar
      (key: string)
      (var: 'var)
      : State<unit, unit, EquivalenceClasses<'var, 'value>, Errors> =
      state.SetState(fun classes ->
        { classes with
            Variables = classes.Variables |> Map.add var key })

    static member private tryGetVarClass
      (key: string)
      : State<EquivalenceClass<'value>, unit, EquivalenceClasses<'var, 'value>, Errors> =
      state {
        let! (classes: EquivalenceClasses<'var, 'value>) = state.GetState()

        return!
          classes.Classes
          |> Map.tryFindWithError key "classes" (key.ToString())
          |> state.OfSum
      }

    static member private getVarClass
      (valueOperations: EquivalenceClassValueOperations<'var, 'value>)
      (key: string)
      (var: 'var)
      : State<EquivalenceClass<'value>, unit, EquivalenceClasses<'var, 'value>, Errors> =
      state {
        let! varClass = EquivalenceClasses.tryGetVarClass key |> state.Catch

        match varClass with
        | Left varClass -> return varClass
        | Right _ ->
          let initialClass = var |> valueOperations.toValue |> Set.singleton
          do! state.SetState(EquivalenceClasses.Updaters.Classes(Map.add key initialClass))

          return initialClass
      }

    static member private updateVarClass key u : State<unit, unit, EquivalenceClasses<'var, 'value>, Errors> =
      state.SetState(fun (classes: EquivalenceClasses<'var, 'value>) ->
        { classes with
            Classes =
              classes.Classes
              |> Map.change key (function
                | Some c -> c |> u |> Some
                | None -> Set.empty |> u |> Some) })

    static member private deleteVarClass key : State<unit, unit, EquivalenceClasses<'var, 'value>, Errors> =
      state.SetState(fun (classes: EquivalenceClasses<'var, 'value>) ->
        { classes with
            Classes = classes.Classes |> Map.remove key })

    static member Bind
      (valueOperations: EquivalenceClassValueOperations<'var, 'value>)
      (var: 'var, value: 'value)
      : State<unit, unit, EquivalenceClasses<'var, 'value>, Errors> =
      state {
        let! key = EquivalenceClasses.getKey var // get the key associated with var or create a fresh key
        do! EquivalenceClasses.bindKeyVar key var // bind the key and the var (needed if the key is fresh)

        let! varClass = EquivalenceClasses.getVarClass valueOperations key var

        if varClass |> Set.contains value then
          return ()
        else
          do!
            varClass
            |> Seq.map (fun otherValue -> valueOperations.equalize (otherValue, value))
            |> state.All
            |> state.Map ignore

          match! value |> valueOperations.asVar |> state.OfSum |> state.Catch with
          | Left otherVar ->
            let! otherKey = EquivalenceClasses.getKey otherVar
            let! otherClassToBeMerged = EquivalenceClasses.getVarClass valueOperations otherKey otherVar
            do! EquivalenceClasses.deleteVarClass otherKey
            do! EquivalenceClasses.bindKeyVar key otherVar

            do!
              otherClassToBeMerged
              |> Seq.map (fun otherValue ->
                state {
                  do! EquivalenceClasses.Bind valueOperations (var, otherValue)
                  return ()
                })
              |> state.All
              |> state.Map ignore
          | Right _ -> return ()

          do! EquivalenceClasses.updateVarClass key (Set.add value)

          return ()
      }

// type TypeValue with
//   static member Unify
//     (type1: TypeValue, type2: TypeValue)
//     : State<TypeBindings, EquivalenceClasses<TypeValue>, unit, Errors> =
//     // same structures unify recursively
//     // same lookups and variables unify directly
//     // different lookups are looked up and then recursed -> map error
//     // different type variables try and bind in the equivalence class
//     // --> tryBindValues uses Unify
//     state { return failwith "" }
