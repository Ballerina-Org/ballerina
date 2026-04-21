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
  open Ballerina.Fun
  open Ballerina.Cat.Collections.OrderedMap

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckView<'valueExt when 'valueExt: comparison>
      (config: TypeCheckingConfig<'valueExt>)
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      (context_t: Option<TypeValue<'valueExt>>)
      (loc0: Location, v: ExprView<TypeExpr<'valueExt>, Identifier, 'valueExt>)
      : TypeCheckerResult<
          TypeCheckedExpr<'valueExt> * TypeCheckContext<'valueExt>,
          'valueExt
         >
      =
      let { ViewTypeSymbol = view_type_symbol
            ViewPropsTypeSymbol = view_props_type_symbol
            MkViewType = mk_view_type
            MkViewPropsType = mk_view_props_type } =
        config

      let mkFreshVar (name: string) =
        let guid = Guid.CreateVersion7()

        { TypeVar.Name = name + "_view_" + guid.ToString()
          Synthetic = true
          Guid = guid }

      let ensureVar (v: TypeVar) =
        state.SetState(
          TypeCheckState.Updaters.Vars(
            UnificationState.EnsureVariableExists v
          )
        )

      state {
        let! ctx = state.GetContext()

        // 1. Evaluate type annotation (required for views)
        let! param_t =
          match v.ParamType with
          | Some typeExpr ->
            state {
              let! t, _k =
                typeExpr
                |> TypeExpr.Eval config typeCheckExpr None loc0
                |> Expr<'T, 'Id, 'valueExt>.liftTypeEval
              return t
            }
          | None ->
            Errors.Singleton loc0 (fun () ->
              "Error: view expressions require an explicit type annotation on the props parameter")
            |> state.Throw

        // 2. Decompose param type as View::Props[schema][ctx][st]
        let! schema, ctx_type, st =
          state {
            match param_t with
            | TypeValue.Imported { Sym = sym
                                   Arguments = [ schema; ctx_type; st ] } when
              sym = view_props_type_symbol
              ->
              return schema, ctx_type, st
            | _ ->
              let schema_var = mkFreshVar "schema"
              let ctx_var = mkFreshVar "ctx"
              let st_var = mkFreshVar "st"
              do! ensureVar schema_var
              do! ensureVar ctx_var
              do! ensureVar st_var

              let schema = TypeValue.Var schema_var
              let ctx_type = TypeValue.Var ctx_var
              let st = TypeValue.Var st_var

              do!
                TypeValue.Unify(
                  loc0,
                  param_t,
                  mk_view_props_type schema ctx_type st
                )
                |> Expr<'T, 'Id, 'valueExt>.liftUnification

              return schema, ctx_type, st
          }

        // 3. Synthesize record fields for Props: { schema; context; state; setState }
        let setStateType =
          TypeValue.CreateArrow(
            TypeValue.CreateArrow(st, st),
            TypeValue.CreateUnit()
          )

        let schemaFieldSym =
          TypeSymbol.Create(Identifier.LocalScope "schema")

        let contextFieldSym =
          TypeSymbol.Create(Identifier.LocalScope "context")

        let stateFieldSym =
          TypeSymbol.Create(Identifier.LocalScope "state")

        let setStateFieldSym =
          TypeSymbol.Create(Identifier.LocalScope "setState")

        let propsFields =
          [ schemaFieldSym, (schema, Kind.Schema)
            contextFieldSym, (ctx_type, Kind.Star)
            stateFieldSym, (st, Kind.Star)
            setStateFieldSym, (setStateType, Kind.Star) ]
          |> OrderedMap.ofList

        // Register record fields so RecordDes can resolve props.context etc.
        let fieldDefs =
          [ ("schema", schemaFieldSym, schema)
            ("context", contextFieldSym, ctx_type)
            ("state", stateFieldSym, st)
            ("setState", setStateFieldSym, setStateType) ]

        do!
          fieldDefs
          |> List.map (fun (name, fieldSym, fieldType) ->
            state {
              let id0 = Identifier.LocalScope name
              let resolvedId = id0 |> TypeCheckScope.Empty.Resolve

              // Map identifier → resolved identifier (for TryResolveIdentifier)
              do!
                TypeCheckState.bindIdentifierToResolvedIdentifier resolvedId id0

              // Map TypeSymbol ↔ ResolvedIdentifier (for TryResolveIdentifier on TypeSymbol)
              do!
                TypeCheckState.bindRecordFieldSymbol resolvedId fieldSym

              // Map ResolvedIdentifier → (fields, fieldType) (for TryFindRecordField)
              do!
                TypeCheckState.bindRecordField
                  resolvedId
                  (propsFields, fieldType)
                |> Expr<'T, 'Id, 'valueExt>.liftTypeEval
            })
          |> state.All
          |> state.Ignore

        // 4. Bind the props parameter in context with the structural record type
        let param_id =
          v.Param.Name |> Identifier.LocalScope |> ctx.Scope.Resolve

        let propsRecordType = TypeValue.CreateRecord propsFields

        // 5. Typecheck the JSX body within the extended context
        let! body_checked =
          Expr<'T, 'Id, 'valueExt>.TypeCheckViewNode
            config
            typeCheckExpr
            view_type_symbol
            mk_view_props_type
            schema
            v.Body
          |> state.MapContext(
            TypeCheckContext.Updaters.Values(
              Map.add param_id (propsRecordType, Kind.Star)
            )
          )

        // 6. Construct result type
        let result_t = mk_view_type schema ctx_type st

        // 7. Unify with expected type if available
        match context_t with
        | Some expected_t ->
          do!
            TypeValue.Unify(loc0, result_t, expected_t)
            |> Expr<'T, 'Id, 'valueExt>.liftUnification
        | None -> ()

        let view_checked: TypeCheckedExprView<'valueExt> =
          { TypeCheckedExprView.Param = v.Param
            TypeCheckedExprView.ParamType = param_t
            TypeCheckedExprView.Body = body_checked
            TypeCheckedExprView.Location = loc0 }

        return
          TypeCheckedExpr.View(
            view_checked,
            result_t,
            Kind.Star,
            loc0,
            ctx.Scope
          ),
          ctx
      }

    static member internal TypeCheckViewNode<'valueExt when 'valueExt: comparison>
      (config: TypeCheckingConfig<'valueExt>)
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      (viewTypeSymbol: TypeSymbol)
      (mkViewPropsType:
        TypeValue<'valueExt>
          -> TypeValue<'valueExt>
          -> TypeValue<'valueExt>
          -> TypeValue<'valueExt>)
      (schema: TypeValue<'valueExt>)
      (node: ExprViewNode<TypeExpr<'valueExt>, Identifier, 'valueExt>)
      : TypeCheckerResult<TypeCheckedViewNode<'valueExt>, 'valueExt> =
      let (=>) c e = typeCheckExpr c e

      state {
        match node.Node with
        | ExprViewNodeRec.ViewText text ->
          return
            { TypeCheckedViewNode.Location = node.Location
              Node = TypeCheckedViewNodeRec.ViewText text }

        | ExprViewNodeRec.ViewFragment children ->
          let! children_checked =
            children
            |> List.map (fun child ->
              Expr<'T, 'Id, 'valueExt>.TypeCheckViewNode
                config
                typeCheckExpr
                viewTypeSymbol
                mkViewPropsType
                schema
                child)
            |> state.All

          return
            { TypeCheckedViewNode.Location = node.Location
              Node = TypeCheckedViewNodeRec.ViewFragment children_checked }

        | ExprViewNodeRec.ViewExprContainer expr ->
          let! expr_checked, _ = None => expr

          return
            { TypeCheckedViewNode.Location = node.Location
              Node = TypeCheckedViewNodeRec.ViewExprContainer expr_checked }

        | ExprViewNodeRec.ViewElement el ->
          return!
            Expr<'T, 'Id, 'valueExt>.TypeCheckViewElement
              config
              typeCheckExpr
              viewTypeSymbol
              mkViewPropsType
              schema
              node.Location
              el
      }

    static member internal TypeCheckViewElement<'valueExt when 'valueExt: comparison>
      (config: TypeCheckingConfig<'valueExt>)
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      (viewTypeSymbol: TypeSymbol)
      (mkViewPropsType:
        TypeValue<'valueExt>
          -> TypeValue<'valueExt>
          -> TypeValue<'valueExt>
          -> TypeValue<'valueExt>)
      (schema: TypeValue<'valueExt>)
      (loc0: Location)
      (el: ExprViewElement<TypeExpr<'valueExt>, Identifier, 'valueExt>)
      : TypeCheckerResult<TypeCheckedViewNode<'valueExt>, 'valueExt> =
      let (=>) c e = typeCheckExpr c e

      state {
        let! ctx = state.GetContext()

        // Try to look up the tag as a custom component
        let tagId =
          el.Tag |> Identifier.LocalScope |> ctx.Scope.Resolve

        let componentType =
          ctx.Values |> Map.tryFind tagId

        match componentType with
        | Some(TypeValue.Imported imported, _kind) when
          imported.Sym = viewTypeSymbol
          && imported.Arguments.Length = 3
          ->
          let comp_schema = imported.Arguments.[0]
          let comp_ctx = imported.Arguments.[1]
          let comp_st = imported.Arguments.[2]
          // Custom component: typecheck the props attribute
          let expectedPropsType =
            mkViewPropsType comp_schema comp_ctx comp_st

          // Unify the component's schema with the view's schema
          do!
            TypeValue.Unify(loc0, schema, comp_schema)
            |> Expr<'T, 'Id, 'valueExt>.liftUnification

          // Find the props attribute
          let! attrs_checked =
            el.Attributes
            |> List.map (fun attr ->
              state {
                match attr with
                | ExprViewAttribute.ViewAttrStringValue(name, value) ->
                  return
                    TypeCheckedViewAttribute.ViewAttrStringValue(name, value)
                | ExprViewAttribute.ViewAttrExprValue(name, expr) ->
                  let expected =
                    if name = "props" then
                      Some expectedPropsType
                    else
                      None

                  let! expr_checked, _ = expected => expr
                  return TypeCheckedViewAttribute.ViewAttrExprValue(name, expr_checked)
              })
            |> state.All

          let! children_checked =
            el.Children
            |> List.map (fun child ->
              Expr<'T, 'Id, 'valueExt>.TypeCheckViewNode
                config
                typeCheckExpr
                viewTypeSymbol
                mkViewPropsType
                schema
                child)
            |> state.All

          return
            { TypeCheckedViewNode.Location = loc0
              Node =
                TypeCheckedViewNodeRec.ViewElement
                  { TypeCheckedViewElement.Tag = el.Tag
                    Attributes = attrs_checked
                    Children = children_checked
                    SelfClosing = el.SelfClosing } }

        | _ ->
          // HTML element: typecheck all attributes and children normally
          let! attrs_checked =
            el.Attributes
            |> List.map (fun attr ->
              state {
                match attr with
                | ExprViewAttribute.ViewAttrStringValue(name, value) ->
                  return
                    TypeCheckedViewAttribute.ViewAttrStringValue(name, value)
                | ExprViewAttribute.ViewAttrExprValue(name, expr) ->
                  let! expr_checked, _ = None => expr
                  return TypeCheckedViewAttribute.ViewAttrExprValue(name, expr_checked)
              })
            |> state.All

          let! children_checked =
            el.Children
            |> List.map (fun child ->
              Expr<'T, 'Id, 'valueExt>.TypeCheckViewNode
                config
                typeCheckExpr
                viewTypeSymbol
                mkViewPropsType
                schema
                child)
            |> state.All

          return
            { TypeCheckedViewNode.Location = loc0
              Node =
                TypeCheckedViewNodeRec.ViewElement
                  { TypeCheckedViewElement.Tag = el.Tag
                    Attributes = attrs_checked
                    Children = children_checked
                    SelfClosing = el.SelfClosing } }
      }
