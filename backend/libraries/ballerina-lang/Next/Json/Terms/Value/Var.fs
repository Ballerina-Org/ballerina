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

  type Var with
    static member FromJson =
      sum.AssertKindAndContinueWithField "var" "name" (fun nameJson ->
        sum {
          let! name = nameJson |> JsonValue.AsString
          return name |> Var.Create
        })

  type Value<'T> with
    static member FromJsonVar: JsonValue -> ValueParser<'T> =
      fun json -> json |> Var.FromJson |> sum.Map(Value.Var) |> reader.OfSum

    static member ToJsonVar: Var -> JsonValue =
      _.Name >> JsonValue.String >> Json.kind "var" "name"
