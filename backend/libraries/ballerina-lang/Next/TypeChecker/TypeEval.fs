namespace Ballerina.DSL.Next.Types.TypeChecker

module Eval =
  open System
  open Ballerina.Fun
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.StdLib.String
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.Errors
  open Ballerina.Collections.Map
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.KindEval
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Terms.Patterns

  type TypeQueryRowExpr<'valueExt> with
    static member Eval<'ve when 've: comparison>(config: TypeCheckingConfig<'ve>) : TypeQueryRowExprEval<'ve> =
      fun typeCheckExpr _n loc0 q_row ->
        state {
          let (!) = TypeExpr.Eval<'ve> config typeCheckExpr None loc0
          // let (!!) = TypeQueryRowExpr.Eval<'ve> () typeCheckExpr None loc0
          // let! ctx = state.GetContext()

          // let error e = Errors.Singleton loc0 e

          let ofSum (p: Sum<'a, Errors<Unit>>) =
            p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

          match q_row with
          | TypeQueryRowExpr.PrimaryKey q ->
            let! q, q_k = !q
            do! q_k |> Kind.AsStar |> ofSum |> state.Ignore

            return TypeQueryRow.PrimaryKey q
          | TypeQueryRowExpr.Json q ->
            let! q, q_k = !q
            do! q_k |> Kind.AsStar |> ofSum |> state.Ignore

            return TypeQueryRow.Json q
          | TypeQueryRowExpr.PrimitiveType(pt, is_nullable) -> return TypeQueryRow.PrimitiveType(pt, is_nullable)
          | TypeQueryRowExpr.Tuple qs ->
            let! qs =
              qs
              |> List.map (fun q ->
                state {
                  let! q, q_k = !q
                  let! q = q |> TypeValue.AsQueryRow |> ofSum
                  do! q_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return q
                })
              |> state.All

            return TypeQueryRow.Tuple qs
          | TypeQueryRowExpr.Record qs ->
            let! qs =
              qs
              |> Map.map (fun _k q ->
                state {
                  let! q, q_k = !q
                  let! q = q |> TypeValue.AsQueryRow |> ofSum
                  do! q_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return q
                })
              |> state.AllMap

            return TypeQueryRow.Record qs
        }

  and TypeExpr<'valueExt> with
    static member EvalAsSymbol<'ve when 've: comparison>(config: TypeCheckingConfig<'ve>) : TypeExprSymbolEval<'ve> =
      fun exprTypeCheck loc0 t ->
        state {
          let (!) = TypeExpr.EvalAsSymbol config exprTypeCheck loc0
          let (!!) = TypeExpr.Eval config exprTypeCheck None loc0
          let! ctx = state.GetContext()

          let error e = Errors.Singleton loc0 e

          let ofSum (p: Sum<'a, Errors<Unit>>) =
            p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

          match t with
          | TypeExpr.NewSymbol name -> return TypeSymbol.Create(Identifier.LocalScope name)
          | TypeExpr.Lookup v ->

            return!
              reader.Any(
                NonEmptyList.OfList(
                  TypeCheckState.tryFindTypeSymbol (v |> ctx.Scope.Resolve, loc0),
                  [ TypeCheckState.tryFindRecordFieldSymbol (v |> ctx.Scope.Resolve, loc0)
                    TypeCheckState.tryFindUnionCaseSymbol (v |> ctx.Scope.Resolve, loc0)
                    TypeCheckState.tryFindTypeSymbol (v |> TypeCheckScope.Empty.Resolve, loc0)
                    TypeCheckState.tryFindRecordFieldSymbol (v |> TypeCheckScope.Empty.Resolve, loc0)
                    TypeCheckState.tryFindUnionCaseSymbol (v |> TypeCheckScope.Empty.Resolve, loc0) ]
                )
              )
              |> state.OfStateReader
          | TypeExpr.Apply(f, a) ->
            let! f, f_k = !!f
            do! Kind.AsArrow f_k |> ofSum |> state.Ignore
            let! a, a_k = !!a

            let! param, body = f |> TypeValue.AsLambda |> ofSum |> state.Map WithSourceMapping.Getters.Value

            let! ctx = state.GetContext()
            let closure = ctx.TypeVariables |> Map.add (param.Name) (a, a_k)

            return!
              !body
              |> state.MapContext(TypeCheckContext.Updaters.TypeVariables(replaceWith closure))
          | _ ->
            return!
              (fun () -> $"Error: invalid type expression when evaluating for symbol, got {t}")
              |> error
              |> state.Throw
        }

    static member Eval<'ve when 've: comparison>(config: TypeCheckingConfig<'ve>) : TypeExprEval<'ve> =
      fun typeCheckExpr n loc0 t ->
        state {
          let { QueryTypeSymbol = _query_type_symbol
                MkQueryType = _mk_query_type
                MkListType = mk_list_type } =
            config

          let (!) = TypeExpr.Eval<'ve> config typeCheckExpr None loc0

          let (!!) = TypeExpr.EvalAsSymbol<'ve> config typeCheckExpr loc0

          let (!!!) = TypeQueryRowExpr.Eval<'ve> config typeCheckExpr None loc0

          let! ctx = state.GetContext()

          let error e = Errors.Singleton loc0 e

          let ofSum (p: Sum<'a, Errors<Unit>>) =
            p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

          let source =
            match n with
            | Some name -> TypeExprSourceMapping.OriginExprTypeLet(name, t)
            | None -> TypeExprSourceMapping.OriginTypeExpr t

          match t with
          | TypeExpr.Entities schema ->
            let! schema, schema_k = !schema
            do! schema_k |> Kind.AsSchema |> ofSum |> state.Ignore

            let! a_schema = schema |> TypeValue.AsSchema |> ofSum
            return TypeValue.CreateEntities a_schema, Kind.Star
          | TypeExpr.Relations schema ->
            let! schema, schema_k = !schema
            do! schema_k |> Kind.AsSchema |> ofSum |> state.Ignore

            let! a_schema = schema |> TypeValue.AsSchema |> ofSum
            return TypeValue.CreateRelations a_schema, Kind.Star
          | TypeExpr.Entity(s, e, e_with_props, id) ->
            let! s, s_k = !s
            do! s_k |> Kind.AsSchema |> ofSum |> state.Ignore
            let! e, e_k = !e
            do! e_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! e_with_props, e_with_props_k = !e_with_props
            do! e_with_props_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! id, id_k = !id
            do! id_k |> Kind.AsStar |> ofSum |> state.Ignore

            let! a_schema = s |> TypeValue.AsSchema |> ofSum
            return TypeValue.CreateEntity(a_schema, e, e_with_props, id), Kind.Star
          | TypeExpr.Relation(s, f, f_with_props, f_id, t, t_with_props, t_id) ->
            let! s, s_k = !s
            do! s_k |> Kind.AsSchema |> ofSum |> state.Ignore
            let! f, f_k = !f
            do! f_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! f_with_props, f_with_props_k = !f_with_props
            do! f_with_props_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! f_id, f_id_k = !f_id
            do! f_id_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! t, t_k = !t
            do! t_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! t_with_props, t_with_props_k = !t_with_props
            do! t_with_props_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! t_id, t_id_k = !t_id
            do! t_id_k |> Kind.AsStar |> ofSum |> state.Ignore

            let! a_schema = s |> TypeValue.AsSchema |> ofSum

            return
              TypeValue.CreateRelation(
                a_schema,
                "@implicit" |> SchemaRelationName.Create,
                None,
                f,
                f_with_props,
                f_id,
                t,
                t_with_props,
                t_id
              ),
              Kind.Star
          | TypeExpr.RelationLookupOne(s, t', t_id, f_id) ->
            let! s, s_k = !s
            do! s_k |> Kind.AsSchema |> ofSum |> state.Ignore
            let! t', t'_k = !t'
            do! t'_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! f_id, f_id_k = !f_id
            do! f_id_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! t_id, t_id_k = !t_id
            do! t_id_k |> Kind.AsStar |> ofSum |> state.Ignore

            let! a_schema = s |> TypeValue.AsSchema |> ofSum

            return TypeValue.CreateRelationLookupOne(a_schema, t', f_id, t_id), Kind.Star
          | TypeExpr.RelationLookupOption(s, t', t_id, f_id) ->
            let! s, s_k = !s
            do! s_k |> Kind.AsSchema |> ofSum |> state.Ignore
            let! t', t'_k = !t'
            do! t'_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! f_id, f_id_k = !f_id
            do! f_id_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! t_id, t_id_k = !t_id
            do! t_id_k |> Kind.AsStar |> ofSum |> state.Ignore

            let! a_schema = s |> TypeValue.AsSchema |> ofSum

            return TypeValue.CreateRelationLookupOption(a_schema, t', f_id, t_id), Kind.Star
          | TypeExpr.RelationLookupMany(s, t', t_id, f_id) ->
            let! s, s_k = !s
            do! s_k |> Kind.AsSchema |> ofSum |> state.Ignore
            let! t', t'_k = !t'
            do! t'_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! f_id, f_id_k = !f_id
            do! f_id_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! t_id, t_id_k = !t_id
            do! t_id_k |> Kind.AsStar |> ofSum |> state.Ignore

            let! a_schema = s |> TypeValue.AsSchema |> ofSum

            return TypeValue.CreateRelationLookupMany(a_schema, t', f_id, t_id), Kind.Star
          | TypeExpr.Schema parsed_schema ->
            return! SchemaTypeEval.evalSchemaExpr config typeCheckExpr (!) loc0 parsed_schema
          | TypeExpr.FromTypeValue tv ->
            // do Console.WriteLine($"Instantiating type value {tv}")
            let! (ctx: TypeCheckContext<'ve>) = state.GetContext()
            let! (s: TypeCheckState<'ve>) = state.GetState()
            let scope = ctx.TypeVariables |> Map.map (fun _ (_, k) -> k)
            let scope = Map.merge (fun _ -> id) scope ctx.TypeParameters
            let! k = TypeValue.KindEval () n loc0 tv |> state.MapContext(fun _ -> scope)

            let! tv =
              tv
              |> TypeValue.Instantiate () (TypeExpr.Eval config typeCheckExpr) loc0
              |> State.Run(TypeInstantiateContext.FromEvalContext(ctx), s)
              |> sum.Map fst
              |> sum.MapError fst
              |> state.OfSum

            // do Console.WriteLine($"Instantiated type value to {tv}")

            return TypeValue.SetSourceMapping(tv, source), k
          | TypeExpr.Imported i ->
            let! parameters =
              i.Parameters
              |> List.map (fun p -> !(TypeExpr.Lookup(Identifier.LocalScope p.Name)) |> state.Map fst)
              |> state.All

            let! args = i.Arguments |> List.map (fun p -> !p.AsExpr |> state.Map fst) |> state.All

            return
              TypeValue.Imported
                { i with
                    Parameters = []
                    Arguments = parameters @ args },
              Kind.Star
          | TypeExpr.NewSymbol _ -> return! (fun () -> $"Errors cannot evaluate {t} as a type") |> error |> state.Throw
          | TypeExpr.Primitive p ->
            return
              TypeValue.Primitive
                { value = p
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.Lookup v ->
            // do Console.WriteLine($"Looking up type for {v} in scope {ctx.Scope}")

            let! res =
              state.Any(
                NonEmptyList.OfList(
                  TypeCheckContext.tryFindTypeVariable (v.LocalName, loc0) |> state.OfReader,
                  [ TypeCheckContext.tryFindTypeParameter (v.LocalName, loc0)
                    |> reader.Map(fun k -> TypeValue.Lookup v, k)
                    |> state.OfReader
                    TypeCheckState.tryFindType (v |> TypeCheckScope.Empty.Resolve, loc0)
                    |> state.OfStateReader

                    state {
                      match v with
                      | Identifier.LocalScope "SchemaEntities" ->
                        return
                          TypeValue.CreateLambda(
                            TypeParameter.Create("s", Kind.Schema),
                            TypeExpr.Entities(TypeExpr.Lookup(Identifier.LocalScope "s"))
                          ),
                          Kind.Arrow(Kind.Schema, Kind.Star)
                      | Identifier.LocalScope "SchemaEntity" ->
                        return
                          TypeValue.CreateLambda(
                            TypeParameter.Create("s", Kind.Schema),
                            TypeExpr.Lambda(
                              TypeParameter.Create("e", Kind.Star),
                              TypeExpr.Lambda(
                                TypeParameter.Create("e_with_props", Kind.Star),
                                TypeExpr.Lambda(
                                  TypeParameter.Create("id", Kind.Star),
                                  TypeExpr.Entity(
                                    TypeExpr.Lookup(Identifier.LocalScope "s"),
                                    TypeExpr.Lookup(Identifier.LocalScope "e"),
                                    TypeExpr.Lookup(Identifier.LocalScope "e_with_props"),
                                    TypeExpr.Lookup(Identifier.LocalScope "id")
                                  )
                                )
                              )
                            )
                          ),
                          Kind.Arrow(
                            Kind.Schema,
                            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))
                          )
                      | Identifier.LocalScope "SchemaRelation" ->
                        return
                          TypeValue.CreateLambda(
                            TypeParameter.Create("s", Kind.Schema),
                            TypeExpr.Lambda(
                              TypeParameter.Create("f", Kind.Star),
                              TypeExpr.Lambda(
                                TypeParameter.Create("f_with_props", Kind.Star),
                                TypeExpr.Lambda(
                                  TypeParameter.Create("f_id", Kind.Star),
                                  TypeExpr.Lambda(
                                    TypeParameter.Create("t", Kind.Star),
                                    TypeExpr.Lambda(
                                      TypeParameter.Create("t_with_props", Kind.Star),
                                      TypeExpr.Lambda(
                                        TypeParameter.Create("t_id", Kind.Star),
                                        TypeExpr.Relation(
                                          TypeExpr.Lookup(Identifier.LocalScope "s"),
                                          TypeExpr.Lookup(Identifier.LocalScope "f"),
                                          TypeExpr.Lookup(Identifier.LocalScope "f_with_props"),
                                          TypeExpr.Lookup(Identifier.LocalScope "f_id"),
                                          TypeExpr.Lookup(Identifier.LocalScope "t"),
                                          TypeExpr.Lookup(Identifier.LocalScope "t_with_props"),
                                          TypeExpr.Lookup(Identifier.LocalScope "t_id")
                                        )
                                      )
                                    )
                                  )
                                )
                              )
                            )
                          ),
                          Kind.Arrow(
                            Kind.Schema,
                            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))
                          )
                      | Identifier.LocalScope "SchemaLookupOne" ->
                        return
                          TypeValue.CreateLambda(
                            TypeParameter.Create("s", Kind.Schema),
                            TypeExpr.Lambda(
                              TypeParameter.Create("f_id", Kind.Star),
                              TypeExpr.Lambda(
                                TypeParameter.Create("t_id", Kind.Star),
                                TypeExpr.Lambda(
                                  TypeParameter.Create("t_with_props", Kind.Star),
                                  TypeExpr.RelationLookupOne(
                                    TypeExpr.Lookup(Identifier.LocalScope "s"),
                                    TypeExpr.Lookup(Identifier.LocalScope "f_id"),
                                    TypeExpr.Lookup(Identifier.LocalScope "t_id"),
                                    TypeExpr.Lookup(Identifier.LocalScope "t_with_props")
                                  )
                                )
                              )
                            )
                          ),
                          Kind.Arrow(
                            Kind.Schema,
                            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))
                          )
                      | Identifier.LocalScope "SchemaLookupOption" ->
                        return
                          TypeValue.CreateLambda(
                            TypeParameter.Create("s", Kind.Schema),
                            TypeExpr.Lambda(
                              TypeParameter.Create("f_id", Kind.Star),
                              TypeExpr.Lambda(
                                TypeParameter.Create("t_id", Kind.Star),
                                TypeExpr.Lambda(
                                  TypeParameter.Create("t_with_props", Kind.Star),
                                  TypeExpr.RelationLookupOption(
                                    TypeExpr.Lookup(Identifier.LocalScope "s"),
                                    TypeExpr.Lookup(Identifier.LocalScope "f_id"),
                                    TypeExpr.Lookup(Identifier.LocalScope "t_id"),
                                    TypeExpr.Lookup(Identifier.LocalScope "t_with_props")
                                  )
                                )
                              )
                            )
                          ),
                          Kind.Arrow(
                            Kind.Schema,
                            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))
                          )
                      | Identifier.LocalScope "SchemaLookupMany" ->
                        return
                          TypeValue.CreateLambda(
                            TypeParameter.Create("s", Kind.Schema),
                            TypeExpr.Lambda(
                              TypeParameter.Create("f_id", Kind.Star),
                              TypeExpr.Lambda(
                                TypeParameter.Create("t_id", Kind.Star),
                                TypeExpr.Lambda(
                                  TypeParameter.Create("t_with_props", Kind.Star),
                                  TypeExpr.RelationLookupMany(
                                    TypeExpr.Lookup(Identifier.LocalScope "s"),
                                    TypeExpr.Lookup(Identifier.LocalScope "f_id"),
                                    TypeExpr.Lookup(Identifier.LocalScope "t_id"),
                                    TypeExpr.Lookup(Identifier.LocalScope "t_with_props")
                                  )
                                )
                              )
                            )
                          ),
                          Kind.Arrow(
                            Kind.Schema,
                            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))
                          )
                      | _ ->
                        let! c = state.GetContext()

                        return!
                          (fun () -> $"Error: cannot find type for {v} with context ({c.TypeVariables.AsFSharpString})")
                          |> error
                          |> state.Throw
                          |> state.MapError(Errors<_>.MapPriority(replaceWith ErrorPriority.High))
                    } ]
                )
              )
              |> state.MapError(Errors<_>.FilterHighestPriorityOnly)

            // do Console.WriteLine($"Lookup {v} resolved to {res}")

            return res

          | TypeExpr.LetSymbols(xts, symbolsKind, rest) ->
            do!
              xts
              |> Seq.map (fun x ->
                state {
                  let x0 = Identifier.LocalScope x
                  // match ctx.Scope.Type with
                  // | Some t -> Identifier.FullyQualified([ t ], x)
                  // | None -> Identifier.LocalScope x

                  let s_x = TypeSymbol.Create(x0)
                  let x = x0 |> ctx.Scope.Resolve

                  do! TypeCheckState.bindIdentifierToResolvedIdentifier x x0

                  match ctx.Scope.Type with
                  | Some t ->
                    do!
                      TypeCheckState.bindIdentifierToResolvedIdentifier
                        x
                        (Identifier.FullyQualified([ t ], x0.LocalName))
                  | None -> ()

                  // do Console.WriteLine($"Binding symbol {s_x.Name.ToString()} to {x.ToString()}")

                  match symbolsKind with
                  | RecordFields -> do! TypeCheckState.bindRecordFieldSymbol x s_x
                  | UnionConstructors -> do! TypeCheckState.bindUnionCaseSymbol x s_x

                })
              |> state.All
              |> state.Ignore

            let! resultValue, resultKind = !rest
            return TypeValue.SetSourceMapping(resultValue, source), resultKind
          | TypeExpr.Let(x, t_x, rest) ->

            return!
              state.Either3
                (state {
                  let! t_x = !t_x

                  let! resultValue, resultKind =
                    !rest
                    |> state.MapContext(TypeCheckContext.Updaters.TypeVariables(Map.add x t_x))

                  return TypeValue.SetSourceMapping(resultValue, source), resultKind
                })
                (state {
                  let! s_x = !!t_x
                  let x = Identifier.LocalScope x |> ctx.Scope.Resolve
                  do! TypeCheckState.bindTypeSymbol x s_x
                  let! resultValue, resultKind = !rest
                  return TypeValue.SetSourceMapping(resultValue, source), resultKind
                })
                (state { return! (fun () -> $"Error: cannot evaluate let binding {x}") |> error |> state.Throw }
                 |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High)))

          | TypeExpr.Apply(f, a) ->
            // do Console.WriteLine($"Evaluating type application of {f} to {a}")
            let! f, f_k = !f
            // do Console.WriteLine($"Evaluated function part to {f}\n{f_k}")
            // do Console.ReadLine() |> ignore
            let! f_k_i, f_k_o = f_k |> Kind.AsArrow |> ofSum

            return!
              state.Either5
                (state {
                  let! param, body = f |> TypeValue.AsLambda |> ofSum |> state.Map WithSourceMapping.Getters.Value

                  return!
                    state {

                      match param.Kind with
                      | Kind.Symbol ->
                        let! a = !!a

                        do!
                          TypeCheckState.bindTypeSymbol
                            (param.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve)
                            a

                        let! resultValue, resultKind = !body
                        return TypeValue.SetSourceMapping(resultValue, source), resultKind
                      | _ ->
                        let! a = !a

                        // do Console.WriteLine($"Applying type lambda param {param} to argument {a |> fst} in {body}")
                        // do Console.ReadLine() |> ignore

                        let! resultValue, resultKind =
                          !body
                          |> state.MapContext(TypeCheckContext.Updaters.TypeVariables(Map.add param.Name a))

                        // do Console.WriteLine($"Result of type application is {resultValue.AsFSharpString}")
                        // do Console.ReadLine() |> ignore

                        return TypeValue.SetSourceMapping(resultValue, source), resultKind
                    }
                    |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
                })
                (state {
                  let! f_i = f |> TypeValue.AsImported |> ofSum

                  return!
                    state {

                      // let! ctx = state.GetContext()
                      // do Console.WriteLine($"Evaluating argument part {a}")
                      // do Console.WriteLine($"Context is {ctx.TypeParameters.ToFSharpString}")
                      // do Console.ReadLine() |> ignore
                      let! a, a_k = !a
                      // do Console.WriteLine($"aka {a}")
                      // do Console.ReadLine() |> ignore

                      if a_k <> f_k_i then
                        return!
                          (fun () -> $"Error: cannot apply type {f} of input kind {f_k_i} to argument of kind {a_k}")
                          |> error
                          |> state.Throw
                      else
                        match f_i.Parameters with
                        | [] ->
                          return!
                            (fun () -> $"Error: cannot apply imported type {f_i.Id.Name} with no parameters")
                            |> error
                            |> state.Throw
                        | _ :: ps ->
                          return
                            TypeValue.Imported
                              { f_i with
                                  Parameters = ps
                                  Arguments = f_i.Arguments @ [ a ] },
                            f_k_o
                    }
                    |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
                })
                (state {
                  let! f_l = f |> TypeValue.AsLookup |> ofSum

                  return!
                    state {

                      let! a, a_k = !a

                      if a_k <> f_k_i then
                        return!
                          (fun () -> $"Error: cannot apply type {f} of input kind {f_k_i} to argument of kind {a_k}")
                          |> error
                          |> state.Throw
                      else
                        return TypeValue.CreateApplication(SymbolicTypeApplication.Lookup(f_l, a)), f_k_o
                    }
                    |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
                })
                (state {
                  let! { value = f_app } = f |> TypeValue.AsApplication |> ofSum

                  return!
                    state {

                      let! a, a_k = !a

                      if a_k <> f_k_i then
                        return!
                          (fun () -> $"Error: cannot apply type {f} of input kind {f_k_i} to argument of kind {a_k}")
                          |> error
                          |> state.Throw
                      else
                        return TypeValue.CreateApplication(SymbolicTypeApplication.Application(f_app, a)), f_k_o
                    }
                    |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
                })
                (state {
                  do! f |> TypeValue.AsQueryTypeFunction |> ofSum

                  return!
                    state {
                      let! a_t, a_k = !a
                      // do Console.WriteLine $"Applying query type function to argument of type {a_t} and kind {a_k}"
                      do! a_k |> Kind.AsQueryRow |> ofSum |> state.Ignore

                      return!
                        state.Either3
                          (state {
                            let! a_t = a_t |> TypeValue.AsQueryRow |> ofSum

                            let rec try_convert a_t =
                              state {
                                match a_t with
                                | TypeQueryRow.Json j -> return j, Kind.Star
                                | TypeQueryRow.PrimaryKey k -> return k, Kind.Star
                                | TypeQueryRow.PrimitiveType(pt, false) ->
                                  return TypeValue.CreatePrimitive pt, Kind.Star
                                | TypeQueryRow.PrimitiveType(pt, true) ->
                                  return
                                    TypeValue.CreateSum [ TypeValue.CreateUnit(); TypeValue.CreatePrimitive pt ],
                                    Kind.Star
                                | TypeQueryRow.Tuple ts ->
                                  let! ts = ts |> List.map (fun t -> try_convert t) |> state.All
                                  let ts = ts |> List.map fst
                                  return TypeValue.CreateTuple ts, Kind.Star
                                | TypeQueryRow.Record _ ->
                                  return!
                                    (fun () ->
                                      $"Error: cannot return record query rows, please convert to a tuple first")
                                    |> error
                                    |> state.Throw
                                | TypeQueryRow.Array inner ->
                                  let! inner_t, _ = try_convert inner
                                  return mk_list_type inner_t, Kind.Star
                              }

                            let! result, _ = a_t |> try_convert

                            return TypeValue.SetSourceMapping(result, source), Kind.Star
                          })
                          (state {
                            let! a_t = a_t |> TypeValue.AsLookup |> ofSum
                            return TypeValue.CreateApplication(SymbolicTypeApplication.FromQueryRow a_t), Kind.Star
                          })
                          (state {
                            let! a_t = a_t |> TypeValue.AsVar |> ofSum

                            return
                              TypeValue.CreateApplication(
                                SymbolicTypeApplication.FromQueryRow(a_t.Name |> Identifier.LocalScope)
                              ),
                              Kind.Star
                          })
                    }
                    |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
                })
              |> state.MapError(Errors<_>.FilterHighestPriorityOnly)
          | TypeExpr.Lambda(param, bodyExpr) ->
            // let fresh_var_t =
            //   TypeValue.Var(
            //     { TypeVar.Name = param.Name
            //       Guid = Guid.CreateVersion7() }
            //   )

            // let! ctx = state.GetContext()
            // let closure = ctx.TypeVariables
            // let closure = ctx.TypeVariables |> Map.add param.Name (fresh_var_t, param.Kind)
            // do Console.WriteLine($"Type lambda closure: {closure.ToFSharpString}")
            // do Console.ReadLine() |> ignore

            // do Console.WriteLine($"Evaluating type lambda with param {param} and body {bodyExpr}")

            let! body_t, body_k =
              !bodyExpr
              // |> TypeExpr.KindEval n loc0
              |> state.MapContext(
                TypeCheckContext.Updaters.TypeParameters(Map.add param.Name param.Kind)
                >> TypeCheckContext.Updaters.TypeVariables(Map.remove param.Name)
              )

            // do Console.WriteLine($"Evaluated type lambda body to {body_t}")

            // |> state.MapContext(TypeExprEvalContext.Updaters.TypeVariables(replaceWith closure))

            return
              TypeValue.Lambda
                { value = param, body_t.AsExpr
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Arrow(param.Kind, body_k)
          | TypeExpr.Arrow(input, output) ->
            // do Console.WriteLine($"Evaluating arrow from {input} to {output}")
            let! input, input_k = !input
            // do Console.WriteLine $"Evaluated input type to {input} with kind {input_k}"
            let! output, output_k = !output

            do!
              sum.Any2 (input_k |> Kind.AsStar) (input_k |> Kind.AsSchema)
              |> ofSum
              |> state.Ignore

            do! output_k |> Kind.AsStar |> ofSum |> state.Ignore

            return
              TypeValue.Arrow
                { value = (input, output)
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.Record(fields) ->
            let! fields =
              fields
              |> Seq.map (fun (k, v) ->
                state {
                  let! k = !!k
                  let! v, v_k = !v
                  // do! v_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return (k, (v, v_k))
                })
              |> state.All
              |> state.Map(OrderedMap.ofSeq)

            return
              TypeValue.Record
                { value = fields
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.Tuple(items) ->
            let! items =
              items
              |> List.map (fun i ->
                state {
                  let! i, i_k = !i
                  do! i_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return i
                })
              |> state.All

            return
              TypeValue.Tuple
                { value = items
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.Union(cases) ->
            let! cases =
              cases
              |> Seq.map (fun (k, v) ->
                state {
                  let! k = !!k
                  let! v, v_k = !v
                  do! v_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return (k, v)
                })
              |> state.All
              |> state.Map(OrderedMap.ofSeq)

            return
              TypeValue.Union
                { value = cases
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.Set(element) ->
            let! element, element_k = !element
            do! element_k |> Kind.AsStar |> ofSum |> state.Ignore

            return
              TypeValue.Set
                { value = element
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.KeyOf(arg) ->
            let! arg, arg_k = !arg
            do! arg_k |> Kind.AsStar |> ofSum |> state.Ignore

            let! cases = arg |> TypeValue.AsRecord |> ofSum

            let mappedCasesFixThis =
              cases
              |> OrderedMap.map (fun _ _ -> TypeValue.CreatePrimitive PrimitiveType.Unit)

            return
              TypeValue.Union
                { value = mappedCasesFixThis
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.Sum(variants) ->
            let! variants =
              variants
              |> List.map (fun i ->
                state {
                  let! i, i_k = !i
                  do! i_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return i
                })
              |> state.All

            return
              TypeValue.Sum
                { value = variants
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.RecordDes(t, comp) ->
            let! tv, tk = !t
            do! tk |> Kind.AsStar |> ofSum |> state.Ignore

            let failure =
              (fun () -> $"Error: cannot evaluate record destructuring {tv}.{comp}")
              |> error
              |> state.Throw
              |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))

            match comp with
            | Left field ->
              return!
                state.Any(
                  state {
                    let! fields = tv |> TypeValue.AsRecord |> ofSum

                    let! (_, (field_t, field_k)) =
                      fields
                      |> OrderedMap.toSeq
                      |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = field.Name)
                      |> Sum.fromOption (fun () ->
                        (fun () -> $"Error: cannot find field {field} in record type {tv}")
                        |> Errors.Singleton loc0)
                      |> state.OfSum

                    return TypeValue.SetSourceMapping(field_t, source), field_k
                  },
                  [ state {
                      let! _, cases = tv |> TypeValue.AsUnion |> ofSum

                      let! (_, case_t) =
                        cases
                        |> OrderedMap.toSeq
                        |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = field.Name)
                        |> Sum.fromOption (fun () ->
                          (fun () -> $"Error: cannot find case {field} in union type {tv}")
                          |> Errors.Singleton loc0)
                        |> state.OfSum

                      return TypeValue.SetSourceMapping(case_t, source), Kind.Star
                    }
                    failure ]
                )
            | Right item ->
              return!
                state.Any(
                  state {
                    let! items = tv |> TypeValue.AsTuple |> ofSum

                    if item < 1 || item > items.Length then
                      return!
                        (fun () -> $"Error: tuple index {item} is out of bounds for tuple type {tv}")
                        |> Errors.Singleton loc0
                        |> state.Throw
                    else
                      let! item_t =
                        items
                        |> Seq.tryItem (item - 1)
                        |> Sum.fromOption (fun () ->
                          (fun () -> $"Error: cannot find tuple index {item} in tuple type {tv}")
                          |> Errors.Singleton loc0)
                        |> state.OfSum

                      return TypeValue.SetSourceMapping(item_t, source), Kind.Star
                  },
                  [ state {
                      let! items = tv |> TypeValue.AsSum |> ofSum

                      if item < 1 || item > items.Length then
                        return!
                          (fun () -> $"Error: sum case {item} is out of bounds for sum type {tv}")
                          |> Errors.Singleton loc0
                          |> state.Throw
                      else
                        let! item_t =
                          items
                          |> Seq.tryItem (item - 1)
                          |> Sum.fromOption (fun () ->
                            (fun () -> $"Error: cannot find sum case {item} in sum type {tv}")
                            |> Errors.Singleton loc0)
                          |> state.OfSum

                        return TypeValue.SetSourceMapping(item_t, source), Kind.Star
                    }
                    state {
                      let! imported = tv |> TypeValue.AsImported |> ofSum

                      let! item_t =
                        imported.Arguments
                        |> List.tryItem (item - 1)
                        |> Sum.fromOption (fun () ->
                          (fun () -> $"Error: cannot find argument {item} in imported type {tv}")
                          |> Errors.Singleton loc0)
                        |> state.OfSum

                      return TypeValue.SetSourceMapping(item_t, source), Kind.Star
                    }
                    failure ]
                )
          | TypeExpr.Flatten(type1, type2) ->
            let! type1, type1_k = !type1
            let! type2, type2_k = !type2
            do! type1_k |> Kind.AsStar |> ofSum |> state.Ignore
            do! type2_k |> Kind.AsStar |> ofSum |> state.Ignore

            return!
              state.Either3
                (state {
                  let! cases1 = type1 |> TypeValue.AsUnion |> sum.Map snd |> ofSum

                  let! cases2 = type2 |> TypeValue.AsUnion |> sum.Map snd |> ofSum

                  let cases1 = cases1 |> OrderedMap.toSeq
                  let keys1 = cases1 |> Seq.map fst |> Set.ofSeq

                  let cases2 = cases2 |> OrderedMap.toSeq
                  let keys2 = cases2 |> Seq.map fst |> Set.ofSeq

                  if keys1 |> Set.intersect keys2 |> Set.isEmpty then
                    let cases = cases1 |> Seq.append cases2 |> OrderedMap.ofSeq

                    return
                      TypeValue.Union
                        { value = cases
                          typeExprSource = source
                          typeCheckScopeSource = ctx.Scope },
                      Kind.Star
                  else
                    return!
                      (fun () -> $"Error: cannot flatten types with overlapping keys: {keys1} and {keys2}")
                      |> error
                      |> state.Throw
                })
                (state {
                  let! fields1 = type1 |> TypeValue.AsRecord |> ofSum

                  let! fields2 = type2 |> TypeValue.AsRecord |> ofSum

                  let fields1 = fields1 |> OrderedMap.toSeq
                  let keys1 = fields1 |> Seq.map fst |> Set.ofSeq

                  let fields2 = fields2 |> OrderedMap.toSeq
                  let keys2 = fields2 |> Seq.map fst |> Set.ofSeq

                  if keys1 |> Set.intersect keys2 |> Set.isEmpty then
                    let fields = fields1 |> Seq.append fields2 |> OrderedMap.ofSeq

                    return
                      TypeValue.Record
                        { value = fields
                          typeExprSource = source
                          typeCheckScopeSource = ctx.Scope },
                      Kind.Star
                  else
                    return!
                      (fun () -> $"Error: cannot flatten types with overlapping keys: {keys1} and {keys2}")
                      |> error
                      |> state.Throw
                })
                (state { return! (fun () -> $"Error: cannot evaluate flattening ") |> error |> state.Throw }
                 |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High)))
          | TypeExpr.Exclude(type1, type2) ->
            let! type1, type1_k = !type1
            let! type2, type2_k = !type2
            do! type1_k |> Kind.AsStar |> ofSum |> state.Ignore
            do! type2_k |> Kind.AsStar |> ofSum |> state.Ignore

            return!
              state.Either3
                (state {
                  let! cases1 = type1 |> TypeValue.AsUnion |> sum.Map snd |> ofSum

                  let! cases2 = type2 |> TypeValue.AsUnion |> sum.Map snd |> ofSum

                  let keys2 = cases2 |> OrderedMap.keys |> Set.ofSeq
                  let cases = cases1 |> OrderedMap.filter (fun k _ -> keys2 |> Set.contains k |> not)

                  return
                    TypeValue.Union
                      { value = cases
                        typeExprSource = source
                        typeCheckScopeSource = ctx.Scope },
                    Kind.Star
                })
                (state {
                  let! fields1 = type1 |> TypeValue.AsRecord |> ofSum
                  let! fields2 = type2 |> TypeValue.AsRecord |> ofSum

                  let keys2 = fields2 |> OrderedMap.keys |> Set.ofSeq

                  let fields =
                    fields1 |> OrderedMap.filter (fun k _ -> keys2 |> Set.contains k |> not)

                  return
                    TypeValue.Record
                      { value = fields
                        typeExprSource = source
                        typeCheckScopeSource = ctx.Scope },
                    Kind.Star
                })
                (state { return! (fun () -> $"Error: cannot evaluate exclude ") |> error |> state.Throw }
                 |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High)))
          | TypeExpr.Rotate(t) ->
            let! t, t_k = !t
            do! t_k |> Kind.AsStar |> ofSum |> state.Ignore

            return!
              state.Either3
                (state {
                  let! cases = t |> TypeValue.AsUnion |> sum.Map snd |> ofSum
                  let cases = cases |> OrderedMap.map (fun _ v -> v, Kind.Star)

                  return
                    TypeValue.Record
                      { value = cases
                        typeExprSource = source
                        typeCheckScopeSource = ctx.Scope },
                    Kind.Star
                })
                (state {
                  let! fields = t |> TypeValue.AsRecord |> ofSum

                  let! _ =
                    fields
                    |> OrderedMap.map (fun _ (_, k) -> k |> Kind.AsStar |> ofSum |> state.Ignore)
                    |> OrderedMap.toList
                    |> List.map snd
                    |> state.All

                  return
                    TypeValue.Union
                      { value = fields |> OrderedMap.map (fun _ (v, _) -> v)
                        typeExprSource = source
                        typeCheckScopeSource = ctx.Scope },
                    Kind.Star
                })
                (state { return! (fun () -> $"Error: cannot evaluate rotation") |> error |> state.Throw }
                 |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High)))
          | TypeExpr.QueryRow q_row ->

            let! q_row = !!!q_row

            return TypeValue.QueryRow q_row, Kind.QueryRow

          | TypeExpr.FromQueryRow -> return TypeValue.QueryTypeFunction, Kind.Arrow(Kind.QueryRow, Kind.Star)

        }
