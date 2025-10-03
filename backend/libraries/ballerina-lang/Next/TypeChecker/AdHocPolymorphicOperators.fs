namespace Ballerina.DSL.Next.Types

module AdHocPolymorphicOperators =
  open Ballerina.StdLib.String
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.Errors
  open System
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Unification
  open Eval
  open Ballerina.Fun

  let adHocPolymorphismBinary =
    let operators =
      [ {| OperatorNames = Set.ofList [ "&&"; "||" ]
           Resolutions =
            [ {| MatchedInput = PrimitiveType.Bool
                 OtherInput = PrimitiveType.Bool
                 Output = PrimitiveType.Bool
                 Namespace = "Bool" |} ] |}
        {| OperatorNames = Set.ofList [ "=="; "!=" ]
           Resolutions =
            [ {| MatchedInput = PrimitiveType.Int32
                 OtherInput = PrimitiveType.Int32
                 Output = PrimitiveType.Bool
                 Namespace = "Int32" |}
              {| MatchedInput = PrimitiveType.Int64
                 OtherInput = PrimitiveType.Int64
                 Output = PrimitiveType.Bool
                 Namespace = "Int64" |}
              {| MatchedInput = PrimitiveType.Float32
                 OtherInput = PrimitiveType.Float32
                 Output = PrimitiveType.Bool
                 Namespace = "Float32" |}
              {| MatchedInput = PrimitiveType.Float64
                 OtherInput = PrimitiveType.Float64
                 Output = PrimitiveType.Bool
                 Namespace = "Float64" |}
              {| MatchedInput = PrimitiveType.TimeSpan
                 OtherInput = PrimitiveType.TimeSpan
                 Output = PrimitiveType.Bool
                 Namespace = "TimeSpan" |}
              {| MatchedInput = PrimitiveType.DateOnly
                 OtherInput = PrimitiveType.DateOnly
                 Output = PrimitiveType.Bool
                 Namespace = "DateOnly" |}
              {| MatchedInput = PrimitiveType.DateTime
                 OtherInput = PrimitiveType.DateTime
                 Output = PrimitiveType.Bool
                 Namespace = "DateTime" |}
              {| MatchedInput = PrimitiveType.Guid
                 OtherInput = PrimitiveType.Guid
                 Output = PrimitiveType.Bool
                 Namespace = "Guid" |}
              {| MatchedInput = PrimitiveType.String
                 OtherInput = PrimitiveType.String
                 Output = PrimitiveType.Bool
                 Namespace = "String" |} ] |}
        {| OperatorNames = Set.ofList [ ">"; "<"; ">="; "<=" ]
           Resolutions =
            [ {| MatchedInput = PrimitiveType.Int32
                 OtherInput = PrimitiveType.Int32
                 Output = PrimitiveType.Bool
                 Namespace = "Int32" |}
              {| MatchedInput = PrimitiveType.Int64
                 OtherInput = PrimitiveType.Int64
                 Output = PrimitiveType.Bool
                 Namespace = "Int64" |}
              {| MatchedInput = PrimitiveType.Float32
                 OtherInput = PrimitiveType.Float32
                 Output = PrimitiveType.Bool
                 Namespace = "Float32" |}
              {| MatchedInput = PrimitiveType.Float64
                 OtherInput = PrimitiveType.Float64
                 Output = PrimitiveType.Bool
                 Namespace = "Float64" |} ] |}
        {| OperatorNames = Set.ofList [ "+"; "-"; "*"; "/"; "%" ]
           Resolutions =
            [ {| MatchedInput = PrimitiveType.Int32
                 OtherInput = PrimitiveType.Int32
                 Output = PrimitiveType.Int32
                 Namespace = "Int32" |}
              {| MatchedInput = PrimitiveType.Int64
                 OtherInput = PrimitiveType.Int64
                 Output = PrimitiveType.Int64
                 Namespace = "Int64" |}
              {| MatchedInput = PrimitiveType.Float32
                 OtherInput = PrimitiveType.Float32
                 Output = PrimitiveType.Float32
                 Namespace = "Float32" |}
              {| MatchedInput = PrimitiveType.Float64
                 OtherInput = PrimitiveType.Float64
                 Output = PrimitiveType.Float64
                 Namespace = "Float64" |} ] |} ]

    seq {
      for op in operators do
        for name in op.OperatorNames do
          for resolution in op.Resolutions do
            yield (name, resolution.MatchedInput), resolution
    }
    |> Map.ofSeq

  let adHocPolymorphismBinaryAllOperatorNames =
    adHocPolymorphismBinary |> Map.keys |> Seq.map fst |> Set.ofSeq
