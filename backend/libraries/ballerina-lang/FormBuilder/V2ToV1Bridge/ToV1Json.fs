namespace Ballerina.DSL.FormBuilder.V2ToV1Bridge

module ToV1JSON =

  open Ballerina.DSL.FormBuilder.Model.FormAST
  open Ballerina.DSL.FormBuilder.V2ToV1Bridge.Types
  open Ballerina.DSL.FormBuilder.V2ToV1Bridge.Forms
  open Ballerina.DSL.Next.Types
  open Ballerina.Cat.Collections.OrderedMap
  open FSharp.Data
  open System.Text.Json

  let private generateLaunchers (forms: OrderedMap<FormIdentifier, Form<TypeValue<'valueExt>>>) : JsonValue =
    forms
    |> OrderedMap.toSeq
    |> Seq.filter (fun (_, form) -> form.IsEntryPoint)
    |> Seq.map (fun (formId, _) ->
      let (FormIdentifier formName) = formId

      let launcherJson =
        JsonValue.Record
          [| "kind", JsonValue.String "passthrough"
             "form", JsonValue.String formName
             "configType", JsonValue.String "EmptyConfig" |]

      (formName, launcherJson))
    |> Array.ofSeq
    |> JsonValue.Record

  type FormDefinitions<'typeValue> with
    static member toV1Json(formDefinitions: FormDefinitions<TypeValue<'valueExt>>) : JsonElement =
      let typesJson = Types.generateFormsTypeJson formDefinitions.Forms
      let formsJson = Forms.generateForms formDefinitions.Forms
      let launchersJson = generateLaunchers formDefinitions.Forms

      let rootJson =
        JsonValue.Record
          [| ("types", typesJson)
             ("apis", JsonValue.Record [||])
             ("forms", formsJson)
             ("launchers", launchersJson) |]

      JsonSerializer.Deserialize<JsonElement>(rootJson.ToString())
