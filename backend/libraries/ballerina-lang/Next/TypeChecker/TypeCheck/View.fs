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
          let! checkedAttrs =
            el.Attributes
            |> List.map (fun attr ->
              state {
                match attr with
                | ExprViewAttribute.ViewAttrStringValue(name, value) ->
                  return TypeCheckedViewAttribute.ViewAttrStringValue(name, value)
                | ExprViewAttribute.ViewAttrExprValue(name, expr) ->
                  let! checkedExpr, _ = None => expr
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
              Errors.Singleton loc0 (fun () ->
                "Error: view parameters require an explicit type annotation")
              |> state.Throw

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

          // 3. Register the record fields for Props
          let propsFields =
            OrderedMap.ofList
              [ TypeSymbol.Create(Identifier.LocalScope "schema"), (schema_t, Kind.Schema)
                TypeSymbol.Create(Identifier.LocalScope "context"), (ctx_t, Kind.Star)
                TypeSymbol.Create(Identifier.LocalScope "state"), (st_t, Kind.Star)
                TypeSymbol.Create(Identifier.LocalScope "setState"),
                (TypeValue.CreateArrow(
                   TypeValue.CreateArrow(st_t, st_t),
                   TypeValue.CreatePrimitive PrimitiveType.Unit
                 ),
                 Kind.Star) ]

          do!
            propsFields
            |> OrderedMap.toSeq
            |> Seq.map (fun (k, (field_t, _field_k)) ->
              state {
                do!
                  TypeCheckState.bindRecordField
                    (k.Name |> TypeCheckScope.Empty.Resolve)
                    (propsFields, field_t)
                  |> Expr.liftTypeEval
              })
            |> state.All
            |> state.Ignore

          // 4. Bind the parameter in the typing context
          let! body =
            Expr<'T, 'Id, 'valueExt>.TypeCheckViewNode config typeCheckExpr body
            |> state.MapContext(
              TypeCheckContext.Updaters.Values(
                Map.add
                  (param.Name |> Identifier.LocalScope |> ctx.Scope.Resolve)
                  param_t
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
