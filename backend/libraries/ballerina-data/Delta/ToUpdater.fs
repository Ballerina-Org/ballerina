namespace Ballerina.Data.Delta

[<AutoOpen>]
module ToUpdater =
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Data.Delta.Model
  open Ballerina.Errors
  open Ballerina.Fun

  //type Value<'valueExtension> = Ballerina.DSL.Next.Terms.Model.Value<TypeValue, 'valueExtension>

  type Delta<'valueExtension, 'deltaExtension> with
    static member ToUpdater
      (deltaExtensionHandler:
        'deltaExtension
          -> Value<TypeValue<'valueExtension>, 'valueExtension>
          -> Sum<Value<TypeValue<'valueExtension>, 'valueExtension>, Errors>)
      (delta: Delta<'valueExtension, 'deltaExtension>)
      : Sum<
          Value<TypeValue<'valueExtension>, 'valueExtension>
            -> Sum<Value<TypeValue<'valueExtension>, 'valueExtension>, Errors>,
          Errors
         >
      =
      sum {
        match delta with
        | Multiple deltas ->
          let! updaters = deltas |> Seq.map (Delta.ToUpdater deltaExtensionHandler) |> sum.All

          return updaters |> List.fold (fun acc updater -> acc >> Sum.bind updater) Sum.Left

        | Replace v -> return replaceWith v >> sum.Return
        | Delta.Record(fieldName, fieldDelta) ->
          let! fieldUpdater = Delta.ToUpdater deltaExtensionHandler fieldDelta

          return
            fun (v: Value<TypeValue<'valueExtension>, 'valueExtension>) ->
              sum {
                let! fieldValues = Value.AsRecord v

                let! targetSymbol, currentValue =
                  fieldValues
                  |> Map.tryFindByWithError (fun (ts, _) -> ts.Name = fieldName) "field values" fieldName

                let! updatedValue = fieldUpdater currentValue

                return fieldValues |> Map.add targetSymbol updatedValue |> Value.Record
              }

        | Delta.Union(caseName, caseDelta) ->
          let! caseUpdater = caseDelta |> Delta.ToUpdater deltaExtensionHandler

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
          let! fieldUpdater = fieldDelta |> Delta.ToUpdater deltaExtensionHandler

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
          let! caseUpdater = caseDelta |> Delta.ToUpdater deltaExtensionHandler

          return
            fun v ->
              sum {
                let! valueCaseIndex, caseValue = v |> Value.AsSum

                if caseIndex <> valueCaseIndex.Case then
                  return v
                else
                  let! caseValue = caseUpdater caseValue
                  return Value.Sum(valueCaseIndex, caseValue)
              }
        | Delta.Ext ext -> return deltaExtensionHandler ext
      }
