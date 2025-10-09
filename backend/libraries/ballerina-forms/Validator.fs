namespace Ballerina.DSL.FormEngine

module Validator =

  open Ballerina.StdLib.Object
  open Ballerina.DSL.FormEngine.Model
  open Ballerina.DSL.Parser.Patterns
  open Ballerina.DSL.FormEngine.Parser.FormsPatterns
  open Ballerina.Collections.Tuple
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.DSL.Expr.Types.Patterns
  open Ballerina.DSL.Expr.Types.TypeCheck
  open Ballerina.DSL.Expr.Types.Unification
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors
  open System

  let private (>>=) m f = sum.Bind(m, f)

  let private validateGroupPredicates
    (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
    (typeCheck: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
    (vars: Map<VarName, ExprType>)
    (localType: ExprType)
    (visibleExpr: Expr<'ExprExtension, 'ValueExtension>)
    : State<Unit, CodeGenConfig, ValidationState, Errors> =
    state {
      let! eType =
        typeCheck (ctx.Types |> Seq.map (fun tb -> tb.Value.TypeId, tb.Value.Type) |> Map.ofSeq) vars visibleExpr
        |> state.OfSum

      let! eTypeSetArg = ExprType.AsSet eType |> state.OfSum

      let getLookedUpExprType (eType: ExprType) : Sum<ExprType, Errors> =
        sum {
          match eType with
          | ExprType.LookupType l ->
            let! typeBinding = ctx.Types |> Map.tryFindWithError l.VarName "types" "types"
            return typeBinding.Type
          | _ -> return eType
        }

      let! eTypeInSet = getLookedUpExprType eTypeSetArg >>= ExprType.AsRecord |> state.OfSum

      let! eTypeEnum = eTypeInSet |> Map.tryFindWithError "Value" "fields" "fields" |> state.OfSum

      let! eTypeEnumCases = getLookedUpExprType eTypeEnum >>= ExprType.AsUnion |> state.OfSum


      match eTypeEnumCases |> Seq.tryFind (fun c -> c.Value.Fields.IsUnitType |> not) with
      | Some nonUnitCaseFields ->
        return!
          state.Throw(
            Errors.Singleton
              $"Error: all cases of {eTypeEnum.ToString()} should be of type unit (ie the type is a proper enum), but {nonUnitCaseFields.Key} has type {nonUnitCaseFields.Value}"
          )
      | _ ->
        let caseNames = eTypeEnumCases.Keys |> Seq.map (fun c -> c.CaseName) |> Set.ofSeq
        let! fields = localType |> ExprType.AsRecord |> state.OfSum
        let fields = fields |> Seq.map (fun c -> c.Key) |> Set.ofSeq

        let missingFields = caseNames - fields
        let missingCaseNames = fields - caseNames

        let warn (msg: string) =
          do Console.ForegroundColor <- ConsoleColor.DarkMagenta
          Console.WriteLine msg
          do Console.ResetColor()

        if missingFields |> Set.isEmpty |> not then
          warn
            $"Warning: the group provides fields {caseNames |> Seq.toList} but the form type has fields {fields |> Seq.toList}: fields {missingFields |> Seq.toList} are missing from the type and so toggling that field will have no effect!"

        if missingCaseNames |> Set.isEmpty |> not then
          warn
            $"Warning: the group provides fields {caseNames |> Seq.toList} but the form type has fields {fields |> Seq.toList}: cases {missingCaseNames |> Seq.toList} are missing from the group and so toggling that field is not possible!"

        return ()
    }

  type NestedRenderer<'ExprExtension, 'ValueExtension> with
    static member Validate
      (codegen: CodeGenConfig)
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (expectedType: ExprType)
      (fr: NestedRenderer<'ExprExtension, 'ValueExtension>)
      : Sum<ExprType, Errors> =
      Renderer.Validate codegen ctx expectedType fr.Renderer

  and Renderer<'ExprExtension, 'ValueExtension> with
    static member Validate
      (codegen: CodeGenConfig)
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (expectedType: ExprType)
      (fr: Renderer<'ExprExtension, 'ValueExtension>)
      : Sum<ExprType, Errors> =
      let (!) = Renderer.Validate codegen ctx

      let unify (expected: ExprType) (formType: ExprType) : Sum<Unit, Errors> =
        ExprType.Unify
          Map.empty
          (ctx.Types |> Map.values |> Seq.map (fun v -> v.TypeId, v.Type) |> Map.ofSeq)
          expected
          formType
        |> Sum.map ignore

      let handleInlineFormRenderer (i: FormBody<'ExprExtension, 'ValueExtension>) : Sum<ExprType, Errors> =
        sum {
          let! formType = FormBody.Type ctx.Types i

          let formType =
            match formType with
            | ExprType.TableType row -> row
            | _ -> formType

          do! unify expectedType formType
          do! FormBody.Validate codegen ctx i |> Sum.map ignore
          // NOTE: no need to recurse in the form body because the top-level-forms have already been validated
          formType
        }
        |> sum.WithErrorContext $"...when validating inline form renderer"

      let handleFormRenderer (f: FormConfigId) (typeId: ExprTypeId) : Sum<ExprType, Errors> =
        sum {
          let! formTypeBinding = ctx.TryFindType typeId.VarName
          let! form = ctx.TryFindForm f.FormName
          let formType = formTypeBinding.Type

          do! unify expectedType formType

          return! FormBody.Validate codegen ctx form.Body
        }



      let handlePrimitiveRenderer
        ({ Type = rendererType
           PrimitiveRendererName = RendererName rendererName }: PrimitiveRenderer)
        : Sum<ExprType, Errors> =
        sum {
          do! unify expectedType rendererType

          expectedType
        }
        |> sum.WithErrorContext(sprintf "...when validating primitive renderer %s" rendererName)

      let recordRendererMatchesSupportedRenderersFields
        ({ Renderer = rendererName
           Fields = fields }: RecordRenderer<'ExprExtension, 'ValueExtension>)
        : Sum<Unit, Errors> =
        sum {
          match rendererName with
          | Some renderer ->
            let! rendererFields =
              codegen.Record.SupportedRenderers
              |> Map.tryFindWithError
                renderer
                "record renderer"
                (renderer
                 |> (function
                 | RendererName r -> r))

            if rendererFields |> Set.isEmpty |> not then
              let renderedFields = fields.Fields |> Map.keys |> Set.ofSeq

              if renderedFields <> rendererFields then
                return!
                  sum.Throw(
                    Errors.Singleton
                      $"Error: form renderer expects exactly fields {rendererFields |> List.ofSeq}, instead found {renderedFields |> List.ofSeq}"
                  )
              else
                return ()
            else
              return ()
          | None -> return ()
        }


      sum {
        let error (expectedType: ExprType) (renderer: Renderer<'ExprExtension, 'ValueExtension>) =
          sum.Throw(
            Errors.Singleton(
              sprintf "Error: unexpected renderer for %s type: %s" (expectedType.ToString()) (renderer.ToString())
            )
          )

        match expectedType with
        | ExprType.UnitType ->
          match fr with
          | Renderer.RecordRenderer _ -> return! error expectedType fr
          | Renderer.UnionRenderer _ -> return! error expectedType fr
          | Renderer.InlineFormRenderer i -> return! handleInlineFormRenderer i
          | Renderer.PrimitiveRenderer renderer -> return! handlePrimitiveRenderer renderer
          | Renderer.MapRenderer _ -> return! error expectedType fr
          | Renderer.SumRenderer _ -> return! error expectedType fr
          | Renderer.ListRenderer _ -> return! error expectedType fr
          | Renderer.OptionRenderer _ -> return! error expectedType fr
          | Renderer.OneRenderer _ -> return! error expectedType fr
          | Renderer.ManyRenderer _ -> return! error expectedType fr
          | Renderer.ReadOnlyRenderer _ -> return! error expectedType fr
          | Renderer.EnumRenderer _ -> return! error expectedType fr
          | Renderer.StreamRenderer _ -> return! error expectedType fr
          | Renderer.FormRenderer(formId, typeId) -> return! handleFormRenderer formId typeId
          | Renderer.TableFormRenderer _ -> return! error expectedType fr
          | Renderer.TupleRenderer _ -> return! error expectedType fr
        | ExprType.CustomType _ ->
          match fr with
          | Renderer.RecordRenderer _ -> return! error expectedType fr
          | Renderer.UnionRenderer _ -> return! error expectedType fr
          | Renderer.InlineFormRenderer i -> return! handleInlineFormRenderer i
          | Renderer.PrimitiveRenderer renderer -> return! handlePrimitiveRenderer renderer
          | Renderer.MapRenderer _ -> return! error expectedType fr
          | Renderer.SumRenderer _ -> return! error expectedType fr
          | Renderer.ListRenderer _ -> return! error expectedType fr
          | Renderer.OptionRenderer _ -> return! error expectedType fr
          | Renderer.OneRenderer _ -> return! error expectedType fr
          | Renderer.ManyRenderer _ -> return! error expectedType fr
          | Renderer.ReadOnlyRenderer _ -> return! error expectedType fr
          | Renderer.EnumRenderer _ -> return! error expectedType fr
          | Renderer.StreamRenderer _ -> return! error expectedType fr
          | Renderer.FormRenderer(formId, typeId) -> return! handleFormRenderer formId typeId
          | Renderer.TableFormRenderer _ -> return! error expectedType fr
          | Renderer.TupleRenderer _ -> return! error expectedType fr
        | ExprType.VarType _ -> return! sum.Throw(Errors.Singleton "Error: unexpected renderer for var type")
        | ExprType.LookupType typeId ->
          let! lookupType = ctx.TryFindType typeId.VarName
          return! ! lookupType.Type fr
        | ExprType.KeyOf _ -> return! sum.Throw(Errors.Singleton "Error: unexpected renderer for key of")
        | ExprType.PrimitiveType _ ->
          match fr with
          | Renderer.RecordRenderer _ -> return! error expectedType fr
          | Renderer.UnionRenderer _ -> return! error expectedType fr
          | Renderer.InlineFormRenderer i -> return! handleInlineFormRenderer i
          | Renderer.PrimitiveRenderer renderer -> return! handlePrimitiveRenderer renderer
          | Renderer.MapRenderer _ -> return! error expectedType fr
          | Renderer.SumRenderer _ -> return! error expectedType fr
          | Renderer.ListRenderer _ -> return! error expectedType fr
          | Renderer.OptionRenderer _ -> return! error expectedType fr
          | Renderer.OneRenderer _ -> return! error expectedType fr
          | Renderer.ManyRenderer _ -> return! error expectedType fr
          | Renderer.ReadOnlyRenderer _ -> return! error expectedType fr
          | Renderer.EnumRenderer _ -> return! error expectedType fr
          | Renderer.StreamRenderer _ -> return! error expectedType fr
          | Renderer.FormRenderer(formId, typeId) -> return! handleFormRenderer formId typeId
          | Renderer.TableFormRenderer _ -> return! error expectedType fr
          | Renderer.TupleRenderer _ -> return! error expectedType fr
        | ExprType.RecordType _ ->
          match fr with
          | Renderer.RecordRenderer fields ->
            do! recordRendererMatchesSupportedRenderersFields fields

            do!
              sum.All(
                fields.Fields.Fields
                |> Map.values
                |> Seq.map (FieldConfig.Validate codegen ctx expectedType)
                |> Seq.toList
              )
              |> Sum.map ignore

            expectedType // FIXME: this should be reconstructed from the fields
          | Renderer.UnionRenderer _ -> return! error expectedType fr
          | Renderer.InlineFormRenderer i -> return! handleInlineFormRenderer i
          | Renderer.PrimitiveRenderer renderer -> return! handlePrimitiveRenderer renderer
          | Renderer.MapRenderer _ -> return! error expectedType fr
          | Renderer.SumRenderer _ -> return! error expectedType fr
          | Renderer.ListRenderer _ -> return! error expectedType fr
          | Renderer.OptionRenderer _ -> return! error expectedType fr
          | Renderer.OneRenderer _ -> return! error expectedType fr
          | Renderer.ManyRenderer _ -> return! error expectedType fr
          | Renderer.ReadOnlyRenderer _ -> return! error expectedType fr
          | Renderer.EnumRenderer _ -> return! error expectedType fr
          | Renderer.StreamRenderer _ -> return! error expectedType fr
          | Renderer.FormRenderer(formId, typeId) -> return! handleFormRenderer formId typeId
          | Renderer.TableFormRenderer _ -> return! error expectedType fr
          | Renderer.TupleRenderer _ -> return! error expectedType fr
        | ExprType.UnionType typeCases ->
          match fr with
          | Renderer.RecordRenderer _ -> return! error expectedType fr
          | Renderer.UnionRenderer formCases ->
            let unallowedCases =
              typeCases
              |> Map.filter (fun _ typeCase -> typeCase.Fields.IsUnitType || typeCase.Fields.IsLookupType |> not)

            let typeCaseNames =
              typeCases |> Map.values |> Seq.map (fun c -> c.CaseName) |> Set.ofSeq

            let formCaseNames = formCases.Cases |> Map.keys |> Set.ofSeq

            let missingTypeCases = typeCaseNames - formCaseNames
            let missingFormCases = formCaseNames - typeCaseNames

            if missingTypeCases |> Set.isEmpty |> not then
              return! sum.Throw(Errors.Singleton $"Error: missing type cases {missingTypeCases.ToFSharpString}")
            elif missingFormCases |> Set.isEmpty |> not then
              return! sum.Throw(Errors.Singleton $"Error: missing form cases {missingFormCases.ToFSharpString}")
            elif unallowedCases |> Map.isEmpty |> not then
              return!
                sum.Throw(
                  Errors.Singleton
                    $"Error: case(s) {unallowedCases |> Map.keys |> Seq.map (fun c -> c.CaseName) |> Set.ofSeq} have unallowed type(s), only lookup types and unit types are allowed"
                )
            else
              do!
                typeCases
                |> Seq.map (fun typeCase ->
                  match formCases.Cases |> Map.tryFind typeCase.Key.CaseName with
                  | None ->
                    sum.Throw(Errors.Singleton $"Error: cannot find form case for type case {typeCase.Key.CaseName}")
                  | Some formCase ->
                    NestedRenderer.Validate codegen ctx typeCase.Value.Fields formCase
                    |> sum.WithErrorContext $"...when validating case {typeCase.Key.CaseName}")
                |> sum.All
                |> Sum.map ignore

              expectedType // FIXME: this should be reconstructed from the cases
          | Renderer.InlineFormRenderer i -> return! handleInlineFormRenderer i
          | Renderer.PrimitiveRenderer renderer -> return! handlePrimitiveRenderer renderer
          | Renderer.MapRenderer _ -> return! error expectedType fr
          | Renderer.SumRenderer _ -> return! error expectedType fr
          | Renderer.ListRenderer _ -> return! error expectedType fr
          | Renderer.OptionRenderer _ -> return! error expectedType fr
          | Renderer.OneRenderer _ -> return! error expectedType fr
          | Renderer.ManyRenderer _ -> return! error expectedType fr
          | Renderer.ReadOnlyRenderer _ -> return! error expectedType fr
          | Renderer.EnumRenderer _ -> return! error expectedType fr
          | Renderer.StreamRenderer _ -> return! error expectedType fr
          | Renderer.FormRenderer(formId, typeId) -> return! handleFormRenderer formId typeId
          | Renderer.TableFormRenderer _ -> return! error expectedType fr
          | Renderer.TupleRenderer _ -> return! error expectedType fr
        | ExprType.MapType(expectedKeyType, expectedValueType) ->
          match fr with
          | Renderer.RecordRenderer _ -> return! error expectedType fr
          | Renderer.UnionRenderer _ -> return! error expectedType fr
          | Renderer.InlineFormRenderer i -> return! handleInlineFormRenderer i
          | Renderer.PrimitiveRenderer renderer -> return! handlePrimitiveRenderer renderer
          | Renderer.MapRenderer renderer ->
            let! validatedKeyType = ! expectedKeyType renderer.Key.Renderer
            let! validatedValueType = ! expectedValueType renderer.Value.Renderer
            ExprType.MapType(validatedKeyType, validatedValueType)
          | Renderer.SumRenderer _ -> return! error expectedType fr
          | Renderer.ListRenderer _ -> return! error expectedType fr
          | Renderer.OptionRenderer _ -> return! error expectedType fr
          | Renderer.OneRenderer _ -> return! error expectedType fr
          | Renderer.ManyRenderer _ -> return! error expectedType fr
          | Renderer.ReadOnlyRenderer _ -> return! error expectedType fr
          | Renderer.EnumRenderer _ -> return! error expectedType fr
          | Renderer.StreamRenderer _ -> return! error expectedType fr
          | Renderer.FormRenderer(formId, typeId) -> return! handleFormRenderer formId typeId
          | Renderer.TableFormRenderer _ -> return! error expectedType fr
          | Renderer.TupleRenderer _ -> return! error expectedType fr
        | ExprType.SumType(expectedLeftType, expectedRightType) ->
          match fr with
          | Renderer.RecordRenderer _ -> return! error expectedType fr
          | Renderer.UnionRenderer _ -> return! error expectedType fr
          | Renderer.InlineFormRenderer i -> return! handleInlineFormRenderer i
          | Renderer.PrimitiveRenderer renderer -> return! handlePrimitiveRenderer renderer
          | Renderer.MapRenderer _ -> return! error expectedType fr
          | Renderer.SumRenderer renderer ->
            let! validatedLeftType = ! expectedLeftType renderer.Left.Renderer
            let! validatedRightType = ! expectedRightType renderer.Right.Renderer
            ExprType.SumType(validatedLeftType, validatedRightType)
          | Renderer.ListRenderer _ -> return! error expectedType fr
          | Renderer.OptionRenderer _ -> return! error expectedType fr
          | Renderer.OneRenderer _ -> return! error expectedType fr
          | Renderer.ManyRenderer _ -> return! error expectedType fr
          | Renderer.ReadOnlyRenderer _ -> return! error expectedType fr
          | Renderer.EnumRenderer _ -> return! error expectedType fr
          | Renderer.StreamRenderer _ -> return! error expectedType fr
          | Renderer.FormRenderer(formId, typeId) -> return! handleFormRenderer formId typeId
          | Renderer.TableFormRenderer _ -> return! error expectedType fr
          | Renderer.TupleRenderer _ -> return! error expectedType fr
        | ExprType.TupleType elementTypes ->
          match fr with
          | Renderer.RecordRenderer _ -> return! error expectedType fr
          | Renderer.UnionRenderer _ -> return! error expectedType fr
          | Renderer.InlineFormRenderer i -> return! handleInlineFormRenderer i
          | Renderer.PrimitiveRenderer renderer -> return! handlePrimitiveRenderer renderer
          | Renderer.MapRenderer _ -> return! error expectedType fr
          | Renderer.SumRenderer _ -> return! error expectedType fr
          | Renderer.ListRenderer _ -> return! error expectedType fr
          | Renderer.OptionRenderer _ -> return! error expectedType fr
          | Renderer.OneRenderer _ -> return! error expectedType fr
          | Renderer.ManyRenderer _ -> return! error expectedType fr
          | Renderer.ReadOnlyRenderer _ -> return! error expectedType fr
          | Renderer.EnumRenderer _ -> return! error expectedType fr
          | Renderer.StreamRenderer _ -> return! error expectedType fr
          | Renderer.FormRenderer(formId, typeId) -> return! handleFormRenderer formId typeId
          | Renderer.TableFormRenderer _ -> return! error expectedType fr
          | Renderer.TupleRenderer renderer ->
            if List.length renderer.Elements <> List.length elementTypes then
              return!
                $"Error: tuple renderer has {List.length renderer.Elements} elements but the expected type has {List.length elementTypes} elements"
                |> Errors.Singleton
                |> sum.Throw
            else

              let! validatedElements =
                List.zip elementTypes renderer.Elements
                |> List.mapi (fun index (t, r) ->
                  ! t r.Renderer
                  |> sum.WithErrorContext
                    $"...when validating tuple element {index + 1}/{List.length renderer.Elements}")
                |> sum.All

              ExprType.TupleType validatedElements
        | ExprType.OptionType expectedInnerType ->
          match fr with
          | Renderer.RecordRenderer _ -> return! error expectedType fr
          | Renderer.UnionRenderer _ -> return! error expectedType fr
          | Renderer.InlineFormRenderer i -> return! handleInlineFormRenderer i
          | Renderer.PrimitiveRenderer renderer -> return! handlePrimitiveRenderer renderer
          | Renderer.MapRenderer _ -> return! error expectedType fr
          | Renderer.SumRenderer _ -> return! error expectedType fr
          | Renderer.ListRenderer _ -> return! error expectedType fr
          | Renderer.OptionRenderer renderer ->
            do! ! ExprType.UnitType renderer.None.Renderer |> Sum.map ignore
            return! ! expectedInnerType renderer.Some.Renderer |> Sum.map ExprType.OptionType
          | Renderer.OneRenderer _ -> return! error expectedType fr
          | Renderer.ManyRenderer _ -> return! error expectedType fr
          | Renderer.ReadOnlyRenderer _ -> return! error expectedType fr
          | Renderer.EnumRenderer(_, _, enumRendererType, rendererTypeId, RendererName rendererName) ->
            match enumRendererType with
            | EnumRendererType.Option ->
              return!
                sum {
                  do! unify expectedType (ExprType.OptionType(ExprType.LookupType rendererTypeId))

                  expectedType
                }
                |> sum.WithErrorContext(sprintf "...when validating enum renderer %s" rendererName)
            | EnumRendererType.Set ->
              return! sum.Throw(Errors.Singleton "Error: set enum renderer is not supported on option type")
          | Renderer.StreamRenderer(_, _, streamRendererType, rendererTypeId, RendererName rendererName) ->
            match streamRendererType with
            | StreamRendererType.Option ->
              return!
                sum {
                  do! unify expectedType (ExprType.OptionType(ExprType.LookupType rendererTypeId))

                  expectedType
                }
                |> sum.WithErrorContext(sprintf "...when validating stream renderer %s" rendererName)
            | StreamRendererType.Set ->
              return! sum.Throw(Errors.Singleton "Error: set stream renderer is not supported on option type")
          | Renderer.FormRenderer(formId, typeId) -> return! handleFormRenderer formId typeId
          | Renderer.TableFormRenderer _ -> return! error expectedType fr
          | Renderer.TupleRenderer _ -> return! error expectedType fr
        | ExprType.OneType expectedInnerType ->
          match fr with
          | Renderer.RecordRenderer _ -> return! error expectedType fr
          | Renderer.UnionRenderer _ -> return! error expectedType fr
          | Renderer.InlineFormRenderer i -> return! handleInlineFormRenderer i
          | Renderer.PrimitiveRenderer renderer -> return! handlePrimitiveRenderer renderer
          | Renderer.MapRenderer _ -> return! error expectedType fr
          | Renderer.SumRenderer _ -> return! error expectedType fr
          | Renderer.ListRenderer _ -> return! error expectedType fr
          | Renderer.OptionRenderer _ -> return! error expectedType fr
          | Renderer.OneRenderer renderer ->
            let! apiMethods =
              sum {
                let! oneApiEntity, oneApiMethods =
                  ctx.TryFindOne (fst renderer.OneApiId).VarName (snd renderer.OneApiId)

                let! oneApiType = ctx.TryFindType oneApiEntity.TypeId.VarName
                let oneApi = oneApiType.Type

                do! unify (expectedInnerType |> ExprType.OneType) (oneApi |> ExprType.OneType)

                return oneApiMethods
              }

            match renderer.Preview with
            | Some preview ->
              if apiMethods |> Set.contains CrudMethod.GetManyUnlinked |> not then
                return!
                  sum.Throw(
                    Errors.Singleton
                      $"Error: api {renderer.OneApiId.ToFSharpString} is used in a preview but has no {CrudMethod.GetManyUnlinked.ToFSharpString} method."
                  )
              else
                return ()

              do! ! expectedInnerType preview.Renderer |> Sum.map ignore
            | None -> return ()

            return! ! expectedInnerType renderer.Details.Renderer |> Sum.map ExprType.OneType
          | Renderer.ManyRenderer _ -> return! error expectedType fr
          | Renderer.ReadOnlyRenderer _ -> return! error expectedType fr
          | Renderer.EnumRenderer _ -> return! error expectedType fr
          | Renderer.StreamRenderer _ -> return! error expectedType fr
          | Renderer.FormRenderer(formId, typeId) -> return! handleFormRenderer formId typeId
          | Renderer.TableFormRenderer _ -> return! error expectedType fr
          | Renderer.TupleRenderer _ -> return! error expectedType fr
        | ExprType.ReadOnlyType innerType ->
          match fr with
          | Renderer.RecordRenderer _ -> return! error expectedType fr
          | Renderer.UnionRenderer _ -> return! error expectedType fr
          | Renderer.InlineFormRenderer i -> return! handleInlineFormRenderer i
          | Renderer.PrimitiveRenderer renderer -> return! handlePrimitiveRenderer renderer
          | Renderer.MapRenderer _ -> return! error expectedType fr
          | Renderer.SumRenderer _ -> return! error expectedType fr
          | Renderer.ListRenderer _ -> return! error expectedType fr
          | Renderer.OptionRenderer _ -> return! error expectedType fr
          | Renderer.OneRenderer _ -> return! error expectedType fr
          | Renderer.ManyRenderer _ -> return! error expectedType fr
          | Renderer.ReadOnlyRenderer renderer ->
            return! ! innerType renderer.Value.Renderer |> Sum.map ExprType.ReadOnlyType
          | Renderer.EnumRenderer _ -> return! error expectedType fr
          | Renderer.StreamRenderer _ -> return! error expectedType fr
          | Renderer.FormRenderer(formId, typeId) -> return! handleFormRenderer formId typeId
          | Renderer.TableFormRenderer _ -> return! error expectedType fr
          | Renderer.TupleRenderer _ -> return! error expectedType fr
        | ExprType.ManyType _ -> return! sum.Throw(Errors.Singleton "Error: unexpected renderer for many type")
        | ExprType.ListType innerType ->
          match fr with
          | Renderer.RecordRenderer _ -> return! error expectedType fr
          | Renderer.UnionRenderer _ -> return! error expectedType fr
          | Renderer.InlineFormRenderer i -> return! handleInlineFormRenderer i
          | Renderer.PrimitiveRenderer renderer -> return! handlePrimitiveRenderer renderer
          | Renderer.MapRenderer _ -> return! error expectedType fr
          | Renderer.SumRenderer _ -> return! error expectedType fr
          | Renderer.ListRenderer renderer -> return! ! innerType renderer.Element.Renderer |> Sum.map ExprType.ListType
          | Renderer.OptionRenderer _ -> return! error expectedType fr
          | Renderer.OneRenderer _ -> return! error expectedType fr
          | Renderer.ManyRenderer _ -> return! error expectedType fr
          | Renderer.ReadOnlyRenderer _ -> return! error expectedType fr
          | Renderer.EnumRenderer _ -> return! error expectedType fr
          | Renderer.StreamRenderer _ -> return! error expectedType fr
          | Renderer.FormRenderer(formId, typeId) -> return! handleFormRenderer formId typeId
          | Renderer.TableFormRenderer _ -> return! error expectedType fr
          | Renderer.TupleRenderer _ -> return! error expectedType fr
        | ExprType.TableType expectedRowType ->
          match fr with
          | Renderer.RecordRenderer _ -> return! error expectedType fr
          | Renderer.UnionRenderer _ -> return! error expectedType fr
          | Renderer.InlineFormRenderer i -> return! handleInlineFormRenderer i
          | Renderer.PrimitiveRenderer renderer -> return! handlePrimitiveRenderer renderer
          | Renderer.MapRenderer _ -> return! error expectedType fr
          | Renderer.SumRenderer _ -> return! error expectedType fr
          | Renderer.ListRenderer _ -> return! error expectedType fr
          | Renderer.OptionRenderer _ -> return! error expectedType fr
          | Renderer.OneRenderer _ -> return! error expectedType fr
          | Renderer.ManyRenderer _ -> return! error expectedType fr
          | Renderer.ReadOnlyRenderer _ -> return! error expectedType fr
          | Renderer.EnumRenderer _ -> return! error expectedType fr
          | Renderer.StreamRenderer _ -> return! error expectedType fr
          | Renderer.FormRenderer(formId, typeId) -> return! handleFormRenderer formId typeId
          | Renderer.TableFormRenderer(formConfigId, formRowType, tableApiId) ->
            let! _ = ctx.TryFindForm formConfigId.FormName
            let! tableApi, _ = ctx.TryFindTableApi tableApiId.TableName
            let! apiRowType = ctx.TryFindType tableApi.TypeId.VarName

            do! unify (expectedRowType |> ExprType.TableType) (apiRowType.Type |> ExprType.TableType)

            ExprType.TableType formRowType // NOTE: nothing to recurse into
          | Renderer.TupleRenderer _ -> return! error expectedType fr
        | ExprType.SetType _ ->
          match fr with
          | Renderer.RecordRenderer _ -> return! error expectedType fr
          | Renderer.UnionRenderer _ -> return! error expectedType fr
          | Renderer.InlineFormRenderer i -> return! handleInlineFormRenderer i
          | Renderer.PrimitiveRenderer renderer -> return! handlePrimitiveRenderer renderer
          | Renderer.MapRenderer _ -> return! error expectedType fr
          | Renderer.SumRenderer _ -> return! error expectedType fr
          | Renderer.ListRenderer _ -> return! error expectedType fr
          | Renderer.OptionRenderer _ -> return! error expectedType fr
          | Renderer.OneRenderer _ -> return! error expectedType fr
          | Renderer.ManyRenderer _ -> return! error expectedType fr
          | Renderer.ReadOnlyRenderer _ -> return! error expectedType fr
          | Renderer.EnumRenderer(_, _, enumRendererType, rendererTypeId, RendererName rendererName) ->
            match enumRendererType with
            | EnumRendererType.Option ->
              return! sum.Throw(Errors.Singleton "Error: option enum renderer is not supported on set type")
            | EnumRendererType.Set ->
              return!
                sum {
                  do! unify expectedType (ExprType.SetType(ExprType.LookupType rendererTypeId))
                  expectedType
                }
                |> sum.WithErrorContext(sprintf "...when validating enum renderer %s" rendererName)
          | Renderer.StreamRenderer(_, _, streamRendererType, rendererTypeId, RendererName rendererName) ->
            match streamRendererType with
            | StreamRendererType.Option ->
              return! sum.Throw(Errors.Singleton "Error: option stream renderer is not supported on set type")
            | StreamRendererType.Set ->
              return!
                sum {
                  do! unify expectedType (ExprType.SetType(ExprType.LookupType rendererTypeId))

                  expectedType
                }
                |> sum.WithErrorContext(sprintf "...when validating stream renderer %s" rendererName)
          | Renderer.FormRenderer(formId, typeId) -> return! handleFormRenderer formId typeId
          | Renderer.TableFormRenderer _ -> return! error expectedType fr
          | Renderer.TupleRenderer _ -> return! error expectedType fr
        | ExprType.ArrowType _ -> return! sum.Throw(Errors.Singleton "Error: unexpected renderer for arrow type")
        | ExprType.GenericType _ -> return! sum.Throw(Errors.Singleton "Error: unexpected renderer for generic type")
        | ExprType.GenericApplicationType _ ->
          return! sum.Throw(Errors.Singleton "Error: unexpected renderer for generic application")
        | ExprType.TranslationOverride { Label = label; KeyType = keyType } ->
          match keyType with
          | ExprType.LookupType { VarName = varName } ->
            let (LanguageStreamType languageStreamType) = codegen.LanguageStreamType

            if languageStreamType = varName then
              let expectedType = TranslationOverride.Type { Label = label; KeyType = keyType }
              do! ! expectedType fr |> Sum.map ignore
              ExprType.TranslationOverride { Label = label; KeyType = keyType }
            else
              return!
                sum.Throw(
                  Errors.Singleton(
                    sprintf
                      "Error: translation override key type %s is not the configured language stream type %s"
                      varName
                      languageStreamType
                  )
                )
          | _ ->
            return!
              sum.Throw(
                Errors.Singleton(
                  sprintf "Error: translation override key type %s is not a lookup type" (keyType.ToString())
                )
              )
      }

  and NestedRenderer<'ExprExtension, 'ValueExtension> with
    static member ValidatePredicates
      validateFormConfigPredicates
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (typeCheck: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
      (globalType: ExprType)
      (rootType: ExprType)
      (localType: ExprType)
      (r: NestedRenderer<'ExprExtension, 'ValueExtension>)
      : State<Unit, CodeGenConfig, ValidationState, Errors> =
      state {
        do!
          Renderer.ValidatePredicates
            validateFormConfigPredicates
            ctx
            typeCheck
            globalType
            rootType
            localType
            r.Renderer
      }

  and Renderer<'ExprExtension, 'ValueExtension> with
    static member ValidatePredicates
      validateFormConfigPredicates
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (typeCheck: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
      (globalType: ExprType)
      (rootType: ExprType)
      (localType: ExprType)
      (r: Renderer<'ExprExtension, 'ValueExtension>)
      : State<Unit, CodeGenConfig, ValidationState, Errors> =
      let (!!) =
        NestedRenderer.ValidatePredicates validateFormConfigPredicates ctx typeCheck globalType rootType localType

      state {
        match r with
        | Renderer.RecordRenderer fields ->
          do! FormFields.ValidatePredicates ctx typeCheck globalType rootType localType fields.Fields
        | Renderer.UnionRenderer cases ->
          let! typeCases = localType |> ExprType.AsUnion |> state.OfSum

          for case in cases.Cases do
            let! typeCase =
              typeCases
              |> Map.tryFind ({ CaseName = case.Key })
              |> Sum.fromOption (fun () -> Errors.Singleton $"Error: cannot find type case {case.Key}")
              |> state.OfSum

            do!
              NestedRenderer.ValidatePredicates
                FormConfig.ValidatePredicates
                ctx
                typeCheck
                globalType
                rootType
                typeCase.Fields
                case.Value
        | Renderer.InlineFormRenderer i ->
          let! formType = FormBody.Type ctx.Types i |> state.OfSum

          let formType =
            if i.IsTable |> not then
              formType
            else
              match formType with
              | ExprType.TableType row -> row
              | _ -> formType

          do! FormBody.ValidatePredicates ctx typeCheck globalType rootType formType i
        | Renderer.PrimitiveRenderer _ -> return ()
        | Renderer.EnumRenderer _ -> return ()
        | Renderer.TupleRenderer e ->

          for element in e.Elements do
            do! !!element

        | Renderer.OneRenderer e ->
          do! !!e.Details

          match e.Preview with
          | Some preview -> do! !!preview
          | _ -> return ()

        | Renderer.ManyRenderer(ManyAllRenderer e) -> do! !!e.Element

        | Renderer.ManyRenderer(ManyLinkedUnlinkedRenderer e) ->
          do! !!e.Linked

          match e.Unlinked with
          | Some unlinked -> do! !!unlinked
          | None -> return ()

        | Renderer.OptionRenderer e ->
          do! !!e.None
          do! !!e.Some

        | Renderer.ReadOnlyRenderer e -> do! !!e.Value

        | Renderer.ListRenderer e -> do! !!e.Element

        // | Renderer.TableRenderer e ->
        //   do! !e.Table
        //   do! !!e.Row

        //   do! e.Children |> validateChildrenPredicates
        | Renderer.MapRenderer kv ->
          do! !!kv.Key
          do! !!kv.Value

        | Renderer.SumRenderer s ->
          do! !!s.Left
          do! !!s.Right

        | Renderer.StreamRenderer _ -> return ()
        | Renderer.FormRenderer(f, _) ->
          let! f = ctx.TryFindForm f.FormName |> state.OfSum
          let! _ = state.GetState()

          do! validateFormConfigPredicates ctx typeCheck globalType rootType f

        | Renderer.TableFormRenderer(f, _, _) ->
          // let! f = ctx.TryFindForm f.FormName |> state.OfSum
          // let! api = ctx.TryFindTableApi tableApiId.TableName |> state.OfSum
          // let! apiRowType = ctx.TryFindType api.TypeId.VarName |> state.OfSum

          let! f = ctx.TryFindForm f.FormName |> state.OfSum
          let! _ = state.GetState()

          do! validateFormConfigPredicates ctx typeCheck globalType rootType f
      // | Renderer.UnionRenderer cs ->
      //   do! !cs.Union

      //   do!
      //     cs.Cases
      //     |> Seq.map (fun e -> e.Value)
      //     |> Seq.map (fun c ->
      //       Renderer.ValidatePredicates validateFormConfigPredicates ctx globalType rootType c.Type c)
      //     |> state.All
      //     |> state.Map ignore
      }

  and FieldConfig<'ExprExtension, 'ValueExtension> with
    static member Validate
      (codegen: CodeGenConfig)
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (recordType: ExprType)
      (fc: FieldConfig<'ExprExtension, 'ValueExtension>)
      : Sum<Unit, Errors> =
      sum {

        match recordType with
        | RecordType fields ->
          match fields |> Map.tryFind fc.FieldName with
          | Some expectedFieldType ->
            let! expectedFieldType = ExprType.ResolveLookup ctx.Types expectedFieldType

            do!
              Renderer.Validate codegen ctx expectedFieldType fc.Renderer
              |> sum.WithErrorContext $"...when validating field config renderer for {fc.FieldName}"
              |> Sum.map ignore

            return ()
          | None ->
            return!
              sum.Throw(
                Errors.Singleton(sprintf "Error: field name %A is not found in type %A" fc.FieldName recordType)
              )
        | _ -> return! sum.Throw(Errors.Singleton(sprintf "Error: form type %A is not a record type" recordType))
      }
      |> sum.WithErrorContext $"...when validating field {fc.FieldName}"

    static member ValidatePredicates
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (typeCheck: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
      (globalType: ExprType)
      (rootType: ExprType)
      (localType: ExprType)
      (includeLocalTypeInScope: bool)
      (fc: FieldConfig<'ExprExtension, 'ValueExtension>)
      : State<Unit, CodeGenConfig, ValidationState, Errors> =
      state {
        let vars =
          [ ("global", globalType); ("root", rootType) ]
          @ (if includeLocalTypeInScope then
               [ ("local", localType) ]
             else
               [])
          |> Seq.map (VarName.Create <*> id)
          |> Map.ofSeq

        let! visibleExprType =
          typeCheck (ctx.Types |> Seq.map (fun tb -> tb.Value.TypeId, tb.Value.Type) |> Map.ofSeq) vars fc.Visible
          |> state.OfSum
        // do System.Console.WriteLine $"{fc.Visible.ToFSharpString}"
        // do System.Console.WriteLine $"{visibleExprType}"
        do!
          ExprType.Unify
            Map.empty
            (ctx.Types |> Map.values |> Seq.map (fun v -> v.TypeId, v.Type) |> Map.ofSeq)
            visibleExprType
            (ExprType.PrimitiveType PrimitiveType.BoolType)
          |> Sum.map ignore
          |> state.OfSum

        match fc.Disabled with
        | Some disabled ->
          let! disabledExprType =
            typeCheck (ctx.Types |> Seq.map (fun tb -> tb.Value.TypeId, tb.Value.Type) |> Map.ofSeq) vars disabled
            |> state.OfSum

          do!
            ExprType.Unify
              Map.empty
              (ctx.Types |> Map.values |> Seq.map (fun v -> v.TypeId, v.Type) |> Map.ofSeq)
              disabledExprType
              (ExprType.PrimitiveType PrimitiveType.BoolType)
            |> Sum.map ignore
            |> state.OfSum
        | _ -> return ()

        do!
          Renderer.ValidatePredicates
            FormConfig.ValidatePredicates
            ctx
            typeCheck
            globalType
            rootType
            localType
            fc.Renderer
      }
      |> state.WithErrorContext $"...when validating field predicates for {fc.FieldName}"

  and FormFields<'ExprExtension, 'ValueExtension> with
    static member ValidatePredicates
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (typeCheck: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
      (globalType: ExprType)
      (rootType: ExprType)
      (localType: ExprType)
      (formFields: FormFields<'ExprExtension, 'ValueExtension>)
      : State<Unit, CodeGenConfig, ValidationState, Errors> =
      state {
        for f in formFields.Fields do
          do!
            FieldConfig.ValidatePredicates ctx typeCheck globalType rootType localType true f.Value
            |> state.Map ignore

        let disabledFieldsValidation =
          match formFields.Disabled with
          | FormGroup.Computed e ->
            let vars =
              [ ("global", globalType); ("root", rootType); ("local", localType) ]
              |> Seq.map (VarName.Create <*> id)
              |> Map.ofSeq

            validateGroupPredicates ctx typeCheck vars localType e
          | _ -> state.Return()

        let tabsValidation: State<Unit, CodeGenConfig, ValidationState, Errors> =
          formFields.Tabs.FormTabs
          |> Map.values
          |> Seq.collect (fun tab ->
            tab.FormColumns
            |> Map.values
            |> Seq.collect (fun col ->
              col.FormGroups
              |> Map.values
              |> Seq.choose (fun group ->
                match group with
                | FormGroup.Computed e ->
                  let vars =
                    [ ("global", globalType); ("root", rootType); ("local", localType) ]
                    |> Seq.map (VarName.Create <*> id)
                    |> Map.ofSeq

                  Some(validateGroupPredicates ctx typeCheck vars localType e)
                | _ -> None)))
          |> Seq.toList
          |> state.All
          |> state.Map ignore

        do! state.All2 disabledFieldsValidation tabsValidation |> state.Map ignore
      }



  and FormBody<'ExprExtension, 'ValueExtension> with
    static member Validate
      (codegen: CodeGenConfig)
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (body: FormBody<'ExprExtension, 'ValueExtension>)
      : Sum<ExprType, Errors> =
      sum {
        match body with
        | FormBody.Annotated renderer ->
          return! Renderer.Validate codegen ctx (ExprType.LookupType renderer.TypeId) renderer.Renderer
        | FormBody.Table table ->
          let! rowType = ctx.TryFindType table.RowTypeId.VarName

          match table.Details with
          | Some details -> do! NestedRenderer.Validate codegen ctx rowType.Type details |> Sum.map ignore
          | None -> return ()

          // match table.Preview with
          // | Some preview -> do! FormBody.Validate codegen ctx localType preview |> Sum.map ignore
          // | None -> return ()
          let! rowFields = rowType.Type |> ExprType.AsRecord

          do!
            table.HighlightedFilters
            |> Seq.map (fun f -> rowFields |> Map.tryFindWithError f "highlightedFilters" f)
            |> sum.All
            |> sum.Map ignore

          do!
            sum.All(
              table.Columns
              |> Map.values
              |> Seq.map (fun c ->
                FieldConfig.Validate codegen ctx rowType.Type c.FieldConfig
                |> sum.WithErrorContext $"...when validating table column {c.FieldConfig.FieldName}")
              |> Seq.toList
            )
            |> Sum.map ignore

          ExprType.TableType rowType.Type
      }

    static member ValidatePredicates
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (typeCheck: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
      (globalType: ExprType)
      (rootType: ExprType)
      (localType: ExprType)
      (body: FormBody<'ExprExtension, 'ValueExtension>)
      : State<Unit, CodeGenConfig, ValidationState, Errors> =
      state {
        match body with
        | FormBody.Annotated renderer ->
          do!
            Renderer.ValidatePredicates
              FormConfig.ValidatePredicates
              ctx
              typeCheck
              globalType
              rootType
              localType
              renderer.Renderer
        | FormBody.Table table ->
          let rowType = localType
          let! rowTypeFields = rowType |> ExprType.AsRecord |> state.OfSum

          for column in table.Columns do
            let! columnType =
              rowTypeFields
              |> Map.tryFindWithError (column.Key) "fields" "fields"
              |> state.OfSum

            do!
              FieldConfig.ValidatePredicates ctx typeCheck globalType rootType columnType false column.Value.FieldConfig

            match table.Details with
            | Some details ->
              do!
                NestedRenderer.ValidatePredicates
                  FormConfig.ValidatePredicates
                  ctx
                  typeCheck
                  globalType
                  rootType
                  localType
                  details
            | None -> return ()

          // match table.Preview with
          // | Some preview -> do! FormBody.ValidatePredicates ctx globalType rootType localType preview
          // | None -> return ()

          match table.VisibleColumns with
          | Inlined _ -> return ()
          | Computed visibleExpr ->
            let vars =
              [ ("global", globalType) ] |> Seq.map (VarName.Create <*> id) |> Map.ofSeq

            return! validateGroupPredicates ctx typeCheck vars localType visibleExpr
      }

  and FormConfig<'ExprExtension, 'ValueExtension> with
    static member Validate
      (config: CodeGenConfig)
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (formConfig: FormConfig<'ExprExtension, 'ValueExtension>)
      : Sum<Unit, Errors> =
      sum { do! FormBody.Validate config ctx formConfig.Body |> Sum.map ignore }
      |> sum.WithErrorContext $"...when validating form config {formConfig.FormName}"

    static member ValidatePredicates
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (typeCheck: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
      (globalType: ExprType)
      (rootType: ExprType)
      (formConfig: FormConfig<'ExprExtension, 'ValueExtension>)
      : State<Unit, CodeGenConfig, ValidationState, Errors> =
      state {
        let! s = state.GetState()

        let processedForm =
          { Form = formConfig |> FormConfig.Id
            GlobalType = globalType
            RootType = rootType }

        if s.PredicateValidationHistory |> Set.contains processedForm |> not then
          do! state.SetState(ValidationState.Updaters.PredicateValidationHistory(Set.add processedForm))

          let! formType = formConfig.Body |> FormBody.FormDeclarationType ctx.Types |> state.OfSum

          do! FormBody.ValidatePredicates ctx typeCheck globalType rootType formType formConfig.Body

          return ()
        else
          // do Console.WriteLine($$"""Prevented reprocessing of form {{processedForm}}""")
          // do Console.ReadLine() |> ignore
          return ()
      }
      |> state.WithErrorContext $"...when validating form predicates for {formConfig.FormName}"

  and FormLauncher with
    static member Validate
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (typeCheck: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
      (formLauncher: FormLauncher)
      : State<Unit, CodeGenConfig, ValidationState, Errors> =
      state {
        let! formConfig = ctx.TryFindForm formLauncher.Form.FormName |> state.OfSum

        let! formType = formConfig.Body |> FormBody.FormDeclarationType ctx.Types |> state.OfSum

        match formLauncher.Mode with
        | FormLauncherMode.Create({ EntityApi = entityApi
                                    ConfigEntityApi = configEntityApi })
        | FormLauncherMode.Edit({ EntityApi = entityApi
                                  ConfigEntityApi = configEntityApi }) ->
          let! entityApi = ctx.TryFindEntityApi entityApi.EntityName |> state.OfSum
          let! entityApiType = ctx.TryFindType (entityApi |> fst).TypeId.VarName |> state.OfSum
          let! configEntityApi = ctx.TryFindEntityApi configEntityApi.EntityName |> state.OfSum

          if Set.ofList [ CrudMethod.Get ] |> Set.isSuperset (configEntityApi |> snd) then
            let! configEntityApiType = ctx.TryFindType (configEntityApi |> fst).TypeId.VarName |> state.OfSum

            do!
              ExprType.Unify
                Map.empty
                (ctx.Types |> Map.values |> Seq.map (fun v -> v.TypeId, v.Type) |> Map.ofSeq)
                formType
                entityApiType.Type
              |> Sum.map ignore
              |> state.OfSum

            do!
              FormConfig.ValidatePredicates ctx typeCheck configEntityApiType.Type entityApiType.Type formConfig
              |> state.Map ignore

            match formLauncher.Mode with
            | FormLauncherMode.Create _ ->
              if Set.singleton CrudMethod.Get |> Set.isSuperset (entityApi |> snd) then
                return ()
              else
                return!
                  sum.Throw(
                    Errors.Singleton(
                      sprintf
                        "Error in launcher %A: entity APIs for 'create' launchers need at least methods CREATE and DEFAULT, found %A"
                        formLauncher.LauncherName
                        (entityApi |> snd)
                    )
                  )
                  |> state.OfSum
            | _ ->
              if Set.singleton CrudMethod.Get |> Set.isSuperset (entityApi |> snd) then
                return ()
              else
                return!
                  sum.Throw(
                    Errors.Singleton(
                      sprintf
                        "Error in launcher %A: entity APIs for 'edit' launchers need at least methods GET and UPDATE, found %A"
                        formLauncher.LauncherName
                        (entityApi |> snd)
                    )
                  )
                  |> state.OfSum
          else
            return!
              sum.Throw(
                Errors.Singleton(
                  sprintf
                    "Error in launcher %A: entity APIs for 'config' launchers need at least method GET, found %A"
                    formLauncher.LauncherName
                    (configEntityApi |> snd)
                )
              )
              |> state.OfSum
        | FormLauncherMode.Passthrough m ->
          let! configEntityType = ctx.TryFindType m.ConfigType.VarName |> state.OfSum

          let! entityType = (formConfig.Body |> FormBody.FormDeclarationType ctx.Types) |> state.OfSum

          do!
            FormConfig.ValidatePredicates ctx typeCheck configEntityType.Type entityType formConfig
            |> state.Map ignore
        | FormLauncherMode.PassthroughTable m ->
          let! configEntityType = ctx.TryFindType m.ConfigType.VarName |> state.OfSum
          let! api = ctx.TryFindTableApi m.TableApi.TableName |> state.OfSum
          let! (apiType: TypeBinding) = api |> fst |> (fun a -> ctx.TryFindType a.TypeId.VarName) |> state.OfSum
          let apiType = apiType.Type

          do!
            ExprType.Unify
              Map.empty
              (ctx.Types |> Map.values |> Seq.map (fun v -> v.TypeId, v.Type) |> Map.ofSeq)
              formType
              apiType
            |> Sum.map ignore
            |> state.OfSum

          let! entityType = (formConfig.Body |> FormBody.FormDeclarationType ctx.Types) |> state.OfSum

          do!
            FormConfig.ValidatePredicates ctx typeCheck configEntityType.Type entityType formConfig
            |> state.Map ignore
      }
      |> state.WithErrorContext $"...when validating launcher {formLauncher.LauncherName}"

  type FormApis<'ExprExtension, 'ValueExtension> with
    static member inline private extractTypes<'k, 'v when 'v: (static member Type: 'v -> ExprTypeId) and 'k: comparison>
      (m: Map<'k, 'v>)
      =
      m
      |> Map.values
      |> Seq.map (fun e -> e |> 'v.Type |> Set.singleton)
      |> Seq.fold (+) Set.empty

    static member GetTypesFreeVars(fa: FormApis<'ExprExtension, 'ValueExtension>) : Set<ExprTypeId> =
      FormApis.extractTypes fa.Enums
      + FormApis.extractTypes fa.Streams
      + FormApis.extractTypes (fa.Entities |> Map.map (fun _ -> fst))

  type EnumApi with
    static member Validate<'ExprExtension, 'ValueExtension>
      valueFieldName
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (enumApi: EnumApi)
      : Sum<Unit, Errors> =
      sum {
        let! enumType = ExprType.Find ctx.Types enumApi.TypeId
        let! enumType = ExprType.ResolveLookup ctx.Types enumType
        let! fields = ExprType.GetFields enumType

        let error =
          sum.Throw(
            $$"""Error: type {{enumType}} in enum {{enumApi.EnumName}} is invalid: expected only one field '{{valueFieldName}}' of type 'enum' but found {{fields}}"""
            |> Errors.Singleton
          )

        match fields with
        | [ (value, valuesType) ] when value = valueFieldName ->
          let! valuesType = ExprType.ResolveLookup ctx.Types valuesType
          let! cases = ExprType.GetCases valuesType

          if cases |> Map.values |> Seq.exists (fun case -> case.Fields.IsUnitType |> not) then
            return! error
          else
            return ()
        | _ -> return! error
      }
      |> sum.WithErrorContext $"...when validating enum {enumApi.EnumName}"

  type StreamApi with
    static member Validate<'ExprExtension, 'ValueExtension>
      (_: GeneratedLanguageSpecificConfig)
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (streamApi: StreamApi)
      : Sum<Unit, Errors> =
      sum {
        let! streamType = ExprType.Find ctx.Types streamApi.TypeId
        let! streamType = ExprType.ResolveLookup ctx.Types streamType
        let! fields = ExprType.GetFields streamType

        let error =
          sum.Throw(
            $$"""Error: type {{streamType}} in stream {{streamApi.StreamName}} is invalid: expected fields id:(entityIdUUID|entityIdString), displayValue:string but found {{fields}}"""
            |> Errors.Singleton
          )

        let! id, displayName = sum.All2 (fields |> sum.TryFindField "Id") (fields |> sum.TryFindField "DisplayValue")

        match id, displayName with
        | ExprType.PrimitiveType(PrimitiveType.EntityIdStringType),
          ExprType.PrimitiveType(PrimitiveType.CalculatedDisplayValueType)
        | ExprType.PrimitiveType(PrimitiveType.EntityIdUUIDType),
          ExprType.PrimitiveType(PrimitiveType.CalculatedDisplayValueType) -> return ()
        | _ -> return! error
      }
      |> sum.WithErrorContext $"...when validating stream {streamApi.StreamName}"

  type TableApi<'ExprExtension, 'ValueExtension> with
    static member Validate<'ExprExtension, 'ValueExtension>
      (_: GeneratedLanguageSpecificConfig)
      (codegen: CodeGenConfig)
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (tableApi: TableApi<'ExprExtension, 'ValueExtension> * Set<TableMethod>)
      : Sum<Unit, Errors> =
      sum {
        let tableApiFst = tableApi |> fst
        let! tableType = ExprType.Find ctx.Types tableApiFst.TypeId
        let! tableType = ExprType.ResolveLookup ctx.Types tableType
        let! fields = ExprType.GetFields tableType

        do!
          tableApiFst.Sorting
          |> Seq.map (fun sortableField -> fields |> sum.TryFindField sortableField |> sum.Map ignore)
          |> sum.All
          |> sum.Map ignore
          |> sum.WithErrorContext $"...when validating sorting"

        do!
          tableApiFst.Filters
          |> Map.map (fun filterableField filtering ->
            sum {
              do! fields |> sum.TryFindField filterableField |> sum.Map ignore

              do!
                NestedRenderer.Validate codegen ctx filtering.Type filtering.Display
                |> Sum.map ignore

              return ()
            }
            |> sum.WithErrorContext $"...when validating filterable field {filterableField}")
          |> sum.AllMap
          |> sum.Map ignore
          |> sum.WithErrorContext $"...when validating filters"


        let error =
          sum.Throw(
            $$"""Error: type {{tableType}} in table {{tableApiFst.TableName}} is invalid: expected field id:(entityIdUUID|entityIdString) but found {{fields}}"""
            |> Errors.Singleton
          )

        let! id = fields |> sum.TryFindField "Id"

        match id with
        | ExprType.PrimitiveType(PrimitiveType.EntityIdUUIDType)
        | ExprType.PrimitiveType(PrimitiveType.EntityIdStringType) -> return ()
        | _ -> return! error
      }
      |> sum.WithErrorContext $"...when validating table {(fst tableApi).TableName}"

  type LookupApi<'ExprExtension, 'ValueExtension> with
    static member GetIdType(lookupType: TypeBinding) : Sum<ExprType, Errors> =
      sum {
        let! (idType: ExprType) =
          sum {
            let! fields = lookupType.Type |> ExprType.AsRecord

            return!
              (sum.Any2
                (fields |> Map.tryFindWithError "id" "key" "id")
                (fields |> Map.tryFindWithError "Id" "key" "Id"))
              |> sum.MapError(Errors.WithPriority ErrorPriority.High)
          }
          |> sum.MapError Errors.HighestPriority

        match idType with
        | ExprType.PrimitiveType(PrimitiveType.EntityIdUUIDType) -> idType
        | ExprType.PrimitiveType(PrimitiveType.EntityIdStringType) -> idType
        | _ ->
          return!
            sum.Throw(
              Errors.Singleton
                $"Error: type {lookupType.TypeId.VarName} is expected to have an 'Id' field of type 'entityIdString' or 'entityIdUUID', but it has one of type '{idType}'."
            )
      }
      |> sum.WithErrorContext(sprintf "...when getting ID for %s" lookupType.TypeId.VarName)

  type LookupApi<'ExprExtension, 'ValueExtension> with
    static member Validate<'ExprExtension, 'ValueExtension>
      (_: GeneratedLanguageSpecificConfig)
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (lookupApi: LookupApi<'ExprExtension, 'ValueExtension>)
      : Sum<Unit, Errors> =
      sum {
        let! lookupType = ctx.TryFindType lookupApi.EntityName

        do! LookupApi.GetIdType lookupType |> Sum.map ignore
        return ()
      }

  type ParsedFormsContext<'ExprExtension, 'ValueExtension> with
    static member Validate
      codegenTargetConfig
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (typeCheck: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
      : State<Unit, CodeGenConfig, ValidationState, Errors> =
      state {
        do!
          sum.All(
            ctx.Apis.Enums
            |> Map.values
            |> Seq.map (fun enumApi ->
              EnumApi.Validate codegenTargetConfig.EnumValueFieldName ctx enumApi
              |> sum.WithErrorContext(sprintf "...when validating enum API for enum %s" enumApi.EnumName))
            |> Seq.toList
          )
          |> Sum.map ignore
          |> state.OfSum

        do!
          sum.All(
            ctx.Apis.Streams
            |> Map.values
            |> Seq.map (fun streamApi ->
              StreamApi.Validate codegenTargetConfig ctx streamApi
              |> sum.WithErrorContext(sprintf "...when validating stream API for stream %s" streamApi.StreamName))
            |> Seq.toList
          )
          |> Sum.map ignore
          |> state.OfSum

        let! codegenConfig = state.GetContext()

        do!
          sum.All(
            ctx.Apis.Tables
            |> Map.values
            |> Seq.map (fun tableApi ->
              TableApi.Validate codegenTargetConfig codegenConfig ctx tableApi
              |> sum.WithErrorContext(sprintf "...when validating table API for table %s" (tableApi |> fst).TableName))
            |> Seq.toList
          )
          |> Sum.map ignore
          |> state.OfSum

        do!
          sum.All(
            ctx.Apis.Lookups
            |> Map.values
            |> Seq.map (fun lookupApi ->
              LookupApi.Validate codegenTargetConfig ctx lookupApi
              |> sum.WithErrorContext(sprintf "...when validating lookup API for entity %s" lookupApi.EntityName))
            |> Seq.toList
          )
          |> Sum.map ignore
          |> state.OfSum

        // do System.Console.WriteLine(ctx.Forms.ToFSharpString)
        // do System.Console.ReadLine() |> ignore

        do!
          sum.All(
            ctx.Forms
            |> Map.values
            |> Seq.map (fun formConfig ->
              FormConfig.Validate codegenConfig ctx formConfig
              |> sum.WithErrorContext(
                sprintf
                  "...when validating form config for form %s"
                  (formConfig.FormName |> fun (FormName name) -> name)
              ))
            |> Seq.toList
          )
          |> Sum.map ignore
          |> state.OfSum

        for launcher in ctx.Launchers |> Map.values do
          do! FormLauncher.Validate ctx typeCheck launcher
      }
      |> state.WithErrorContext $"...when validating spec"
