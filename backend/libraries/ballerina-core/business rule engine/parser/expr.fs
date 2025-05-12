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

    static member ToName =
      BinaryOperator.ByName |> Map.toSeq |> Seq.map (fun (k, v) -> v, k) |> Map.ofSeq

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

    static member ToJson<'config, 'context>(expr: Expr) : Sum<JsonValue, Errors> =
      let (!) = Expr.ToJson

      sum {
        match expr with
        | Expr.Value value -> return! Value.ToJson value
        | Expr.Binary(op, l, r) ->
          let! jsonL = !l
          let! jsonR = !r

          let! operatorName =
            Map.tryFind op BinaryOperator.ToName
            |> Sum.fromOption (fun () -> Errors.Singleton $"No name for binary operator {op}")

          JsonValue.Record
            [| "kind", JsonValue.String operatorName
               "operands", JsonValue.Array [| jsonL; jsonR |] |]
        | Expr.MatchCase(expr, cases) ->
          let! jsonExpr = !expr

          let! jsonCases =
            cases
            |> Map.toList
            |> List.map (fun (caseName, (varName, body)) ->
              sum {
                let! jsonBody = !body

                return
                  JsonValue.Record
                    [| "caseName", JsonValue.String caseName
                       "handler",
                       JsonValue.Record
                         [| "kind", JsonValue.String "lambda"
                            "parameter", JsonValue.String varName.VarName
                            "body", jsonBody |] |]
              })
            |> sum.All

          JsonValue.Record
            [| "kind", JsonValue.String "matchCase"
               "operands", JsonValue.Array(Array.append [| jsonExpr |] (jsonCases |> List.toArray)) |]
        | Expr.Apply(func, arg) ->
          let! jsonFunc = !func
          let! jsonArg = !arg

          JsonValue.Record
            [| "kind", JsonValue.String "apply"
               "function", jsonFunc
               "argument", jsonArg |]
        | Expr.VarLookup varName ->
          JsonValue.Record
            [| "kind", JsonValue.String "varLookup"
               "varName", JsonValue.String varName.VarName |]
        | Expr.RecordFieldLookup(expr, fieldName) ->
          let! jsonExpr = !expr

          JsonValue.Record
            [| "kind", JsonValue.String "fieldLookup"
               "operands", JsonValue.Array [| jsonExpr; JsonValue.String fieldName |] |]
        | Expr.IsCase(caseName, expr) ->
          let! jsonExpr = !expr

          JsonValue.Record
            [| "kind", JsonValue.String "isCase"
               "operands", JsonValue.Array [| jsonExpr; JsonValue.String caseName |] |]
        | Expr.Project(expr, index) ->
          let! jsonExpr = !expr

          JsonValue.Record
            [| "kind", JsonValue.String "itemLookup"
               "operands", JsonValue.Array [| jsonExpr; JsonValue.Number(decimal index) |] |]
        | Expr.MakeRecord _ -> return! sum.Throw(Errors.Singleton "Error: MakeRecord not implemented")
        | Expr.MakeTuple _ -> return! sum.Throw(Errors.Singleton "Error: MakeTuple not implemented")
        | Expr.MakeSet _ -> return! sum.Throw(Errors.Singleton "Error: MakeSet not implemented")
        | Expr.MakeCase _ -> return! sum.Throw(Errors.Singleton "Error: MakeCase not implemented")
        | Expr.Exists _ -> return! sum.Throw(Errors.Singleton "Error: Exists not implemented")
        | Expr.SumBy _ -> return! sum.Throw(Errors.Singleton "Error: SumBy not implemented")
        | Expr.Unary _ -> return! sum.Throw(Errors.Singleton "Error: Unary not implemented")
        | Expr.FieldLookup _ -> return! sum.Throw(Errors.Singleton "Error: FieldLookup not implemented")

      }
      |> sum.MapError Errors.HighestPriority

  and Value with
    static member ToJson(value: Value) : Sum<JsonValue, Errors> =
      sum {
        match value with
        | Value.ConstBool b -> JsonValue.Boolean b
        | Value.ConstInt i -> JsonValue.Number(decimal i)
        | Value.ConstString s -> JsonValue.String s
        | Value.ConstGuid _ -> return! sum.Throw(Errors.Singleton "Error: ConstGuid not implemented")
        | Value.Unit -> return! sum.Throw(Errors.Singleton "Error: Unit not implemented")
        | Value.Lambda(parameter, body) ->
          let! jsonBody = Expr.ToJson body

          JsonValue.Record
            [| "kind", JsonValue.String "lambda"
               "parameter", JsonValue.String parameter.VarName
               "body", jsonBody |]
        | Value.CaseCons _ -> return! sum.Throw(Errors.Singleton "Error: CaseCons not implemented")
        | Value.Tuple _ -> return! sum.Throw(Errors.Singleton "Error: Tuple not implemented")
        | Value.Record _ -> return! sum.Throw(Errors.Singleton "Error: Record not implemented")
        | Value.ConstFloat _ -> return! sum.Throw(Errors.Singleton "Error: ConstFloat not implemented")
        | Value.Var _ -> return! sum.Throw(Errors.Singleton "Error: Var not implemented")
      }
