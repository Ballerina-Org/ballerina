namespace Ballerina.DSL.Next

[<AutoOpen>]
module Unification =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Collections.NonEmptyList
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open System
  open Ballerina.Fun
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.EquivalenceClasses
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap


  type TypeExpr<'ve> with
    static member FreeVariables<'valueExt when 'valueExt: comparison>
      (t: TypeExpr<'valueExt>)
      : Reader<Set<TypeVar>, UnificationContext<'valueExt>, Errors<Location>> =
      reader {
        let! ctx = reader.GetContext()

        let (!) x =
          x |> Identifier.LocalScope |> ctx.Scope.Resolve

        match t with
        | TypeExpr.RecordDes(t, _) -> return! TypeExpr.FreeVariables t
        | TypeExpr.Entities s -> return! TypeExpr.FreeVariables s
        | TypeExpr.Relations s -> return! TypeExpr.FreeVariables s
        | TypeExpr.Entity(s, e, e_with_props, id) ->
          let! sVars = TypeExpr.FreeVariables s
          let! eVars = TypeExpr.FreeVariables e
          let! eWithPropsVars = TypeExpr.FreeVariables e_with_props
          let! idVars = TypeExpr.FreeVariables id
          return Set.unionMany [ sVars; eVars; eWithPropsVars; idVars ]
        | TypeExpr.Relation(s, f, f_with_props, f_id, t, t_with_props, t_id) ->
          let! sVars = TypeExpr.FreeVariables s
          let! fVars = TypeExpr.FreeVariables f
          let! fWithPropsVars = TypeExpr.FreeVariables f_with_props
          let! fIdVars = TypeExpr.FreeVariables f_id
          let! tVars = TypeExpr.FreeVariables t
          let! tWithPropsVars = TypeExpr.FreeVariables t_with_props
          let! tIdVars = TypeExpr.FreeVariables t_id
          return Set.unionMany [ sVars; fVars; fWithPropsVars; fIdVars; tVars; tWithPropsVars; tIdVars ]
        | TypeExpr.RelationLookupOne(s, t', f_id) ->
          let! sVars = TypeExpr.FreeVariables s
          let! tVars = TypeExpr.FreeVariables t'
          let! fIdVars = TypeExpr.FreeVariables f_id
          return Set.unionMany [ sVars; tVars; fIdVars ]
        | TypeExpr.RelationLookupOption(s, t', f_id) ->
          let! sVars = TypeExpr.FreeVariables s
          let! tVars = TypeExpr.FreeVariables t'
          let! fIdVars = TypeExpr.FreeVariables f_id
          return Set.unionMany [ sVars; tVars; fIdVars ]
        | TypeExpr.RelationLookupMany(s, t', f_id) ->
          let! sVars = TypeExpr.FreeVariables s
          let! tVars = TypeExpr.FreeVariables t'
          let! fIdVars = TypeExpr.FreeVariables f_id
          return Set.unionMany [ sVars; tVars; fIdVars ]
        | TypeExpr.Schema s ->
          return!
            s.Entities
            |> Seq.map (fun e ->
              reader {
                let! t = TypeExpr.FreeVariables e.Type
                let! id = TypeExpr.FreeVariables e.Id
                return Set.union t id
              })
            |> reader.All
            |> reader.Map(Set.unionMany)
        | TypeExpr.FromTypeValue tv -> return! TypeValue.FreeVariables tv
        | TypeExpr.Lambda(p, t) ->
          return!
            TypeExpr.FreeVariables<'valueExt> t
            |> reader.MapContext(
              UnificationContext.Updaters.EvalState(
                TypeCheckState.Updaters.Bindings(
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
            TypeCheckState.tryFindType (l |> ctx.Scope.Resolve, Location.Unknown)
            |> reader.MapContext(fun ctx -> ctx.EvalState)
            |> reader.Catch

          match t with
          | Left(t, _) -> return! TypeValue.FreeVariables t
          | Right _ -> return Set.empty
      }

  and SymbolicTypeApplication<'ve> with
    static member FreeVariables<'valueExt when 'valueExt: comparison>
      (t: SymbolicTypeApplication<'valueExt>)
      : Reader<Set<TypeVar>, UnificationContext<'valueExt>, Errors<Location>> =
      reader {
        // let! ctx = reader.GetContext()

        // let (!) x =
        //   x |> Identifier.LocalScope |> ctx.Scope.Resolve

        match t with
        | SymbolicTypeApplication.Lookup(l, a) ->
          let! tVars = TypeValue.FreeVariables(TypeValue.Lookup l)
          let! aVars = TypeValue.FreeVariables a
          return Set.union tVars aVars
        | SymbolicTypeApplication.Application(f, a) ->
          let! fVars = SymbolicTypeApplication.FreeVariables f
          let! aVars = TypeValue.FreeVariables a
          return Set.union fVars aVars
      }

  and TypeValue<'ve> with
    static member FreeVariables<'valueExt when 'valueExt: comparison>
      (t: TypeValue<'valueExt>)
      : Reader<Set<TypeVar>, UnificationContext<'valueExt>, Errors<Location>> =
      reader {
        let! ctx = reader.GetContext()

        let (!) x =
          x |> Identifier.LocalScope |> ctx.Scope.Resolve

        let schema_free_vars s =
          s.Entities
          |> OrderedMap.values
          |> Seq.map (fun e ->
            reader {
              let! tVars = TypeValue.FreeVariables e.TypeOriginal
              let! tWithPropsVars = TypeValue.FreeVariables e.TypeWithProps
              let! idVars = TypeValue.FreeVariables e.Id
              return Set.unionMany [ tVars; tWithPropsVars; idVars ]
            })
          |> reader.All
          |> reader.Map(Set.unionMany)

        match t with
        | TypeValue.Schema s -> return! schema_free_vars s
        | TypeValue.Entity(s, t, t', id) ->
          let! sVars = schema_free_vars s
          let! tVars = TypeValue.FreeVariables t
          let! tWithPropsVars = TypeValue.FreeVariables t'
          let! idVars = TypeValue.FreeVariables id
          return Set.unionMany [ sVars; tVars; tWithPropsVars; idVars ]
        | TypeValue.Entities s -> return! schema_free_vars s
        | TypeValue.Relations s -> return! schema_free_vars s
        | TypeValue.RelationLookupOption(s, e, id)
        | TypeValue.RelationLookupOne(s, e, id)
        | TypeValue.RelationLookupMany(s, e, id) ->
          let! sVars = schema_free_vars s
          let! eVars = TypeValue.FreeVariables e
          let! idVars = TypeValue.FreeVariables id
          return Set.unionMany [ sVars; eVars; idVars ]
        | TypeValue.Relation _
        | TypeValue.ForeignKeyRelation _ -> return Set.empty
        | TypeValue.Var v ->

          if ctx.EvalState.Bindings.ContainsKey !v.Name then
            return Set.empty
          else
            return Set.singleton v
        | TypeValue.Application { value = a } -> return! SymbolicTypeApplication.FreeVariables a
        | TypeValue.Lambda { value = p, t } ->
          return!
            TypeExpr.FreeVariables t
            |> reader.MapContext(
              UnificationContext.Updaters.EvalState(
                TypeCheckState.Updaters.Bindings(
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
        // | TypeValue.Apply { value = _, e }
        | TypeValue.Set { value = e } -> return! TypeValue.FreeVariables e
        | TypeValue.Tuple { value = es }
        | TypeValue.Sum { value = es } ->
          let! vars = es |> Seq.map TypeValue.FreeVariables |> reader.All
          return vars |> Set.unionMany
        | TypeValue.Record { value = es } ->
          let! vars =
            es
            |> OrderedMap.toSeq
            |> Seq.map (fun (_, (v, _)) -> TypeValue.FreeVariables v)
            |> reader.All

          return vars |> Set.unionMany
        | TypeValue.Union { value = es } ->
          let! vars =
            es
            |> OrderedMap.toSeq
            |> Seq.map (fun (_, v) -> TypeValue.FreeVariables v)
            |> reader.All

          return vars |> Set.unionMany
        | TypeValue.Imported _
        | TypeValue.Primitive _ -> return Set.empty
        | TypeValue.Lookup l ->
          let! t, _ =
            TypeCheckState.tryFindType (l |> ctx.Scope.Resolve, Location.Unknown)
            |> reader.MapContext(fun ctx -> ctx.EvalState)

          return! TypeValue.FreeVariables t
      }

  type TypeValue<'ve> with
    static member MostSpecific<'valueExt when 'valueExt: comparison>
      (loc0: Location, t1: TypeValue<'valueExt>, t2: TypeValue<'valueExt>)
      : Reader<TypeValue<'valueExt>, TypeCheckState<'valueExt>, Errors<Location>> =
      reader {
        let error e = Errors.Singleton loc0 e

        let (==) a b =
          TypeValue.MostSpecific<'valueExt>(loc0, a, b)

        match t1, t2 with
        | TypeValue.Primitive p1, TypeValue.Primitive p2 when p1.value = p2.value -> return t1
        | TypeValue.Lookup l1, TypeValue.Lookup l2 when l1 = l2 -> return t1
        | TypeValue.Lookup l1, TypeValue.Lookup l2 when l1 <> l2 ->
          return!
            (fun () -> $"Cannot determine most specific type between {t1} and {t2}")
            |> error
            |> reader.Throw
        | TypeValue.Lookup _, _ -> return t2
        | _, TypeValue.Lookup _ -> return t1
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

        | TypeValue.Imported e1, TypeValue.Imported e2 when e1.Arguments.Length = e2.Arguments.Length ->
          let! arguments =
            List.zip e1.Arguments e2.Arguments
            |> Seq.map (fun (v1, v2) -> v1 == v2)
            |> reader.All

          return TypeValue.Imported { e1 with Arguments = arguments }
        | TypeValue.Sum e1, TypeValue.Sum e2 when e1.value.Length = e2.value.Length ->
          let! items = List.zip e1.value e2.value |> Seq.map (fun (v1, v2) -> v1 == v2) |> reader.All

          return TypeValue.CreateSum items
        | TypeValue.Record e1, TypeValue.Record e2 when e1.value.Count = e2.value.Count ->
          let! items =
            e1.value
            |> OrderedMap.map (fun k (v1, k1) ->
              reader {
                let ofSum = Sum.mapRight (Errors.MapContext(replaceWith loc0)) >> reader.OfSum
                let! v2, _ = e2.value |> OrderedMap.tryFindWithError k "record" k.Name.LocalName |> ofSum

                let! res = v1 == v2
                return res, k1
              })
            |> reader.AllMapOrdered

          return TypeValue.CreateRecord items
        | TypeValue.Union e1, TypeValue.Union e2 when e1.value.Count = e2.value.Count ->
          let! items =
            e1.value
            |> OrderedMap.map (fun k v1 ->
              reader {
                let ofSum = Sum.mapRight (Errors.MapContext(replaceWith loc0)) >> reader.OfSum
                let! v2 = e2.value |> OrderedMap.tryFindWithError k "union" k.Name.LocalName |> ofSum

                return! v1 == v2
              })
            |> reader.AllMapOrdered

          return TypeValue.CreateUnion items
        | _ ->
          return!
            (fun () -> $"Cannot determine most specific type between {t1} and {t2}")
            |> error
            |> reader.Throw

      }


  type TypeValue<'ve> with
    static member EquivalenceClassesOp<'res, 'valueExt when 'valueExt: comparison>
      (loc0: Location)
      : State<'res, _, _, Errors<Location>>
          -> State<_, UnificationContext<'valueExt>, UnificationState<'valueExt>, Errors<Location>>
      =
      fun op ->
        state {
          let! ctx = state.GetContext()

          return!
            op
            |> State.mapContext<UnificationContext<'valueExt>> (fun (_ctx: UnificationContext<'valueExt>) ->
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

                      do!
                        TypeValue<'ve>.Unify<'valueExt>(loc0, left, right)
                        |> State.mapState (fun (s, _) -> s |> UnificationState.Create) (fun (s, _) _ -> s.Classes)
                        |> state.MapContext(fun _ -> ctx)
                    } })
        }

    static member Unify<'valueExt when 'valueExt: comparison>
      (loc0: Location, left: TypeValue<'valueExt>, right: TypeValue<'valueExt>)
      : State<Unit, UnificationContext<'valueExt>, UnificationState<'valueExt>, Errors<Location>> =

      // do Console.WriteLine($"Unifying {left} and {right}")
      // do Console.ReadLine() |> ignore

      let left = TypeValue.DropSourceMapping left
      let right = TypeValue.DropSourceMapping right

      let error e = Errors.Singleton loc0 e

      let ofSum = Sum.mapRight (Errors.MapContext(replaceWith loc0)) >> state.OfSum

      let (==) a b = TypeValue.Unify(loc0, a, b)

      let unifySchemas e1 e2 =
        state {
          if
            not (
              e1.Entities.Count = e2.Entities.Count
              && e1.Relations.Count = e2.Relations.Count
              && e1.DeclaredAtForNominalEquality = e2.DeclaredAtForNominalEquality
            )
          then
            return!
              (fun () -> $"Cannot unify types: {left} and {right}, the number of entities and relations does not match")
              |> error
              |> state.Throw
          else
            for (k1, v1) in e1.Entities |> OrderedMap.toSeq do
              let ofSum = Sum.mapRight (Errors.MapContext(replaceWith loc0)) >> state.OfSum

              let! v2 = e2.Entities |> OrderedMap.tryFindWithError k1 "schema entity" k1.Name |> ofSum

              do! v1.Id == v2.Id
              do! v1.TypeOriginal == v2.TypeOriginal
              do! v1.TypeWithProps == v2.TypeWithProps

        // for (k1, v1) in e1.Relations |> OrderedMap.toSeq do
        //   let ofSum = Sum.mapRight (Errors.MapContext(replaceWith loc0)) >> state.OfSum

        //   let! v2 =
        //     e2.Relations
        //     |> OrderedMap.tryFindWithError k1 "schema relation" (fun () -> k1.Name)
        //     |> ofSum

        }

      let bind
        (var: TypeVar, value: TypeValue<'valueExt>)
        : State<Unit, UnificationContext<'valueExt>, UnificationState<'valueExt>, Errors<Location>> =
        let bind_eq_c =
          EquivalenceClasses.Bind(
            var,
            (match value with
             | TypeValue.Var var -> Left var
             | _ -> Right value),
            loc0
          )

        let bind_un_s =
          bind_eq_c
          |> State.mapState (fun (s, _) -> s.Classes) (fun (s, _) _ -> UnificationState.Create s)

        bind_un_s |> TypeValue.EquivalenceClassesOp<_, 'valueExt> loc0


      state {
        let! ctx = state.GetContext()

        match left, right with
        | TypeValue.Primitive p1, TypeValue.Primitive p2 when p1.value = p2.value -> return ()
        | TypeValue.Lookup l1, TypeValue.Lookup l2 when l1 = l2 -> return ()
        | TypeValue.Lookup l, t2
        | t2, TypeValue.Lookup l when ctx.TypeParameters |> Map.containsKey l.LocalName |> not ->
          let! t1, _ =
            TypeCheckState.tryFindType (l |> ctx.Scope.Resolve, loc0)
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
        | t, TypeValue.Var v -> return! bind (v, t)
        | TypeValue.Lambda { value = p1, t1 }, TypeValue.Lambda { value = p2, t2 } ->
          if p1.Kind <> p2.Kind then
            return!
              (fun () -> $"Cannot unify type parameters: {p1} and {p2}")
              |> error
              |> state.Throw
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
                  do! bind (v1, v2 |> TypeValue.Var)
                  let v1 = TypeValue.Var v1, Kind.Star
                  let v2 = TypeValue.Var v2, Kind.Star

                  return
                    ctx
                    |> UnificationContext.Updaters.EvalState(
                      TypeCheckState.Updaters.Bindings(Map.add !p1.Name v1 >> Map.add !p2.Name v2)
                    ),
                    ctx
                    |> UnificationContext.Updaters.EvalState(TypeCheckState.Updaters.Bindings(Map.add !p1.Name v1)),
                    ctx
                    |> UnificationContext.Updaters.EvalState(TypeCheckState.Updaters.Bindings(Map.add !p2.Name v2))
                else
                  let s1 = TypeSymbol.Create(Identifier.LocalScope p1.Name)
                  let s2 = TypeSymbol.Create(Identifier.LocalScope p2.Name)

                  return
                    ctx
                    |> UnificationContext.Updaters.EvalState(
                      TypeCheckState.Updaters.Symbols.Types(Map.add !p1.Name s1 >> Map.add !p2.Name s2)
                    ),
                    ctx
                    |> UnificationContext.Updaters.EvalState(TypeCheckState.Updaters.Symbols.Types(Map.add !p1.Name s1)),
                    ctx
                    |> UnificationContext.Updaters.EvalState(TypeCheckState.Updaters.Symbols.Types(Map.add !p2.Name s2))
              }

            let! v1 =
              t1
              |> TypeExpr.AsValue
                loc0
                ((fun t -> TypeCheckState.tryFindType (t |> ctx1.Scope.Resolve, loc0))
                 >> reader.Map fst
                 >> Reader.Run ctx1.EvalState)
                ((fun s -> TypeCheckState.tryFindTypeSymbol (s |> ctx1.Scope.Resolve, loc0))
                 >> Reader.Run ctx1.EvalState)
              |> state.OfSum

            let! v2 =
              t2
              |> TypeExpr.AsValue
                loc0
                ((fun t -> TypeCheckState.tryFindType (t |> ctx2.Scope.Resolve, loc0))
                 >> reader.Map fst
                 >> Reader.Run ctx2.EvalState)
                ((fun s -> TypeCheckState.tryFindTypeSymbol (s |> ctx2.Scope.Resolve, loc0))
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
        // | TypeValue.Apply { value = v1, a1 }, TypeValue.Apply { value = v2, a2 } ->
        //   do! (v1 |> TypeValue.Var) == (v2 |> TypeValue.Var)
        //   do! a1 == a2
        | Set { value = e1 }, Set { value = e2 } -> do! e1 == e2
        | TypeValue.Tuple { value = e1 }, TypeValue.Tuple { value = e2 }
        | TypeValue.Sum { value = e1 }, TypeValue.Sum { value = e2 } when List.length e1 = List.length e2 ->
          for (v1, v2) in List.zip e1 e2 do
            do! v1 == v2
        | TypeValue.Record { value = e1 }, TypeValue.Record { value = e2 } when e1.Count = e2.Count ->
          for (k1, (v1, _)) in e1 |> OrderedMap.toSeq do
            let! v2, _ = e2 |> OrderedMap.tryFindWithError k1 "record field" k1.Name.LocalName |> ofSum
            do! v1 == v2
        | TypeValue.Union { value = e1 }, TypeValue.Union { value = e2 } when e1.Count = e2.Count ->
          for (k1, v1) in e1 |> OrderedMap.toSeq do
            let ofSum = Sum.mapRight (Errors.MapContext(replaceWith loc0)) >> state.OfSum
            let! v2 = e2 |> OrderedMap.tryFindWithError k1 "union field" k1.Name.LocalName |> ofSum
            do! v1 == v2
        | TypeValue.Schema e1, TypeValue.Schema e2 -> do! unifySchemas e1 e2
        | TypeValue.Entities e1, TypeValue.Entities e2 -> do! unifySchemas e1 e2
        | TypeValue.Relations e1, TypeValue.Relations e2 -> do! unifySchemas e1 e2
        | TypeValue.Entity(s1, e1, e1with_props, eid1), TypeValue.Entity(s2, e2, e2with_props, eid2) ->
          do! unifySchemas s1 s2
          do! e1 == e2
          do! e1with_props == e2with_props
          do! eid1 == eid2
        | TypeValue.Relation(s1, _, _, f1, f1with_props, fid1, t1, t1with_props, tid1),
          TypeValue.Relation(s2, _, _, f2, f2with_props, fid2, t2, t2with_props, tid2)
        | TypeValue.ForeignKeyRelation(s1, _, f1, f1with_props, fid1, t1, t1with_props, tid1),
          TypeValue.ForeignKeyRelation(s2, _, f2, f2with_props, fid2, t2, t2with_props, tid2) ->
          do! unifySchemas s1 s2
          do! f1 == f2
          do! f1with_props == f2with_props
          do! fid1 == fid2
          do! t1 == t2
          do! t1with_props == t2with_props
          do! tid1 == tid2
        | TypeValue.RelationLookupOption(s1, e1, id1), TypeValue.RelationLookupOption(s2, e2, id2) ->
          do! unifySchemas s1 s2
          do! e1 == e2
          do! id1 == id2
        | TypeValue.RelationLookupOne(s1, e1, id1), TypeValue.RelationLookupOne(s2, e2, id2) ->
          do! unifySchemas s1 s2
          do! e1 == e2
          do! id1 == id2
        | TypeValue.RelationLookupMany(s1, e1, id1), TypeValue.RelationLookupMany(s2, e2, id2) ->
          do! unifySchemas s1 s2
          do! e1 == e2
          do! id1 == id2
        | _ -> return! (fun () -> $"Cannot unify types: {left} and {right}") |> error |> state.Throw
      }

  type SymbolicTypeApplication<'ve> with
    static member Instantiate<'valueExt when 'valueExt: comparison>
      ()
      : TypeExprEvalPlain<'valueExt>
          -> Location
          -> SymbolicTypeApplication<'valueExt>
          -> State<TypeValue<'valueExt>, TypeInstantiateContext<'valueExt>, TypeCheckState<'valueExt>, Errors<Location>>
      =
      fun typeEval loc0 t ->
        state {
          // let error e = Errors.Singleton loc0 e

          match t with
          | SymbolicTypeApplication.Lookup(l, a) ->
            let! f = TypeValue.Instantiate () typeEval loc0 (l |> TypeValue.Lookup)
            let! a = TypeValue.Instantiate () typeEval loc0 a

            match f with
            | TypeValue.Application { value = f } ->
              return SymbolicTypeApplication.Application(f, a) |> TypeValue.CreateApplication
            | TypeValue.Lookup l -> return SymbolicTypeApplication.Lookup(l, a) |> TypeValue.CreateApplication
            | _ ->
              let application = TypeExpr.Apply(TypeExpr.FromTypeValue f, TypeExpr.FromTypeValue a)

              let! res =
                typeEval None loc0 application
                |> state.MapContext(TypeCheckContext.FromInstantiateContext)
                |> state.Map fst

              return res
          | SymbolicTypeApplication.Application(f, a) ->
            let! f = SymbolicTypeApplication.Instantiate () typeEval loc0 f
            let! a = TypeValue.Instantiate () typeEval loc0 a

            match f with
            | TypeValue.Application { value = f } ->
              return SymbolicTypeApplication.Application(f, a) |> TypeValue.CreateApplication
            | _ ->
              let application = TypeExpr.Apply(TypeExpr.FromTypeValue f, TypeExpr.FromTypeValue a)

              let! res =
                typeEval None loc0 application
                |> state.MapContext(TypeCheckContext.FromInstantiateContext)
                |> state.Map fst

              return res
        }

  and TypeValue<'ve> with
    static member Instantiate<'valueExt when 'valueExt: comparison>
      ()
      : TypeExprEvalPlain<'valueExt>
          -> Location
          -> TypeValue<'valueExt>
          -> State<TypeValue<'valueExt>, TypeInstantiateContext<'valueExt>, TypeCheckState<'valueExt>, Errors<Location>>
      =
      fun typeEval loc0 t ->
        state {
          let error e = Errors.Singleton loc0 e

          let instantiateSchema schema =
            state {
              let! entities =
                schema.Entities
                |> OrderedMap.map (fun _ e ->
                  state {
                    let! t = TypeValue.Instantiate () typeEval loc0 e.TypeOriginal
                    let! t' = TypeValue.Instantiate () typeEval loc0 e.TypeWithProps
                    let! id = TypeValue.Instantiate () typeEval loc0 e.Id

                    return
                      { e with
                          TypeOriginal = t
                          TypeWithProps = t'
                          Id = id }
                  })
                |> state.AllMapOrdered

              let! relations =
                schema.Relations
                |> OrderedMap.map (fun _ r -> state { return r })
                |> state.AllMapOrdered

              return
                { schema with
                    Entities = entities
                    Relations = relations }
            }

          match t with
          | TypeValue.Schema schema ->
            let! schema = instantiateSchema schema

            return schema |> TypeValue.Schema
          | TypeValue.Entities schema ->
            let! schema = instantiateSchema schema

            return schema |> TypeValue.Entities
          | TypeValue.Relations schema ->
            let! schema = instantiateSchema schema

            return schema |> TypeValue.Relations
          | TypeValue.Entity(schema, entityType, entityTypeWithProps, entityId) ->
            let! schema = instantiateSchema schema

            let! entityType = TypeValue.Instantiate () typeEval loc0 entityType
            let! entityTypeWithProps = TypeValue.Instantiate () typeEval loc0 entityTypeWithProps
            let! entityId = TypeValue.Instantiate () typeEval loc0 entityId

            return TypeValue.Entity(schema, entityType, entityTypeWithProps, entityId)
          | TypeValue.Relation(schema, relation_name, cardinality, from, fromWithProps, fromId, to_, toWithProps, toId) ->
            let! schema = instantiateSchema schema

            let! fromType = TypeValue.Instantiate () typeEval loc0 from
            let! fromTypeWithProps = TypeValue.Instantiate () typeEval loc0 fromWithProps
            let! fromId = TypeValue.Instantiate () typeEval loc0 fromId
            let! toType = TypeValue.Instantiate () typeEval loc0 to_
            let! toTypeWithProps = TypeValue.Instantiate () typeEval loc0 toWithProps
            let! toId = TypeValue.Instantiate () typeEval loc0 toId

            return
              TypeValue.Relation(
                schema,
                relation_name,
                cardinality,
                fromType,
                fromTypeWithProps,
                fromId,
                toType,
                toTypeWithProps,
                toId
              )
          | TypeValue.ForeignKeyRelation(schema, relation_name, from, fromWithProps, fromId, to_, toWithProps, toId) ->
            let! schema = instantiateSchema schema

            let! fromType = TypeValue.Instantiate () typeEval loc0 from
            let! fromTypeWithProps = TypeValue.Instantiate () typeEval loc0 fromWithProps
            let! fromId = TypeValue.Instantiate () typeEval loc0 fromId
            let! toType = TypeValue.Instantiate () typeEval loc0 to_
            let! toTypeWithProps = TypeValue.Instantiate () typeEval loc0 toWithProps
            let! toId = TypeValue.Instantiate () typeEval loc0 toId

            return
              TypeValue.ForeignKeyRelation(
                schema,
                relation_name,
                fromType,
                fromTypeWithProps,
                fromId,
                toType,
                toTypeWithProps,
                toId
              )
          | TypeValue.RelationLookupOption(schema, elementType, elementId) ->
            let! schema = instantiateSchema schema

            let! elementType = TypeValue.Instantiate () typeEval loc0 elementType
            let! elementId = TypeValue.Instantiate () typeEval loc0 elementId

            return TypeValue.RelationLookupOption(schema, elementType, elementId)
          | TypeValue.RelationLookupOne(schema, elementType, elementId) ->
            let! schema = instantiateSchema schema
            let! elementType = TypeValue.Instantiate () typeEval loc0 elementType
            let! elementId = TypeValue.Instantiate () typeEval loc0 elementId
            return TypeValue.RelationLookupOne(schema, elementType, elementId)
          | TypeValue.RelationLookupMany(schema, elementType, elementId) ->
            let! schema = instantiateSchema schema
            let! elementType = TypeValue.Instantiate () typeEval loc0 elementType
            let! elementId = TypeValue.Instantiate () typeEval loc0 elementId
            return TypeValue.RelationLookupMany(schema, elementType, elementId)
          | TypeValue.Imported({ Arguments = arguments } as t) ->
            // do Console.WriteLine $"Instantiating imported type {t}"
            // do Console.WriteLine $"Arguments: {arguments |> Seq.map (fun a -> a.ToFSharpString) |> Seq.toList}"
            let! args = arguments |> Seq.map (TypeValue.Instantiate () typeEval loc0) |> state.All
            // do Console.WriteLine $"Arguments instantiated: {args |> Seq.map (fun a -> a.ToFSharpString) |> Seq.toList}"
            // do Console.ReadLine() |> ignore
            let t = { t with Arguments = args }
            return TypeValue.Imported(t)
          | TypeValue.Application { value = app } -> return! SymbolicTypeApplication.Instantiate () typeEval loc0 app
          | TypeValue.Var v ->
            let! ctx = state.GetContext()
            let! s = state.GetState()

            if ctx.VisitedVars.Contains v then
              // return! Errors.Singleton $"Infinite type instantiation for variable {v}" |> state.Throw
              return t
            else
              let localCtx =
                { EvalState = s
                  Scope = ctx.Scope
                  TypeParameters = ctx.TypeParameters }

              let localState = s.Vars

              let! vClass, newLocalState =
                EquivalenceClasses<TypeVar, TypeValue<'valueExt>>.tryFind (v, loc0)
                |> State.mapState (fun (s, _) -> s.Classes) (fun (s, _) _ -> UnificationState.Create s)
                |> TypeValue.EquivalenceClassesOp<_, 'valueExt> loc0
                |> State.Run(localCtx, localState)
                |> sum.MapError fst
                |> state.OfSum

              do!
                state.SetState(
                  TypeCheckState.Updaters.Vars(newLocalState |> Option.map replaceWith |> Option.defaultValue id)
                )

              // |> state.MapContext(fun ctx ->
              //   { EvalState = ctx.Bindings
              //     Scope = ctx.Scope })

              // do Console.WriteLine($"Equivalence class for variable {v}: {vClass}")

              match vClass.Representative with
              | Some rep ->
                return!
                  TypeValue.Instantiate () typeEval loc0 rep
                  |> state.MapContext(TypeInstantiateContext.Updaters.VisitedVars(Set.add v))
              | None ->
                match
                  vClass.Variables
                  |> Set.toSeq
                  |> Seq.map (TypeValue.Var)
                  |> Seq.map (
                    TypeValue.Instantiate () typeEval loc0
                    >> state.MapContext(TypeInstantiateContext.Updaters.VisitedVars(Set.add v))
                  )
                  |> Seq.toList
                with
                | [] ->
                  return!
                    (fun () -> $"Variable {v} has no representative in the equivalence class")
                    |> error
                    |> state.Throw
                | x :: xs -> return! NonEmptyList.OfList(x, xs) |> state.Any
          | TypeValue.Lookup l ->
            let! ctx = state.GetContext()
            let! s = state.GetState()

            let! t =
              s.Bindings
              |> TypeBindings.tryFindWithError (l |> ctx.Scope.Resolve) "lookup" (fun () -> l.AsFSharpString) loc0
              |> state.OfSum
              |> state.Catch

            match t with
            | Left(t, _) -> return! TypeValue.Instantiate () typeEval loc0 t
            | Right _ ->
              return!
                state.Either
                  (ctx.TypeVariables
                   |> TypeVariablesScope.tryFindWithError l.LocalName "type variable" (fun () -> l.AsFSharpString) loc0
                   |> sum.Map fst
                   |> state.OfSum)
                  (ctx.TypeParameters
                   |> TypeParametersScope.tryFindWithError
                     l.LocalName
                     "type parameter"
                     (fun () -> l.AsFSharpString)
                     loc0
                   |> sum.Map(fun _ -> TypeValue.Lookup l)
                   |> state.OfSum)
          | TypeValue.Lambda { value = par, body } ->
            match body with
            | TypeExpr.FromTypeValue bodyTv ->

              let! bodyTv' =
                TypeValue.Instantiate () typeEval loc0 bodyTv
                |> state.MapContext(
                  TypeInstantiateContext.Updaters.TypeParameters(Map.add par.Name par.Kind)
                  >> TypeInstantiateContext.Updaters.TypeVariables(Map.remove par.Name)
                )

              return TypeValue.CreateLambda(par, TypeExpr.FromTypeValue bodyTv')
            | _ -> return TypeValue.CreateLambda(par, body)
          | TypeValue.Arrow { value = l, r
                              typeExprSource = n
                              typeCheckScopeSource = scope } ->
            let! l' = TypeValue.Instantiate () typeEval loc0 l
            let! r' = TypeValue.Instantiate () typeEval loc0 r

            return
              TypeValue.Arrow
                { value = l', r'
                  typeExprSource = n
                  typeCheckScopeSource = scope }
          // | TypeValue.Apply { value = var, arg; source = n } ->
          //   let! arg' = TypeValue.Instantiate () (TypeExpr.Eval ()) loc0 arg
          //   return TypeValue.Apply { value = var, arg'; source = n }
          | TypeValue.Map { value = l, r
                            typeExprSource = n
                            typeCheckScopeSource = scope } ->
            let! l' = TypeValue.Instantiate () typeEval loc0 l
            let! r' = TypeValue.Instantiate () typeEval loc0 r

            return
              TypeValue.Map
                { value = l', r'
                  typeExprSource = n
                  typeCheckScopeSource = scope }
          | TypeValue.Set { value = v
                            typeExprSource = n
                            typeCheckScopeSource = scope } ->
            let! v' = TypeValue.Instantiate () typeEval loc0 v

            return
              TypeValue.Set
                { value = v'
                  typeExprSource = n
                  typeCheckScopeSource = scope }
          | TypeValue.Tuple { value = es
                              typeExprSource = n
                              typeCheckScopeSource = scope } ->
            let! es' = es |> Seq.map (TypeValue.Instantiate () typeEval loc0) |> state.All

            return
              TypeValue.Tuple
                { value = es'
                  typeExprSource = n
                  typeCheckScopeSource = scope }
          | TypeValue.Sum { value = es
                            typeExprSource = n
                            typeCheckScopeSource = scope } ->
            let! es' = es |> Seq.map (TypeValue.Instantiate () typeEval loc0) |> state.All

            return
              TypeValue.Sum
                { value = es'
                  typeExprSource = n
                  typeCheckScopeSource = scope }
          | TypeValue.Record { value = es
                               typeExprSource = n
                               typeCheckScopeSource = scope } ->
            let! es' =
              es
              |> OrderedMap.map (fun _ (tv, k) ->
                tv |> TypeValue.Instantiate () typeEval loc0 |> state.Map(fun res -> res, k))
              |> state.AllMapOrdered

            return
              TypeValue.Record
                { value = es'
                  typeExprSource = n
                  typeCheckScopeSource = scope }
          | TypeValue.Union { value = es
                              typeExprSource = n
                              typeCheckScopeSource = scope } ->
            let! es' =
              es
              |> OrderedMap.map (fun _ -> TypeValue.Instantiate () typeEval loc0)
              |> state.AllMapOrdered

            return
              TypeValue.Union
                { value = es'
                  typeExprSource = n
                  typeCheckScopeSource = scope }
          | TypeValue.Primitive _ -> return t
        }
