namespace Ballerina.Data.Delta

module Patterns =
  open Ballerina.Errors
  open Ballerina.Collections.Sum
  open Ballerina.Data.Delta.Model
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Model

  type Delta<'valueExtension> with
    static member AsMultiple(delta: Delta<'valueExtension>) : Sum<List<Delta<'valueExtension>>, Errors> =
      match delta with
      | Multiple deltas -> deltas |> sum.Return
      | other -> sum.Throw(Errors.Singleton $"Expected a multiple delta but got {other}")

    static member AsReplace(delta: Delta<'valueExtension>) : Sum<Value<TypeValue, 'valueExtension>, Errors> =
      match delta with
      | Replace v -> sum.Return v
      | other -> sum.Throw(Errors.Singleton $"Expected a replace delta but got {other}")

    static member AsRecord(delta: Delta<'valueExtension>) : Sum<string * Delta<'valueExtension>, Errors> =
      match delta with
      | Delta.Record(fieldName, fieldDelta) -> sum.Return(fieldName, fieldDelta)
      | other -> sum.Throw(Errors.Singleton $"Expected a record delta but got {other}")

    static member AsUnion(delta: Delta<'valueExtension>) : Sum<string * Delta<'valueExtension>, Errors> =
      match delta with
      | Delta.Union(caseName, caseDelta) -> sum.Return(caseName, caseDelta)
      | other -> sum.Throw(Errors.Singleton $"Expected a union delta but got {other}")

    static member AsTuple(delta: Delta<'valueExtension>) : Sum<int * Delta<'valueExtension>, Errors> =
      match delta with
      | Delta.Tuple(fieldIndex, fieldDelta) -> sum.Return(fieldIndex, fieldDelta)
      | other -> sum.Throw(Errors.Singleton $"Expected a tuple delta but got {other}")

    static member AsSum(delta: Delta<'valueExtension>) : Sum<int * Delta<'valueExtension>, Errors> =
      match delta with
      | Delta.Sum(fieldIndex, fieldDelta) -> sum.Return(fieldIndex, fieldDelta)
      | other -> sum.Throw(Errors.Singleton $"Expected a sum delta but got {other}")
