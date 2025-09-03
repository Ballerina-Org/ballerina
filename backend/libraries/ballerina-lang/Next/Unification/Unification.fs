namespace Ballerina.DSL.Next

[<AutoOpen>]
module Unification =
  open Ballerina.Collections.Sum
  open Ballerina.Collections.NonEmptyList
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.Fun
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.KitchenSink
  open Ballerina.DSL.Next.EquivalenceClasses
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.Eval

  type UnificationContext = TypeExprEvalState

  type UnificationState = EquivalenceClasses<TypeVar, TypeValue>

  let private (!) = Identifier.LocalScope

  type TypeExpr with
    static member FreeVariables(t: TypeExpr) : Reader<Set<TypeVar>, TypeExprEvalState, Errors> =
      reader {
        match t with
        | TypeExpr.Lambda(p, t) ->
          return!
            TypeExpr.FreeVariables t
            |> reader.MapContext(
              TypeExprEvalState.Updaters.Bindings(Map.add !p.Name (TypeValue.Primitive PrimitiveType.Unit, Kind.Star))
            )
        | TypeExpr.Exclude(l, r)
        | TypeExpr.Flatten(l, r)
        | TypeExpr.Let(_, l, r)
        | TypeExpr.Apply(l, r)
        | TypeExpr.Arrow(l, r)
        | TypeExpr.Map(l, r) ->
          let! lVars = TypeExpr.FreeVariables l
          let! rVars = TypeExpr.FreeVariables r
          return Set.union lVars rVars
        | TypeExpr.KeyOf e
        | TypeExpr.Rotate e
        | TypeExpr.List e
        | TypeExpr.Set e -> return! TypeExpr.FreeVariables e
        | TypeExpr.Tuple es
        | TypeExpr.Sum es ->
          let! vars = es |> Seq.map TypeExpr.FreeVariables |> reader.All
          return vars |> Set.unionMany
        | TypeExpr.Record es
        | TypeExpr.Union es ->
          let! vars = es |> Seq.map (fun (_, v) -> TypeExpr.FreeVariables v) |> reader.All
          let! keys = es |> Seq.map (fun (k, _) -> TypeExpr.FreeVariables k) |> reader.All
          return keys @ vars |> Set.unionMany
        | TypeExpr.Primitive _
        | TypeExpr.NewSymbol _ -> return Set.empty
        | TypeExpr.Lookup l ->
          let! t = UnificationContext.tryFindType l |> reader.Catch

          match t with
          | Left(t, _) -> return! TypeValue.FreeVariables t
          | Right _ -> return Set.empty
      }

  and TypeValue with
    static member FreeVariables(t: TypeValue) : Reader<Set<TypeVar>, TypeExprEvalState, Errors> =
      reader {
        match t with
        | TypeValue.Var v ->
          let! ctx = reader.GetContext()

          if ctx.Bindings.ContainsKey !v.Name then
            return Set.empty
          else
            return Set.singleton v
        | TypeValue.Lambda(p, t) ->
          return!
            TypeExpr.FreeVariables t
            |> reader.MapContext(
              TypeExprEvalState.Updaters.Bindings(Map.add !p.Name (TypeValue.Primitive PrimitiveType.Unit, Kind.Star))
            )
        | TypeValue.Arrow(l, r) ->
          let! lVars = TypeValue.FreeVariables l
          let! rVars = TypeValue.FreeVariables r
          return Set.union lVars rVars
        | TypeValue.Map(l, r) ->
          let! lVars = TypeValue.FreeVariables l
          let! rVars = TypeValue.FreeVariables r
          return Set.union lVars rVars
        | TypeValue.Apply(_, e)
        | TypeValue.List e
        | TypeValue.Set e -> return! TypeValue.FreeVariables e
        | TypeValue.Tuple es
        | TypeValue.Sum es ->
          let! vars = es |> Seq.map TypeValue.FreeVariables |> reader.All
          return vars |> Set.unionMany
        | TypeValue.Record es
        | TypeValue.Union es ->
          let! vars =
            es
            |> Map.toSeq
            |> Seq.map (fun (_, v) -> TypeValue.FreeVariables v)
            |> reader.All

          return vars |> Set.unionMany
        | TypeValue.Primitive _ -> return Set.empty
        | Lookup l ->
          let! t, _ = UnificationContext.tryFindType l
          return! TypeValue.FreeVariables t
      }

  type TypeValue with
    static member MostSpecific(t1: TypeValue, t2: TypeValue) : Reader<TypeValue, TypeExprEvalState, Errors> =
      reader {
        match t1, t2 with
        | TypeValue.Primitive p1, TypeValue.Primitive p2 when p1 = p2 -> return t1
        | Lookup l1, Lookup l2 when l1 = l2 -> return t1
        | Lookup l1, Lookup l2 when l1 <> l2 ->
          return!
            $"Cannot determine most specific type between {t1} and {t2}"
            |> Errors.Singleton
            |> reader.Throw
        | Lookup _, _ -> return t2
        | _, Lookup _ -> return t1
        | TypeValue.Var _, t
        | t, TypeValue.Var _ -> return t
        | TypeValue.Arrow(l1, r1), TypeValue.Arrow(l2, r2) ->
          let! l = TypeValue.MostSpecific(l1, l2)
          let! r = TypeValue.MostSpecific(r1, r2)
          return TypeValue.Arrow(l, r)
        | TypeValue.Map(l1, r1), TypeValue.Map(l2, r2) ->
          let! l = TypeValue.MostSpecific(l1, l2)
          let! r = TypeValue.MostSpecific(r1, r2)
          return TypeValue.Map(l, r)
        | TypeValue.List e1, TypeValue.List e2 -> return! TypeValue.MostSpecific(e1, e2) |> reader.Map(TypeValue.List)
        | TypeValue.Set e1, TypeValue.List e2 -> return! TypeValue.MostSpecific(e1, e2) |> reader.Map(TypeValue.Set)
        | TypeValue.Tuple e1, TypeValue.Tuple e2 when e1.Length = e2.Length ->
          let! items =
            List.zip e1 e2
            |> Seq.map (fun (v1, v2) -> TypeValue.MostSpecific(v1, v2))
            |> reader.All

          return TypeValue.Tuple items
        | TypeValue.Sum e1, TypeValue.Sum e2 when e1.Length = e2.Length ->
          let! items =
            List.zip e1 e2
            |> Seq.map (fun (v1, v2) -> TypeValue.MostSpecific(v1, v2))
            |> reader.All

          return TypeValue.Sum items
        | TypeValue.Record e1, TypeValue.Record e2 when e1.Count = e2.Count ->
          let! items =
            e1
            |> Map.map (fun k v1 ->
              reader {
                let! v2 = e2 |> Map.tryFindWithError k "record" k.Name |> reader.OfSum
                return! TypeValue.MostSpecific(v1, v2)
              })
            |> reader.AllMap

          return TypeValue.Record items
        | TypeValue.Union e1, TypeValue.Union e2 when e1.Count = e2.Count ->
          let! items =
            e1
            |> Map.map (fun k v1 ->
              reader {
                let! v2 = e2 |> Map.tryFindWithError k "union" k.Name |> reader.OfSum
                return! TypeValue.MostSpecific(v1, v2)
              })
            |> reader.AllMap

          return TypeValue.Union items
        | _ ->
          return!
            $"Cannot determine most specific type between {t1} and {t2}"
            |> Errors.Singleton
            |> reader.Throw

      }


  type TypeValue with
    static member EquivalenceClassesOp op =
      state {
        let! ctx = state.GetContext()

        return!
          op
          |> State.mapContext<UnificationContext> (fun (_ctx: UnificationContext) ->
            { tryCompare = fun (v1, v2) -> TypeValue.MostSpecific(v1, v2) |> Reader.Run _ctx |> Sum.toOption
              equalize =
                fun (left, right) ->
                  state {
                    // let! s = state.GetState()
                    // do Console.WriteLine($"Equalizing {left} and {right} with state {s.ToFSharpString}")
                    // do Console.ReadLine() |> ignore

                    do! TypeValue.Unify(left, right) |> state.MapContext(fun _ -> ctx)
                  } })
      }

    static member bind(var: TypeVar, value: TypeValue) =
      TypeValue.EquivalenceClassesOp
      <| EquivalenceClasses.Bind(
        var,
        match value with
        | TypeValue.Var var -> Left var
        | _ -> Right value
      )

    static member Unify(left: TypeValue, right: TypeValue) : State<Unit, UnificationContext, UnificationState, Errors> =

      // do Console.WriteLine($"Unifying {left} and {right}")
      // do Console.ReadLine() |> ignore

      state {
        match left, right with
        | TypeValue.Primitive p1, TypeValue.Primitive p2 when p1 = p2 -> return ()
        | Lookup l1, Lookup l2 when l1 = l2 -> return ()
        | Lookup l, t2
        | t2, Lookup l ->
          let! t1, _ = UnificationContext.tryFindType l |> state.OfReader
          return! TypeValue.Unify(t1, t2)
        | TypeValue.Var v, t
        | t, TypeValue.Var v ->
          // let! ctx = state.GetContext()
          // let! s = state.GetState()
          // do Console.WriteLine($"Binding variable {v} to type {t}")
          // do Console.WriteLine($"Context = {ctx.ToFSharpString} and state = {s.ToFSharpString}")
          // do Console.ReadLine() |> ignore

          return! TypeValue.bind (v, t)
        | TypeValue.Lambda(p1, t1), TypeValue.Lambda(p2, t2) ->
          if p1.Kind <> p2.Kind then
            return!
              $"Cannot unify type parameters: {p1} and {p2}"
              |> Errors.Singleton
              |> state.Throw
          else
            let! ctx = state.GetContext()
            let! s = state.GetState()

            let! ctx, ctx1, ctx2 =
              state {

                if p1.Kind = Kind.Star then
                  let v1 = p1.Name |> TypeVar.Create
                  let v2 = p2.Name |> TypeVar.Create
                  do! TypeValue.bind (v1, v2 |> TypeValue.Var)
                  let v1 = TypeValue.Var v1, Kind.Star
                  let v2 = TypeValue.Var v2, Kind.Star

                  ctx
                  |> UnificationContext.Updaters.Bindings(Map.add !p1.Name v1 >> Map.add !p2.Name v2),
                  ctx |> UnificationContext.Updaters.Bindings(Map.add !p1.Name v1),
                  ctx |> UnificationContext.Updaters.Bindings(Map.add !p2.Name v2)
                else
                  let s1 = TypeSymbol.Create p1.Name
                  let s2 = TypeSymbol.Create p2.Name

                  ctx
                  |> UnificationContext.Updaters.Symbols(Map.add !p1.Name s1 >> Map.add !p2.Name s2),
                  ctx |> UnificationContext.Updaters.Symbols(Map.add !p1.Name s1),
                  ctx |> UnificationContext.Updaters.Symbols(Map.add !p2.Name s2)
              }

            let! v1 =
              t1
              |> TypeExpr.AsValue
                (UnificationContext.tryFindType >> reader.Map fst >> Reader.Run ctx1)
                (UnificationContext.tryFindSymbol >> Reader.Run ctx1)
              |> state.OfSum

            let! v2 =
              t2
              |> TypeExpr.AsValue
                (UnificationContext.tryFindType >> reader.Map fst >> Reader.Run ctx2)
                (UnificationContext.tryFindSymbol >> Reader.Run ctx2)
              |> state.OfSum

            // do Console.WriteLine($"Unifying lambda types: {v1} and {v2}")
            // do Console.ReadLine() |> ignore

            do! TypeValue.Unify(v1, v2) |> state.MapContext(replaceWith ctx)
            do! state.SetState(replaceWith s)
        | Arrow(l1, r1), Arrow(l2, r2)
        | Map(l1, r1), Map(l2, r2) ->
          do! TypeValue.Unify(l1, l2)
          do! TypeValue.Unify(r1, r2)
        | TypeValue.Apply(v1, a1), TypeValue.Apply(v2, a2) ->
          do! TypeValue.Unify(v1 |> TypeValue.Var, v2 |> TypeValue.Var)
          do! TypeValue.Unify(a1, a2)
        | List(e1), List(e2)
        | Set(e1), Set(e2) -> do! TypeValue.Unify(e1, e2)
        | TypeValue.Tuple(e1), TypeValue.Tuple(e2)
        | TypeValue.Sum(e1), TypeValue.Sum(e2) when List.length e1 = List.length e2 ->
          for (v1, v2) in List.zip e1 e2 do
            do! TypeValue.Unify(v1, v2)
        | TypeValue.Record(e1), TypeValue.Record(e2)
        | TypeValue.Union(e1), TypeValue.Union(e2) when Map.count e1 = Map.count e2 ->
          for (k1, v1) in e1 |> Map.toSeq do
            let! v2 = e2 |> Map.tryFindWithError k1 "record" k1.Name |> state.OfSum
            do! TypeValue.Unify(v1, v2)
        | _ -> return! $"Cannot unify types: {left} and {right}" |> Errors.Singleton |> state.Throw
      }

  type TypeInstantiateContext =
    { Bindings: TypeExprEvalState
      VisitedVars: Set<TypeVar> }

    static member Empty =
      { Bindings = TypeExprEvalState.Empty
        VisitedVars = Set.empty }

    static member Updaters =
      {| Bindings = fun f (ctx: TypeInstantiateContext) -> { ctx with Bindings = f ctx.Bindings }
         VisitedVars =
          fun f (ctx: TypeInstantiateContext) ->
            { ctx with
                VisitedVars = f ctx.VisitedVars } |}

  type TypeValue with
    static member Instantiate: TypeValue -> State<TypeValue, TypeInstantiateContext, UnificationState, Errors> =
      fun t ->
        state {
          match t with
          | TypeValue.Var v ->
            let! ctx = state.GetContext()

            if ctx.VisitedVars.Contains v then
              // return! Errors.Singleton $"Infinite type instantiation for variable {v}" |> state.Throw
              return t
            else
              let! vClass =
                EquivalenceClasses.tryFind v
                |> TypeValue.EquivalenceClassesOp
                |> state.MapContext(fun ctx -> ctx.Bindings)

              match vClass.Representative with
              | Some rep ->
                return!
                  TypeValue.Instantiate rep
                  |> state.MapContext(TypeInstantiateContext.Updaters.VisitedVars(Set.add v))
              | None ->
                match
                  vClass.Variables
                  |> Set.toSeq
                  |> Seq.map (TypeValue.Var)
                  |> Seq.map (
                    TypeValue.Instantiate
                    >> state.MapContext(TypeInstantiateContext.Updaters.VisitedVars(Set.add v))
                  )
                  |> Seq.toList
                with
                | [] ->
                  return!
                    $"Variable {v} has no representative in the equivalence class"
                    |> Errors.Singleton
                    |> state.Throw
                | x :: xs -> return! NonEmptyList.OfList(x, xs) |> state.Any
          | TypeValue.Lookup l ->
            let! ctx = state.GetContext()

            let! t, _ =
              ctx.Bindings.Bindings
              |> TypeBindings.tryFindWithError l "lookup" l.ToFSharpString
              |> state.OfSum

            return! TypeValue.Instantiate t
          | TypeValue.Lambda(p, t) -> return TypeValue.Lambda(p, t)
          | TypeValue.Arrow(l, r) ->
            let! l' = TypeValue.Instantiate l
            let! r' = TypeValue.Instantiate r
            return TypeValue.Arrow(l', r')
          | TypeValue.Apply(v, e) ->
            let! e' = TypeValue.Instantiate e
            return TypeValue.Apply(v, e')
          | TypeValue.Map(l, r) ->
            let! l' = TypeValue.Instantiate l
            let! r' = TypeValue.Instantiate r
            return TypeValue.Map(l', r')
          | TypeValue.List e ->
            let! e' = TypeValue.Instantiate e
            return TypeValue.List e'
          | TypeValue.Set e ->
            let! e' = TypeValue.Instantiate e
            return TypeValue.Set e'
          | TypeValue.Tuple es ->
            let! es' = es |> Seq.map TypeValue.Instantiate |> state.All
            return TypeValue.Tuple es'
          | TypeValue.Sum es ->
            let! es' = es |> Seq.map TypeValue.Instantiate |> state.All
            return TypeValue.Sum es'
          | TypeValue.Record es ->
            let! es' = es |> Map.map (fun _ -> TypeValue.Instantiate) |> state.AllMap
            return TypeValue.Record es'
          | TypeValue.Union es ->
            let! es' = es |> Map.map (fun _ -> TypeValue.Instantiate) |> state.AllMap
            return TypeValue.Union es'
          | TypeValue.Primitive _ -> return t
        }
