namespace Ballerina.DSL.Expr.Extensions

module Primitives =
  open Ballerina.StdLib.Json
  open Ballerina.DSL.Expr.Model
  open FSharp.Data
  open Ballerina.Fun
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.DSL.Expr.Types.Unification
  open Ballerina.Coroutines.Model
  open Ballerina.DSL.Expr.Eval
  open Ballerina.DSL.Parser.Expr
  open Ballerina.DSL.Parser.Patterns
  open Ballerina.DSL.Expr.Types.TypeCheck
  open Ballerina.Collections.NonEmptyList
  open System

  type UnaryOperator =
    | Not
    | Minus

  type BinaryOperator =
    | Plus
    | Minus
    | GreaterThan
    | Equals
    | GreaterThanEquals
    | Times
    | DividedBy
    | And
    | Or


  type PrimitivesExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail> =
    | Binary of BinaryOperator * Expr<'ExprExtension, 'ValueExtension> * Expr<'ExprExtension, 'ValueExtension>
    | Unary of UnaryOperator * Expr<'ExprExtension, 'ValueExtension>
    | Rest of 'ExprExtensionTail

    override e.ToString() =
      match e with
      | Binary(op, e1, e2) -> $"({e1.ToString()} {op.ToString()} {e2.ToString()})"
      | Unary(op, e) -> $"({op.ToString()}{e.ToString()}"
      | Rest t -> t.ToString()


  type PrimitivesValueExtension<'ExprExtensionTail, 'ValueExtensionTail> =
    | ConstInt of int
    | ConstFloat of decimal
    | ConstString of string
    | ConstBool of bool
    | ConstGuid of Guid
    | Rest of 'ValueExtensionTail

    override v.ToString() =
      match v with
      | ConstInt v -> v.ToString()
      | ConstFloat v -> v.ToString()
      | ConstBool v -> v.ToString()
      | ConstGuid v -> v.ToString()
      | ConstString v -> v.ToString()
      | Rest e -> e.ToString()

  type PrimitivesExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail> =
    { fromExpr:
        Expr<'ExprExtension, 'ValueExtension>
          -> Sum<
            PrimitivesExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>,
            Errors
           >
      toExpr:
        PrimitivesExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
          -> Expr<'ExprExtension, 'ValueExtension>
      fromValue:
        Value<'ExprExtension, 'ValueExtension>
          -> Sum<PrimitivesValueExtension<'ExprExtensionTail, 'ValueExtensionTail>, Errors>
      toValue:
        PrimitivesValueExtension<'ExprExtensionTail, 'ValueExtensionTail> -> Value<'ExprExtension, 'ValueExtension> }

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

  type PrimitivesValueExtension<'ExprExtensionTail, 'ValueExtensionTail> with
    static member AsInt
      (ctx: PrimitivesExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (v: Value<'ExprExtension, 'ValueExtension>)
      : Sum<int, Errors> =
      sum {
        let! v = ctx.fromValue v

        match v with
        | PrimitivesValueExtension.ConstInt v -> return v
        | _ -> return! sum.Throw(Errors.Singleton $"Error: expected int, found {v.ToString()}")
      }

    static member AsFloat
      (ctx: PrimitivesExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (v: Value<'ExprExtension, 'ValueExtension>)
      : Sum<decimal, Errors> =
      sum {
        let! v = ctx.fromValue v

        match v with
        | PrimitivesValueExtension.ConstFloat v -> return v
        | _ -> return! sum.Throw(Errors.Singleton $"Error: expected float, found {v.ToString()}")
      }

  type Expr<'ExprExtension, 'ValueExtension> with
    static member private ParseIntForBackwardCompatibility
      (ctx: PrimitivesExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! v = JsonValue.AsNumber json
        return PrimitivesValueExtension.ConstInt(int v) |> ctx.toValue |> Expr.Value
      }

    static member private ParseFloat
      (ctx: PrimitivesExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "float" json

        return!
          sum {
            let! valueJson = fieldsJson |> sum.TryFindField "value"
            let! value = JsonValue.AsString valueJson

            match System.Decimal.TryParse value with
            | true, v -> return PrimitivesValueExtension.ConstFloat v |> ctx.toValue |> Expr.Value
            | false, _ -> return! sum.Throw(Errors.Singleton $"Error: could not parse {value} as float")
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseInt
      (ctx: PrimitivesExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "int" json

        return!
          sum {
            let! valueJson = fieldsJson |> sum.TryFindField "value"
            let! value = JsonValue.AsString valueJson

            match System.Int32.TryParse value with
            | true, v -> return PrimitivesValueExtension.ConstInt v |> ctx.toValue |> Expr.Value
            | false, _ -> return! sum.Throw(Errors.Singleton $"Error: could not parse {value} as int")
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseBool
      (ctx: PrimitivesExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! v = JsonValue.AsBoolean json
        return PrimitivesValueExtension.ConstBool v |> ctx.toValue |> Expr.Value
      }

    static member private ParseString
      (ctx: PrimitivesExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! v = JsonValue.AsString json
        return PrimitivesValueExtension.ConstString v |> ctx.toValue |> Expr.Value
      }

    static member private ParseBinaryOperator
      (parseRoot: ExprParser<'ExprExtension, 'ValueExtension>)
      (ctx: PrimitivesExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = JsonValue.AsRecord json
        let! kindJson = fieldsJson |> sum.TryFindField "kind"
        let! operator = kindJson |> JsonValue.AsEnum BinaryOperator.AllNames

        return!
          sum {
            let! operandsJson = fieldsJson |> sum.TryFindField "operands"
            let! firstJson, secondJson = JsonValue.AsPair operandsJson
            let! first = parseRoot firstJson
            let! second = parseRoot secondJson

            let! operator =
              BinaryOperator.ByName
              |> Map.tryFindWithError operator "binary operator" operator

            return PrimitivesExprExtension.Binary(operator, first, second) |> ctx.toExpr
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)

      }

  let parsePrimitives
    : PrimitivesExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
        -> ExprParser<'ExprExtension, 'ValueExtension>
        -> JsonValue
        -> Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
    fun ctx parseRootExpr jsonValue ->
      sum.Any(
        NonEmptyList.OfList(
          Expr.ParseIntForBackwardCompatibility,
          [ Expr.ParseInt
            Expr.ParseFloat
            Expr.ParseBool
            Expr.ParseString
            Expr.ParseBinaryOperator parseRootExpr ]
        )
        |> NonEmptyList.map (fun f -> f ctx jsonValue)
      )

  let evalPrimitives
    : PrimitivesExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
        -> (ExprEval<'ExprExtension, 'ValueExtension> -> EvalFrom<'ExprExtension, 'ValueExtension, 'ExprExtensionTail>)
        -> ExprEval<'ExprExtension, 'ValueExtension>
        -> EvalFrom<
          'ExprExtension,
          'ValueExtension,
          PrimitivesExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
         > =
    fun ctx evalTail evalRoot e ->
      let (!) = evalRoot

      co {
        match e with
        | Binary(Plus, e1, e2) ->
          let! v1 = !e1
          let! v2 = !e2

          return!
            co.Any
              [ co {
                  let! v1 = v1 |> PrimitivesValueExtension.AsInt ctx |> co.ofSum
                  let! v2 = v2 |> PrimitivesValueExtension.AsInt ctx |> co.ofSum

                  let res = PrimitivesValueExtension.ConstInt(v1 + v2)
                  let res = res |> ctx.toValue
                  return res
                }
                co {
                  let! v1 = v1 |> PrimitivesValueExtension.AsFloat ctx |> co.ofSum
                  let! v2 = v2 |> PrimitivesValueExtension.AsFloat ctx |> co.ofSum

                  let res = PrimitivesValueExtension.ConstFloat(v1 + v2)
                  let res = res |> ctx.toValue
                  return res
                } ]

        | PrimitivesExprExtension.Rest tail -> return! evalTail evalRoot tail
        | e -> return! $"Error: unsupported eval for expression {e}" |> Errors.Singleton |> co.Throw

      }

  let toJsonPrimitivesValue
    (_toJsonExprTail: 'ExprExtensionTail -> Sum<JsonValue, Errors>)
    (toJsonValueTail: 'ValueExtensionTail -> Sum<JsonValue, Errors>)
    (_toJsonRootExpr: Expr<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
    (_toJsonRootValue: Value<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
    (value: PrimitivesValueExtension<'ExprExtensionTail, 'ValueExtensionTail>)
    : Sum<JsonValue, Errors> =
    sum {
      match value with
      | ConstInt i ->
        return JsonValue.Record [| "kind", JsonValue.String "int"; "value", JsonValue.String(i.ToString()) |]
      | ConstFloat value ->
        return
          JsonValue.Record
            [| "kind", JsonValue.String "float"
               "value", JsonValue.String(value.ToString()) |]
      | ConstBool b -> return JsonValue.Boolean b
      | ConstString s -> return JsonValue.String s
      | ConstGuid _ -> return! "Error: ConstGuid not implemented" |> Errors.Singleton |> sum.Throw
      | Rest t -> return! toJsonValueTail t
    }

  let toJsonPrimitivesExpr
    (toJsonExprTail: 'ExprExtensionTail -> Sum<JsonValue, Errors>)
    (_toJsonValueTail: 'ValueExtensionTail -> Sum<JsonValue, Errors>)
    (toJsonRootExpr: Expr<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
    (_toJsonRootValue: Value<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
    (expr: PrimitivesExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
    : Sum<JsonValue, Errors> =
    sum {
      match expr with
      | Binary(op, l, r) ->
        let! jsonL = l |> toJsonRootExpr
        let! jsonR = r |> toJsonRootExpr

        let! operatorName =
          Map.tryFind op BinaryOperator.ToName
          |> Sum.fromOption (fun () -> Errors.Singleton $"No name for binary operator {op}")

        return
          JsonValue.Record
            [| "kind", JsonValue.String operatorName
               "operands", JsonValue.Array [| jsonL; jsonR |] |]
      | Unary _ -> return! sum.Throw(Errors.Singleton "Error: Unary not implemented")
      | PrimitivesExprExtension.Rest t -> return! toJsonExprTail t
    }

  let typeCheckExprPrimitives
    (_ctx: PrimitivesExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
    (typeCheckTail: TypeChecker<'ExprExtensionTail>)
    (typeCheckRootExpr: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
    (_typeCheckRootValue: TypeChecker<Value<'ExprExtension, 'ValueExtension>>)
    : TypeChecker<PrimitivesExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>> =
    fun typeBindings vars e ->
      let notImplementedError exprName =
        sum.Throw(Errors.Singleton $"Error: not implemented Expr type checker for expression {exprName}")

      let typeCheckBinaryOperator
        (vars: VarTypes)
        (op: BinaryOperator)
        (e1: Expr<'ExprExtension, 'ValueExtension>)
        (e2: Expr<'ExprExtension, 'ValueExtension>)
        : Sum<ExprType, Errors> =
        let (!) = typeCheckRootExpr typeBindings vars

        sum {
          match op with
          | Or ->
            let! t1 = !e1
            let! t2 = !e2

            match t1, t2 with
            | PrimitiveType BoolType, PrimitiveType BoolType -> PrimitiveType BoolType
            | _ -> return! sum.Throw($$"""Error: invalid type of expression {{e}}""" |> Errors.Singleton)
          | Equals ->
            let! t1 = !e1
            let! t2 = !e2

            if t1 = t2 then
              PrimitiveType BoolType
            else
              return!
                sum.Throw(
                  $$"""Error: cannot compare different types {{t1}} and {{t2}}"""
                  |> Errors.Singleton
                )
          | Plus ->
            let! t1 = !e1
            let! t2 = !e2

            do! ExprType.Unify vars typeBindings t1 (PrimitiveType IntType) |> Sum.map ignore
            do! ExprType.Unify vars typeBindings t2 (PrimitiveType IntType) |> Sum.map ignore

            return PrimitiveType IntType
          | And -> return! notImplementedError "Binary op: And"
          | Minus -> return! notImplementedError "Binary op: Minus"
          | Times -> return! notImplementedError "Binary op: Times"
          | GreaterThan -> return! notImplementedError "Binary op: GreaterThan"
          | GreaterThanEquals -> return! notImplementedError "Binary op: GreaterThanEquals"
          | DividedBy -> return! notImplementedError "Binary op: DividedBy"
        }

      sum {
        match e with
        | PrimitivesExprExtension.Binary(op, e1, e2) -> return! typeCheckBinaryOperator vars op e1 e2
        | PrimitivesExprExtension.Unary(_, _) -> return! notImplementedError "Unary"
        | PrimitivesExprExtension.Rest t -> return! typeCheckTail typeBindings vars t
      }

  let typeCheckValuePrimitives
    (_ctx: PrimitivesExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
    (typeCheckTail: TypeChecker<'ValueExtensionTail>)
    (_typeCheckRootExpr: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
    (_typeCheckRootValue: TypeChecker<Value<'ExprExtension, 'ValueExtension>>)
    : TypeChecker<PrimitivesValueExtension<'ExprExtensionTail, 'ValueExtensionTail>> =
    fun typeBindings vars v ->
      sum {
        match v with
        | ConstInt _ -> return PrimitiveType PrimitiveType.IntType
        | ConstFloat _ -> return PrimitiveType PrimitiveType.FloatType
        | ConstBool _ -> PrimitiveType PrimitiveType.BoolType
        | ConstString _ -> PrimitiveType PrimitiveType.StringType
        | ConstGuid _ -> PrimitiveType PrimitiveType.GuidType
        | Rest e -> return! typeCheckTail typeBindings vars e
      }

  let operatorEvalPrimitivesExtension
    (tailOperatorEvalExtensions: OperatorEvalExtensions<'ExprExtension, 'ValueExtension, 'ValueExtensionTail>)
    : OperatorEvalExtensions<
        'ExprExtension,
        'ValueExtension,
        PrimitivesValueExtension<'ExprExtensionTail, 'ValueExtensionTail>
       >
    =
    {| Apply =
        fun inputs ->
          match inputs.func with
          | ConstInt _ -> co.Throw(Errors.Singleton "Nothing to apply in primitives extension")
          | ConstFloat _ -> co.Throw(Errors.Singleton "Nothing to apply in primitives extension")
          | ConstBool _ -> co.Throw(Errors.Singleton "Nothing to apply in primitives extension")
          | ConstString _ -> co.Throw(Errors.Singleton "Nothing to apply in primitives extension")
          | ConstGuid _ -> co.Throw(Errors.Singleton "Nothing to apply in primitives extension")
          | Rest r -> tailOperatorEvalExtensions.Apply {| inputs with func = r |}
       GenericApply =
        fun inputs ->
          match inputs.typeFunc with
          | ConstInt _ -> co.Throw(Errors.Singleton "Nothing to generic apply in primitives extension")
          | ConstFloat _ -> co.Throw(Errors.Singleton "Nothing to generic apply in primitives extension")
          | ConstBool _ -> co.Throw(Errors.Singleton "Nothing to generic apply in primitives extension")
          | ConstString _ -> co.Throw(Errors.Singleton "Nothing to generic apply in primitives extension")
          | ConstGuid _ -> co.Throw(Errors.Singleton "Nothing to generic apply in primitives extension")
          | Rest r -> tailOperatorEvalExtensions.GenericApply {| inputs with typeFunc = r |}
       MatchCase =
        fun inputs ->
          match inputs.value with
          | ConstInt _ -> co.Throw(Errors.Singleton "Nothing to match case in primitives extension")
          | ConstFloat _ -> co.Throw(Errors.Singleton "Nothing to match case in primitives extension")
          | ConstBool _ -> co.Throw(Errors.Singleton "Nothing to match case in primitives extension")
          | ConstString _ -> co.Throw(Errors.Singleton "Nothing to match case in primitives extension")
          | ConstGuid _ -> co.Throw(Errors.Singleton "Nothing to match case in primitives extension")
          | Rest r -> tailOperatorEvalExtensions.MatchCase {| inputs with value = r |} |}
