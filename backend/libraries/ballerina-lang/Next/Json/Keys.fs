namespace Ballerina.DSL.Next.Json

open FSharp.Data
open Ballerina.Collections.Sum
open Ballerina.StdLib.Json.Sum
open Ballerina.Errors
open Ballerina.Reader.WithError
open Ballerina.StdLib.Json.Reader

module Keys =
  let discriminatorKey = "discriminator"
  let valueKey = "value"

  module Sum =

    let assertDiscriminatorAndContinueWithValue<'T>
      (discriminatorValue: string)
      (k: JsonValue -> Sum<'T, Errors>)
      (json: JsonValue)
      : Sum<'T, Errors> =
      sum.AssertDiscriminatorAndContinueWithValue discriminatorKey valueKey discriminatorValue k json

    let assertDiscriminatorAndContinue<'T>
      (discriminatorValue: string)
      (k: Unit -> Sum<'T, Errors>)
      (json: JsonValue)
      : Sum<'T, Errors> =
      sum.AssertDiscriminatorAndContinue discriminatorKey discriminatorValue k json

  module Reader =

    let assertDiscriminatorAndContinueWithValue<'T, 'ctx>
      (discriminatorValue: string)
      (json: JsonValue)
      (k: JsonValue -> Reader<'T, 'ctx, Errors>)
      : Reader<'T, 'ctx, Errors> =
      reader.AssertDiscriminatorAndContinueWithValue discriminatorKey valueKey discriminatorValue json k

    let assertDiscriminatorAndContinue<'T, 'ctx>
      (discriminatorValue: string)
      (k: Unit -> Reader<'T, 'ctx, Errors>)
      (json: JsonValue)
      : Reader<'T, 'ctx, Errors> =
      reader.AssertDiscriminatorAndContinue discriminatorKey discriminatorValue k json
