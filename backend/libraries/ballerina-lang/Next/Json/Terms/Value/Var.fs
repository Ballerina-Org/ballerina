namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module Var =

  open Ballerina.Reader.WithError
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.Errors

  let private kindKey = "var"
  let private fieldKey = "name"

  type Var with
    static member FromJson =
      sum.AssertKindAndContinueWithField kindKey fieldKey (fun nameJson ->
        sum {
          let! name = nameJson |> JsonValue.AsString
          return name |> Var.Create
        })

  type Value<'T, 'valueExtension> with
    static member FromJsonVar(json: JsonValue) : ValueParserReader<'T, 'valueExtension> =
      json |> Var.FromJson |> sum.Map(Value.Var) |> reader.OfSum

    static member ToJsonVar: Var -> JsonValue =
      _.Name >> JsonValue.String >> Json.kind kindKey fieldKey
