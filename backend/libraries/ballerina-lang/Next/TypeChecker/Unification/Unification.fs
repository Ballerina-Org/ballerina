namespace Ballerina.DSL.Next

[<AutoOpen>]
module Unification =
  open Ballerina.Collections.Sum
  open Ballerina.Collections.NonEmptyList
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open System
  open Ballerina.Fun
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.EquivalenceClasses
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.Eval
  open Ballerina.StdLib.OrderPreservingMap

  type UnificationContext =
    { EvalState: TypeExprEvalState
      Scope: TypeCheckScope }

    static member Empty: UnificationContext =
      { EvalState = TypeExprEvalState.Empty
        Scope = TypeCheckScope.Empty }

    static member Create(x) : UnificationContext =
      { EvalState = TypeExprEvalState.Create x
        Scope = TypeCheckScope.Empty }

    static member Updaters =
      {| EvalState =
          fun f ctx ->
            { ctx with
                UnificationContext.EvalState = f ctx.EvalState }
         Scope =
          fun f ctx ->
            { ctx with
                UnificationContext.Scope = f ctx.Scope } |}

  type UnificationState = EquivalenceClasses<TypeVar, TypeValue>


  type TypeExpr with
    static member FreeVariables(t: TypeExpr) : Reader<Set<TypeVar>, UnificationContext, Errors> =
      reader {
        let! ctx = reader.GetContext()

        let (!) x =
          x |> Identifier.LocalScope |> ctx.Scope.Resolve

        match t with
        | TypeExpr.Lambda(p, t) ->
          return!
            TypeExpr.FreeVariables t
            |> reader.MapContext(
              UnificationContext.Updaters.EvalState(
                TypeExprEvalState.Updaters.Bindings(
                  Map.add !p.Name (TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                )
              )
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
        | TypeExpr.LetSymbols(_, _, e)
        | TypeExpr.KeyOf e
        | TypeExpr.Rotate e
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
        | TypeExpr.Imported _
        | TypeExpr.NewSymbol _ -> return Set.empty
        | TypeExpr.Lookup l ->
          let! t =
            TypeExprEvalState.tryFindType (l |> ctx.Scope.Resolve, Location.Unknown)
            |> reader.MapContext(fun ctx -> ctx.EvalState)
            |> reader.Catch

          match t with
          | Left(t, _) -> return! TypeValue.FreeVariables t
          | Right _ -> return Set.empty
      }

  and TypeValue with
    static member FreeVariables(t: TypeValue) : Reader<Set<TypeVar>, UnificationContext, Errors> =
      reader {
        let! ctx = reader.GetContext()

        let (!) x =
          x |> Identifier.LocalScope |> ctx.Scope.Resolve

        match t with
        | TypeValue.Var v ->

          if ctx.EvalState.Bindings.ContainsKey !v.Name then
            return Set.empty
          else
            return Set.singleton v
        | TypeValue.Lambda { value = p, t } ->
          return!
            TypeExpr.FreeVariables t
            |> reader.MapContext(
              UnificationContext.Updaters.EvalState(
                TypeExprEvalState.Updaters.Bindings(
                  Map.add !p.Name (TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                )
              )
            )
        | TypeValue.Arrow { value = l, r } ->
          let! lVars = TypeValue.FreeVariables l
          let! rVars = TypeValue.FreeVariables r
          return Set.union lVars rVars
        | TypeValue.Map { value = l, r } ->
          let! lVars = TypeValue.FreeVariables l
          let! rVars = TypeValue.FreeVariables r
          return Set.union lVars rVars
        | TypeValue.Apply { value = _, e }
        | TypeValue.Set { value = e } -> return! TypeValue.FreeVariables e
        | TypeValue.Tuple { value = es }
        | TypeValue.Sum { value = es } ->
          let! vars = es |> Seq.map TypeValue.FreeVariables |> reader.All
          return vars |> Set.unionMany
        | TypeValue.Record { value = es }
        | TypeValue.Union { value = es } ->
          let! vars =
            es
            |> OrderedMap.toSeq
            |> Seq.map (fun (_, v) -> TypeValue.FreeVariables v)
            |> reader.All

          return vars |> Set.unionMany
        | TypeValue.Imported _
        | TypeValue.Primitive _ -> return Set.empty
        | Lookup l ->
          let! t, _ =
            TypeExprEvalState.tryFindType (l |> ctx.Scope.Resolve, Location.Unknown)
            |> reader.MapContext(fun ctx -> ctx.EvalState)

          return! TypeValue.FreeVariables t
      }

  type TypeValue with
    static member MostSpecific
      (loc0: Location, t1: TypeValue, t2: TypeValue)
      : Reader<TypeValue, TypeExprEvalState, Errors> =
      reader {
        let error e = Errors.Singleton(loc0, e)
        let (==) a b = TypeValue.MostSpecific(loc0, a, b)
        let ofSum = Sum.mapRight (Errors.FromErrors loc0) >> reader.OfSum

        match t1, t2 with
        | TypeValue.Primitive p1, TypeValue.Primitive p2 when p1 = p2 -> return t1
        | Lookup l1, Lookup l2 when l1 = l2 -> return t1
        | Lookup l1, Lookup l2 when l1 <> l2 ->
          return!
            $"Cannot determine most specific type between {t1} and {t2}"
            |> error
            |> reader.Throw
        | Lookup _, _ -> return t2
        | _, Lookup _ -> return t1
        | TypeValue.Var _, t
        | t, TypeValue.Var _ -> return t
        | Arrow { value = l1, l2 }, Arrow { value = r1, r2 } ->
          let! l = l1 == l2
          let! r = r1 == r2
          return TypeValue.CreateArrow(l, r)
        | TypeValue.Map { value = (l1, r1) }, TypeValue.Map { value = (l2, r2) } ->
          let! l = l1 == l2
          let! r = r1 == r2
          return TypeValue.CreateMap(l, r)
        | TypeValue.Set e1, TypeValue.Set e2 ->
          let! e = e1.value == e2.value
          return TypeValue.CreateSet e
        | TypeValue.Tuple e1, TypeValue.Tuple e2 when e1.value.Length = e2.value.Length ->
          let! items = List.zip e1.value e2.value |> Seq.map (fun (v1, v2) -> v1 == v2) |> reader.All

          return TypeValue.CreateTuple items
        | TypeValue.Sum e1, TypeValue.Sum e2 when e1.value.Length = e2.value.Length ->
          let! items = List.zip e1.value e2.value |> Seq.map (fun (v1, v2) -> v1 == v2) |> reader.All

          return TypeValue.CreateSum items
        | TypeValue.Record e1, TypeValue.Record e2 when e1.value.Count = e2.value.Count ->
          let! items =
            e1.value
            |> OrderedMap.map (fun k v1 ->
              reader {
                let! v2 = e2.value |> OrderedMap.tryFindWithError k "record" k.Name.LocalName |> ofSum

                return! v1 == v2
              })
            |> reader.AllMapOrdered

          return TypeValue.CreateRecord items
        | TypeValue.Union e1, TypeValue.Union e2 when e1.value.Count = e2.value.Count ->
          let! items =
            e1.value
            |> OrderedMap.map (fun k v1 ->
              reader {
                let! v2 = e2.value |> OrderedMap.tryFindWithError k "union" k.Name.LocalName |> ofSum

                return! v1 == v2
              })
            |> reader.AllMapOrdered

          return TypeValue.CreateUnion items
        | _ ->
          return!
            $"Cannot determine most specific type between {t1} and {t2}"
            |> error
            |> reader.Throw

      }


  type TypeValue with
    static member EquivalenceClassesOp(loc0: Location) =
      fun (op) ->
        state {
          let! ctx = state.GetContext()

          return!
            op
            |> State.mapContext<UnificationContext> (fun (_ctx: UnificationContext) ->
              { tryCompare =
                  fun (v1, v2) ->
                    TypeValue.MostSpecific(loc0, v1, v2)
                    |> Reader.Run _ctx.EvalState
                    |> Sum.toOption
                equalize =
                  fun (left, right) ->
                    state {
                      // let! s = state.GetState()
                      // do Console.WriteLine($"Equalizing {left} and {right} with state {s.ToFSharpString}")
                      // do Console.ReadLine() |> ignore

                      do! TypeValue.Unify(loc0, left, right) |> state.MapContext(fun _ -> ctx)
                    } })
        }

    static member bind(loc0: Location) =
      fun (var: TypeVar, value: TypeValue) ->
        (TypeValue.EquivalenceClassesOp loc0)
        <| EquivalenceClasses.Bind(
          var,
          (match value with
           | TypeValue.Var var -> Left var
           | _ -> Right value),
          loc0
        )

    static member Unify
      (loc0: Location, left: TypeValue, right: TypeValue)
      : State<Unit, UnificationContext, UnificationState, Errors> =

      // do Console.WriteLine($"Unifying {left} and {right}")
      // do Console.ReadLine() |> ignore

      let left = TypeValue.DropSourceMapping left
      let right = TypeValue.DropSourceMapping right

      let error e = Errors.Singleton(loc0, e)

      let ofSum = Sum.mapRight (Errors.FromErrors loc0) >> state.OfSum

      let (==) a b = TypeValue.Unify(loc0, a, b)

      state {
        let! ctx = state.GetContext()

        match left, right with
        | TypeValue.Primitive p1, TypeValue.Primitive p2 when p1 = p2 -> return ()
        | Lookup l1, Lookup l2 when l1 = l2 -> return ()
        | Lookup l, t2
        | t2, Lookup l ->
          let! t1, _ =
            TypeExprEvalState.tryFindType (l |> ctx.Scope.Resolve, loc0)
            |> reader.MapContext(fun ctx -> ctx.EvalState)
            |> state.OfReader

          return! t1 == t2
        | TypeValue.Imported i1, TypeValue.Imported i2 when i1.Sym = i2.Sym && i1.Arguments.Length = i2.Arguments.Length ->
          do!
            List.zip i1.Arguments i2.Arguments
            |> List.map (fun (a1, a2) -> a1 == a2)
            |> state.All
            |> state.Ignore

          return ()
        | TypeValue.Var v, t
        | t, TypeValue.Var v -> return! TypeValue.bind loc0 (v, t)
        | TypeValue.Lambda { value = p1, t1 }, TypeValue.Lambda { value = p2, t2 } ->
          if p1.Kind <> p2.Kind then
            return! $"Cannot unify type parameters: {p1} and {p2}" |> error |> state.Throw
          else
            let! ctx = state.GetContext()
            let! s = state.GetState()

            let (!) x =
              x |> Identifier.LocalScope |> ctx.Scope.Resolve


            let! ctx, ctx1, ctx2 =
              state {

                if p1.Kind = Kind.Star then
                  let v1 = p1.Name |> TypeVar.Create
                  let v2 = p2.Name |> TypeVar.Create
                  do! TypeValue.bind loc0 (v1, v2 |> TypeValue.Var)
                  let v1 = TypeValue.Var v1, Kind.Star
                  let v2 = TypeValue.Var v2, Kind.Star

                  return
                    ctx
                    |> UnificationContext.Updaters.EvalState(
                      TypeExprEvalState.Updaters.Bindings(Map.add !p1.Name v1 >> Map.add !p2.Name v2)
                    ),
                    ctx
                    |> UnificationContext.Updaters.EvalState(TypeExprEvalState.Updaters.Bindings(Map.add !p1.Name v1)),
                    ctx
                    |> UnificationContext.Updaters.EvalState(TypeExprEvalState.Updaters.Bindings(Map.add !p2.Name v2))
                else
                  let s1 = TypeSymbol.Create(Identifier.LocalScope p1.Name)
                  let s2 = TypeSymbol.Create(Identifier.LocalScope p2.Name)

                  return
                    ctx
                    |> UnificationContext.Updaters.EvalState(
                      TypeExprEvalState.Updaters.Symbols.Types(Map.add !p1.Name s1 >> Map.add !p2.Name s2)
                    ),
                    ctx
                    |> UnificationContext.Updaters.EvalState(
                      TypeExprEvalState.Updaters.Symbols.Types(Map.add !p1.Name s1)
                    ),
                    ctx
                    |> UnificationContext.Updaters.EvalState(
                      TypeExprEvalState.Updaters.Symbols.Types(Map.add !p2.Name s2)
                    )
              }

            let! v1 =
              t1
              |> TypeExpr.AsValue
                loc0
                ((fun t -> TypeExprEvalState.tryFindType (t |> ctx1.Scope.Resolve, loc0))
                 >> reader.Map fst
                 >> Reader.Run ctx1.EvalState)
                ((fun s -> TypeExprEvalState.tryFindTypeSymbol (s |> ctx1.Scope.Resolve, loc0))
                 >> Reader.Run ctx1.EvalState)
              |> state.OfSum

            let! v2 =
              t2
              |> TypeExpr.AsValue
                loc0
                ((fun t -> TypeExprEvalState.tryFindType (t |> ctx2.Scope.Resolve, loc0))
                 >> reader.Map fst
                 >> Reader.Run ctx2.EvalState)
                ((fun s -> TypeExprEvalState.tryFindTypeSymbol (s |> ctx2.Scope.Resolve, loc0))
                 >> Reader.Run ctx2.EvalState)
              |> state.OfSum

            // do Console.WriteLine($"Unifying lambda types: {v1} and {v2}")
            // do Console.ReadLine() |> ignore

            do! v1 == v2 |> state.MapContext(replaceWith ctx)
            do! state.SetState(replaceWith s)
        | Arrow { value = l1, r1 }, Arrow { value = l2, r2 }
        | Map { value = l1, r1 }, Map { value = l2, r2 } ->
          do! l1 == l2
          do! r1 == r2
        | TypeValue.Apply { value = v1, a1 }, TypeValue.Apply { value = v2, a2 } ->
          do! (v1 |> TypeValue.Var) == (v2 |> TypeValue.Var)
          do! a1 == a2
        | Set { value = e1 }, Set { value = e2 } -> do! e1 == e2
        | TypeValue.Tuple { value = e1 }, TypeValue.Tuple { value = e2 }
        | TypeValue.Sum { value = e1 }, TypeValue.Sum { value = e2 } when List.length e1 = List.length e2 ->
          for (v1, v2) in List.zip e1 e2 do
            do! v1 == v2
        | TypeValue.Record { value = e1 }, TypeValue.Record { value = e2 } when e1.Count = e2.Count ->
          for (k1, v1) in e1 |> OrderedMap.toSeq do
            let! v2 = e2 |> OrderedMap.tryFindWithError k1 "record field" k1.Name.LocalName |> ofSum
            do! v1 == v2
        | TypeValue.Union { value = e1 }, TypeValue.Union { value = e2 } when e1.Count = e2.Count ->
          for (k1, v1) in e1 |> OrderedMap.toSeq do
            let! v2 = e2 |> OrderedMap.tryFindWithError k1 "union field" k1.Name.LocalName |> ofSum
            do! v1 == v2
        | _ -> return! $"Cannot unify types: {left} and {right}" |> error |> state.Throw
      }

  type TypeInstantiateContext =
    { Bindings: TypeExprEvalState
      VisitedVars: Set<TypeVar>
      Scope: TypeCheckScope }

    static member Empty =
      { Bindings = TypeExprEvalState.Empty
        VisitedVars = Set.empty
        Scope = TypeCheckScope.Empty }

    static member Updaters =
      {| Bindings = fun f (ctx: TypeInstantiateContext) -> { ctx with Bindings = f ctx.Bindings }
         VisitedVars =
          fun f (ctx: TypeInstantiateContext) ->
            { ctx with
                VisitedVars = f ctx.VisitedVars } |}

  type TypeValue with
    static member Instantiate
      : Location -> TypeValue -> State<TypeValue, TypeInstantiateContext, UnificationState, Errors> =
      fun loc0 t ->
        state {
          let error e = Errors.Singleton(loc0, e)

          match t with
          | TypeValue.Imported({ Arguments = arguments } as t) ->
            let! args = arguments |> Seq.map (TypeValue.Instantiate loc0) |> state.All
            let t = { t with Arguments = args }
            return TypeValue.Imported(t)
          | TypeValue.Var v ->
            let! ctx = state.GetContext()

            if ctx.VisitedVars.Contains v then
              // return! Errors.Singleton $"Infinite type instantiation for variable {v}" |> state.Throw
              return t
            else
              let! vClass =
                EquivalenceClasses.tryFind (v, loc0)
                |> TypeValue.EquivalenceClassesOp loc0
                |> state.MapContext(fun ctx ->
                  { EvalState = ctx.Bindings
                    Scope = ctx.Scope })

              match vClass.Representative with
              | Some rep ->
                return!
                  TypeValue.Instantiate loc0 rep
                  |> state.MapContext(TypeInstantiateContext.Updaters.VisitedVars(Set.add v))
              | None ->
                match
                  vClass.Variables
                  |> Set.toSeq
                  |> Seq.map (TypeValue.Var)
                  |> Seq.map (
                    TypeValue.Instantiate loc0
                    >> state.MapContext(TypeInstantiateContext.Updaters.VisitedVars(Set.add v))
                  )
                  |> Seq.toList
                with
                | [] ->
                  return!
                    $"Variable {v} has no representative in the equivalence class"
                    |> error
                    |> state.Throw
                | x :: xs -> return! NonEmptyList.OfList(x, xs) |> state.Any
          | TypeValue.Lookup l ->
            let! ctx = state.GetContext()

            let! t, _ =
              ctx.Bindings.Bindings
              |> TypeBindings.tryFindWithError (l |> ctx.Scope.Resolve) "lookup" l.ToFSharpString loc0
              |> state.OfSum

            return! TypeValue.Instantiate loc0 t
          | TypeValue.Lambda v -> return TypeValue.Lambda v
          | TypeValue.Arrow { value = l, r; source = n } ->
            let! l' = TypeValue.Instantiate loc0 l
            let! r' = TypeValue.Instantiate loc0 r
            return TypeValue.Arrow { value = l', r'; source = n }
          | TypeValue.Apply { value = var, arg; source = n } ->
            let! arg' = TypeValue.Instantiate loc0 arg
            return TypeValue.Apply { value = var, arg'; source = n }
          | TypeValue.Map { value = l, r; source = n } ->
            let! l' = TypeValue.Instantiate loc0 l
            let! r' = TypeValue.Instantiate loc0 r
            return TypeValue.Map { value = l', r'; source = n }
          | TypeValue.Set { value = v; source = n } ->
            let! v' = TypeValue.Instantiate loc0 v
            return TypeValue.Set { value = v'; source = n }
          | TypeValue.Tuple { value = es; source = n } ->
            let! es' = es |> Seq.map (TypeValue.Instantiate loc0) |> state.All
            return TypeValue.Tuple { value = es'; source = n }
          | TypeValue.Sum { value = es; source = n } ->
            let! es' = es |> Seq.map (TypeValue.Instantiate loc0) |> state.All
            return TypeValue.Sum { value = es'; source = n }
          | TypeValue.Record { value = es; source = n } ->
            let! es' =
              es
              |> OrderedMap.map (fun _ -> TypeValue.Instantiate loc0)
              |> state.AllMapOrdered

            return TypeValue.Record { value = es'; source = n }
          | TypeValue.Union { value = es; source = n } ->
            let! es' =
              es
              |> OrderedMap.map (fun _ -> TypeValue.Instantiate loc0)
              |> state.AllMapOrdered

            return TypeValue.Union { value = es'; source = n }
          | TypeValue.Primitive _ -> return t
        }
