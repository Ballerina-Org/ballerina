namespace Ballerina.DSL.Parser

module Expr =
  open Patterns

  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.Core.Json
  open Ballerina.Core.String
  open Ballerina.Core.Object
  open FSharp.Data
  open Ballerina.Collections.NonEmptyList

  type BinaryOperator with
    static member ByName =
      seq {
        "and", BinaryOperator.And
        "/", BinaryOperator.DividedBy
        "equals", BinaryOperator.Equals
        "=", BinaryOperator.Equals
        ">", BinaryOperator.GreaterThan
        ">=", BinaryOperator.GreaterThanEquals
        "-", BinaryOperator.Minus
        "or", BinaryOperator.Or
        "+", BinaryOperator.Plus
        "*", BinaryOperator.Times
      }
      |> Map.ofSeq

    static member AllNames = BinaryOperator.ByName |> Map.keys |> Set.ofSeq

  type Expr with
    static member ParseMatchCase<'config, 'context>
      (json: JsonValue)
      : State<string * VarName * Expr, 'config, 'context, Errors> =
      state {
        let! json = json |> JsonValue.AsRecord |> state.OfSum
        let! caseJson = json |> sum.TryFindField "caseName" |> state.OfSum

        return!
          state {
            let! caseName = caseJson |> JsonValue.AsString |> state.OfSum
            let! handlerJson = json |> sum.TryFindField "handler" |> state.OfSum
            let! handler = handlerJson |> Expr.Parse
            let! varName, body = handler |> Expr.AsLambda |> state.OfSum
            return caseName, varName, body
          }
          |> state.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member Parse<'config, 'context>(json: JsonValue) : State<Expr, 'config, 'context, Errors> =
      state {
        return!
          state.Any(
            NonEmptyList.OfList(
              state {
                let! v = JsonValue.AsBoolean json |> state.OfSum
                return v |> Value.ConstBool |> Expr.Value
              },
              [ state {
                  let! v = JsonValue.AsString json |> state.OfSum
                  return v |> Value.ConstString |> Expr.Value
                }
                state {
                  let! v = JsonValue.AsNumber json |> state.OfSum
                  return v |> int |> Value.ConstInt |> Expr.Value
                }
                state {
                  let! fieldsJson = JsonValue.AsRecord json |> state.OfSum

                  return!
                    state.Any(
                      NonEmptyList.OfList(
                        state {
                          let! kindJson = fieldsJson |> sum.TryFindField "kind" |> state.OfSum

                          let! operator = kindJson |> JsonValue.AsEnum BinaryOperator.AllNames |> state.OfSum

                          return!
                            state {
                              let! operandsJson = fieldsJson |> sum.TryFindField "operands" |> state.OfSum

                              let! (firstJson, secondJson) = JsonValue.AsPair operandsJson |> state.OfSum

                              let! first = Expr.Parse firstJson
                              let! second = Expr.Parse secondJson

                              let! operator =
                                BinaryOperator.ByName
                                |> Map.tryFindWithError operator "binary operator" operator
                                |> state.OfSum

                              return Expr.Binary(operator, first, second)
                            }
                            |> state.MapError(Errors.WithPriority ErrorPriority.High)
                        },
                        [ state {
                            let! kindJson = fieldsJson |> sum.TryFindField "kind" |> state.OfSum

                            do!
                              kindJson
                              |> JsonValue.AsEnum(Set.singleton "lambda")
                              |> state.OfSum
                              |> state.Map ignore

                            return!
                              state {
                                let! parameterJson = fieldsJson |> sum.TryFindField "parameter" |> state.OfSum

                                let! parameterName = parameterJson |> JsonValue.AsString |> state.OfSum

                                let! bodyJson = fieldsJson |> sum.TryFindField "body" |> state.OfSum

                                let! body = bodyJson |> Expr.Parse

                                return Expr.Value(Value.Lambda({ VarName = parameterName }, body))
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state {
                            let! kindJson = fieldsJson |> sum.TryFindField "kind" |> state.OfSum

                            do!
                              kindJson
                              |> JsonValue.AsEnum(Set.singleton "matchCase")
                              |> state.OfSum
                              |> state.Map ignore

                            return!
                              state {
                                let! operandsJson = fieldsJson |> sum.TryFindField "operands" |> state.OfSum

                                let! operandsJson = JsonValue.AsArray operandsJson |> state.OfSum

                                if operandsJson.Length < 1 then
                                  return!
                                    state.Throw(
                                      Errors.Singleton
                                        $"Error: matchCase needs at least one operand, the value to match. Instead, found zero operands."
                                    )
                                else
                                  let valueJson = operandsJson.[0]
                                  let! value = Expr.Parse valueJson
                                  let casesJson = operandsJson |> Seq.skip 1 |> Seq.toList

                                  let! cases = state.All(casesJson |> Seq.map (Expr.ParseMatchCase))

                                  let cases = cases |> Seq.map (fun (c, v, b) -> (c, (v, b))) |> Map.ofSeq

                                  return Expr.MatchCase(value, cases)
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state {
                            let! kindJson = fieldsJson |> sum.TryFindField "kind" |> state.OfSum

                            do!
                              kindJson
                              |> JsonValue.AsEnum(Set.singleton "fieldLookup")
                              |> state.OfSum
                              |> state.Map ignore

                            return!
                              state {
                                let! operandsJson = fieldsJson |> sum.TryFindField "operands" |> state.OfSum

                                let! (firstJson, fieldNameJson) = JsonValue.AsPair operandsJson |> state.OfSum

                                let! fieldName = JsonValue.AsString fieldNameJson |> state.OfSum

                                let! first = Expr.Parse firstJson
                                return Expr.RecordFieldLookup(first, fieldName)
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state {
                            let! kindJson = fieldsJson |> sum.TryFindField "kind" |> state.OfSum

                            do!
                              kindJson
                              |> JsonValue.AsEnum(Set.singleton "isCase")
                              |> state.OfSum
                              |> state.Map ignore

                            return!
                              state {
                                let! operandsJson = fieldsJson |> sum.TryFindField "operands" |> state.OfSum

                                let! (firstJson, caseNameJson) = JsonValue.AsPair operandsJson |> state.OfSum

                                let! caseName = JsonValue.AsString caseNameJson |> state.OfSum

                                let! first = Expr.Parse firstJson
                                return Expr.IsCase(caseName, first)
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state {
                            let! kindJson = fieldsJson |> sum.TryFindField "kind" |> state.OfSum

                            do!
                              kindJson
                              |> JsonValue.AsEnum(Set.singleton "varLookup")
                              |> state.OfSum
                              |> state.Map ignore

                            return!
                              state {
                                let! varNameJson = fieldsJson |> sum.TryFindField "varName" |> state.OfSum

                                let! varName = JsonValue.AsString varNameJson |> state.OfSum
                                return Expr.VarLookup { VarName = varName }
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state {
                            let! kindJson = fieldsJson |> sum.TryFindField "kind" |> state.OfSum

                            do!
                              kindJson
                              |> JsonValue.AsEnum(Set.singleton "itemLookup")
                              |> state.OfSum
                              |> state.Map ignore


                            return!
                              state {
                                let! operandsJson = fieldsJson |> sum.TryFindField "operands" |> state.OfSum

                                let! (firstJson, itemIndexJson) = JsonValue.AsPair operandsJson |> state.OfSum

                                let! itemIndex = JsonValue.AsNumber itemIndexJson |> state.OfSum

                                let! first = Expr.Parse firstJson
                                return Expr.Project(first, itemIndex |> int)
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state.Throw(
                            Errors.Singleton
                              $"Error: cannot parse expression {fieldsJson.ToFSharpString.ReasonablyClamped}."
                          ) ]
                      )
                    )
                }
                |> state.MapError(Errors.HighestPriority) ]
            )
          )
      }
      |> state.MapError(Errors.HighestPriority)
