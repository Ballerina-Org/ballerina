module Ballerina.Forms.Tests.FormValidatorTests

open NUnit.Framework
open Ballerina
open Ballerina.Collections.Sum
open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.FormEngine.Model
open Ballerina.DSL.FormEngine.Validator
open Ballerina.Errors

let private makeTypeBindings
  (bindings: (string * ExprType) list)
  : Map<string, TypeBinding> =
  bindings
  |> List.map (fun (name, t) ->
    name, TypeBinding.Create(ExprTypeId.Create name, t))
  |> Map.ofList

let private newContext<'E, 'V>
  (types: Map<string, TypeBinding>)
  (supportedRecordRenderers: Map<RendererName, Set<string>>)
  (languageStreamType: LanguageStreamType)
  : ParsedFormsContext<'E, 'V> =
  { Types = types
    Apis = FormApis<'E, 'V>.Empty
    Forms = Map.empty
    GenericRenderers = []
    SupportedRecordRenderers = supportedRecordRenderers
    LanguageStreamType = languageStreamType
    Launchers = Map.empty
    NextId = 0 }

[<Test>]
let ``Record fails when renderer expects exact fields`` () =
  let recordType =
    ExprType.RecordType(
      Map.ofList
        [ "x", ExprType.PrimitiveType PrimitiveType.IntType
          "y", ExprType.PrimitiveType PrimitiveType.IntType ]
    )

  let types = makeTypeBindings [ "R", recordType ]
  let rendererName = RendererName "RecRenderer"


  let field name t : FieldConfig<unit, unit> =
    { FieldName = name
      FieldId = 0
      Label = None
      Tooltip = None
      Details = None
      Renderer =
        Renderer.PrimitiveRenderer
          { PrimitiveRendererName = RendererName "Prim"
            PrimitiveRendererId = 0
            Type = t
            Label = None } }

  let body: FormBody<unit, unit> =
    FormBody.Annotated
      {| Renderer =
          Renderer.RecordRenderer
            { Renderer = Some rendererName
              Fields =
                { Fields =
                    Map.ofList
                      [ "x",
                        field "x" (ExprType.PrimitiveType PrimitiveType.IntType)
                        "y",
                        field "y" (ExprType.PrimitiveType PrimitiveType.IntType) ]
                  Disabled = FormGroup.Inlined []
                  DataContextFields = FormGroup.Inlined []
                  Tabs = { FormTabs = Map.empty } } }
         TypeId = ExprTypeId.Create "R" |}

  let ctx =
    newContext
      types
      (Map.ofList [ rendererName, Set.ofList [ "x" ] ])
      (LanguageStreamType "Language")

  let result = FormBody.Validate ctx body

  match result with
  | Left _ -> Assert.Fail("Expected failure but got success")
  | Right(errs: Errors<unit>) ->
    Assert.That(
      Errors.ToString(errs, "\n"),
      Contains.Substring "form renderer expects exactly fields"
    )

[<Test>]
let ``Record succeeds with matching fields and types`` () =
  let recordType =
    ExprType.RecordType(
      Map.ofList [ "x", ExprType.PrimitiveType PrimitiveType.IntType ]
    )

  let types = makeTypeBindings [ "R", recordType ]
  let rendererName = RendererName "RecRenderer"

  let fieldX: FieldConfig<unit, unit> =
    { FieldName = "x"
      FieldId = 0
      Label = None
      Tooltip = None
      Details = None
      Renderer =
        Renderer.PrimitiveRenderer
          { PrimitiveRendererName = RendererName "Prim"
            PrimitiveRendererId = 0
            Type = ExprType.PrimitiveType PrimitiveType.IntType
            Label = None } }

  let body: FormBody<unit, unit> =
    FormBody.Annotated
      {| Renderer =
          Renderer.RecordRenderer
            { Renderer = Some rendererName
              Fields =
                { Fields = Map.ofList [ "x", fieldX ]
                  Disabled = FormGroup.Inlined []
                  DataContextFields = FormGroup.Inlined []
                  Tabs = { FormTabs = Map.empty } } }
         TypeId = ExprTypeId.Create "R" |}

  let ctx =
    newContext
      types
      (Map.ofList [ rendererName, Set.ofList [ "x" ] ])
      (LanguageStreamType "Language")

  let result = FormBody.Validate ctx body

  match result with
  | Left et -> Assert.That(et, Is.EqualTo recordType)
  | Right e -> Assert.Fail($"Expected success but got error: {e}")

