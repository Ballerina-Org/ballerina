module Ballerina.Forms.Tests.TranslationsTests

open NUnit.Framework
open Ballerina.DSL.FormEngine.Model
open Ballerina.DSL.FormEngine.Translations.ExtractionPatterns
open Ballerina.DSL.Expr.Model

[<Test>]
let ``MapRenderer AllLabels returns all labels`` () =
  let mapRenderer: Renderer<unit, unit> =
    Renderer.MapRenderer
      { Label = Some(Label "Map Label")
        Map = RendererName "MapRendererName"
        Key =
          { Label = Some(Label "Key Label")
            Tooltip = None
            Details = None
            Renderer =
              Renderer.PrimitiveRenderer
                { PrimitiveRendererName = RendererName "KeyPrimitiveRendererName"
                  PrimitiveRendererId = 0
                  Type = ExprType.PrimitiveType PrimitiveType.StringType
                  Label = Some(Label "Inner Key Label") } }
        Value =
          { Label = Some(Label "Value Label")
            Tooltip = None
            Details = None
            Renderer =
              Renderer.PrimitiveRenderer
                { PrimitiveRendererName = RendererName "ValuePrimitiveRendererName"
                  PrimitiveRendererId = 0
                  Type = ExprType.PrimitiveType PrimitiveType.StringType
                  Label = Some(Label "Inner Value Label") } } }

  let labels = Renderer.AllLabels mapRenderer

  let expectedLabels: seq<Label> =
    seq {
      Label "Map Label"
      Label "Key Label"
      Label "Inner Key Label"
      Label "Value Label"
      Label "Inner Value Label"
    }

  Assert.That(labels, Is.EquivalentTo expectedLabels)
