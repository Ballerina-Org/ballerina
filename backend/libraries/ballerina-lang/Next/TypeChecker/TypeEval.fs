namespace Ballerina.DSL.Next.Types.TypeChecker

module Eval =
  open System
  open Ballerina.Fun
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.StdLib.String
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
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

  type TypeExpr<'valueExt> with
    static member EvalAsSymbol<'ve when 've: comparison>() : TypeExprSymbolEval<'ve> =
      fun exprTypeCheck loc0 t ->
        state {
          let (!) = TypeExpr.EvalAsSymbol () exprTypeCheck loc0
          let (!!) = TypeExpr.Eval () exprTypeCheck None loc0
          let! ctx = state.GetContext()

          let error e = Errors.Singleton(loc0, e)

          let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
            p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

          match t with
          | TypeExpr.NewSymbol name -> return TypeSymbol.Create(Identifier.LocalScope name)
          | TypeExpr.Lookup v ->

            return!
              reader.Any(
                NonEmptyList.OfList(
                  TypeCheckState.tryFindTypeSymbol (v |> ctx.Scope.Resolve, loc0),
                  [ TypeCheckState.tryFindRecordFieldSymbol (v |> ctx.Scope.Resolve, loc0)
                    TypeCheckState.tryFindUnionCaseSymbol (v |> ctx.Scope.Resolve, loc0) ]
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
              $"Error: invalid type expression when evaluating for symbol, got {t}"
              |> error
              |> state.Throw
        }

    static member Eval<'ve when 've: comparison>() : TypeExprEval<'ve> =
      fun typeCheckExpr n loc0 t ->
        state {
          let (!) = TypeExpr.Eval<'ve> () typeCheckExpr None loc0
          let (!!) = TypeExpr.EvalAsSymbol<'ve> () typeCheckExpr loc0
          let! ctx = state.GetContext()

          let error e = Errors.Singleton(loc0, e)

          let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
            p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

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
          | TypeExpr.Schema schema ->
            let repeatedEntityNames =
              schema.Entities
              |> List.map (fun e -> e.Name)
              |> List.groupBy id
              |> List.filter (fun (_, l) -> List.length l > 1)
              |> List.map (fst >> fun n -> n.Name)

            if not (List.isEmpty repeatedEntityNames) then
              return!
                let sep = ", " in

                $"Error: schema has repeated entity names: {String.join sep repeatedEntityNames}"
                |> error
                |> state.Throw
            else
              let! entities =
                schema.Entities
                |> List.map (fun e ->
                  e.Name,
                  state {
                    let! id, id_k = !e.Id
                    do! id_k |> Kind.AsStar |> ofSum |> state.Ignore
                    let! t, t_k = !e.Type
                    do! t_k |> Kind.AsStar |> ofSum |> state.Ignore

                    let! properties =
                      e.Properties
                      |> Seq.map (fun p ->
                        state {
                          let! p_decl_t, p_decl_k = !p.Type
                          do! p_decl_k |> Kind.AsStar |> ofSum |> state.Ignore

                          let path_segments = p.Path |> Option.defaultValue []

                          let! _, path_scope, resolved_scope =
                            path_segments
                            |> List.fold
                              (fun acc segment ->
                                state {
                                  let! t, segments_acc, resolved_scope = acc

                                  match segment with
                                  | (maybe_var_name, SchemaPathTypeDecompositionExpr.Field f) ->
                                    let! t_record = t |> TypeValue.AsRecordWithSourceMapping |> ofSum
                                    let t_record_scope = t_record.typeCheckScopeSource
                                    let t_record = t_record.value

                                    let! (f_i, (f_t, f_k)) =
                                      t_record
                                      |> OrderedMap.toSeq
                                      |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = f.LocalName)
                                      |> Sum.fromOption (fun () ->
                                        (loc0, $"Error: cannot find field {f} in record type {t}")
                                        |> Errors.Singleton)
                                      |> state.OfSum

                                    do! f_k |> Kind.AsStar |> ofSum |> state.Ignore

                                    let next_t = f_t

                                    let next_segments =
                                      match maybe_var_name with
                                      | Some var_name ->
                                        (Identifier.LocalScope var_name.Name |> TypeCheckScope.Empty.Resolve,
                                         (next_t, f_k))
                                        :: segments_acc
                                      | None -> segments_acc

                                    return
                                      next_t,
                                      next_segments,
                                      (maybe_var_name,
                                       f_i.Name |> t_record_scope.Resolve |> SchemaPathTypeDecomposition.Field)
                                      :: resolved_scope
                                  | (maybe_var_name, SchemaPathTypeDecompositionExpr.UnionCase f) ->
                                    let! _, t_union_scope, t_union = t |> TypeValue.AsUnionWithSourceMapping |> ofSum

                                    let! (case_i, case_t) =
                                      t_union
                                      |> OrderedMap.toSeq
                                      |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = f.LocalName)
                                      |> Sum.fromOption (fun () ->
                                        (loc0, $"Error: cannot find case {f} in union type {t}") |> Errors.Singleton)
                                      |> state.OfSum

                                    let next_t = case_t

                                    let next_segments =
                                      match maybe_var_name with
                                      | Some var_name ->
                                        (Identifier.LocalScope var_name.Name |> TypeCheckScope.Empty.Resolve,
                                         (next_t, Kind.Star))
                                        :: segments_acc
                                      | None -> segments_acc

                                    return
                                      next_t,
                                      next_segments,
                                      (maybe_var_name,
                                       case_i.Name |> t_union_scope.Resolve |> SchemaPathTypeDecomposition.UnionCase)
                                      :: resolved_scope
                                  | (maybe_var_name, SchemaPathTypeDecompositionExpr.SumCase f) ->
                                    let! t_sum = t |> TypeValue.AsSum |> ofSum

                                    if f.Case < 1 || f.Case > t_sum.Length || f.Count <> t_sum.Length then
                                      return!
                                        (loc0, $"Error: sum case {f} is out of bounds for sum type {t}")
                                        |> Errors.Singleton
                                        |> state.Throw
                                    else
                                      let! case_t =
                                        t_sum
                                        |> Seq.tryItem (f.Case - 1)
                                        |> Sum.fromOption (fun () ->
                                          (loc0, $"Error: cannot find sum case {f} in sum type {t}")
                                          |> Errors.Singleton)
                                        |> state.OfSum

                                      let next_t = case_t

                                      let next_segments =
                                        match maybe_var_name with
                                        | Some var_name ->
                                          (Identifier.LocalScope var_name.Name |> TypeCheckScope.Empty.Resolve,
                                           (next_t, Kind.Star))
                                          :: segments_acc
                                        | None -> segments_acc

                                      return
                                        next_t,
                                        next_segments,
                                        (maybe_var_name, SchemaPathTypeDecomposition.SumCase f) :: resolved_scope
                                  | (maybe_var_name, SchemaPathTypeDecompositionExpr.Item f) ->
                                    let! t_tuple = t |> TypeValue.AsTuple |> ofSum

                                    if f.Index < 1 || f.Index > t_tuple.Length then
                                      return!
                                        (loc0, $"Error: tuple index {f} is out of bounds for tuple type {t}")
                                        |> Errors.Singleton
                                        |> state.Throw
                                    else
                                      let! item_t =
                                        t_tuple
                                        |> Seq.tryItem (f.Index - 1)
                                        |> Sum.fromOption (fun () ->
                                          (loc0, $"Error: cannot find tuple index {f} in tuple type {t}")
                                          |> Errors.Singleton)
                                        |> state.OfSum

                                      let next_t = item_t

                                      let next_segments =
                                        match maybe_var_name with
                                        | Some var_name ->
                                          (Identifier.LocalScope var_name.Name |> TypeCheckScope.Empty.Resolve,
                                           (next_t, Kind.Star))
                                          :: segments_acc
                                        | None -> segments_acc

                                      return
                                        next_t,
                                        next_segments,
                                        (maybe_var_name, SchemaPathTypeDecomposition.Item f) :: resolved_scope
                                  | (maybe_var_name, SchemaPathTypeDecompositionExpr.Iterator it) ->
                                    let! container, _ = it.Container |> TypeExpr.Lookup |> (!)
                                    let! t_arg, _ = it.TypeDef |> TypeExpr.Lookup |> (!)
                                    let! mapper, _, _, _ = typeCheckExpr None (it.Mapper |> Expr.Lookup)

                                    let next_t = t_arg

                                    let next_segments =
                                      match maybe_var_name with
                                      | Some var_name ->
                                        (Identifier.LocalScope var_name.Name |> TypeCheckScope.Empty.Resolve,
                                         (next_t, Kind.Star))
                                        :: segments_acc
                                      | None -> segments_acc

                                    return
                                      next_t,
                                      next_segments,
                                      (maybe_var_name,
                                       SchemaPathTypeDecomposition.Iterator
                                         {| Container = container
                                            TypeDef = t_arg
                                            Mapper = mapper |})
                                      :: resolved_scope
                                })
                              (state { return t, [], [] })

                          let resolved_scope = resolved_scope |> List.rev

                          let path_scope =
                            path_scope
                            |> List.map (fun (id, (t, k)) -> Map.add id (t, k))
                            |> List.fold (>>) (fun x -> x)

                          let! body_e, body_t, body_k, _ =
                            typeCheckExpr (Some p_decl_t) p.Body
                            |> state.MapContext(
                              TypeCheckContext.Updaters.Values(
                                Map.add (Identifier.LocalScope "self" |> TypeCheckScope.Empty.Resolve) (t, t_k)
                                >> path_scope
                              )
                              >> TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith)
                            )

                          do! body_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do! TypeValue.Unify(loc0, body_t, p_decl_t) |> Expr.liftUnification

                          return
                            { SchemaEntityProperty.Path = resolved_scope
                              PropertyName = p.Name
                              ReturnType = p_decl_t
                              ReturnKind = p_decl_k
                              Body = body_e }
                        })
                      |> state.All
                      |> state.Map(List.ofSeq)

                    let rec (+)
                      (t: TypeValue<'ve>)
                      (path: List<SchemaPathSegment<'ve>>, name: LocalIdentifier, result_t: TypeValue<'ve>)
                      =
                      state {
                        match path with
                        | [] ->
                          let! fields = t |> TypeValue.AsRecord |> ofSum

                          if
                            fields
                            |> OrderedMap.toSeq
                            |> Seq.filter (fun (k, _) -> k.Name.LocalName = name.Name)
                            |> Seq.isEmpty
                            |> not
                          then
                            return!
                              (loc0,
                               $"Error: a field with the same name as property {name.Name} already exists in record type {t}")
                              |> Errors.Singleton
                              |> state.Throw
                          else
                            let fields =
                              fields
                              |> OrderedMap.add
                                (name.Name |> Identifier.LocalScope |> TypeSymbol.Create)
                                (result_t, Kind.Star)

                            return TypeValue.CreateRecord fields
                        | (_, SchemaPathTypeDecomposition.Field f) :: path ->
                          let! fields = t |> TypeValue.AsRecord |> ofSum

                          let! f_s, (f_t, f_k) =
                            fields
                            |> OrderedMap.toSeq
                            |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = f.Name)
                            |> Sum.fromOption (fun () ->
                              (loc0, $"Error: cannot find field {f} in record type {t}") |> Errors.Singleton)
                            |> state.OfSum

                          let! f_t = f_t + (path, name, result_t)

                          let fields = fields |> OrderedMap.add f_s (f_t, f_k)

                          return TypeValue.CreateRecord fields
                        | (_, SchemaPathTypeDecomposition.UnionCase f) :: path ->
                          let! _, fields = t |> TypeValue.AsUnion |> ofSum

                          let! f_s, f_t =
                            fields
                            |> OrderedMap.toSeq
                            |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = f.Name)
                            |> Sum.fromOption (fun () ->
                              (loc0, $"Error: cannot find field {f} in record type {t}") |> Errors.Singleton)
                            |> state.OfSum

                          let! f_t = f_t + (path, name, result_t)

                          let fields = fields |> OrderedMap.add f_s f_t

                          return TypeValue.CreateUnion fields
                        | (_, SchemaPathTypeDecomposition.SumCase f) :: path ->
                          let! fields = t |> TypeValue.AsSum |> ofSum

                          let! f_t =
                            fields
                            |> Seq.tryItem (f.Case - 1)
                            |> Sum.fromOption (fun () ->
                              (loc0, $"Error: cannot find field {f} in record type {t}") |> Errors.Singleton)
                            |> state.OfSum

                          let! f_t = f_t + (path, name, result_t)

                          let fields = fields |> List.mapi (fun i ft -> if i = f.Case - 1 then f_t else ft)

                          return TypeValue.CreateSum fields
                        | (_, SchemaPathTypeDecomposition.Item f) :: path ->
                          let! fields = t |> TypeValue.AsTuple |> ofSum

                          let! f_t =
                            fields
                            |> Seq.tryItem (f.Index - 1)
                            |> Sum.fromOption (fun () ->
                              (loc0, $"Error: cannot find field {f} in record type {t}") |> Errors.Singleton)
                            |> state.OfSum

                          let! f_t = f_t + (path, name, result_t)

                          let fields = fields |> List.mapi (fun i ft -> if i = f.Index - 1 then f_t else ft)

                          return TypeValue.CreateTuple fields
                        | (_, SchemaPathTypeDecomposition.Iterator f) :: path ->
                          let f_t = f.TypeDef
                          let! f_t = f_t + (path, name, result_t)

                          let! t_res, _ =
                            !TypeExpr.Apply(TypeExpr.FromTypeValue f.Container, TypeExpr.FromTypeValue f_t)

                          return t_res
                      }

                    let! t_with_props =
                      properties
                      |> Seq.fold
                        (fun acc (p: SchemaEntityProperty<'ve>) ->
                          state {
                            let! t = acc
                            return! t + (p.Path, p.PropertyName, p.ReturnType)
                          })
                        (state { return t })

                    return
                      { Name = e.Name
                        Id = id
                        TypeOriginal = t
                        TypeWithProps = t_with_props
                        SearchBy = e.SearchBy
                        Properties = properties }
                  })
                |> OrderedMap.ofList
                |> state.AllMapOrdered

              let repeatedRelationNames =
                schema.Relations
                |> List.map (fun r -> r.Name)
                |> List.groupBy id
                |> List.filter (fun (_, l) -> List.length l > 1)
                |> List.map (fst >> fun n -> n.Name)

              if not (List.isEmpty repeatedRelationNames) then
                return!
                  let sep = ", " in

                  $"Error: schema has repeated relation names: {String.join sep repeatedRelationNames}"
                  |> error
                  |> state.Throw
              else
                let! context = state.GetContext()

                let rec validatePath
                  (source: TypeValue<'ve>)
                  (target: TypeValue<'ve>)
                  (path: SchemaPathSegmentExpr list)
                  =
                  state {
                    match path with
                    | [] -> do! TypeValue.Unify(loc0, source, target) |> Expr.liftUnification
                    | (_, segment) :: rest ->
                      match segment with
                      | SchemaPathTypeDecompositionExpr.Field fieldName ->
                        let! sourceRecord = source |> TypeValue.AsRecord |> ofSum

                        let! (_, (fieldType, _)) =
                          sourceRecord
                          |> OrderedMap.toSeq
                          |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = fieldName.LocalName)
                          |> Sum.fromOption (fun () ->
                            (loc0, $"Error: cannot find field {fieldName} in record type {source}")
                            |> Errors.Singleton)
                          |> state.OfSum

                        return! validatePath fieldType target rest
                      | SchemaPathTypeDecompositionExpr.UnionCase caseName ->
                        let! _, sourceCase = source |> TypeValue.AsUnion |> ofSum

                        let! (_, caseType) =
                          sourceCase
                          |> OrderedMap.toSeq
                          |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = caseName.LocalName)
                          |> Sum.fromOption (fun () ->
                            (loc0, $"Error: cannot find case {caseName} in union type {source}")
                            |> Errors.Singleton)
                          |> state.OfSum

                        return! validatePath caseType target rest
                      | SchemaPathTypeDecompositionExpr.SumCase caseName ->
                        let! sourceCase = source |> TypeValue.AsSum |> ofSum

                        if
                          caseName.Case < 1
                          || caseName.Case > sourceCase.Length
                          || caseName.Count <> sourceCase.Length
                        then
                          return!
                            (loc0, $"Error: sum case {caseName} is out of bounds for sum type {source}")
                            |> Errors.Singleton
                            |> state.Throw
                        else
                          let! caseType =
                            sourceCase
                            |> Seq.tryItem (caseName.Case - 1)
                            |> Sum.fromOption (fun () ->
                              (loc0, $"Error: cannot find sum case {caseName} in sum type {source}")
                              |> Errors.Singleton)
                            |> state.OfSum

                          return! validatePath caseType target rest
                      | SchemaPathTypeDecompositionExpr.Item item ->
                        let! sourceCase = source |> TypeValue.AsTuple |> ofSum

                        if item.Index < 1 || item.Index > sourceCase.Length then
                          return!
                            (loc0, $"Error: tuple index {item} is out of bounds for tuple type {source}")
                            |> Errors.Singleton
                            |> state.Throw
                        else
                          let! caseType =
                            sourceCase
                            |> Seq.tryItem (item.Index - 1)
                            |> Sum.fromOption (fun () ->
                              (loc0, $"Error: cannot find tuple index {item} in tuple type {source}")
                              |> Errors.Singleton)
                            |> state.OfSum

                          return! validatePath caseType target rest
                      | SchemaPathTypeDecompositionExpr.Iterator it ->
                        let! container, _ = it.Container |> TypeExpr.Lookup |> (!)
                        let! t_arg, _ = it.TypeDef |> TypeExpr.Lookup |> (!)

                        let! t_map, _ =
                          context.Values
                          |> Map.tryFindWithError
                            (it.Mapper |> TypeCheckScope.Empty.Resolve)
                            "mapper function"
                            it.Mapper.LocalName
                            loc0
                          |> state.OfSum

                        let! t_map, _ =
                          !TypeExpr.Apply(TypeExpr.Apply(TypeExpr.FromTypeValue t_map, TypeExpr.FromTypeValue t_arg),
                                          TypeExpr.FromTypeValue t_arg)

                        let! expected, _ =
                          !(TypeExpr.Apply(TypeExpr.FromTypeValue container, TypeExpr.FromTypeValue t_arg))

                        let! expected, _ =
                          !(TypeExpr.Arrow(
                            TypeExpr.Arrow(TypeExpr.FromTypeValue t_arg, TypeExpr.FromTypeValue t_arg),
                            TypeExpr.Arrow(TypeExpr.FromTypeValue source, TypeExpr.FromTypeValue expected)
                          ))

                        do! TypeValue.Unify(loc0, t_map, expected) |> Expr.liftUnification

                        return! validatePath t_arg target rest
                  }

                let! relations =
                  schema.Relations
                  |> List.map (fun r ->
                    r.Name,
                    state {
                      let fromPath = r.From |> snd
                      let toPath = r.To |> snd

                      let! fromEntity =
                        entities
                        |> OrderedMap.tryFind ((r.From |> fst).LocalName |> SchemaEntityName.Create)
                        |> Sum.fromOption (fun () ->
                          (loc0, $"Error: cannot find entity {r.From} for relation {r.Name}")
                          |> Errors.Singleton)
                        |> state.OfSum

                      let! toEntity =
                        entities
                        |> OrderedMap.tryFind ((r.To |> fst).LocalName |> SchemaEntityName.Create)
                        |> Sum.fromOption (fun () ->
                          (loc0, $"Error: cannot find entity {r.To} for relation {r.Name}")
                          |> Errors.Singleton)
                        |> state.OfSum

                      match fromPath with
                      | Some fromPath -> do! validatePath fromEntity.TypeOriginal toEntity.Id fromPath
                      | None -> ()

                      match toPath with
                      | Some toPath -> do! validatePath toEntity.TypeOriginal fromEntity.Id toPath
                      | None -> ()

                      return
                        { Name = r.Name
                          From = r.From |> fst
                          To = r.To |> fst }
                    })
                  |> OrderedMap.ofList
                  |> state.AllMapOrdered

                let schema =
                  { DeclaredAtForNominalEquality = loc0
                    Entities = entities
                    Relations = relations }

                return TypeValue.Schema schema, Kind.Schema
          | TypeExpr.FromTypeValue tv ->
            // do Console.WriteLine($"Instantiating type value {tv}")
            let! ctx = state.GetContext()
            let! s = state.GetState()
            let scope = ctx.TypeVariables |> Map.map (fun _ (_, k) -> k)
            let scope = Map.merge (fun _ -> id) scope ctx.TypeParameters
            let! k = TypeValue.KindEval () n loc0 tv |> state.MapContext(fun _ -> scope)

            let! tv =
              tv
              |> TypeValue.Instantiate () (TypeExpr.Eval () typeCheckExpr) loc0
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
          | TypeExpr.NewSymbol _ -> return! $"Errors cannot evaluate {t} as a type" |> error |> state.Throw
          | TypeExpr.Primitive p ->
            return
              TypeValue.Primitive
                { value = p
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.Lookup v ->
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
                      | _ ->
                        let! c = state.GetContext()

                        return!
                          $"Error: cannot find type for {v} with context ({c.TypeVariables.AsFSharpString})"
                          |> error
                          |> state.Throw
                          |> state.MapError(Errors.SetPriority(ErrorPriority.High))
                    } ]
                )
              )
              |> state.MapError(Errors.FilterHighestPriorityOnly)

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
                (state { return! $"Error: cannot evaluate let binding {x}" |> error |> state.Throw }
                 |> state.MapError(Errors.SetPriority ErrorPriority.High))

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
                })
                (state {
                  let! f_i = f |> TypeValue.AsImported |> ofSum

                  // let! ctx = state.GetContext()
                  // do Console.WriteLine($"Evaluating argument part {a.ToFSharpString}")
                  // do Console.WriteLine($"Context is {ctx.TypeParameters.ToFSharpString}")
                  // do Console.ReadLine() |> ignore
                  let! a, a_k = !a
                  // do Console.WriteLine($"aka {a.ToFSharpString}")
                  // do Console.ReadLine() |> ignore

                  if a_k <> f_k_i then
                    return!
                      $"Error: cannot apply type {f} of input kind {f_k_i} to argument of kind {a_k}"
                      |> error
                      |> state.Throw
                  else
                    match f_i.Parameters with
                    | [] ->
                      return!
                        $"Error: cannot apply imported type {f_i.Id.Name} with no parameters"
                        |> error
                        |> state.Throw
                    | _ :: ps ->
                      return
                        TypeValue.Imported
                          { f_i with
                              Parameters = ps
                              Arguments = f_i.Arguments @ [ a ] },
                        f_k_o
                })
                (state {
                  let! f_l = f |> TypeValue.AsLookup |> ofSum

                  let! a, a_k = !a

                  if a_k <> f_k_i then
                    return!
                      $"Error: cannot apply type {f} of input kind {f_k_i} to argument of kind {a_k}"
                      |> error
                      |> state.Throw
                  else
                    return TypeValue.CreateApplication(SymbolicTypeApplication.Lookup(f_l, a)), f_k_o
                })
                (state {
                  let! { value = f_app } = f |> TypeValue.AsApplication |> ofSum

                  let! a, a_k = !a

                  if a_k <> f_k_i then
                    return!
                      $"Error: cannot apply type {f} of input kind {f_k_i} to argument of kind {a_k}"
                      |> error
                      |> state.Throw
                  else
                    return TypeValue.CreateApplication(SymbolicTypeApplication.Application(f_app, a)), f_k_o
                })
                (state { return! $"Error: cannot evaluate type application {t}" |> error |> state.Throw }
                 |> state.MapError(Errors.SetPriority ErrorPriority.High))
              |> state.MapError(Errors.FilterHighestPriorityOnly)
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
            // do Console.ReadLine() |> ignore

            // |> state.MapContext(TypeExprEvalContext.Updaters.TypeVariables(replaceWith closure))

            return
              TypeValue.Lambda
                { value = param, body_t.AsExpr
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Arrow(param.Kind, body_k)
          | TypeExpr.Arrow(input, output) ->
            let! input, input_k = !input
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
          | TypeExpr.Map(key, value) ->
            let! key, key_k = !key
            let! value, value_k = !value
            do! key_k |> Kind.AsStar |> ofSum |> state.Ignore
            do! value_k |> Kind.AsStar |> ofSum |> state.Ignore

            return
              TypeValue.Map
                { value = (key, value)
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
              $"Error: cannot evaluate record destructuring {tv}.{comp}"
              |> error
              |> state.Throw
              |> state.MapError(Errors.SetPriority ErrorPriority.High)

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
                        (loc0, $"Error: cannot find field {field} in record type {tv}")
                        |> Errors.Singleton)
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
                          (loc0, $"Error: cannot find case {field} in union type {tv}")
                          |> Errors.Singleton)
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
                        (loc0, $"Error: tuple index {item} is out of bounds for tuple type {tv}")
                        |> Errors.Singleton
                        |> state.Throw
                    else
                      let! item_t =
                        items
                        |> Seq.tryItem (item - 1)
                        |> Sum.fromOption (fun () ->
                          (loc0, $"Error: cannot find tuple index {item} in tuple type {tv}")
                          |> Errors.Singleton)
                        |> state.OfSum

                      return TypeValue.SetSourceMapping(item_t, source), Kind.Star
                  },
                  [ state {
                      let! items = tv |> TypeValue.AsSum |> ofSum

                      if item < 1 || item > items.Length then
                        return!
                          (loc0, $"Error: sum case {item} is out of bounds for sum type {tv}")
                          |> Errors.Singleton
                          |> state.Throw
                      else
                        let! item_t =
                          items
                          |> Seq.tryItem (item - 1)
                          |> Sum.fromOption (fun () ->
                            (loc0, $"Error: cannot find sum case {item} in sum type {tv}")
                            |> Errors.Singleton)
                          |> state.OfSum

                        return TypeValue.SetSourceMapping(item_t, source), Kind.Star
                    }
                    state {
                      let! imported = tv |> TypeValue.AsImported |> ofSum

                      let! item_t =
                        imported.Arguments
                        |> List.tryItem (item - 1)
                        |> Sum.fromOption (fun () ->
                          (loc0, $"Error: cannot find argument {item} in imported type {tv}")
                          |> Errors.Singleton)
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
                      $"Error: cannot flatten types with overlapping keys: {keys1} and {keys2}"
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
                      $"Error: cannot flatten types with overlapping keys: {keys1} and {keys2}"
                      |> error
                      |> state.Throw
                })
                (state { return! $"Error: cannot evaluate flattening " |> error |> state.Throw }
                 |> state.MapError(Errors.SetPriority ErrorPriority.High))
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
                (state { return! $"Error: cannot evaluate exclude " |> error |> state.Throw }
                 |> state.MapError(Errors.SetPriority ErrorPriority.High))
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
                (state { return! $"Error: cannot evaluate rotation" |> error |> state.Throw }
                 |> state.MapError(Errors.SetPriority ErrorPriority.High))
        }