[<Test>]
let ``Record succeeds when field type is lookup and renderer matches underlying type``
  ()
  =
  let userIdType = ExprType.PrimitiveType PrimitiveType.IntType

  let recordType =
    ExprType.RecordType(
      Map.ofList [ "id", ExprType.LookupType(ExprTypeId.Create "UserId") ]
    )

  let recordTypeId = ExprTypeId.Create "TheRecord"

  let types =
    makeTypeBindings [ "UserId", userIdType; recordTypeId.VarName, recordType ]

  let recordType =
    ExprType.RecordType(
      Map.ofList [ "id", ExprType.LookupType(ExprTypeId.Create "UserId") ]
    )

  let fieldId: FieldConfig<unit, unit> =
    { FieldName = "id"
      FieldId = 0
      Label = None
      Tooltip = None
      Details = None
      Renderer =
        Renderer.PrimitiveRenderer
          { PrimitiveRendererName = RendererName "Prim"
            PrimitiveRendererId = 0
            Type = ExprType.PrimitiveType PrimitiveType.IntType
            Label = None } }

  let body: FormBody<unit, unit> =
    FormBody.Annotated
      {| Renderer =
          Renderer.RecordRenderer
            { Renderer = None
              Fields =
                { Fields = Map.ofList [ "id", fieldId ]
                  Disabled = FormGroup.Inlined []
                  DataContextFields = FormGroup.Inlined []
                  Tabs = { FormTabs = Map.empty } } }
         TypeId = recordTypeId |}

  let ctx = newContext types Map.empty (LanguageStreamType "Language")
  let result = FormBody.Validate ctx body

  match result with
  | Left et -> Assert.That(et, Is.EqualTo recordType)
  | Right e -> Assert.Fail($"Expected success but got error: {e}")

[<Test>]
let ``TranslationOverride succeeds with MapRenderer and proper key/value renderers``
  ()
  =
  let languageTypeId = ExprTypeId.Create "Language"

  let languageType =
    ExprType.RecordType(
      Map.ofList [ "key", ExprType.PrimitiveType PrimitiveType.StringType ]
    )


  let tOverride =
    ExprType.TranslationOverride
      { Label = "unused_label_key"
        KeyType = ExprType.LookupType languageTypeId }

  let typeId = ExprTypeId.Create "T"

  let types =
    makeTypeBindings
      [ languageTypeId.VarName, languageType; typeId.VarName, tOverride ]

  let mapRendererName = RendererName "MapR"

  let stringPrim: Renderer<_, _> =
    Renderer.PrimitiveRenderer
      { PrimitiveRendererName = RendererName "Prim"
        PrimitiveRendererId = 0
        Type = ExprType.PrimitiveType PrimitiveType.StringType
        Label = None }

  let nested (r: Renderer<unit, unit>) : NestedRenderer<unit, unit> =
    { Label = None
      Tooltip = None
      Details = None
      Renderer = r }

  let keyRenderer: Renderer<unit, unit> =
    Renderer.StreamRenderer(
      StreamRendererApi.Stream(
        StreamApi.Id(StreamApi.Create("KeyStream", languageTypeId))
      ),
      None,
      StreamRendererType.Option,
      languageTypeId,
      RendererName "streamKey"
    )

  let valueRenderer: Renderer<unit, unit> = stringPrim

  let body: FormBody<unit, unit> =
    FormBody.Annotated
      {| Renderer =
          Renderer.MapRenderer
            { Label = None
              Map = mapRendererName
              Key = nested keyRenderer
              Value = nested valueRenderer }
         TypeId = typeId |}

  let ctx =
    newContext types Map.empty (LanguageStreamType languageTypeId.VarName)

  let result = FormBody.Validate ctx body

  match result with
  | Left et -> Assert.That(et, Is.EqualTo tOverride)
  | Right e ->
    Assert.Fail(
      sprintf "Expected success but got error: %s" (Errors.ToString(e, "\n"))
    )

[<Test>]
let ``Record with inline form renderer fails when inline form has wrong type``
  ()
  =
  let nestedType = ExprType.PrimitiveType PrimitiveType.IntType

  let recordType = ExprType.RecordType(Map.ofList [ "nested", nestedType ])

  let types = makeTypeBindings [ "R", recordType; "Nested", nestedType ]

  let inlineFormBody: FormBody<unit, unit> =
    FormBody.Annotated
      {| Renderer =
          Renderer.PrimitiveRenderer
            { PrimitiveRendererName = RendererName "Prim"
              PrimitiveRendererId = 0
              Type = ExprType.PrimitiveType PrimitiveType.StringType // Wrong type!
              Label = None }
         TypeId = ExprTypeId.Create "Nested" |}

  let fieldWithInlineForm: FieldConfig<unit, unit> =
    { FieldName = "nested"
      FieldId = 0
      Label = None
      Tooltip = None
      Details = None
      Renderer = Renderer.InlineFormRenderer inlineFormBody }

  let body: FormBody<unit, unit> =
    FormBody.Annotated
      {| Renderer =
          Renderer.RecordRenderer
            { Renderer = None
              Fields =
                { Fields = Map.ofList [ "nested", fieldWithInlineForm ]
                  Disabled = FormGroup.Inlined []
                  DataContextFields = FormGroup.Inlined []
                  Tabs = { FormTabs = Map.empty } } }
         TypeId = ExprTypeId.Create "R" |}

  let ctx = newContext types Map.empty (LanguageStreamType "Language")
  let result = FormBody.Validate ctx body

  match result with
  | Left _ -> Assert.Fail("Expected failure but got success")
  | Right(errs: Errors<unit>) ->
    Assert.That(
      Errors.ToString(errs, "\n"),
      Contains.Substring "cannot be unified"
    )
