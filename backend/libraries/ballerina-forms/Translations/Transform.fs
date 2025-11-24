namespace Ballerina.DSL.FormEngine.Translations.Transform

module Transform =

  open Ballerina.DSL.FormEngine.Model
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.DSL.FormEngine.Translations.ExtractionPatterns
  open FSharp.Data
  open Ballerina.DSL.FormEngine.Parser.Model
  open Ballerina.DSL.FormEngine.Parser.Runner
  open Ballerina.DSL.Parser.Expr
  open Ballerina.Collections.Sum
  open Ballerina.Errors

  let processLabel (label: string) =
    let processed =
      System.Text.RegularExpressions.Regex.Replace(label, "[^a-zA-Z0-9_]", "_")

    if processed.Length > 0 then
      System.Char.ToUpper(processed.[0]).ToString() + processed.Substring(1)
    else
      processed

  let generateTypesJson (allTranslationsOverrideTypeBinding: TypeBinding) (labels: seq<string>) (keyType: ExprType) =
    let fieldsTypeArray =
      labels
      |> Seq.map (fun labelString ->
        let fieldValue =
          JsonValue.Record
            [| ("fun", JsonValue.String "TranslationOverride")
               ("args", JsonValue.Array [| ExprType.ToJson keyType; JsonValue.String labelString |]) |]

        (processLabel labelString, fieldValue))
      |> Seq.distinctBy fst
      |> Array.ofSeq

    JsonValue.Record(
      [| (allTranslationsOverrideTypeBinding.TypeId.VarName,
          JsonValue.Record [| "fields", JsonValue.Record fieldsTypeArray |]) |]
    )

  let generateFormsJson<'BLPExprExtension, 'BLPValueExtension>
    (translationOverrideFormBinding:
      Option<System.Collections.Generic.KeyValuePair<FormName, FormConfig<'BLPExprExtension, 'BLPValueExtension>>>)
    (labels: seq<string>)
    (allTranslationsOverrideTypeName: string)
    : Option<JsonValue> =
    match translationOverrideFormBinding with
    | Some formBinding ->
      match formBinding.Value.Body with
      | Annotated form ->
        match form.Renderer with
        | Renderer.AllTranslationOverridesRenderer renderer ->
          let (FormName formName) = formBinding.Key
          let (RendererName rendererName) = renderer.Renderer
          let (RendererName mapRenderer) = renderer.MapRenderer
          let (RendererName keyRenderer) = renderer.KeyRenderer
          let (RendererName valueRenderer) = renderer.ValueRenderer

          let fieldRenderersJson =
            labels
            |> Seq.map (fun labelString ->
              let fieldValue =
                JsonValue.Record
                  [| "renderer", JsonValue.String(mapRenderer)
                     "keyRenderer",
                     JsonValue.Record
                       [| ("renderer", JsonValue.String(keyRenderer))
                          ("options", JsonValue.String(renderer.Options)) |]
                     "valueRenderer", JsonValue.Record [| ("renderer", JsonValue.String(valueRenderer)) |] |]

              (processLabel labelString, fieldValue))
            |> Seq.distinctBy fst
            |> Array.ofSeq

          let tabsJson =
            JsonValue.Record
              [| "main",
                 JsonValue.Record
                   [| "columns",
                      JsonValue.Record
                        [| "main",
                           JsonValue.Record
                             [| "groups",
                                JsonValue.Record
                                  [| "main",
                                     JsonValue.Array(
                                       labels |> Seq.map (processLabel >> JsonValue.String) |> Array.ofSeq
                                     ) |] |] |] |] |]

          Some(
            JsonValue.Record
              [| formName,
                 JsonValue.Record
                   [| ("type", JsonValue.String allTranslationsOverrideTypeName)
                      ("renderer", JsonValue.String rendererName)
                      ("fields", JsonValue.Record fieldRenderersJson)
                      ("tabs", tabsJson) |] |]
          )
        | _ -> None
      | _ -> None
    | None -> None

  let TransformAllTranslationsOverides<'BLPExprExtension, 'BLPValueExtension>
    (primitivesExt: FormParserPrimitivesExtension<'BLPExprExtension, 'BLPValueExtension>)
    (exprParser: ExprParser<'BLPExprExtension, 'BLPValueExtension>)
    (generatedLanguageSpecificConfig: GeneratedLanguageSpecificConfig)
    (initialContext: ParsedFormsContext<'BLPExprExtension, 'BLPValueExtension>)
    (codegenConfig: CodeGenConfig)
    (mergedJson: TopLevel)
    : Sum<
        (TopLevel * Option<ParsedFormsContext<'BLPExprExtension, 'BLPValueExtension>>),
        (Errors * Option<ParsedFormsContext<'BLPExprExtension, 'BLPValueExtension>>)
       >
    =
    match
      initialContext.Types
      |> Seq.tryFind (fun kv ->
        match kv.Value.Type with
        | ExprType.AllTranslationOverrides _ -> true
        | _ -> false)
    with
    | Some entry ->
      let allTranslationsOverrideTypeName, allTranslationsOverrideTypeBinding =
        entry.Key, entry.Value

      let translationOverrideFormBinding =
        initialContext.Forms
        |> Seq.tryFind (fun kv ->
          match kv.Value.Body with
          | Annotated form ->
            match form.Renderer with
            | Renderer.AllTranslationOverridesRenderer _ -> true
            | _ -> false
          | _ -> false)

      let initialContextWithoutOverrides =
        { initialContext with
            Forms =
              match translationOverrideFormBinding with
              | Some formBinding -> initialContext.Forms |> Map.remove formBinding.Key
              | None -> initialContext.Forms
            Types = initialContext.Types |> Map.remove allTranslationsOverrideTypeName }

      let initialTypesJsonWithoutAllTranslations =
        JsonValue.Record(
          mergedJson.Types
          |> Array.filter (fun (name, _) -> name <> allTranslationsOverrideTypeName)
        )

      let initialFormsJsonWithoutTranslationOverride =
        match translationOverrideFormBinding with
        | Some formBinding ->
          let (FormName formName) = formBinding.Key
          mergedJson.Forms |> Array.filter (fun (name, _) -> name <> formName)
        | None -> mergedJson.Forms

      let initialJsonWithoutAllTranslations =
        JsonValue.Record(
          [| ("types", initialTypesJsonWithoutAllTranslations)
             ("apis",
              JsonValue.Record
                [| ("enumOptions", JsonValue.Record mergedJson.Enums)
                   ("searchableStreams", JsonValue.Record mergedJson.Streams)
                   ("entities", JsonValue.Record mergedJson.Entities)
                   ("tables", JsonValue.Record mergedJson.Tables)
                   ("lookups", JsonValue.Record mergedJson.Lookups) |])
             ("forms", JsonValue.Record initialFormsJsonWithoutTranslationOverride)
             ("launchers", JsonValue.Record mergedJson.Launchers) |]
        )

      match allTranslationsOverrideTypeBinding.Type with
      | ExprType.AllTranslationOverrides { KeyType = keyType } ->
        let labels = initialContext.Forms |> Map.values |> FormConfig.AllLabels |> Set.ofSeq
        let typesJson = generateTypesJson allTranslationsOverrideTypeBinding labels keyType

        let formsJson =
          generateFormsJson translationOverrideFormBinding labels allTranslationsOverrideTypeName

        let rootJson =
          match formsJson with
          | Some formJson -> JsonValue.Record([| ("types", typesJson); ("forms", formJson) |])
          | None -> JsonValue.Record([| ("types", typesJson) |])

        (ParsedFormsContext<'BLPExprExtension, 'BLPValueExtension>.Parse
          primitivesExt
          exprParser
          generatedLanguageSpecificConfig
          [ initialJsonWithoutAllTranslations; rootJson ])
          .run (codegenConfig, initialContextWithoutOverrides)
      | _ ->
        Right(Errors.Singleton "Error: translation override form is not an all translation overrides renderer", None)
    | None -> Left(mergedJson, Some initialContext)
