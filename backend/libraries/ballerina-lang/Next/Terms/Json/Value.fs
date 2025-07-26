namespace Ballerina.DSL.Next.Terms.Json

module Value =
  open FSharp.Data
  open Ballerina.StdLib.Json
  open Ballerina.StdLib.Object
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.Json
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Terms.Json.Primitive

  type JsonParser<'T> = JsonValue -> Sum<'T, Errors>
  type ValueParser<'T> = Reader<Value<'T>, JsonParser<'T>, Errors>
  type ExprParser<'T> = Reader<Expr<'T>, JsonParser<'T>, Errors>

  let inline private (>>=) f g = fun x -> reader.Bind(f x, g) // Using bind

  type Var with
    static member FromJson: JsonParser<Var> =
      JsonValue.AssertKindAndContinueWithField "var" "name" (fun nameJson ->
        sum {
          let! name = nameJson |> JsonValue.AsString
          return name |> Var.Create
        })

  type Value<'T> with
    static member FromJsonPrimitive: JsonValue -> ValueParser<'T> =
      (PrimitiveValue.FromJson >> reader.OfSum)
      >>= (fun primitive -> reader.Return(Value.Primitive primitive))

    static member FromJsonRecord: JsonValue -> ValueParser<'T> =
      fun json ->
        reader {
          let! ctx = reader.GetContext()

          return!
            JsonValue.AssertKindAndContinueWithField
              "record"
              "fields"
              (fun fieldsJson ->
                sum {
                  let! fields = fieldsJson |> JsonValue.AsArray

                  let! fields =
                    fields
                    |> Seq.map (fun field ->
                      sum {
                        let! (k, v) = field |> JsonValue.AsPair
                        let! k = TypeSymbol.FromJson k
                        let! v = (Value.FromJson v) |> Reader.Run ctx
                        return (k, v)
                      })
                    |> sum.All
                    |> sum.Map Map.ofSeq

                  return Value.Record(fields)
                })
              (json)
            |> reader.OfSum
        }

    static member FromJsonUnion: JsonValue -> ValueParser<'T> =
      fun json ->
        reader {
          let! ctx = reader.GetContext()

          return!
            JsonValue.AssertKindAndContinueWithField
              "union-case"
              "union-case"
              (fun caseJson ->
                sum {
                  let! (k, v) = caseJson |> JsonValue.AsPair
                  let! k = TypeSymbol.FromJson k
                  let! v = (Value.FromJson v) |> Reader.Run ctx
                  return Value.UnionCase(k, v)
                })
              (json)
            |> reader.OfSum
        }

    static member FromJsonTuple: JsonValue -> ValueParser<'T> =
      fun json ->
        reader {
          let! ctx = reader.GetContext()

          return!
            JsonValue.AssertKindAndContinueWithField
              "tuple"
              "elements"
              (fun elementsJson ->
                sum {
                  let! elements = elementsJson |> JsonValue.AsArray
                  let! elements = elements |> Seq.map (Value.FromJson >> Reader.Run ctx) |> sum.All
                  return Value.Tuple elements
                })
              (json)
            |> reader.OfSum
        }

    static member FromJsonSum: JsonValue -> ValueParser<'T> =
      fun json ->
        reader {
          let! ctx = reader.GetContext()

          return!
            JsonValue.AssertKindAndContinueWithField
              "sum"
              "case"
              (fun elementsJson ->
                sum {
                  let! (k, v) = elementsJson |> JsonValue.AsPair
                  let! k = k |> JsonValue.AsInt
                  let! v = (Value.FromJson v) |> Reader.Run ctx
                  return Value.Sum(k, v)
                })
              (json)
            |> reader.OfSum
        }

    static member FromJsonVar(json: JsonValue) : ValueParser<'T> =
      Var.FromJson(json) |> sum.Map(Value.Var) |> reader.OfSum


    static member FromJsonLambda(json: JsonValue) : ValueParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "lambda"
            "lambda"
            (fun lambdaJson ->
              sum {
                let! (var, body) = lambdaJson |> JsonValue.AsPair
                let! var = var |> JsonValue.AsString
                let var = Var.Create var
                let! body = body |> Expr.FromJson |> Reader.Run ctx
                return Value.Lambda(var, body)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonTypeLambda(json: JsonValue) : ValueParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "type-lambda"
            "type-lambda"
            (fun typeParamJson ->
              sum {
                let! (typeParam, body) = typeParamJson |> JsonValue.AsPair
                let! typeParam = typeParam |> TypeParameter.FromJson
                let! body = body |> Expr.FromJson |> Reader.Run ctx
                return Value.TypeLambda(typeParam, body)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJson(json: JsonValue) : ValueParser<'T> =
      reader.Any(
        Value.FromJsonPrimitive(json),
        [ Value.FromJsonRecord(json)
          Value.FromJsonUnion(json)
          Value.FromJsonTuple(json)
          Value.FromJsonSum(json)
          Value.FromJsonVar(json)
          Value.FromJsonLambda(json)
          Value.FromJsonTypeLambda(json) ]
      )

  and Expr<'T> with
    static member FromJsonLambda(json: JsonValue) : ExprParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "lambda"
            "lambda"
            (fun lambdaJson ->
              sum {
                let! (var, body) = lambdaJson |> JsonValue.AsPair
                let! var = var |> JsonValue.AsString
                let var = Var.Create var
                let! body = body |> Expr.FromJson |> Reader.Run ctx
                return Expr.Lambda(var, body)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonTypeLambda(json: JsonValue) : ExprParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "type-lambda"
            "type-lambda"
            (fun typeParamJson ->
              sum {
                let! (typeParam, body) = typeParamJson |> JsonValue.AsPair
                let! typeParam = typeParam |> TypeParameter.FromJson
                let! body = body |> Expr.FromJson |> Reader.Run ctx
                return Expr.TypeLambda(typeParam, body)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonTypeApply(json: JsonValue) : ExprParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "type-apply"
            "type-apply"
            (fun application ->
              sum {
                let! f, arg = application |> JsonValue.AsPair
                let! f = f |> Expr.FromJson |> Reader.Run ctx
                let! arg = arg |> ctx
                return Expr.TypeApply(f, arg)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonApply(json: JsonValue) : ExprParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "apply"
            "apply"
            (fun application ->
              sum {
                let! f, arg = application |> JsonValue.AsPair
                let! f = f |> Expr.FromJson |> Reader.Run ctx
                let! arg = arg |> Expr.FromJson |> Reader.Run ctx
                return Expr.Apply(f, arg)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonLet(json: JsonValue) : ExprParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "let"
            "let"
            (fun letJson ->
              sum {
                let! (var, value, body) = letJson |> JsonValue.AsTriple
                let! var = var |> JsonValue.AsString
                let var = Var.Create var
                let! value = value |> Expr.FromJson |> Reader.Run ctx
                let! body = body |> Expr.FromJson |> Reader.Run ctx
                return Expr.Let(var, value, body)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonTypeLet(json: JsonValue) : ExprParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "type-let"
            "type-let"
            (fun typeLetJson ->
              sum {
                let! (typeId, typeArg, body) = typeLetJson |> JsonValue.AsTriple
                let! typeId = typeId |> JsonValue.AsString
                let typeId = TypeIdentifier.Create typeId
                let! typeArg = typeArg |> ctx
                let! body = body |> Expr.FromJson |> Reader.Run ctx
                return Expr.TypeLet(typeId, typeArg, body)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonRecordCons(json: JsonValue) : ExprParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "record-cons"
            "fields"
            (fun fieldsJson ->
              sum {
                let! fields = fieldsJson |> JsonValue.AsArray

                let! fields =
                  fields
                  |> Seq.map (fun field ->
                    sum {
                      let! (k, v) = field |> JsonValue.AsPair
                      let! k = k |> JsonValue.AsString
                      let! v = v |> Expr.FromJson |> Reader.Run ctx
                      return (k, v)
                    })
                  |> sum.All

                return Expr.RecordCons(fields)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonUnionCons(json: JsonValue) : ExprParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "union-case"
            "union-case"
            (fun unionCaseJson ->
              sum {
                let! (k, v) = unionCaseJson |> JsonValue.AsPair
                let! k = k |> JsonValue.AsString
                let! v = v |> Expr.FromJson |> Reader.Run ctx
                return Expr.UnionCons(k, v)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonTupleCons(json: JsonValue) : ExprParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "tuple-cons"
            "elements"
            (fun elementsJson ->
              sum {
                let! elements = elementsJson |> JsonValue.AsArray
                let! elements = elements |> Seq.map (Expr.FromJson >> Reader.Run ctx) |> sum.All
                return Expr.TupleCons(elements)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonSumCons(json: JsonValue) : ExprParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "sum"
            "case"
            (fun elementsJson ->
              sum {
                let! (k, v) = elementsJson |> JsonValue.AsPair
                let! k = k |> JsonValue.AsInt
                let! v = v |> Expr.FromJson |> Reader.Run ctx
                return Expr.SumCons(k, v)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonRecordDes(json: JsonValue) : ExprParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "record-field-lookup"
            "record-field-lookup"
            (fun recordDesJson ->
              sum {
                let! (expr, field) = recordDesJson |> JsonValue.AsPair
                let! expr = expr |> Expr.FromJson |> Reader.Run ctx
                let! field = field |> JsonValue.AsString
                return Expr.RecordDes(expr, field)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonUnionDes(json: JsonValue) : ExprParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "union-match"
            "union-match"
            (fun unionDesJson ->
              sum {
                let! caseHandlers = unionDesJson |> JsonValue.AsArray

                let! caseHandlers =
                  caseHandlers
                  |> Seq.map (fun caseHandler ->
                    sum {
                      let! (caseName, handler) = caseHandler |> JsonValue.AsPair
                      let! caseName = caseName |> JsonValue.AsString
                      let! handlerVar, handlerBody = handler |> JsonValue.AsPair
                      let! handlerVar = handlerVar |> JsonValue.AsString
                      let handlerVar = Var.Create handlerVar
                      let! handlerBody = handlerBody |> Expr.FromJson |> Reader.Run ctx
                      return (caseName, (handlerVar, handlerBody))
                    })
                  |> sum.All
                  |> sum.Map Map.ofSeq

                return Expr.UnionDes(caseHandlers)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonTupleDes(json: JsonValue) : ExprParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "tuple-des"
            "tuple-des"
            (fun tupleDesJson ->
              sum {
                let! (expr, index) = tupleDesJson |> JsonValue.AsPair
                let! expr = expr |> Expr.FromJson |> Reader.Run ctx
                let! index = index |> JsonValue.AsInt
                return Expr.TupleDes(expr, index)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonSumDes(json: JsonValue) : ExprParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "sum-des"
            "sum-des"
            (fun sumDesJson ->
              sum {
                let! caseHandlers = sumDesJson |> JsonValue.AsArray

                let! caseHandlers =
                  caseHandlers
                  |> Seq.map (fun caseHandler ->
                    sum {
                      let! (caseIndex, handler) = caseHandler |> JsonValue.AsPair
                      let! caseIndex = caseIndex |> JsonValue.AsInt
                      let! handlerVar, handlerBody = handler |> JsonValue.AsPair
                      let! handlerVar = handlerVar |> JsonValue.AsString
                      let handlerVar = Var.Create handlerVar
                      let! handlerBody = handlerBody |> Expr.FromJson |> Reader.Run ctx
                      return (caseIndex, (handlerVar, handlerBody))
                    })
                  |> sum.All
                  |> sum.Map Map.ofSeq

                return Expr.SumDes(caseHandlers)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonIf(json: JsonValue) : ExprParser<'T> =
      reader {
        let! ctx = reader.GetContext()

        return!
          JsonValue.AssertKindAndContinueWithField
            "if"
            "if"
            (fun ifJson ->
              sum {
                let! (cond, thenBranch, elseBranch) = ifJson |> JsonValue.AsTriple
                let! cond = cond |> Expr.FromJson |> Reader.Run ctx
                let! thenBranch = thenBranch |> Expr.FromJson |> Reader.Run ctx
                let! elseBranch = elseBranch |> Expr.FromJson |> Reader.Run ctx
                return Expr.If(cond, thenBranch, elseBranch)
              })
            (json)
          |> reader.OfSum
      }

    static member FromJsonPrimitive: JsonValue -> ExprParser<'T> =
      (PrimitiveValue.FromJson >> reader.OfSum)
      >>= (fun primitive -> reader.Return(Expr.Primitive primitive))

    static member FromJsonLookup: JsonValue -> ExprParser<'T> =
      JsonValue.AssertKindAndContinueWithField "lookup" "name" (fun nameJson ->
        sum {
          let! name = nameJson |> JsonValue.AsString
          return Expr.Lookup name
        })
      >> reader.OfSum

    static member FromJson: JsonValue -> ExprParser<'T> =
      fun json ->
        reader.Any(
          Expr.FromJsonLambda(json),
          [ Expr.FromJsonTypeLambda(json)
            Expr.FromJsonTypeApply(json)
            Expr.FromJsonApply(json)
            Expr.FromJsonLet(json)
            Expr.FromJsonTypeLet(json)
            Expr.FromJsonRecordCons(json)
            Expr.FromJsonUnionCons(json)
            Expr.FromJsonTupleCons(json)
            Expr.FromJsonSumCons(json)
            Expr.FromJsonRecordDes(json)
            Expr.FromJsonUnionDes(json)
            Expr.FromJsonTupleDes(json)
            Expr.FromJsonSumDes(json)
            Expr.FromJsonIf(json)
            Expr.FromJsonPrimitive(json)
            Expr.FromJsonLookup(json) ]
        )
