namespace Ballerina.DSL.Next.Types.TypeChecker

module View =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.Cat.Collections.OrderedMap

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckViewNode<'valueExt when 'valueExt: comparison>
      (config: TypeCheckingConfig<'valueExt>)
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      (node: ExprViewNode<TypeExpr<'valueExt>, Identifier, 'valueExt>)
      : State<
          TypeCheckedViewNode<'valueExt>,
          TypeCheckContext<'valueExt>,
          TypeCheckState<'valueExt>,
          Errors<Location>
         >
      =
      let loc0 = node.Location

      let typeCheckNode =
        Expr<'T, 'Id, 'valueExt>.TypeCheckViewNode config typeCheckExpr

      let (=>) c e = typeCheckExpr c e

      state {
        match node.Node with
        | ExprViewNodeRec.ViewText text ->
          return
            { TypeCheckedViewNode.Location = loc0
              Node = TypeCheckedViewNodeRec.ViewText text }

        | ExprViewNodeRec.ViewExprContainer expr ->
          let! checkedExpr, _ = None => expr
          return
            { TypeCheckedViewNode.Location = loc0
              Node = TypeCheckedViewNodeRec.ViewExprContainer checkedExpr }

        | ExprViewNodeRec.ViewFragment children ->
          let! checkedChildren =
            children
            |> List.map typeCheckNode
            |> state.All
          return
            { TypeCheckedViewNode.Location = loc0
              Node = TypeCheckedViewNodeRec.ViewFragment checkedChildren }

        | ExprViewNodeRec.ViewElement el ->
          let! ctx = state.GetContext()
          let tagSchemaOpt = ctx.ViewAttributeSchemas |> Map.tryFind el.Tag

          let! checkedAttrs =
            el.Attributes
            |> List.map (fun attr ->
              state {
                match attr with
                | ExprViewAttribute.ViewAttrStringValue(name, value) ->
                  match tagSchemaOpt with
                  | Some tagSchema ->
                    match tagSchema |> Map.tryFind name with
                    | Some acceptableTypes ->
                      let hasStringType =
                        acceptableTypes
                        |> List.exists (fun t ->
                          match t with
                          | TypeValue.Primitive p when p.value = PrimitiveType.String -> true
                          | _ -> false)

                      if not hasStringType then
                        return!
                          Errors.Singleton loc0 (fun () ->
                            $"Attribute '{name}' on <{el.Tag}> does not accept string values")
                          |> state.Throw
                      else
                        return TypeCheckedViewAttribute.ViewAttrStringValue(name, value)
                    | None ->
                      return!
                        Errors.Singleton loc0 (fun () ->
                          let known =
                            tagSchema
                            |> Map.toList
                            |> List.map fst
                            |> String.concat ", "
                          $"Unknown attribute '{name}' on <{el.Tag}>. Known attributes: {known}")
                        |> state.Throw
                  | None ->
                    return TypeCheckedViewAttribute.ViewAttrStringValue(name, value)
                | ExprViewAttribute.ViewAttrExprValue(name, expr) ->
                  let! checkedExpr, _ = None => expr

                  match tagSchemaOpt with
                  | Some tagSchema ->
                    match tagSchema |> Map.tryFind name with
                    | Some acceptableTypes ->
                      // The 'style' attribute accepts any record of strings (CSS properties)
                      // in addition to plain string values. Skip strict unification for style
                      // to allow inline style objects like { marginTop = "0.75rem" }.
                      if name <> "style" then
                        let exprType = checkedExpr.Type

                        let unifyAttempts =
                          acceptableTypes
                          |> List.map (fun expectedType ->
                            state {
                              do!
                                TypeValue.Unify(loc0, exprType, expectedType)
                                |> Expr<'T, 'Id, 'valueExt>.liftUnification

                              return ()
                            })

                        match unifyAttempts with
                        | first :: rest ->
                          do!
                            state.Any(first, rest)
                            |> state.MapError(fun _ ->
                              let typeNames =
                                acceptableTypes
                                |> List.map (fun t -> $"{t}")
                                |> String.concat " | "

                              Errors.Singleton loc0 (fun () ->
                                $"Attribute '{name}' on <{el.Tag}> expects type {typeNames}, but got {exprType}"))
                        | [] -> ()

                      return TypeCheckedViewAttribute.ViewAttrExprValue(name, checkedExpr)
                    | None ->
                      return!
                        Errors.Singleton loc0 (fun () ->
                          let known =
                            tagSchema
                            |> Map.toList
                            |> List.map fst
                            |> String.concat ", "
                          $"Unknown attribute '{name}' on <{el.Tag}>. Known attributes: {known}")
                        |> state.Throw
                  | None ->
                    return TypeCheckedViewAttribute.ViewAttrExprValue(name, checkedExpr)
              })
            |> state.All

          let! checkedChildren =
            el.Children
            |> List.map typeCheckNode
            |> state.All

          return
            { TypeCheckedViewNode.Location = loc0
              Node =
                TypeCheckedViewNodeRec.ViewElement
                  { Tag = el.Tag
                    Attributes = checkedAttrs
                    Children = checkedChildren
                    SelfClosing = el.SelfClosing } }

        | ExprViewNodeRec.ViewMapContext(mapper, inner) ->
          let! checkedMapper, _ = None => mapper
          let! checkedInner, _ = None => inner
          return
            { TypeCheckedViewNode.Location = loc0
              Node = TypeCheckedViewNodeRec.ViewMapContext(checkedMapper, checkedInner) }

        | ExprViewNodeRec.ViewMapState(mapDown, mapUp, inner) ->
          let! checkedMapDown, _ = None => mapDown
          let! checkedMapUp, _ = None => mapUp
          let! checkedInner, _ = None => inner
          return
            { TypeCheckedViewNode.Location = loc0
              Node = TypeCheckedViewNodeRec.ViewMapState(checkedMapDown, checkedMapUp, checkedInner) }
      }

    static member internal TypeCheckView<'valueExt when 'valueExt: comparison>
      (config: TypeCheckingConfig<'valueExt>)
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      : TypeChecker<
          Location * ExprView<TypeExpr<'valueExt>, Identifier, 'valueExt>,
          'valueExt
         >
      =
      fun
          context_t
          (viewLoc,
           { Param = param
             ParamType = paramTypeExpr
             Body = body
             Location = _ }) ->

        let loc0 = viewLoc

        let _ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        state {
          let! ctx = state.GetContext()

          // 1. Evaluate the type annotation (required for view params)
          let! param_t =
            match paramTypeExpr with
            | Some typeExpr ->
              typeExpr
              |> TypeExpr.Eval config typeCheckExpr None loc0
              |> Expr<'T, 'Id, 'valueExt>.liftTypeEval
            | None ->
              // Synthesized view (JSX-as-expression): infer param type
              // from fresh type variables. The parser wraps standalone JSX
              // elements as view _ -> <node>, so no annotation exists.
              state {
                let schemaGuid = Guid.CreateVersion7()
                let ctxGuid = Guid.CreateVersion7()
                let stGuid = Guid.CreateVersion7()

                let freshSchema =
                  { TypeVar.Name = "schema_synth_" + schemaGuid.ToString()
                    Synthetic = true
                    Guid = Guid.CreateVersion7() }

                let freshCtx =
                  { TypeVar.Name = "ctx_synth_" + ctxGuid.ToString()
                    Synthetic = true
                    Guid = Guid.CreateVersion7() }

                let freshSt =
                  { TypeVar.Name = "st_synth_" + stGuid.ToString()
                    Synthetic = true
                    Guid = Guid.CreateVersion7() }

                do!
                  state.SetState(
                    TypeCheckState.Updaters.Vars(
                      UnificationState.EnsureVariableExists freshSchema
                    )
                  )

                do!
                  state.SetState(
                    TypeCheckState.Updaters.Vars(
                      UnificationState.EnsureVariableExists freshCtx
                    )
                  )

                do!
                  state.SetState(
                    TypeCheckState.Updaters.Vars(
                      UnificationState.EnsureVariableExists freshSt
                    )
                  )

                let paramType =
                  config.MkViewPropsType
                    (TypeValue.Var freshSchema)
                    (TypeValue.Var freshCtx)
                    (TypeValue.Var freshSt)

                return (paramType, Kind.Star)
              }

          let param_t_val = fst param_t

          // 2. Decompose the param type as View::Props[schema][ctx][st]
          let! schema_t, ctx_t, st_t =
            match param_t_val with
            | TypeValue.Imported { Sym = sym; Arguments = [ schema; ctxArg; stArg ] }
              when sym = config.ViewPropsTypeSymbol ->
              state { return schema, ctxArg, stArg }
            | _ ->
              state {
                let schemaGuid = Guid.CreateVersion7()
                let ctxGuid = Guid.CreateVersion7()
                let stGuid = Guid.CreateVersion7()

                let freshSchema =
                  { TypeVar.Name = "schema_view_" + schemaGuid.ToString()
                    Synthetic = true
                    Guid = schemaGuid }

                let freshCtx =
                  { TypeVar.Name = "ctx_view_" + ctxGuid.ToString()
                    Synthetic = true
                    Guid = ctxGuid }

                let freshSt =
                  { TypeVar.Name = "st_view_" + stGuid.ToString()
                    Synthetic = true
                    Guid = stGuid }

                do!
                  state.SetState(
                    TypeCheckState.Updaters.Vars(
                      UnificationState.EnsureVariableExists freshSchema
                    )
                  )

                do!
                  state.SetState(
                    TypeCheckState.Updaters.Vars(
                      UnificationState.EnsureVariableExists freshCtx
                    )
                  )

                do!
                  state.SetState(
                    TypeCheckState.Updaters.Vars(
                      UnificationState.EnsureVariableExists freshSt
                    )
                  )

                let schema_v = TypeValue.Var freshSchema
                let ctx_v = TypeValue.Var freshCtx
                let st_v = TypeValue.Var freshSt

                let expectedPropsType = config.MkViewPropsType schema_v ctx_v st_v

                do!
                  TypeValue.Unify(loc0, param_t_val, expectedPropsType)
                  |> Expr<'T, 'Id, 'valueExt>.liftUnification

                return schema_v, ctx_v, st_v
              }

          // 3. Bind the parameter in the typing context
          let! body =
            Expr<'T, 'Id, 'valueExt>.TypeCheckViewNode config typeCheckExpr body
            |> state.MapContext(
              TypeCheckContext.Updaters.Values(
                Map.add
                  (param.Name |> Identifier.LocalScope |> ctx.Scope.Resolve)
                  param_t
              )
              >> TypeCheckContext.Updaters.RejectedIdentifiers(
                fun rejected ->
                  ctx.ViewRejectedIdentifiers
                  |> Map.fold (fun acc k v -> Map.add k v acc) rejected
              )
            )

          // 5. Construct result type: Frontend::View[schema][ctx][st]
          let result_t = config.MkViewType schema_t ctx_t st_t

          let! result_t =
            match context_t with
            | Some expected_t ->
              state {
                do!
                  TypeValue.Unify(loc0, result_t, expected_t)
                  |> Expr<'T, 'Id, 'valueExt>.liftUnification

                return result_t
              }
            | None -> state { return result_t }

          let! result_t =
            result_t
            |> TypeValue.Instantiate
              ()
              (TypeExpr.Eval config typeCheckExpr)
              loc0
            |> Expr.liftInstantiation

          let! param_t_final =
            param_t_val
            |> TypeValue.Instantiate
              ()
              (TypeExpr.Eval config typeCheckExpr)
              loc0
            |> Expr.liftInstantiation

          return
            TypeCheckedExpr.View(
              { TypeCheckedExprView.Param = param
                ParamType = param_t_final
                Body = body
                Location = loc0 },
              result_t,
              Kind.Star,
              loc0,
              ctx.Scope
            ),
            ctx
        }
