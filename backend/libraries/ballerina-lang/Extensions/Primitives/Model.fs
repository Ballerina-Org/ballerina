namespace Ballerina.DSL.Expr.Extensions

[<RequireQualifiedAccess>]
module Primitives =
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.DSL.Expr.Model
  open FSharp.Data
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


  type ExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail> =
    | Binary of BinaryOperator * Expr<'ExprExtension, 'ValueExtension> * Expr<'ExprExtension, 'ValueExtension>
    | Unary of UnaryOperator * Expr<'ExprExtension, 'ValueExtension>
    | Rest of 'ExprExtensionTail

    override e.ToString() =
      match e with
      | Binary(op, e1, e2) -> $"({e1.ToString()} {op.ToString()} {e2.ToString()})"
      | Unary(op, e) -> $"({op.ToString()}{e.ToString()}"
      | Rest t -> t.ToString()


  type ValueExtension<'ExprExtensionTail, 'ValueExtensionTail> =
    | ConstInt of int
    | ConstFloat of decimal
    | ConstString of string
    | ConstBool of bool
    | ConstGuid of Guid
    | ConstDate of DateOnly
    | Rest of 'ValueExtensionTail

    override v.ToString() =
      match v with
      | ConstInt v -> v.ToString()
      | ConstFloat v -> v.ToString()
      | ConstBool v -> v.ToString()
      | ConstGuid v -> v.ToString()
      | ConstString v -> v.ToString()
      | ConstDate v -> v.ToString()
      | Rest e -> e.ToString()

  type ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail> =
    { fromExpr:
        Expr<'ExprExtension, 'ValueExtension>
          -> Sum<ExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>, Errors>
      toExpr:
        ExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
          -> Expr<'ExprExtension, 'ValueExtension>
      fromValue:
        Value<'ExprExtension, 'ValueExtension> -> Sum<ValueExtension<'ExprExtensionTail, 'ValueExtensionTail>, Errors>
      toValue: ValueExtension<'ExprExtensionTail, 'ValueExtensionTail> -> Value<'ExprExtension, 'ValueExtension> }

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

  type ValueExtension<'ExprExtensionTail, 'ValueExtensionTail> with
    static member AsInt
      (ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (v: Value<'ExprExtension, 'ValueExtension>)
      : Sum<int, Errors> =
      sum {
        let! v = ctx.fromValue v

        match v with
        | ValueExtension.ConstInt v -> return v
        | _ -> return! sum.Throw(Errors.Singleton $"Error: expected int, found {v.ToString()}")
      }

    static member AsFloat
      (ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (v: Value<'ExprExtension, 'ValueExtension>)
      : Sum<decimal, Errors> =
      sum {
        let! v = ctx.fromValue v

        match v with
        | ValueExtension.ConstFloat v -> return v
        | _ -> return! sum.Throw(Errors.Singleton $"Error: expected float, found {v.ToString()}")
      }

  type Expr<'ExprExtension, 'ValueExtension> with
    static member private ParseIntForBackwardCompatibility
      (ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! v = JsonValue.AsNumber json
        return ValueExtension.ConstInt(int v) |> ctx.toValue |> Expr.Value
      }

    static member private ParseFloat
      (ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "float" json

        return!
          sum {
            let! valueJson = fieldsJson |> sum.TryFindField "value"
            let! value = JsonValue.AsString valueJson

            match System.Decimal.TryParse value with
            | true, v -> return ValueExtension.ConstFloat v |> ctx.toValue |> Expr.Value
            | false, _ -> return! sum.Throw(Errors.Singleton $"Error: could not parse {value} as float")
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseInt
      (ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "int" json

        return!
          sum {
            let! valueJson = fieldsJson |> sum.TryFindField "value"
            let! value = JsonValue.AsString valueJson

            match System.Int32.TryParse value with
            | true, v -> return ValueExtension.ConstInt v |> ctx.toValue |> Expr.Value
            | false, _ -> return! sum.Throw(Errors.Singleton $"Error: could not parse {value} as int")
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseDate
      (ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "date" json

        return!
          sum {
            let! valueJson = fieldsJson |> sum.TryFindField "value"
            let! value = JsonValue.AsString valueJson

            match DateOnly.TryParse value with
            | true, v -> return ValueExtension.ConstDate v |> ctx.toValue |> Expr.Value
            | false, _ -> return! sum.Throw(Errors.Singleton $"Error: could not parse {value} as date")
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseBool
      (ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! v = JsonValue.AsBoolean json
        return ValueExtension.ConstBool v |> ctx.toValue |> Expr.Value
      }

    static member private ParseString
      (ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! v = JsonValue.AsString json
        return ValueExtension.ConstString v |> ctx.toValue |> Expr.Value
      }

    static member private ParseBinaryOperator
      (parseRoot: ExprParser<'ExprExtension, 'ValueExtension>)
      (ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
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

            return ExprExtension.Binary(operator, first, second) |> ctx.toExpr
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)

      }

  let parse
    : ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
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
            Expr.ParseDate
            Expr.ParseBinaryOperator parseRootExpr ]
        )
        |> NonEmptyList.map (fun f -> f ctx jsonValue)
      )

  let eval
    : ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
        -> (ExprEval<'ExprExtension, 'ValueExtension> -> EvalFrom<'ExprExtension, 'ValueExtension, 'ExprExtensionTail>)
        -> ExprEval<'ExprExtension, 'ValueExtension>
        -> EvalFrom<
          'ExprExtension,
          'ValueExtension,
          ExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
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
                  let! v1 = v1 |> ValueExtension.AsInt ctx |> co.ofSum
                  let! v2 = v2 |> ValueExtension.AsInt ctx |> co.ofSum

                  let res = ValueExtension.ConstInt(v1 + v2)
                  let res = res |> ctx.toValue
                  return res
                }
                co {
                  let! v1 = v1 |> ValueExtension.AsFloat ctx |> co.ofSum
                  let! v2 = v2 |> ValueExtension.AsFloat ctx |> co.ofSum

                  let res = ValueExtension.ConstFloat(v1 + v2)
                  let res = res |> ctx.toValue
                  return res
                } ]

        | ExprExtension.Rest tail -> return! evalTail evalRoot tail
        | e -> return! $"Error: unsupported eval for expression {e}" |> Errors.Singleton |> co.Throw

      }

  let toJsonValue
    (_toJsonExprTail: 'ExprExtensionTail -> Sum<JsonValue, Errors>)
    (toJsonValueTail: 'ValueExtensionTail -> Sum<JsonValue, Errors>)
    (_toJsonRootExpr: Expr<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
    (_toJsonRootValue: Value<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
    (value: ValueExtension<'ExprExtensionTail, 'ValueExtensionTail>)
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
      | ConstDate d ->
        return JsonValue.Record [| "kind", JsonValue.String "date"; "value", JsonValue.String(d.ToString()) |]
      | ConstGuid _ -> return! "Error: ConstGuid not implemented" |> Errors.Singleton |> sum.Throw
      | Rest t -> return! toJsonValueTail t
    }

  let toJsonExpr
    (toJsonExprTail: 'ExprExtensionTail -> Sum<JsonValue, Errors>)
    (_toJsonValueTail: 'ValueExtensionTail -> Sum<JsonValue, Errors>)
    (toJsonRootExpr: Expr<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
    (_toJsonRootValue: Value<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
    (expr: ExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
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
      | ExprExtension.Rest t -> return! toJsonExprTail t
    }

  let initialTypeBindings: TypeBindings = Map.empty

  let typeCheckExpr
    (_ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
    (typeCheckTail: TypeChecker<'ExprExtensionTail>)
    (typeCheckRootExpr: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
    (_typeCheckRootValue: TypeChecker<Value<'ExprExtension, 'ValueExtension>>)
    : TypeChecker<ExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>> =
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
          | Or
          | And ->
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
          | Plus
          | Minus
          | Times
          | DividedBy ->
            let! t1 = !e1
            let! t2 = !e2

            do! ExprType.Unify vars typeBindings t1 (PrimitiveType IntType) |> Sum.map ignore
            do! ExprType.Unify vars typeBindings t2 (PrimitiveType IntType) |> Sum.map ignore

            return PrimitiveType IntType
          | GreaterThan
          | GreaterThanEquals ->
            let! t1 = !e1
            let! t2 = !e2

            do! ExprType.Unify vars typeBindings t1 (PrimitiveType IntType) |> Sum.map ignore
            do! ExprType.Unify vars typeBindings t2 (PrimitiveType IntType) |> Sum.map ignore
            return PrimitiveType BoolType
        }

      sum {
        match e with
        | ExprExtension.Binary(op, e1, e2) -> return! typeCheckBinaryOperator vars op e1 e2
        | ExprExtension.Unary(_, _) -> return! notImplementedError "Unary"
        | ExprExtension.Rest t -> return! typeCheckTail typeBindings vars t
      }

  let typeCheckValue
    (_ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
    (typeCheckTail: TypeChecker<'ValueExtensionTail>)
    (_typeCheckRootExpr: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
    (_typeCheckRootValue: TypeChecker<Value<'ExprExtension, 'ValueExtension>>)
    : TypeChecker<ValueExtension<'ExprExtensionTail, 'ValueExtensionTail>> =
    fun typeBindings vars v ->
      sum {
        match v with
        | ConstInt _ -> return PrimitiveType PrimitiveType.IntType
        | ConstFloat _ -> return PrimitiveType PrimitiveType.FloatType
        | ConstBool _ -> PrimitiveType PrimitiveType.BoolType
        | ConstString _ -> PrimitiveType PrimitiveType.StringType
        | ConstGuid _ -> PrimitiveType PrimitiveType.GuidType
        | ConstDate _ -> PrimitiveType PrimitiveType.DateOnlyType
        | Rest e -> return! typeCheckTail typeBindings vars e
      }

  let operatorEvalExtension
    (tailOperatorEvalExtensions: OperatorEvalExtensions<'ExprExtension, 'ValueExtension, 'ValueExtensionTail>)
    : OperatorEvalExtensions<'ExprExtension, 'ValueExtension, ValueExtension<'ExprExtensionTail, 'ValueExtensionTail>> =
    {| Apply =
        fun inputs ->
          match inputs.func with
          | ConstInt _ -> co.Throw(Errors.Singleton "Nothing to apply in primitives extension")
          | ConstFloat _ -> co.Throw(Errors.Singleton "Nothing to apply in primitives extension")
          | ConstBool _ -> co.Throw(Errors.Singleton "Nothing to apply in primitives extension")
          | ConstString _ -> co.Throw(Errors.Singleton "Nothing to apply in primitives extension")
          | ConstGuid _ -> co.Throw(Errors.Singleton "Nothing to apply in primitives extension")
          | ConstDate _ -> co.Throw(Errors.Singleton "Nothing to apply in primitives extension")
          | Rest r -> tailOperatorEvalExtensions.Apply {| inputs with func = r |}
       GenericApply =
        fun inputs ->
          match inputs.typeFunc with
          | ConstInt _ -> co.Throw(Errors.Singleton "Nothing to generic apply in primitives extension")
          | ConstFloat _ -> co.Throw(Errors.Singleton "Nothing to generic apply in primitives extension")
          | ConstBool _ -> co.Throw(Errors.Singleton "Nothing to generic apply in primitives extension")
          | ConstString _ -> co.Throw(Errors.Singleton "Nothing to generic apply in primitives extension")
          | ConstGuid _ -> co.Throw(Errors.Singleton "Nothing to generic apply in primitives extension")
          | ConstDate _ -> co.Throw(Errors.Singleton "Nothing to generic apply in primitives extension")
          | Rest r -> tailOperatorEvalExtensions.GenericApply {| inputs with typeFunc = r |}
       MatchCase =
        fun inputs ->
          match inputs.value with
          | ConstInt _ -> co.Throw(Errors.Singleton "Nothing to match case in primitives extension")
          | ConstFloat _ -> co.Throw(Errors.Singleton "Nothing to match case in primitives extension")
          | ConstBool _ -> co.Throw(Errors.Singleton "Nothing to match case in primitives extension")
          | ConstString _ -> co.Throw(Errors.Singleton "Nothing to match case in primitives extension")
          | ConstGuid _ -> co.Throw(Errors.Singleton "Nothing to match case in primitives extension")
          | ConstDate _ -> co.Throw(Errors.Singleton "Nothing to match case in primitives extension")
          | Rest r -> tailOperatorEvalExtensions.MatchCase {| inputs with value = r |} |}
