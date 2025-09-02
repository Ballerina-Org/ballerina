namespace Ballerina.Data.Delta

module ToUpdater =
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Data.Delta.Model
  open Ballerina.Errors
  open Ballerina.Fun

  type Value = Ballerina.DSL.Next.Terms.Model.Value<TypeValue>

  type Delta with
    static member ToUpdater (valueType: TypeValue) (delta: Delta) : Sum<Value -> Sum<Value, Errors>, Errors> =
      sum {
        match delta with
        | Multiple deltas ->
          let! updaters = deltas |> Seq.map (Delta.ToUpdater valueType) |> sum.All

          return updaters |> List.fold (fun acc updater -> acc >> Sum.bind updater) Sum.Left

        | Replace v -> return replaceWith v >> sum.Return
        | Delta.Record(fieldName, fieldDelta) ->
          let! fields = TypeValue.AsRecord valueType

          let! _, fieldType =
            fields
            |> Map.tryFindByWithError (fun (ts, _) -> ts.Name = fieldName) "fields" fieldName

          let! fieldUpdater = Delta.ToUpdater fieldType fieldDelta

          return
            fun (v: Value) ->
              sum {
                let! fieldValues = Value.AsRecord v

                let! targetSymbol, currentValue =
                  fieldValues
                  |> Map.tryFindByWithError (fun (ts, _) -> ts.Name = fieldName) "field values" fieldName

                let! updatedValue = fieldUpdater currentValue

                return fieldValues |> Map.add targetSymbol updatedValue |> Value.Record
              }

        | Delta.Union(caseName, caseDelta) ->
          let! cases = valueType |> TypeValue.AsUnion

          let! _, caseType =
            cases
            |> Map.tryFindByWithError (fun (ts, _) -> ts.Name = caseName) "cases" caseName

          let! caseUpdater = caseDelta |> Delta.ToUpdater caseType

          return
            fun v ->
              sum {
                let! valueCaseName, caseValue = v |> Value.AsUnion

                if caseName <> valueCaseName.Name then
                  return v
                else
                  let! caseValue = caseUpdater caseValue
                  return Value.UnionCase(valueCaseName, caseValue)
              }
        | Delta.Tuple(fieldIndex, fieldDelta) ->
          let! fields = valueType |> TypeValue.AsTuple

          let! fieldType =
            fields
            |> List.tryItem fieldIndex
            |> Sum.fromOption (fun () -> Errors.Singleton $"Error: tuple does not have field at index {fieldIndex}")

          let! fieldUpdater = fieldDelta |> Delta.ToUpdater fieldType

          return
            fun v ->
              sum {
                let! fieldValues = v |> Value.AsTuple

                let! fieldValue =
                  fieldValues
                  |> List.tryItem fieldIndex
                  |> Sum.fromOption (fun () ->
                    Errors.Singleton $"Error: tuple does not have field at index {fieldIndex}")

                let! fieldValue = fieldUpdater fieldValue
                let fields = fieldValues |> List.updateAt fieldIndex fieldValue
                return Value.Tuple(fields)
              }
        | Delta.Sum(caseIndex, caseDelta) ->
          let! cases = valueType |> TypeValue.AsSum

          let! caseType =
            cases
            |> List.tryItem caseIndex
            |> Sum.fromOption (fun () -> Errors.Singleton $"Error: sum does not have case at index {caseIndex}")

          let! caseUpdater = caseDelta |> Delta.ToUpdater caseType

          return
            fun v ->
              sum {
                let! valueCaseIndex, caseValue = v |> Value.AsSum

                if caseIndex <> valueCaseIndex then
                  return v
                else
                  let! caseValue = caseUpdater caseValue
                  return Value.Sum(valueCaseIndex, caseValue)
              }
      }
