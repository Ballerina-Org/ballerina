namespace Ballerina.DSL.Next.Json

open FSharp.Data
open Ballerina
open Ballerina.Collections.Sum
open Ballerina.StdLib.Json.Sum
open Ballerina.Errors
open Ballerina.Reader.WithError
open Ballerina.StdLib.Json.Reader

module Keys =


  module Sum =

    let assertDiscriminatorAndContinueWithValue<'T>
      (discriminatorValue: string)
      (k: JsonValue -> Sum<'T, Errors<unit>>)
      (json: JsonValue)
      : Sum<'T, Errors<unit>> =
      sum.AssertDiscriminatorAndContinueWithValue discriminatorKey valueKey discriminatorValue k json

    let assertDiscriminatorAndContinue<'T>
      (discriminatorValue: string)
      (k: Unit -> Sum<'T, Errors<unit>>)
      (json: JsonValue)
      : Sum<'T, Errors<unit>> =
      sum.AssertDiscriminatorAndContinue discriminatorKey discriminatorValue k json

  module Reader =

    let assertDiscriminatorAndContinueWithValue<'T, 'ctx>
      (discriminatorValue: string)
      (json: JsonValue)
      (k: JsonValue -> Reader<'T, 'ctx, Errors<unit>>)
      : Reader<'T, 'ctx, Errors<unit>> =
      reader.AssertDiscriminatorAndContinueWithValue discriminatorKey valueKey discriminatorValue json k

    let assertDiscriminatorAndContinue<'T, 'ctx>
      (discriminatorValue: string)
      (k: Unit -> Reader<'T, 'ctx, Errors<unit>>)
      (json: JsonValue)
      : Reader<'T, 'ctx, Errors<unit>> =
      reader.AssertDiscriminatorAndContinue discriminatorKey discriminatorValue k json
