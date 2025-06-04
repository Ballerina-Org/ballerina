namespace Ballerina.DSL.Parser

module Expr =
  open Patterns

  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.Core.Json
  open Ballerina.Core.String
  open Ballerina.Core.Object
  open FSharp.Data
  open Ballerina.Collections.NonEmptyList

  let private assertKindIs expected kindJson =
    kindJson |> JsonValue.AsEnum(Set.singleton expected) |> Sum.map ignore

  let private assertKindIsAndGetFields expected json =
    sum {
      let! fieldsJson = JsonValue.AsRecord json
      let! kindJson = fieldsJson |> sum.TryFindField "kind"

      do! kindJson |> JsonValue.AsEnum(Set.singleton expected) |> Sum.map ignore

      fieldsJson
    }

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
    static member private ParseMatchCase(json: JsonValue) : Sum<string * VarName * Expr, Errors> =
      sum {
        let! json = json |> JsonValue.AsRecord
        let! caseJson = json |> sum.TryFindField "caseName"

        return!
          sum {
            let! caseName = caseJson |> JsonValue.AsString
            let! handlerJson = json |> sum.TryFindField "handler"
            let! handler = handlerJson |> Expr.Parse
            let! varName, body = handler |> Expr.AsLambda
            return caseName, varName, body
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseBinaryOperator(json: JsonValue) : Sum<Expr, Errors> =
      sum {
        let! fieldsJson = JsonValue.AsRecord json
        let! kindJson = fieldsJson |> sum.TryFindField "kind"
        let! operator = kindJson |> JsonValue.AsEnum BinaryOperator.AllNames

        return!
          sum {
            let! operandsJson = fieldsJson |> sum.TryFindField "operands"
            let! firstJson, secondJson = JsonValue.AsPair operandsJson
            let! first = Expr.Parse firstJson
            let! second = Expr.Parse secondJson

            let! operator =
              BinaryOperator.ByName
              |> Map.tryFindWithError operator "binary operator" operator

            Expr.Binary(operator, first, second)
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)

      }

    static member private ParseLambda(json: JsonValue) : Sum<Expr, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "lambda" json

        return!
          sum {

            let! parameterJson = fieldsJson |> sum.TryFindField "parameter"
            let! parameterName = parameterJson |> JsonValue.AsString
            let! bodyJson = fieldsJson |> sum.TryFindField "body"
            let! body = bodyJson |> Expr.Parse
            Expr.Value(Value.Lambda({ VarName = parameterName }, body))
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member ParseMatchCases(json: JsonValue) : Sum<Expr, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "matchCase" json

        return!
          sum {
            let! operandsJson = fieldsJson |> sum.TryFindField "operands"
            let! operandsJson = JsonValue.AsArray operandsJson

            if operandsJson.Length < 1 then
              return!
                sum.Throw(
                  Errors.Singleton
                    $"Error: matchCase needs at least one operand, the value to match. Instead, found zero operands."
                )
            else
              let valueJson = operandsJson.[0]
              let! value = Expr.Parse valueJson
              let casesJson = operandsJson |> Seq.skip 1 |> Seq.toList
              let! cases = sum.All(casesJson |> Seq.map (Expr.ParseMatchCase))
              let cases = cases |> Seq.map (fun (c, v, b) -> (c, (v, b))) |> Map.ofSeq
              return Expr.MatchCase(value, cases)
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseFieldLookup(json: JsonValue) : Sum<Expr, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "fieldLookup" json

        return!
          sum {
            let! operandsJson = fieldsJson |> sum.TryFindField "operands"
            let! firstJson, fieldNameJson = JsonValue.AsPair operandsJson
            let! fieldName = JsonValue.AsString fieldNameJson
            let! first = Expr.Parse firstJson
            return Expr.RecordFieldLookup(first, fieldName)
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseVarLookup(json: JsonValue) : Sum<Expr, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "varLookup" json

        return!
          sum {
            let! varNameJson = fieldsJson |> sum.TryFindField "varName"
            let! varName = JsonValue.AsString varNameJson
            return Expr.VarLookup { VarName = varName }
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseItemLookup(json: JsonValue) : Sum<Expr, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "itemLookup" json

        return!
          sum {
            let! operandsJson = fieldsJson |> sum.TryFindField "operands"
            let! firstJson, itemIndexJson = JsonValue.AsPair operandsJson
            let! itemIndex = JsonValue.AsNumber itemIndexJson
            let! first = Expr.Parse firstJson
            return Expr.Project(first, itemIndex |> int)
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member Parse(json: JsonValue) : Sum<Expr, Errors> =
      sum.Any(
        NonEmptyList.OfList(
          (Value.Parse >> Sum.map Expr.Value) json,
          [ Expr.ParseBinaryOperator json
            Expr.ParseLambda json
            Expr.ParseMatchCases json
            Expr.ParseFieldLookup json
            Expr.ParseVarLookup json
            Expr.ParseItemLookup json
            sum.Throw(Errors.Singleton $"Error: cannot parse expression {json.ToFSharpString.ReasonablyClamped}.") ]
        )
      )
      |> sum.MapError Errors.HighestPriority

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
        | Expr.Project(expr, index) ->
          let! jsonExpr = !expr

          JsonValue.Record
            [| "kind", JsonValue.String "itemLookup"
               "operands", JsonValue.Array [| jsonExpr; JsonValue.Number(decimal index) |] |]
        | Expr.MakeRecord _ -> return! sum.Throw(Errors.Singleton "Error: MakeRecord not implemented")
        | Expr.MakeTuple _ -> return! sum.Throw(Errors.Singleton "Error: MakeTuple not implemented")
        | Expr.MakeSet _ -> return! sum.Throw(Errors.Singleton "Error: MakeSet not implemented")
        | Expr.MakeCase _ -> return! sum.Throw(Errors.Singleton "Error: MakeCase not implemented")
        | Expr.Unary _ -> return! sum.Throw(Errors.Singleton "Error: Unary not implemented")

      }
      |> sum.MapError Errors.HighestPriority

  and Value with

    static member private ParseBool(json: JsonValue) : Sum<Value, Errors> =
      sum {
        let! v = JsonValue.AsBoolean json
        return Value.ConstBool v
      }

    static member private ParseIntForBackwardCompatibility(json: JsonValue) : Sum<Value, Errors> =
      sum {
        let! v = JsonValue.AsNumber json
        return Value.ConstInt(int v)
      }

    static member private ParseString(json: JsonValue) : Sum<Value, Errors> =
      sum {
        let! v = JsonValue.AsString json
        return Value.ConstString v
      }

    static member private ParseUnit(json: JsonValue) : Sum<Value, Errors> =
      sum {
        let! fieldsJson = JsonValue.AsRecord json
        let! kindJson = fieldsJson |> sum.TryFindField "kind"
        do! assertKindIs "unit" kindJson
        return Value.Unit
      }

    static member private ParseRecord(json: JsonValue) : Sum<Value, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "record" json

        return!
          sum {
            let! fieldsJson = fieldsJson |> sum.TryFindField "fields"
            let! fieldAsRecord = fieldsJson |> JsonValue.AsRecord

            let! fieldValues =
              fieldAsRecord
              |> List.ofArray
              |> List.map (fun (name, valueJson) ->
                sum {
                  let! value = Value.Parse valueJson
                  return name, value
                })
              |> sum.All

            fieldValues |> Map.ofList |> Value.Record
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseCaseCons(json: JsonValue) : Sum<Value, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "caseCons" json

        return!
          sum {
            let! caseJson = fieldsJson |> sum.TryFindField "case"
            let! valueJson = fieldsJson |> sum.TryFindField "value"
            let! case = JsonValue.AsString caseJson
            let! value = Value.Parse valueJson
            Value.CaseCons(case, value)
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseTuple(json: JsonValue) : Sum<Value, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "tuple" json

        return!
          sum {
            let! elementsJson = fieldsJson |> sum.TryFindField "elements"
            let! elementsArray = elementsJson |> JsonValue.AsArray
            let! elements = elementsArray |> Array.toList |> List.map Value.Parse |> sum.All
            Value.Tuple elements
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseInt(json: JsonValue) : Sum<Value, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "int" json

        return!
          sum {
            let! valueJson = fieldsJson |> sum.TryFindField "value"
            let! value = JsonValue.AsString valueJson

            match System.Int32.TryParse value with
            | true, v -> Value.ConstInt v
            | false, _ -> return! sum.Throw(Errors.Singleton $"Error: could not parse {value} as int")
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseFloat(json: JsonValue) : Sum<Value, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "float" json

        return!
          sum {
            let! valueJson = fieldsJson |> sum.TryFindField "value"
            let! value = JsonValue.AsString valueJson

            match System.Decimal.TryParse value with
            | true, v -> return Value.ConstFloat v
            | false, _ -> return! sum.Throw(Errors.Singleton $"Error: could not parse {value} as float")
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseList(json: JsonValue) : Sum<Value, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "list" json

        return!
          sum {
            let! elementsJson = fieldsJson |> sum.TryFindField "elements"
            let! elementsArray = elementsJson |> JsonValue.AsArray
            let! elements = elementsArray |> Array.toList |> List.map Value.Parse |> sum.All
            Value.List elements
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member Parse(json: JsonValue) : Sum<Value, Errors> =
      sum.Any(
        NonEmptyList.OfList(
          Value.ParseBool,
          [ Value.ParseIntForBackwardCompatibility
            Value.ParseString
            Value.ParseUnit
            Value.ParseRecord
            Value.ParseCaseCons
            Value.ParseTuple
            Value.ParseInt
            Value.ParseFloat
            Value.ParseList ]
        )
        |> NonEmptyList.map (fun f -> f json)
      )

    static member ToJson(value: Value) : Sum<JsonValue, Errors> =
      sum {
        match value with
        | Value.ConstBool b -> JsonValue.Boolean b
        | Value.ConstInt i ->
          JsonValue.Record [| "kind", JsonValue.String "int"; "value", JsonValue.String(i.ToString()) |]
        | Value.ConstFloat value ->
          JsonValue.Record
            [| "kind", JsonValue.String "float"
               "value", JsonValue.String(value.ToString()) |]
        | Value.ConstString s -> JsonValue.String s
        | Value.ConstGuid _ -> return! sum.Throw(Errors.Singleton "Error: ConstGuid not implemented")
        | Value.Unit -> JsonValue.Record [| "kind", JsonValue.String "unit" |]
        | Value.Lambda(parameter, body) ->
          let! jsonBody = Expr.ToJson body

          JsonValue.Record
            [| "kind", JsonValue.String "lambda"
               "parameter", JsonValue.String parameter.VarName
               "body", jsonBody |]
        | Value.CaseCons(case, value) ->
          let! jsonValue = Value.ToJson value

          JsonValue.Record
            [| "kind", JsonValue.String "caseCons"
               "case", JsonValue.String case
               "value", jsonValue |]
        | Value.Tuple elements ->
          let! jsonElements = elements |> List.map Value.ToJson |> sum.All

          JsonValue.Record
            [| "kind", JsonValue.String "tuple"
               "elements", jsonElements |> Array.ofList |> JsonValue.Array |]
        | Value.Record fields ->
          let! jsonFields =
            fields
            |> Map.toList
            |> List.map (fun (fieldName, fieldValue) ->
              sum {
                let! jsonValue = Value.ToJson fieldValue
                fieldName, jsonValue
              })
            |> sum.All

          JsonValue.Record
            [| "kind", JsonValue.String "record"
               "fields", jsonFields |> Array.ofList |> JsonValue.Record |]
        | Value.Var _ -> return! sum.Throw(Errors.Singleton "Error: Var not implemented")
        | Value.List elements ->
          let! jsonElements = elements |> List.map Value.ToJson |> sum.All

          JsonValue.Record
            [| "kind", JsonValue.String "list"
               "elements", jsonElements |> Array.ofList |> JsonValue.Array |]
      }
