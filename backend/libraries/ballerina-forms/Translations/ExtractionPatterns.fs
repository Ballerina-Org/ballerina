namespace Ballerina.DSL.FormEngine.Translations

module ExtractionPatterns =

  open Ballerina.DSL.FormEngine.Model

  let LABEL_SCOPE_CHAR = ":"

  type FormConfig<'ExprExtension, 'ValueExtension> with
    static member AllLabels(formConfigs: seq<FormConfig<'ExprExtension, 'ValueExtension>>) =
      seq {
        for { FormName = FormName formName
              Body = body } in formConfigs do
          let labels =
            body
            |> FormBody.AllLabels
            |> Seq.map (fun (Label label) -> sprintf "%s%s%s" formName LABEL_SCOPE_CHAR label)

          yield! labels
      }

  and FormBody<'ExprExtension, 'ValueExtension> with
    static member AllLabels(f: FormBody<'ExprExtension, 'ValueExtension>) : seq<Label> =
      seq {
        match f with
        | FormBody.Annotated fs -> yield! fs.Renderer |> Renderer.AllLabels
        | FormBody.Table t ->
          for c in t.Columns |> Map.values do
            yield! c.FieldConfig |> FieldConfig.AllLabels

          for r in t.Details |> Option.toList do
            yield! r |> NestedRenderer.AllLabels

          yield! t.MethodLabels |> Map.values
      }

  and FieldConfig<'ExprExtension, 'ValueExtension> with
    static member AllLabels(f: FieldConfig<'ExprExtension, 'ValueExtension>) : seq<Label> =
      seq {
        yield! f.Label |> Option.toList |> List.toSeq
        yield! f.Renderer |> Renderer.AllLabels
      }

  and NestedRenderer<'ExprExtension, 'ValueExtension> with
    static member AllLabels(r: NestedRenderer<'ExprExtension, 'ValueExtension>) : seq<Label> =
      seq {
        yield! r.Label |> Option.toList |> List.toSeq
        yield! r.Renderer |> Renderer.AllLabels
      }

  and Renderer<'ExprExtension, 'ValueExtension> with
    static member AllTopLevelLabels(r: Renderer<'ExprExtension, 'ValueExtension>) =
      seq {
        match r with
        | Renderer.ListRenderer r -> yield! r.MethodLabels |> Map.values
        | _ -> yield! Array.empty
      }

  and Renderer<'ExprExtension, 'ValueExtension> with
    static member AllLabels(r: Renderer<'ExprExtension, 'ValueExtension>) : seq<Label> =
      let (!) = Renderer.AllTopLevelLabels
      let (!!!) = NestedRenderer.AllLabels
      let (!!!!) = FormBody.AllLabels


      seq {
        yield! !r

        match r with
        | Renderer.RecordRenderer r ->
          for (tabName, columns) in r.Fields.Tabs.FormTabs |> Map.toSeq do
            yield! columns.FormColumns |> Map.keys |> Seq.map Label
            yield Label tabName

          for f in r.Fields.Fields |> Map.values do
            yield! f |> FieldConfig.AllLabels
        | Renderer.UnionRenderer u ->
          for caseName, case in u.Cases |> Map.toSeq do
            yield!
              case
              |> NestedRenderer.AllLabels
              |> Seq.map (fun (Label label) -> sprintf "%s%s%s" caseName LABEL_SCOPE_CHAR label |> Label)
        | Renderer.EnumRenderer(_, l, _, _, _) -> yield! l |> Option.toArray
        | Renderer.OneRenderer r ->
          yield! r.Label |> Option.toArray
          yield! !!!r.Details

          match r.Preview with
          | Some p -> yield! !!!p
          | _ -> ()
        | Renderer.ManyRenderer(ManyAllRenderer r) ->
          yield! r.Label |> Option.toArray
          yield! !!!r.Element

        | Renderer.ManyRenderer(ManyLinkedUnlinkedRenderer r) ->
          yield! r.Label |> Option.toArray
          yield! !!!r.Linked

          match r.Unlinked with
          | Some unlinked -> yield! !!!unlinked
          | None -> ()

        | Renderer.ReadOnlyRenderer r ->
          yield! r.Label |> Option.toArray
          yield! !!!r.Value
        | Renderer.ListRenderer r ->
          yield! r.Label |> Option.toArray
          yield! !!!r.Element
        | Renderer.MapRenderer r ->
          yield! r.Label |> Option.toArray
          yield! !!!r.Key
          yield! !!!r.Value

        | Renderer.OptionRenderer r ->
          yield! r.Label |> Option.toArray
          yield! !!!r.None
          yield! !!!r.Some
        | Renderer.PrimitiveRenderer r -> yield! r.Label |> Option.toArray
        | Renderer.FormRenderer _
        | Renderer.TableFormRenderer _
        | Renderer.AllTranslationOverridesRenderer _ -> ()
        | Renderer.StreamRenderer(_, labelOption, _, _, _) -> yield! labelOption |> Option.toArray
        | Renderer.SumRenderer r ->
          yield! r.Label |> Option.toArray
          yield! !!!r.Left
          yield! !!!r.Right
        | Renderer.TupleRenderer r ->
          yield! r.Label |> Option.toArray

          for r in r.Elements do
            yield! !!!r
        | Renderer.InlineFormRenderer r -> yield! !!!!r
      }
