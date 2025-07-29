namespace Ballerina.DSL.Next.Terms.Json

module Value =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.Object
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.StdLib.Json.Sum
  open Ballerina.StdLib.Json.Reader
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
      sum.AssertKindAndContinueWithField "var" "name" (fun nameJson ->
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


          return!
            reader.AssertKindAndContinueWithField
              "record"
              "fields"
              (fun fieldsJson ->
                reader {
                  let! fields = fieldsJson |> JsonValue.AsArray |> reader.OfSum

                  let! fields =
                    fields
                    |> Seq.map (fun field ->
                      reader {
                        let! (k, v) = field |> JsonValue.AsPair |> reader.OfSum
                        let! k = TypeSymbol.FromJson k |> reader.OfSum
                        let! v = (Value.FromJson v)
                        return (k, v)
                      })
                    |> reader.All
                    |> reader.Map Map.ofSeq

                  return Value.Record(fields)
                })
              (json)
        }

    static member FromJsonUnion: JsonValue -> ValueParser<'T> =
      fun json ->
        reader {


          return!
            reader.AssertKindAndContinueWithField
              "union-case"
              "union-case"
              (fun caseJson ->
                reader {
                  let! (k, v) = caseJson |> JsonValue.AsPair |> reader.OfSum
                  let! k = TypeSymbol.FromJson k |> reader.OfSum
                  let! v = (Value.FromJson v)
                  return Value.UnionCase(k, v)
                })
              (json)
        }

    static member FromJsonTuple: JsonValue -> ValueParser<'T> =
      fun json ->
        reader {


          return!
            reader.AssertKindAndContinueWithField
              "tuple"
              "elements"
              (fun elementsJson ->
                reader {
                  let! elements = elementsJson |> JsonValue.AsArray |> reader.OfSum
                  let! elements = elements |> Seq.map (Value.FromJson) |> reader.All
                  return Value.Tuple elements
                })
              (json)
        }

    static member FromJsonSum: JsonValue -> ValueParser<'T> =
      fun json ->
        reader {


          return!
            reader.AssertKindAndContinueWithField
              "sum"
              "case"
              (fun elementsJson ->
                reader {
                  let! (k, v) = elementsJson |> JsonValue.AsPair |> reader.OfSum
                  let! k = k |> JsonValue.AsInt |> reader.OfSum
                  let! v = (Value.FromJson v)
                  return Value.Sum(k, v)
                })
              (json)
        }

    static member FromJsonVar(json: JsonValue) : ValueParser<'T> =
      Var.FromJson(json) |> sum.Map(Value.Var) |> reader.OfSum


    static member FromJsonLambda(json: JsonValue) : ValueParser<'T> =
      reader {


        return!
          reader.AssertKindAndContinueWithField
            "lambda"
            "lambda"
            (fun lambdaJson ->
              reader {
                let! (var, body) = lambdaJson |> JsonValue.AsPair |> reader.OfSum
                let! var = var |> JsonValue.AsString |> reader.OfSum
                let var = Var.Create var
                let! body = body |> Expr.FromJson
                return Value.Lambda(var, body)
              })
            (json)
      }

    static member FromJsonTypeLambda(json: JsonValue) : ValueParser<'T> =
      reader {


        return!
          reader.AssertKindAndContinueWithField
            "type-lambda"
            "type-lambda"
            (fun typeParamJson ->
              reader {
                let! (typeParam, body) = typeParamJson |> JsonValue.AsPair |> reader.OfSum
                let! typeParam = typeParam |> TypeParameter.FromJson |> reader.OfSum
                let! body = body |> Expr.FromJson
                return Value.TypeLambda(typeParam, body)
              })
            (json)
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
    static member FromJsonLambda: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "lambda" "lambda" (fun lambdaJson ->
        reader {
          let! (var, body) = lambdaJson |> JsonValue.AsPair |> reader.OfSum
          let! var = var |> JsonValue.AsString |> reader.OfSum
          let var = Var.Create var
          let! body = body |> Expr.FromJson
          return Expr.Lambda(var, body)
        })

    static member FromJsonTypeLambda: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "type-lambda" "type-lambda" (fun typeParamJson ->
        reader {
          let! (typeParam, body) = typeParamJson |> JsonValue.AsPair |> reader.OfSum
          let! typeParam = typeParam |> TypeParameter.FromJson |> reader.OfSum
          let! body = body |> Expr.FromJson
          return Expr.TypeLambda(typeParam, body)
        })

    static member FromJsonTypeApply: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "type-apply" "type-apply" (fun application ->
        reader {
          let! f, arg = application |> JsonValue.AsPair |> reader.OfSum
          let! f = f |> Expr.FromJson
          let! ctx = reader.GetContext()
          let! arg = arg |> ctx |> reader.OfSum
          return Expr.TypeApply(f, arg)
        })

    static member FromJsonApply: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "apply" "apply" (fun application ->
        reader {
          let! f, arg = application |> JsonValue.AsPair |> reader.OfSum
          let! f = f |> Expr.FromJson
          let! arg = arg |> Expr.FromJson
          return Expr.Apply(f, arg)
        })

    static member FromJsonLet: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "let" "let" (fun letJson ->
        reader {
          let! (var, value, body) = letJson |> JsonValue.AsTriple |> reader.OfSum
          let! var = var |> JsonValue.AsString |> reader.OfSum
          let var = Var.Create var
          let! value = value |> Expr.FromJson
          let! body = body |> Expr.FromJson
          return Expr.Let(var, value, body)
        })

    static member FromJsonTypeLet: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "type-let" "type-let" (fun typeLetJson ->
        reader {
          let! (typeId, typeArg, body) = typeLetJson |> JsonValue.AsTriple |> reader.OfSum
          let! typeId = typeId |> JsonValue.AsString |> reader.OfSum
          let typeId = TypeIdentifier.Create typeId
          let! ctx = reader.GetContext()
          let! typeArg = typeArg |> ctx |> reader.OfSum
          let! body = body |> Expr.FromJson
          return Expr.TypeLet(typeId, typeArg, body)
        })

    static member FromJsonRecordCons: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "record-cons" "fields" (fun fieldsJson ->
        reader {
          let! fields = fieldsJson |> JsonValue.AsArray |> reader.OfSum

          let! fields =
            fields
            |> Seq.map (fun field ->
              reader {
                let! (k, v) = field |> JsonValue.AsPair |> reader.OfSum
                let! k = k |> JsonValue.AsString |> reader.OfSum
                let! v = v |> Expr.FromJson
                return (k, v)
              })
            |> reader.All

          return Expr.RecordCons(fields)
        })

    static member FromJsonUnionCons: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "union-case" "union-case" (fun unionCaseJson ->
        reader {
          let! (k, v) = unionCaseJson |> JsonValue.AsPair |> reader.OfSum
          let! k = k |> JsonValue.AsString |> reader.OfSum
          let! v = v |> Expr.FromJson
          return Expr.UnionCons(k, v)
        })

    static member FromJsonTupleCons: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "tuple-cons" "elements" (fun elementsJson ->
        reader {
          let! elements = elementsJson |> JsonValue.AsArray |> reader.OfSum
          let! elements = elements |> Seq.map (Expr.FromJson) |> reader.All
          return Expr.TupleCons(elements)
        })

    static member FromJsonSumCons: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "sum" "case" (fun elementsJson ->
        reader {
          let! (case, count, v) = elementsJson |> JsonValue.AsTriple |> reader.OfSum
          let! case = case |> JsonValue.AsInt |> reader.OfSum
          let! count = count |> JsonValue.AsInt |> reader.OfSum
          let! v = v |> Expr.FromJson
          return Expr.SumCons({ Case = case; Count = count }, v)
        })

    static member FromJsonRecordDes: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "record-field-lookup" "record-field-lookup" (fun recordDesJson ->
        reader {
          let! (expr, field) = recordDesJson |> JsonValue.AsPair |> reader.OfSum
          let! expr = expr |> Expr.FromJson
          let! field = field |> JsonValue.AsString |> reader.OfSum
          return Expr.RecordDes(expr, field)
        })

    static member FromJsonUnionDes: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "union-match" "union-match" (fun unionDesJson ->
        reader {
          let! caseHandlers = unionDesJson |> JsonValue.AsArray |> reader.OfSum

          let! caseHandlers =
            caseHandlers
            |> Seq.map (fun caseHandler ->
              reader {
                let! (caseName, handler) = caseHandler |> JsonValue.AsPair |> reader.OfSum
                let! caseName = caseName |> JsonValue.AsString |> reader.OfSum
                let! handlerVar, handlerBody = handler |> JsonValue.AsPair |> reader.OfSum
                let! handlerVar = handlerVar |> JsonValue.AsString |> reader.OfSum
                let handlerVar = Var.Create handlerVar
                let! handlerBody = handlerBody |> Expr.FromJson
                return (caseName, (handlerVar, handlerBody))
              })
            |> reader.All
            |> reader.Map Map.ofSeq

          return Expr.UnionDes(caseHandlers)
        })

    static member FromJsonTupleDes: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "tuple-des" "tuple-des" (fun tupleDesJson ->
        reader {
          let! (expr, index, count) = tupleDesJson |> JsonValue.AsTriple |> reader.OfSum
          let! expr = expr |> Expr.FromJson
          let! index = index |> JsonValue.AsInt |> reader.OfSum
          let! count = count |> JsonValue.AsInt |> reader.OfSum
          return Expr.TupleDes(expr, { Index = index; Count = count })
        })

    static member FromJsonSumDes: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "sum-des" "sum-des" (fun sumDesJson ->
        reader {
          let! caseHandlers = sumDesJson |> JsonValue.AsArray |> reader.OfSum

          let! caseHandlers =
            caseHandlers
            |> Seq.map (fun caseHandler ->
              reader {
                let! (caseIndex, handler) = caseHandler |> JsonValue.AsPair |> reader.OfSum
                let! caseIndex = caseIndex |> JsonValue.AsInt |> reader.OfSum
                let! handlerVar, handlerBody = handler |> JsonValue.AsPair |> reader.OfSum
                let! handlerVar = handlerVar |> JsonValue.AsString |> reader.OfSum
                let handlerVar = Var.Create handlerVar
                let! handlerBody = handlerBody |> Expr.FromJson
                return (caseIndex, (handlerVar, handlerBody))
              })
            |> reader.All
            |> reader.Map Map.ofSeq

          return Expr.SumDes(caseHandlers)
        })

    static member FromJsonIf: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "if" "if" (fun ifJson ->
        reader {
          let! (cond, thenBranch, elseBranch) = ifJson |> JsonValue.AsTriple |> reader.OfSum
          let! cond = cond |> Expr.FromJson
          let! thenBranch = thenBranch |> Expr.FromJson
          let! elseBranch = elseBranch |> Expr.FromJson
          return Expr.If(cond, thenBranch, elseBranch)
        })

    static member FromJsonPrimitive: JsonValue -> ExprParser<'T> =
      (PrimitiveValue.FromJson >> reader.OfSum)
      >>= (fun primitive -> reader.Return(Expr.Primitive primitive))

    static member FromJsonLookup: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "lookup" "name" (fun nameJson ->
        reader {
          let! name = nameJson |> JsonValue.AsString |> reader.OfSum
          return Expr.Lookup name
        })

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
