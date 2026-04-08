namespace Ballerina.DSL.Next.Types.TypeChecker

module QueryOperators =
  open Ballerina.Collections.Map
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Types.Model

  let binary_operators: Map<QueryIntrinsic, NonEmptyList<PrimitiveType>> =
    [ QueryIntrinsic.Multiply,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal ]
      )
      QueryIntrinsic.Divide,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal ]
      )
      QueryIntrinsic.Minus,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal ]
      )
      QueryIntrinsic.Modulo,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal ]
      )
      QueryIntrinsic.Plus,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal
          PrimitiveType.String ]
      )
      QueryIntrinsic.And,
      NonEmptyList.ofList<PrimitiveType> (PrimitiveType.Bool, [])
      QueryIntrinsic.Or,
      NonEmptyList.ofList<PrimitiveType> (PrimitiveType.Bool, []) ]
    |> List.map (fun (op, types) -> op, types)
    |> Map.ofList

  let comparison_operators =
    [ QueryIntrinsic.GreaterThan,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal
          PrimitiveType.TimeSpan
          PrimitiveType.DateOnly
          PrimitiveType.DateTime ]
      )
      QueryIntrinsic.LessThan,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal
          PrimitiveType.TimeSpan
          PrimitiveType.DateOnly
          PrimitiveType.DateTime ]
      )
      QueryIntrinsic.GreaterThanOrEqual,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal
          PrimitiveType.TimeSpan
          PrimitiveType.DateOnly
          PrimitiveType.DateTime ]
      )
      QueryIntrinsic.LessThanOrEqual,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal
          PrimitiveType.TimeSpan
          PrimitiveType.DateOnly
          PrimitiveType.DateTime ]
      )
      QueryIntrinsic.Equals,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal
          PrimitiveType.String
          PrimitiveType.Bool
          PrimitiveType.TimeSpan
          PrimitiveType.DateOnly
          PrimitiveType.DateTime
          PrimitiveType.Guid ]
      )
      QueryIntrinsic.NotEquals,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal
          PrimitiveType.String
          PrimitiveType.Bool
          PrimitiveType.TimeSpan
          PrimitiveType.DateOnly
          PrimitiveType.DateTime
          PrimitiveType.Guid ]
      ) ]
    |> List.map (fun (op, types) -> op, types)
    |> Map.ofList
