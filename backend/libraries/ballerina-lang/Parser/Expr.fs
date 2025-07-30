namespace Ballerina.DSL.Parser

module Expr =
  open Patterns

  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Patterns
  open Ballerina.DSL.Parser.ExprType
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open FSharp.Data
  open Ballerina.Collections.NonEmptyList
  open Ballerina.State.WithError
  open System

  type Parse<'ExprExtension, 'ValueExtension> = JsonValue -> Sum<Expr<'ExprExtension, 'ValueExtension>, Errors>

  let assertKindIs expected kindJson =
    kindJson |> JsonValue.AsEnum(Set.singleton expected) |> Sum.map ignore

  let assertKindIsAndGetFields expected json =
    sum {
      let! fieldsJson = JsonValue.AsRecord json
      let! kindJson = fieldsJson |> sum.TryFindField "kind"

      do! kindJson |> JsonValue.AsEnum(Set.singleton expected) |> Sum.map ignore

      fieldsJson
    }

  type ExprParser<'ExprExtension, 'ValueExtension> = JsonValue -> Sum<Expr<'ExprExtension, 'ValueExtension>, Errors>
  type ValueParser<'ExprExtension, 'ValueExtension> = JsonValue -> Sum<Value<'ExprExtension, 'ValueExtension>, Errors>

  type Expr<'ExprExtension, 'ValueExtension> with
    static member private ParseMatchCase
      (parseExpr: ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : Sum<string * VarName * Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! json = json |> JsonValue.AsRecord
        let! caseJson = json |> sum.TryFindField "caseName"

        return!
          sum {
            let! caseName = caseJson |> JsonValue.AsString
            let! handlerJson = json |> sum.TryFindField "handler"

            // Validate that handler is a lambda with string parameter (no type annotations)
            let! handlerFields = handlerJson |> JsonValue.AsRecord
            let! kindJson = handlerFields |> sum.TryFindField "kind"
            let! kind = kindJson |> JsonValue.AsString

            if kind = "lambda" then
              let! parameterJson = handlerFields |> sum.TryFindField "parameter"

              // Check if parameter is an object (which would indicate type annotations)
              match parameterJson with
              | JsonValue.Record _ ->
                return!
                  sum.Throw(
                    Errors.Singleton
                      $"Error: match case handlers cannot have type annotations. Handler for case '{caseName}' has typed parameter."
                  )
              | _ ->
                // Parameter is a string, proceed with normal parsing
                let! handler = handlerJson |> parseExpr
                let! varName, _varType, _returnType, body = handler |> Expr.AsLambda
                return caseName, varName, body
            else
              // Not a lambda, proceed with normal parsing
              let! handler = handlerJson |> parseExpr
              let! varName, _varType, _returnType, body = handler |> Expr.AsLambda
              return caseName, varName, body
          }
          |> sum.MapError(Errors.WithPriority High)
      }

    static member private ParseApplication
      (parseExpr: ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "apply" json

        return!
          sum {

            let! functionJson = fieldsJson |> sum.TryFindField "function"
            let! functionValue = functionJson |> parseExpr
            let! argumentJson = fieldsJson |> sum.TryFindField "argument"
            let! argument = argumentJson |> parseExpr
            Expr.Apply(functionValue, argument)
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseGenericApplication
      (parseExpr: ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "Apply" json

        return!
          sum {

            let! functionJson = fieldsJson |> sum.TryFindField "function"
            let! functionValue = functionJson |> parseExpr
            let! argumentJson = fieldsJson |> sum.TryFindField "argument"

            let! argument = argumentJson |> ExprType.Parse


            Expr.GenericApply(functionValue, argument)
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseGenericLambda
      (parseExpr: ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "Lambda" json

        return!
          sum {

            let! parameterJson = fieldsJson |> sum.TryFindField "parameter"
            let! parameterName = parameterJson |> JsonValue.AsString
            let! bodyJson = fieldsJson |> sum.TryFindField "body"
            let! body = bodyJson |> parseExpr
            Expr.Value(Value.GenericLambda({ VarName = parameterName }, body))
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseLambda
      (parseExpr: ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "lambda" json

        return!
          sum {
            let! parameterJson = fieldsJson |> sum.TryFindField "parameter"

            let! parameterName, parameterType =
              sum.Any2
                (sum {
                  let! parameterFields = parameterJson |> JsonValue.AsRecord
                  let! parameterName = parameterFields |> sum.TryFindField "name"
                  let! parameterName = parameterName |> JsonValue.AsString
                  let! parameterType = parameterFields |> sum.TryFindField "type"
                  return parameterName, Some parameterType
                })
                (sum {
                  let! parameterName = parameterJson |> JsonValue.AsString
                  return parameterName, None
                })

            let! parameterType =
              match parameterType with
              | Some parameterType -> parameterType |> ExprType.Parse |> sum.Map Some
              | None -> sum.Return None

            // Parse optional return type annotation
            let! returnType =
              sum.Any2
                (sum {
                  let! returnsJson = fieldsJson |> sum.TryFindField "returnType"
                  let! parsedReturnType = returnsJson |> ExprType.Parse
                  return Some parsedReturnType
                })
                (sum.Return None)

            let! bodyJson = fieldsJson |> sum.TryFindField "body"
            let! body = bodyJson |> parseExpr
            Expr.Value(Value.Lambda({ VarName = parameterName }, parameterType, returnType, body))
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }


    static member private ParseLet
      (parseExpr: ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "let" json

        return!
          sum {

            let! varJson = fieldsJson |> sum.TryFindField "varName"
            let! varName = varJson |> JsonValue.AsString
            let! exprJson = fieldsJson |> sum.TryFindField "expr"
            let! expr = exprJson |> parseExpr
            let! inJson = fieldsJson |> sum.TryFindField "in"
            let! in_ = inJson |> parseExpr
            Expr.Let({ VarName = varName }, expr, in_)
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseLetType
      (parseExpr: ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "typeDecl" json


        return!
          sum {
            let! typeNameJson = fieldsJson |> sum.TryFindField "typeName"
            let! typeName = typeNameJson |> JsonValue.AsString
            let! typeDefJson = fieldsJson |> sum.TryFindField "typeDef"
            let! typeDef = typeDefJson |> ExprType.Parse
            let! inJson = fieldsJson |> sum.TryFindField "in"
            let! in_ = inJson |> parseExpr
            Expr.LetType({ VarName = typeName }, typeDef, in_)
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member ParseMatchCases
      (parseExpr: ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
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
              let! value = parseExpr valueJson
              let casesJson = operandsJson |> Seq.skip 1 |> Seq.toList
              let! cases = sum.All(casesJson |> Seq.map (Expr.ParseMatchCase parseExpr))
              let cases = cases |> Seq.map (fun (c, v, b) -> (c, (v, b))) |> Map.ofSeq
              return Expr.MatchCase(value, cases)
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseRecord
      (parseExpr: ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
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
                  let! value = parseExpr valueJson
                  return name, value
                })
              |> sum.All

            fieldValues |> Map.ofList |> Expr.MakeRecord
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseTuple
      (parseExpr: ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "tuple" json

        return!
          sum {
            let! elementsJson = fieldsJson |> sum.TryFindField "elements"
            let! elementsArray = elementsJson |> JsonValue.AsArray
            let! elements = elementsArray |> Array.toList |> List.map parseExpr |> sum.All
            Expr.MakeTuple elements
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }


    static member private ParseCaseCons
      (parseExpr: ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =

      sum {
        let! fieldsJson = assertKindIsAndGetFields "caseCons" json

        return!
          sum {
            let! caseJson = fieldsJson |> sum.TryFindField "caseName"
            let! valueJson = fieldsJson |> sum.TryFindField "value"
            let! caseName = JsonValue.AsString caseJson
            let! value = valueJson |> parseExpr
            return Expr.MakeCase(caseName, value)
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseFieldLookup
      (parseExpr: ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "fieldLookup" json

        return!
          sum {
            let! operandsJson = fieldsJson |> sum.TryFindField "operands"
            let! firstJson, fieldNameJson = JsonValue.AsPair operandsJson
            let! fieldName = JsonValue.AsString fieldNameJson
            let! first = parseExpr firstJson
            return Expr.RecordFieldLookup(first, fieldName)
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseVarLookup(json: JsonValue) : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
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

    static member private ParseItemLookup
      (parseExpr: ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "itemLookup" json

        return!
          sum {
            let! operandsJson = fieldsJson |> sum.TryFindField "operands"
            let! firstJson, itemIndexJson = JsonValue.AsPair operandsJson
            let! itemIndex = JsonValue.AsNumber itemIndexJson
            let! first = parseExpr firstJson
            return Expr.Project(first, itemIndex |> int)
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member Parse
      (parseExtension: ExprParser<'ExprExtension, 'ValueExtension> -> ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum.Any(
        NonEmptyList.OfList(
          (Value.Parse >> Sum.map Expr.Value) json,
          [ Expr.ParseLambda (Expr.Parse parseExtension) json
            Expr.ParseGenericLambda (Expr.Parse parseExtension) json
            Expr.ParseRecord (Expr.Parse parseExtension) json
            Expr.ParseTuple (Expr.Parse parseExtension) json
            Expr.ParseCaseCons (Expr.Parse parseExtension) json
            Expr.ParseLet (Expr.Parse parseExtension) json
            Expr.ParseLetType (Expr.Parse parseExtension) json
            Expr.ParseApplication (Expr.Parse parseExtension) json
            Expr.ParseGenericApplication (Expr.Parse parseExtension) json
            Expr.ParseMatchCases (Expr.Parse parseExtension) json
            Expr.ParseFieldLookup (Expr.Parse parseExtension) json
            Expr.ParseVarLookup json
            Expr.ParseItemLookup (Expr.Parse parseExtension) json
            parseExtension (Expr.Parse parseExtension) json
            sum.Throw(Errors.Singleton $"Error: cannot parse expression {json.ToFSharpString.ReasonablyClamped}.") ]
        )
      )
      |> sum.MapError Errors.HighestPriority

    static member ToJson
      (toJsonTailExpr:
        ((Expr<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
          -> (Value<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
          -> 'ExprExtension
          -> Sum<JsonValue, Errors>))
      (toJsonTailValue:
        ((Expr<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
          -> (Value<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
          -> 'ValueExtension
          -> Sum<JsonValue, Errors>))
      (expr: Expr<'ExprExtension, 'ValueExtension>)
      : Sum<JsonValue, Errors> =
      let (!) = Expr.ToJson toJsonTailExpr toJsonTailValue
      let (!!) = Value.ToJson toJsonTailExpr toJsonTailValue

      sum {
        match expr with
        | Expr.Value value -> return! !!value
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

          return
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

          return
            JsonValue.Record
              [| "kind", JsonValue.String "fieldLookup"
                 "operands", JsonValue.Array [| jsonExpr; JsonValue.String fieldName |] |]
        | Expr.Project(expr, index) ->
          let! jsonExpr = !expr

          JsonValue.Record
            [| "kind", JsonValue.String "itemLookup"
               "operands", JsonValue.Array [| jsonExpr; JsonValue.Number(decimal index) |] |]
        | Expr.MakeRecord r ->
          let! jsonFields =
            r
            |> Map.toList
            |> List.map (fun (fieldName, fieldValue) ->
              sum {
                let! jsonValue = !fieldValue
                return fieldName, jsonValue
              })
            |> sum.All

          JsonValue.Record
            [| "kind", JsonValue.String "record"
               "fields", jsonFields |> Array.ofList |> JsonValue.Record |]
        | Expr.MakeTuple _ -> return! sum.Throw(Errors.Singleton "Error: MakeTuple not implemented")
        | Expr.MakeSet _ -> return! sum.Throw(Errors.Singleton "Error: MakeSet not implemented")
        | Expr.MakeCase _ -> return! sum.Throw(Errors.Singleton "Error: MakeCase not implemented")
        | Expr.Annotate _ -> return! sum.Throw(Errors.Singleton "Error: Annotate not implemented")
        | Expr.GenericApply(func, arg) ->
          let! jsonFunc = !func

          JsonValue.Record
            [| "kind", JsonValue.String "Apply"
               "function", jsonFunc
               "argument", ExprType.ToJson arg |]
        | Expr.Let _ -> return! sum.Throw(Errors.Singleton "Error: Let not implemented")
        | Expr.LetType(typeName, typeDef, inner) ->
          let! inner = !inner

          JsonValue.Record
            [| "kind", JsonValue.String "typeDecl"
               "typeName", JsonValue.String typeName.VarName
               "typeDef", ExprType.ToJson typeDef
               "in", inner |]
        | Expr.Extension exprExt ->
          return!
            toJsonTailExpr
              (Expr.ToJson toJsonTailExpr toJsonTailValue)
              (Value.ToJson toJsonTailExpr toJsonTailValue)
              exprExt
      }
      |> sum.MapError Errors.HighestPriority

  and Value<'ExprExtension, 'ValueExtension> with

    static member private ParseUnit(json: JsonValue) : Sum<Value<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = JsonValue.AsRecord json
        let! kindJson = fieldsJson |> sum.TryFindField "kind"
        do! assertKindIs "unit" kindJson
        return Value.Unit
      }

    static member Parse(json: JsonValue) : Sum<Value<'ExprExtension, 'ValueExtension>, Errors> =
      sum.Any(NonEmptyList.OfList(Value.ParseUnit, []) |> NonEmptyList.map (fun f -> f json))

    static member ToJson
      (toJsonTailExpr:
        (Expr<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
          -> (Value<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
          -> 'ExprExtension
          -> Sum<JsonValue, Errors>)
      (toJsonTailValue:
        (Expr<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
          -> (Value<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
          -> 'ValueExtension
          -> Sum<JsonValue, Errors>)
      (value: Value<'ExprExtension, 'ValueExtension>)
      : Sum<JsonValue, Errors> =
      let (!) = Expr.ToJson toJsonTailExpr toJsonTailValue
      let (!!) = Value.ToJson toJsonTailExpr toJsonTailValue

      sum {
        match value with
        | Value.Unit -> JsonValue.Record [| "kind", JsonValue.String "unit" |]
        | Value.Lambda(parameter, parameterType, returnType, body) ->
          let! jsonBody = !body

          let parameter =
            match parameterType with
            | Some parameterType ->
              JsonValue.Record
                [| "name", JsonValue.String parameter.VarName
                   "type", parameterType |> ExprType.ToJson |]
            | None -> JsonValue.String parameter.VarName

          let baseFields =
            [| "kind", JsonValue.String "lambda"
               "parameter", parameter
               "body", jsonBody |]

          let fieldsWithReturns =
            match returnType with
            | Some returnType -> Array.append baseFields [| "returnType", returnType |> ExprType.ToJson |]
            | None -> baseFields

          JsonValue.Record fieldsWithReturns
        | Value.CaseCons(case, value) ->
          let! jsonValue = !!value

          JsonValue.Record
            [| "kind", JsonValue.String "caseCons"
               "case", JsonValue.String case
               "value", jsonValue |]
        | Value.Tuple elements ->
          let! jsonElements = elements |> List.map (!!) |> sum.All

          JsonValue.Record
            [| "kind", JsonValue.String "tuple"
               "elements", jsonElements |> Array.ofList |> JsonValue.Array |]
        | Value.Record fields ->
          let! jsonFields =
            fields
            |> Map.toList
            |> List.map (fun (fieldName, fieldValue) ->
              sum {
                let! jsonValue = (!!) fieldValue
                fieldName, jsonValue
              })
            |> sum.All

          JsonValue.Record
            [| "kind", JsonValue.String "record"
               "fields", jsonFields |> Array.ofList |> JsonValue.Record |]
        | Value.Var v ->
          JsonValue.Record
            [| "kind", JsonValue.String "varLookup"
               "varName", JsonValue.String v.VarName |]
        | Value.GenericLambda _ -> return! sum.Throw(Errors.Singleton "Error: GenericLambda not implemented")
        | Value.Extension varExt ->
          return!
            toJsonTailValue
              (Expr.ToJson toJsonTailExpr toJsonTailValue)
              (Value.ToJson toJsonTailExpr toJsonTailValue)
              varExt
      }
