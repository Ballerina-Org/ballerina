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

  type NestedRenderer<'ExprExtension, 'ValueExtension> with
    static member Validate
      (codegen: CodeGenConfig)
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (formType: ExprType)
      (fr: NestedRenderer<'ExprExtension, 'ValueExtension>)
      : Sum<ExprType, Errors> =
      Renderer.Validate codegen ctx formType fr.Renderer

  and Renderer<'ExprExtension, 'ValueExtension> with
    static member Validate
      (codegen: CodeGenConfig)
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (formType: ExprType)
      (fr: Renderer<'ExprExtension, 'ValueExtension>)
      : Sum<ExprType, Errors> =
      let (!) = Renderer.Validate codegen ctx formType

      sum {
        match fr with
        | Renderer.Multiple(m) ->
          do! !m.First.NestedRenderer.Renderer |> Sum.map ignore

          do!
            (m.Rest |> Map.values)
            |> Seq.map (fun r -> !r.Renderer)
            |> sum.All
            |> Sum.map ignore

          do!
            (m.Rest |> Map.values)
            |> Seq.map (fun r ->
              ExprType.Unify
                Map.empty
                (ctx.Types |> Map.values |> Seq.map (fun v -> v.TypeId, v.Type) |> Map.ofSeq)
                r.Renderer.Type
                m.First.NestedRenderer.Type)
            |> sum.All
            |> Sum.map ignore


          return m.First.NestedRenderer.Type
        | Renderer.InlineFormRenderer i ->
          let formType = i.Body.Type

          let formType =
            if i.Body.IsTable |> not then
              formType
            else
              match formType with
              | ExprType.TableType row -> row
              | _ -> formType

          return! FormBody.Validate codegen ctx formType i.Body
        | Renderer.FormRenderer(_, _) -> return fr.Type
        | Renderer.TableFormRenderer(f, _, tableApiId) ->
          let! _ = ctx.TryFindForm f.FormName
          let! api = ctx.TryFindTableApi tableApiId.TableName
          let! apiRowType = api |> fst |> (fun a -> ctx.TryFindType a.TypeId.VarName)

          do!
            ExprType.Unify
              Map.empty
              (ctx.Types |> Map.values |> Seq.map (fun v -> v.TypeId, v.Type) |> Map.ofSeq)
              fr.Type
              (apiRowType.Type |> ExprType.TableType)
            |> Sum.map ignore

          return fr.Type
        | Renderer.OneRenderer(l) ->
          do! !l.One |> Sum.map ignore
          do! !l.Details.Renderer |> Sum.map ignore

          let! apiMethods =
            match l.OneApiId with
            | Choice2Of2(apiTypeId, apiName) ->
              sum {
                let! (_, oneApiMethods) = ctx.TryFindOne apiTypeId.VarName apiName
                return oneApiMethods
              }
            | Choice1Of2(_) -> sum { return Set.singleton CrudMethod.GetManyUnlinked }

          match l.Preview with
          | Some preview ->
            if apiMethods |> Set.contains CrudMethod.GetManyUnlinked |> not then
              return!
                sum.Throw(
                  Errors.Singleton
                    $"Error: api {l.OneApiId.ToFSharpString} is used in a preview but has no {CrudMethod.GetManyLinked.ToFSharpString} method."
                )
            else
              return ()

            do! !preview.Renderer |> Sum.map ignore
          | _ -> return ()

          return fr.Type
        | Renderer.ManyRenderer(l) ->
          do! !l.Many |> Sum.map ignore
          do! !l.Details.Renderer |> Sum.map ignore

          let (apiTypeId, apiName) = l.ManyApiId
          let! _, manyApiMethods = ctx.TryFindMany apiTypeId.VarName apiName

          match l.Preview with
          | Some preview ->
            if manyApiMethods |> Set.contains CrudMethod.GetManyUnlinked |> not then
              return!
                sum.Throw(
                  Errors.Singleton
                    $"Error: 'many' api {apiTypeId.VarName} - {apiName} is used in a preview but has no {CrudMethod.GetManyLinked.ToFSharpString} method."
                )
            else
              return ()

            do! !preview.Renderer |> Sum.map ignore
          | _ -> return ()

          return fr.Type
        | Renderer.ReadOnlyRenderer(l) ->
          do! !l.ReadOnly |> Sum.map ignore
          do! !l.Value.Renderer |> Sum.map ignore

          return fr.Type
        | Renderer.OptionRenderer(l) ->
          do! !l.Option |> Sum.map ignore
          do! !l.None.Renderer |> Sum.map ignore
          do! !l.Some.Renderer |> Sum.map ignore

          return fr.Type
        | Renderer.ListRenderer(l) ->
          do! !l.List |> Sum.map ignore
          do! !l.Element.Renderer |> Sum.map ignore

          return fr.Type
        // | Renderer.TableRenderer(t) ->
        //   do! !t.Table |> Sum.map ignore
        //   do! !t.Row.Renderer |> Sum.map ignore

        //   do! t.Children |> validateChildren

        //   return fr.Type
        | Renderer.MapRenderer(m) ->
          do! !m.Map |> Sum.map ignore
          do! !m.Key.Renderer |> Sum.map ignore
          do! !m.Value.Renderer |> Sum.map ignore

          return fr.Type
        | Renderer.SumRenderer(s) ->
          do! !s.Sum |> Sum.map ignore
          do! !s.Left.Renderer |> Sum.map ignore
          do! !s.Right.Renderer |> Sum.map ignore

          return fr.Type
        | Renderer.PrimitiveRenderer _ ->

          return fr.Type
        | Renderer.EnumRenderer(_, enumRenderer) ->
          do! !enumRenderer |> Sum.map ignore
          return fr.Type
        | Renderer.StreamRenderer(_, streamRenderer) ->

          do! !streamRenderer |> Sum.map ignore

          return fr.Type
        | Renderer.TupleRenderer t ->
          do! t.Elements |> Seq.map (fun e -> !e.Renderer) |> sum.All |> Sum.map ignore


          return fr.Type
      // | Renderer.UnionRenderer r ->

      //   do!
      //     r.Cases
      //     |> Seq.map (fun c -> !c.Value |> Sum.map ignore)
      //     |> sum.All
      //     |> Sum.map ignore

      //   return fr.Type
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
      let (!) =
        Renderer.ValidatePredicates validateFormConfigPredicates ctx typeCheck globalType rootType localType

      let (!!) =
        NestedRenderer.ValidatePredicates validateFormConfigPredicates ctx typeCheck globalType rootType localType

      state {
        match r with
        | Renderer.Multiple(m) ->
          do! !!m.First.NestedRenderer
          do! (m.Rest |> Map.values) |> Seq.map (!!) |> state.All |> state.Map ignore
        | Renderer.InlineFormRenderer i ->
          let formType = i.Body.Type

          let formType =
            if i.Body.IsTable |> not then
              formType
            else
              match formType with
              | ExprType.TableType row -> row
              | _ -> formType

          do! FormBody.ValidatePredicates ctx typeCheck globalType rootType formType i.Body
        | Renderer.PrimitiveRenderer _ -> return ()
        | Renderer.EnumRenderer(_, e) -> return! !e
        | Renderer.TupleRenderer e ->
          do! !e.Tuple

          for element in e.Elements do
            do! !!element

        | Renderer.OneRenderer e ->
          do! !e.One
          do! !!e.Details

          match e.Preview with
          | Some preview -> do! !!preview
          | _ -> return ()

        | Renderer.ManyRenderer e ->
          do! !e.Many
          do! !!e.Details

          match e.Preview with
          | Some preview -> do! !!preview
          | _ -> return ()

        | Renderer.OptionRenderer e ->
          do! !e.Option
          do! !!e.None
          do! !!e.Some

        | Renderer.ReadOnlyRenderer e ->
          do! !e.ReadOnly
          do! !!e.Value

        | Renderer.ListRenderer e ->
          do! !e.List
          do! !!e.Element

        // | Renderer.TableRenderer e ->
        //   do! !e.Table
        //   do! !!e.Row

        //   do! e.Children |> validateChildrenPredicates
        | Renderer.MapRenderer kv ->
          do! !kv.Map
          do! !!kv.Key
          do! !!kv.Value

        | Renderer.SumRenderer s ->
          do! !s.Sum
          do! !!s.Left
          do! !!s.Right

        | Renderer.StreamRenderer(_, e) -> return! !e
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
      (formType: ExprType)
      (fc: FieldConfig<'ExprExtension, 'ValueExtension>)
      : Sum<Unit, Errors> =
      sum {
        let! rendererType =
          Renderer.Validate codegen ctx formType fc.Renderer
          |> sum.WithErrorContext $"...when validating field config renderer for {fc.FieldName}"

        match formType with
        | RecordType fields ->
          match fields |> Map.tryFind fc.FieldName with
          | Some fieldType ->
            let! fieldType = ExprType.ResolveLookup ctx.Types fieldType

            do!
              ExprType.Unify
                Map.empty
                (ctx.Types |> Map.values |> Seq.map (fun v -> v.TypeId, v.Type) |> Map.ofSeq)
                rendererType
                fieldType
              |> Sum.map ignore

            return ()
          | None ->
            return!
              sum.Throw(Errors.Singleton(sprintf "Error: field name %A is not found in type %A" fc.FieldName formType))
        | _ -> return! sum.Throw(Errors.Singleton(sprintf "Error: form type %A is not a record type" formType))
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

        for tab in formFields.Tabs.FormTabs |> Map.values do
          for col in tab.FormColumns |> Map.values do
            for group in col.FormGroups |> Map.values do
              match group with
              | FormGroup.Computed e ->
                let vars =
                  [ ("global", globalType); ("root", rootType); ("local", localType) ]
                  |> Seq.map (VarName.Create <*> id)
                  |> Map.ofSeq

                let! eType =
                  typeCheck (ctx.Types |> Seq.map (fun tb -> tb.Value.TypeId, tb.Value.Type) |> Map.ofSeq) vars e
                  |> state.OfSum

                let! eTypeSetArg = ExprType.AsSet eType |> state.OfSum
                let! eTypeRefId = ExprType.AsLookupId eTypeSetArg |> state.OfSum

                let! eTypeRef =
                  ctx.Types
                  |> Map.tryFindWithError eTypeRefId.VarName "types" "types"
                  |> state.OfSum

                let! eTypeRefFields = ExprType.AsRecord eTypeRef.Type |> state.OfSum

                let! eTypeEnum = eTypeRefFields |> Map.tryFindWithError "Value" "fields" "fields" |> state.OfSum
                let! eTypeEnumId = ExprType.AsLookupId eTypeEnum |> state.OfSum

                let! eTypeEnum =
                  ctx.Types
                  |> Map.tryFindWithError eTypeEnumId.VarName "types" "types"
                  |> state.OfSum

                let! eTypeEnumCases = eTypeEnum.Type |> ExprType.AsUnion |> state.OfSum

                match eTypeEnumCases |> Seq.tryFind (fun c -> c.Value.Fields.IsUnitType |> not) with
                | Some nonUnitCaseFields ->
                  return!
                    state.Throw(
                      Errors.Singleton
                        $"Error: all cases of {eTypeEnum.TypeId.VarName} should be of type unit (ie the type is a proper enum), but {nonUnitCaseFields.Key} has type {nonUnitCaseFields.Value}"
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
                      $"Warning: the group provides fields {caseNames |> Seq.toList} but the form type has fields {fields |> Seq.toList}: fields {missingFields |> Seq.toList} are missing from the type and so enabling that field will have no effect!"

                  if missingCaseNames |> Set.isEmpty |> not then
                    warn
                      $"Warning: the group provides fields {caseNames |> Seq.toList} but the form type has fields {fields |> Seq.toList}: cases {missingCaseNames |> Seq.toList} are missing from the group and so toggling that field is not possible!"

                  return ()
              | _ -> return ()
      }

    static member Validate
      (codegen: CodeGenConfig)
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (rootType: ExprType)
      (body: FormFields<'ExprExtension, 'ValueExtension>)
      : Sum<Unit, Errors> =
      sum.All(
        body.Fields
        |> Map.values
        |> Seq.map (FieldConfig.Validate codegen ctx rootType)
        |> Seq.toList
      )
      |> Sum.map ignore

  and FormBody<'ExprExtension, 'ValueExtension> with
    static member Validate
      (codegen: CodeGenConfig)
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (localType: ExprType)
      (body: FormBody<'ExprExtension, 'ValueExtension>)
      : Sum<ExprType, Errors> =
      sum {
        match localType, body with
        | ExprType.UnionType typeCases, FormBody.Union formCases ->
          let typeCaseNames =
            typeCases |> Map.values |> Seq.map (fun c -> c.CaseName) |> Set.ofSeq

          let formCaseNames = formCases.Cases |> Map.keys |> Set.ofSeq

          let missingTypeCases = typeCaseNames - formCaseNames
          let missingFormCases = formCaseNames - typeCaseNames

          if missingTypeCases |> Set.isEmpty |> not then
            return! sum.Throw(Errors.Singleton $"Error: missing type cases {missingTypeCases.ToFSharpString}")
          elif missingFormCases |> Set.isEmpty |> not then
            return! sum.Throw(Errors.Singleton $"Error: missing form cases {missingFormCases.ToFSharpString}")
          else
            do!
              typeCases
              |> Seq.map (fun typeCase ->
                match formCases.Cases |> Map.tryFind typeCase.Key.CaseName with
                | None ->
                  sum.Throw(Errors.Singleton $"Error: cannot find form case for type case {typeCase.Key.CaseName}")
                | Some formCase -> NestedRenderer.Validate codegen ctx typeCase.Value.Fields formCase)
              |> sum.All
              |> Sum.map ignore

            return localType
        | ExprType.UnionType _, _ ->
          return!
            sum.Throw(
              Errors.Singleton $"Error: the form type is a union, expected cases in the body but found fields instead."
            )
        | _, FormBody.Record fields ->
          match fields.Renderer with
          | Some renderer ->
            let! rendererFields =
              codegen.Record.SupportedRenderers
              |> Map.tryFindWithError renderer "record renderer" renderer

            if rendererFields |> Set.isEmpty |> not then
              let renderedFields = fields.Fields.Fields |> Map.keys |> Set.ofSeq

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
          | _ -> return ()

          do! FormFields.Validate codegen ctx localType fields.Fields
          return localType
        | _, FormBody.Table table ->
          match table.Details with
          | Some details -> do! NestedRenderer.Validate codegen ctx localType details |> Sum.map ignore
          | None -> return ()

          // match table.Preview with
          // | Some preview -> do! FormBody.Validate codegen ctx localType preview |> Sum.map ignore
          // | None -> return ()

          do!
            sum.All(
              table.Columns
              |> Map.values
              |> Seq.map (fun c -> FieldConfig.Validate codegen ctx localType c.FieldConfig)
              |> Seq.toList
            )
            |> Sum.map ignore

          return ExprType.TableType localType

        | _ -> return! sum.Throw(Errors.Singleton $"Error: mismatched form type and form body")
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
        | FormBody.Record fields ->
          do! FormFields.ValidatePredicates ctx typeCheck globalType rootType localType fields.Fields
        | FormBody.Union cases ->
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

            let! eType =
              typeCheck (ctx.Types |> Seq.map (fun tb -> tb.Value.TypeId, tb.Value.Type) |> Map.ofSeq) vars visibleExpr
              |> state.OfSum

            let! eTypeSetArg = ExprType.AsSet eType |> state.OfSum
            let! eTypeRefId = ExprType.AsLookupId eTypeSetArg |> state.OfSum

            let! eTypeRef =
              ctx.Types
              |> Map.tryFindWithError eTypeRefId.VarName "types" "types"
              |> state.OfSum

            let! eTypeRefFields = ExprType.AsRecord eTypeRef.Type |> state.OfSum

            let! eTypeEnum = eTypeRefFields |> Map.tryFindWithError "Value" "fields" "fields" |> state.OfSum
            let! eTypeEnumId = ExprType.AsLookupId eTypeEnum |> state.OfSum

            let! eTypeEnum =
              ctx.Types
              |> Map.tryFindWithError eTypeEnumId.VarName "types" "types"
              |> state.OfSum

            let! eTypeEnumCases = eTypeEnum.Type |> ExprType.AsUnion |> state.OfSum

            match eTypeEnumCases |> Seq.tryFind (fun c -> c.Value.Fields.IsUnitType |> not) with
            | Some nonUnitCaseFields ->
              return!
                state.Throw(
                  Errors.Singleton
                    $"Error: all cases of {eTypeEnum.TypeId.VarName} should be of type unit (ie the type is a proper enum), but {nonUnitCaseFields.Key} has type {nonUnitCaseFields.Value}"
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

  and FormConfig<'ExprExtension, 'ValueExtension> with
    static member Validate
      (config: CodeGenConfig)
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (formConfig: FormConfig<'ExprExtension, 'ValueExtension>)
      : Sum<Unit, Errors> =
      sum {
        let formType = formConfig.Body |> FormBody.FormDeclarationType

        do! FormBody.Validate config ctx formType formConfig.Body |> Sum.map ignore

        match formConfig.ContainerRenderer with
        | Some containerName ->
          if config.ContainerRenderers |> Set.contains containerName |> not then
            return!
              sum.Throw(
                Errors.Singleton $"Error: {formConfig.FormName} uses non-existing container renderer {containerName}"
              )
          else
            return ()
        | None -> return ()
      }
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

          let formType = formConfig.Body |> FormBody.FormDeclarationType

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

        let formType = formConfig.Body |> FormBody.FormDeclarationType

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
              if
                Set.ofList [ CrudMethod.Create; CrudMethod.Default ]
                |> Set.isSuperset (entityApi |> snd)
              then
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
              if
                Set.ofList [ CrudMethod.Get; CrudMethod.Update ]
                |> Set.isSuperset (entityApi |> snd)
              then
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

          let entityType = (formConfig.Body |> FormBody.FormDeclarationType)

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

          let entityType = (formConfig.Body |> FormBody.FormDeclarationType)

          do!
            FormConfig.ValidatePredicates ctx typeCheck configEntityType.Type entityType formConfig
            |> state.Map ignore
      }
      |> state.WithErrorContext $"...when validating launcher {formLauncher.LauncherName}"

  type FormApis with
    static member inline private extractTypes<'k, 'v when 'v: (static member Type: 'v -> ExprTypeId) and 'k: comparison>
      (m: Map<'k, 'v>)
      =
      m
      |> Map.values
      |> Seq.map (fun e -> e |> 'v.Type |> Set.singleton)
      |> Seq.fold (+) Set.empty

    static member GetTypesFreeVars(fa: FormApis) : Set<ExprTypeId> =
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
            $$"""Error: type {{streamType}} in stream {{streamApi.StreamName}} is invalid: expected fields id:(Guid|string), displayValue:string but found {{fields}}"""
            |> Errors.Singleton
          )

        let! id, displayName = sum.All2 (fields |> sum.TryFindField "Id") (fields |> sum.TryFindField "DisplayValue")

        match id, displayName with
        | ExprType.PrimitiveType(PrimitiveType.GuidType), ExprType.PrimitiveType(PrimitiveType.StringType)
        | ExprType.PrimitiveType(PrimitiveType.StringType), ExprType.PrimitiveType(PrimitiveType.StringType) ->
          return ()
        | _ -> return! error
      }
      |> sum.WithErrorContext $"...when validating stream {streamApi.StreamName}"

  type TableApi with
    static member Validate<'ExprExtension, 'ValueExtension>
      (_: GeneratedLanguageSpecificConfig)
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (tableApi: TableApi * Set<TableMethod>)
      : Sum<Unit, Errors> =
      sum {
        let tableApiFst = tableApi |> fst
        let! tableType = ExprType.Find ctx.Types tableApiFst.TypeId
        let! tableType = ExprType.ResolveLookup ctx.Types tableType
        let! fields = ExprType.GetFields tableType

        let error =
          sum.Throw(
            $$"""Error: type {{tableType}} in table {{tableApiFst.TableName}} is invalid: expected field id:(Guid|string) but found {{fields}}"""
            |> Errors.Singleton
          )

        let! id = fields |> sum.TryFindField "Id"

        match id with
        | ExprType.PrimitiveType(PrimitiveType.GuidType)
        | ExprType.PrimitiveType(PrimitiveType.StringType) -> return ()
        | _ -> return! error
      }
      |> sum.WithErrorContext $"...when validating table {(fst tableApi).TableName}"

  type LookupApi with
    static member Validate<'ExprExtension, 'ValueExtension>
      (_: GeneratedLanguageSpecificConfig)
      (ctx: ParsedFormsContext<'ExprExtension, 'ValueExtension>)
      (lookupApi: LookupApi)
      : Sum<Unit, Errors> =
      sum {
        let! lookupType = ctx.TryFindType lookupApi.EntityName

        let! (idField: ExprType) =
          sum.Any2
            (sum {
              let! fields = lookupType.Type |> ExprType.AsRecord

              return!
                sum.Any2
                  (fields |> Map.tryFindWithError "id" "key" "id")
                  (fields |> Map.tryFindWithError "Id" "key" "Id")
            })
            (sum {
              let! fields = lookupType.Type |> ExprType.AsTuple

              return!
                fields
                |> Seq.tryHead
                |> Sum.fromOption (fun () -> Errors.Singleton "Error: cannot find first field in tuple")
            })

        match idField with
        | ExprType.PrimitiveType(PrimitiveType.GuidType)
        | ExprType.PrimitiveType(PrimitiveType.StringType) -> return ()
        | _ ->
          return!
            sum.Throw(
              Errors.Singleton
                $"Error: type {lookupApi.EntityName} is expected to have an 'Id' field of type 'string' or 'guid', but it has one of type '{idField}'."
            )
            |> Sum.map ignore

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
            |> Seq.map (EnumApi.Validate codegenTargetConfig.EnumValueFieldName ctx)
            |> Seq.toList
          )
          |> Sum.map ignore
          |> state.OfSum

        do!
          sum.All(
            ctx.Apis.Streams
            |> Map.values
            |> Seq.map (StreamApi.Validate codegenTargetConfig ctx)
            |> Seq.toList
          )
          |> Sum.map ignore
          |> state.OfSum

        do!
          sum.All(
            ctx.Apis.Tables
            |> Map.values
            |> Seq.map (TableApi.Validate codegenTargetConfig ctx)
            |> Seq.toList
          )
          |> Sum.map ignore
          |> state.OfSum

        do!
          sum.All(
            ctx.Apis.Lookups
            |> Map.values
            |> Seq.map (LookupApi.Validate codegenTargetConfig ctx)
            |> Seq.toList
          )
          |> Sum.map ignore
          |> state.OfSum

        // do System.Console.WriteLine(ctx.Forms.ToFSharpString)
        // do System.Console.ReadLine() |> ignore

        let! codegenConfig = state.GetContext()

        do!
          sum.All(
            ctx.Forms
            |> Map.values
            |> Seq.map (FormConfig.Validate codegenConfig ctx)
            |> Seq.toList
          )
          |> Sum.map ignore
          |> state.OfSum

        for launcher in ctx.Launchers |> Map.values do
          do! FormLauncher.Validate ctx typeCheck launcher
      }
      |> state.WithErrorContext $"...when validating spec"
