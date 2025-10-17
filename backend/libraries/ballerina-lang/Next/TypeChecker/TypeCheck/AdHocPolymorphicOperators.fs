namespace Ballerina.DSL.Next.Types.TypeChecker

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
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.Fun

  let adHocPolymorphismBinary =
    let operators =
      [ {| OperatorNames = Set.ofList [ "&&"; "||" ]
           Resolutions =
            [ {| MatchedInput = PrimitiveType.Bool
                 OtherInput = PrimitiveType.Bool
                 Output = PrimitiveType.Bool
                 Namespace = "bool" |} ] |}
        {| OperatorNames = Set.ofList [ "=="; "!=" ]
           Resolutions =
            [ {| MatchedInput = PrimitiveType.Int32
                 OtherInput = PrimitiveType.Int32
                 Output = PrimitiveType.Bool
                 Namespace = "int32" |}
              {| MatchedInput = PrimitiveType.Int64
                 OtherInput = PrimitiveType.Int64
                 Output = PrimitiveType.Bool
                 Namespace = "int64" |}
              {| MatchedInput = PrimitiveType.Float32
                 OtherInput = PrimitiveType.Float32
                 Output = PrimitiveType.Bool
                 Namespace = "float32" |}
              {| MatchedInput = PrimitiveType.Float64
                 OtherInput = PrimitiveType.Float64
                 Output = PrimitiveType.Bool
                 Namespace = "float64" |}
              {| MatchedInput = PrimitiveType.Decimal
                 OtherInput = PrimitiveType.Decimal
                 Output = PrimitiveType.Bool
                 Namespace = "decimal" |}
              {| MatchedInput = PrimitiveType.TimeSpan
                 OtherInput = PrimitiveType.TimeSpan
                 Output = PrimitiveType.Bool
                 Namespace = "timeSpan" |}
              {| MatchedInput = PrimitiveType.DateOnly
                 OtherInput = PrimitiveType.DateOnly
                 Output = PrimitiveType.Bool
                 Namespace = "dateOnly" |}
              {| MatchedInput = PrimitiveType.DateTime
                 OtherInput = PrimitiveType.DateTime
                 Output = PrimitiveType.Bool
                 Namespace = "dateTime" |}
              {| MatchedInput = PrimitiveType.Guid
                 OtherInput = PrimitiveType.Guid
                 Output = PrimitiveType.Bool
                 Namespace = "guid" |}
              {| MatchedInput = PrimitiveType.String
                 OtherInput = PrimitiveType.String
                 Output = PrimitiveType.Bool
                 Namespace = "string" |} ] |}
        {| OperatorNames = Set.ofList [ ">"; "<"; ">="; "<=" ]
           Resolutions =
            [ {| MatchedInput = PrimitiveType.Int32
                 OtherInput = PrimitiveType.Int32
                 Output = PrimitiveType.Bool
                 Namespace = "int32" |}
              {| MatchedInput = PrimitiveType.Int64
                 OtherInput = PrimitiveType.Int64
                 Output = PrimitiveType.Bool
                 Namespace = "int64" |}
              {| MatchedInput = PrimitiveType.String
                 OtherInput = PrimitiveType.String
                 Output = PrimitiveType.Bool
                 Namespace = "string" |}
              {| MatchedInput = PrimitiveType.DateTime
                 OtherInput = PrimitiveType.DateTime
                 Output = PrimitiveType.Bool
                 Namespace = "dateTime" |}
              {| MatchedInput = PrimitiveType.DateOnly
                 OtherInput = PrimitiveType.DateOnly
                 Output = PrimitiveType.Bool
                 Namespace = "dateOnly" |}
              {| MatchedInput = PrimitiveType.TimeSpan
                 OtherInput = PrimitiveType.TimeSpan
                 Output = PrimitiveType.Bool
                 Namespace = "timeSpan" |}
              {| MatchedInput = PrimitiveType.Decimal
                 OtherInput = PrimitiveType.Decimal
                 Output = PrimitiveType.Bool
                 Namespace = "decimal" |}
              {| MatchedInput = PrimitiveType.Float32
                 OtherInput = PrimitiveType.Float32
                 Output = PrimitiveType.Bool
                 Namespace = "float32" |}
              {| MatchedInput = PrimitiveType.Float64
                 OtherInput = PrimitiveType.Float64
                 Output = PrimitiveType.Bool
                 Namespace = "float64" |} ] |}
        {| OperatorNames = Set.ofList [ "+" ]
           Resolutions =
            [ {| MatchedInput = PrimitiveType.Int32
                 OtherInput = PrimitiveType.Int32
                 Output = PrimitiveType.Int32
                 Namespace = "int32" |}
              {| MatchedInput = PrimitiveType.Int64
                 OtherInput = PrimitiveType.Int64
                 Output = PrimitiveType.Int64
                 Namespace = "int64" |}
              {| MatchedInput = PrimitiveType.Float32
                 OtherInput = PrimitiveType.Float32
                 Output = PrimitiveType.Float32
                 Namespace = "float32" |}
              {| MatchedInput = PrimitiveType.Float64
                 OtherInput = PrimitiveType.Float64
                 Output = PrimitiveType.Float64
                 Namespace = "float64" |}
              {| MatchedInput = PrimitiveType.Decimal
                 OtherInput = PrimitiveType.Decimal
                 Output = PrimitiveType.Decimal
                 Namespace = "decimal" |}
              {| MatchedInput = PrimitiveType.String
                 OtherInput = PrimitiveType.String
                 Output = PrimitiveType.String
                 Namespace = "string" |} ] |}
        {| OperatorNames = Set.ofList [ "**" ]
           Resolutions =
            [ {| MatchedInput = PrimitiveType.Int32
                 OtherInput = PrimitiveType.Int32
                 Output = PrimitiveType.Int32
                 Namespace = "int32" |}
              {| MatchedInput = PrimitiveType.Int64
                 OtherInput = PrimitiveType.Int32
                 Output = PrimitiveType.Int64
                 Namespace = "int64" |}
              {| MatchedInput = PrimitiveType.Float32
                 OtherInput = PrimitiveType.Int32
                 Output = PrimitiveType.Float32
                 Namespace = "float32" |}
              {| MatchedInput = PrimitiveType.Float64
                 OtherInput = PrimitiveType.Int32
                 Output = PrimitiveType.Float64
                 Namespace = "float64" |}
              {| MatchedInput = PrimitiveType.Decimal
                 OtherInput = PrimitiveType.Int32
                 Output = PrimitiveType.Decimal
                 Namespace = "decimal" |} ] |}
        {| OperatorNames = Set.ofList [ "-"; "*"; "/"; "%" ]
           Resolutions =
            [ {| MatchedInput = PrimitiveType.Int32
                 OtherInput = PrimitiveType.Int32
                 Output = PrimitiveType.Int32
                 Namespace = "int32" |}
              {| MatchedInput = PrimitiveType.Int64
                 OtherInput = PrimitiveType.Int64
                 Output = PrimitiveType.Int64
                 Namespace = "int64" |}
              {| MatchedInput = PrimitiveType.Float32
                 OtherInput = PrimitiveType.Float32
                 Output = PrimitiveType.Float32
                 Namespace = "float32" |}
              {| MatchedInput = PrimitiveType.Decimal
                 OtherInput = PrimitiveType.Decimal
                 Output = PrimitiveType.Decimal
                 Namespace = "decimal" |}
              {| MatchedInput = PrimitiveType.DateOnly
                 OtherInput = PrimitiveType.DateOnly
                 Output = PrimitiveType.TimeSpan
                 Namespace = "dateOnly" |}
              {| MatchedInput = PrimitiveType.DateTime
                 OtherInput = PrimitiveType.DateTime
                 Output = PrimitiveType.TimeSpan
                 Namespace = "dateTime" |}
              {| MatchedInput = PrimitiveType.Float64
                 OtherInput = PrimitiveType.Float64
                 Output = PrimitiveType.Float64
                 Namespace = "float64" |} ] |} ]

    seq {
      for op in operators do
        for name in op.OperatorNames do
          for resolution in op.Resolutions do
            yield (name, resolution.MatchedInput), resolution
    }
    |> Map.ofSeq

  let adHocPolymorphismBinaryAllOperatorNames =
    adHocPolymorphismBinary |> Map.keys |> Seq.map fst |> Set.ofSeq
