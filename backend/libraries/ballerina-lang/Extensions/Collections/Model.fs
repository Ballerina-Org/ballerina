namespace Ballerina.DSL.Expr.Extensions

[<RequireQualifiedAccess>]
module Collections =
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.DSL.Expr.Model
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.Coroutines.Model
  open Ballerina.DSL.Expr.Eval
  open Ballerina.DSL.Parser.Expr
  open Ballerina.DSL.Parser.Patterns
  open Ballerina.DSL.Expr.Types.TypeCheck
  open Ballerina.DSL.Expr.Types.Unification
  open System
  open Ballerina.DSL.Expr.Types.Model

  type ExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail> =
    | List of List<Expr<'ExprExtension, 'ValueExtension>>
    | Rest of 'ExprExtensionTail

    override e.ToString() =
      match e with
      | List es ->
        let formattedValues = es |> Seq.map (fun e -> e.ToString())
        $"""[{String.Join(", ", formattedValues)}]"""
      | Rest t -> t.ToString()


  type ValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail> =
    | List of List<Value<'ExprExtension, 'ValueExtension>>
    | Rest of 'ValueExtensionTail

    override v.ToString() =
      match v with
      | List vs ->
        let formattedValues = vs |> Seq.map (fun v -> v.ToString())
        $"""[{String.Join(", ", formattedValues)}]"""
      | Rest e -> e.ToString()

  type ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail> =
    { fromExpr:
        Expr<'ExprExtension, 'ValueExtension>
          -> Sum<ExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>, Errors>
      toExpr:
        ExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
          -> Expr<'ExprExtension, 'ValueExtension>
      fromValue:
        Value<'ExprExtension, 'ValueExtension>
          -> Sum<ValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>, Errors>
      toValue:
        ValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
          -> Value<'ExprExtension, 'ValueExtension> }


  type ExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail> with
    static member AsList
      (ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (v: Expr<'ExprExtension, 'ValueExtension>)
      : Sum<List<Expr<'ExprExtension, 'ValueExtension>>, Errors> =
      sum {
        let! v = ctx.fromExpr v

        match v with
        | ExprExtension.List l -> return l
        | _ -> return! sum.Throw(Errors.Singleton $"Error: expected list, found {v.ToString()}")
      }

  type ValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail> with
    static member AsList
      (ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (v: Value<'ExprExtension, 'ValueExtension>)
      : Sum<List<Value<'ExprExtension, 'ValueExtension>>, Errors> =
      sum {
        let! v = ctx.fromValue v

        match v with
        | ValueExtension.List v -> return v
        | _ -> return! sum.Throw(Errors.Singleton $"Error: expected list, found {v.ToString()}")
      }

  type Expr<'ExprExtension, 'ValueExtension> with
    static member private ParseList
      (ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (parseRootExpr: ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
      sum {
        let! fieldsJson = assertKindIsAndGetFields "list" json

        return!
          sum {
            let! elementsJson = fieldsJson |> sum.TryFindField "elements"
            let! elementsArray = elementsJson |> JsonValue.AsArray
            let! elements = elementsArray |> Array.toList |> List.map parseRootExpr |> sum.All
            return ExprExtension.List elements |> ctx.toExpr
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

  let parse
    : ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
        -> ExprParser<'ExprExtension, 'ValueExtension>
        -> JsonValue
        -> Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
    fun ctx parseRootExpr jsonValue -> Expr.ParseList ctx parseRootExpr jsonValue

  let eval
    (ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
    (evalTail:
      ExprEval<'ExprExtension, 'ValueExtension> -> EvalFrom<'ExprExtension, 'ValueExtension, 'ExprExtensionTail>)
    (evalRoot: ExprEval<'ExprExtension, 'ValueExtension>)
    : EvalFrom<
        'ExprExtension,
        'ValueExtension,
        ExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
       >
    =
    fun e ->
      // let (!) = eval

      co {
        match e with
        | ExprExtension.List l ->
          let! vs = l |> List.map evalRoot |> co.All
          return ValueExtension.List vs |> ctx.toValue
        | ExprExtension.Rest tail -> return! evalTail evalRoot tail
      }

  let toJsonValue
    (_toJsonExprTail: 'ExprExtensionTail -> Sum<JsonValue, Errors>)
    (toJsonValueTail: 'ValueExtensionTail -> Sum<JsonValue, Errors>)
    (_toJsonRootExpr: Expr<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
    (toJsonRootValue: Value<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
    (value: ValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
    : Sum<JsonValue, Errors> =
    sum {
      match value with
      | List elements ->
        let! jsonElements = elements |> List.map toJsonRootValue |> sum.All

        return
          JsonValue.Record
            [| "kind", JsonValue.String "list"
               "elements", jsonElements |> Array.ofList |> JsonValue.Array |]
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
      | ExprExtension.List elements ->
        let! jsonElements = elements |> List.map toJsonRootExpr |> sum.All

        return
          JsonValue.Record
            [| "kind", JsonValue.String "list"
               "elements", jsonElements |> Array.ofList |> JsonValue.Array |]
      | ExprExtension.Rest t -> return! toJsonExprTail t
    }

  let collectionsTypeBindings: TypeBindings = Map.empty

  let typeCheckExpr
    (_ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
    (typeCheckExprTail: TypeChecker<'ExprExtensionTail>)
    (_typeCheckValueTail: TypeChecker<'ValueExtensionTail>)
    (typeCheckRootExpr: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
    (_typeCheckRootValue: TypeChecker<Value<'ExprExtension, 'ValueExtension>>)
    : TypeChecker<ExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>> =
    fun typeBindings vars e ->
      let _notImplementedError exprName =
        sum.Throw(Errors.Singleton $"Error: not implemented Expr type checker for expression {exprName}")

      let checkInnerType (elementTypes: List<ExprType>) : Sum<ExprType, Errors> =
        sum {
          match elementTypes with
          | [] -> ExprType.VarType(Guid.CreateVersion7().ToString() |> VarName.Create)
          | xt :: xts ->
            do!
              xts
              |> Seq.map (ExprType.Unify vars typeBindings xt)
              |> sum.All
              |> Sum.map ignore

            xt
        }

      sum {
        match e with
        | ExprExtension.List l ->
          let! tl = l |> List.map (typeCheckRootExpr typeBindings vars) |> sum.All
          let! innerType = checkInnerType tl
          ListType innerType
        | ExprExtension.Rest t -> return! typeCheckExprTail typeBindings vars t
      }

  let typeCheckValue
    (_ctx: ExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
    (_typeCheckExprTail: TypeChecker<'ExprExtensionTail>)
    (typeCheckValueTail: TypeChecker<'ValueExtensionTail>)
    (_typeCheckRootExpr: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
    (typeCheckRootValue: TypeChecker<Value<'ExprExtension, 'ValueExtension>>)
    : TypeChecker<ValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>> =
    fun typeBindings vars v ->
      // let (!) = typeCheckRoot vars

      let checkInnerType (elementTypes: List<ExprType>) : Sum<ExprType, Errors> =
        sum {
          match elementTypes with
          | [] -> ExprType.VarType(Guid.CreateVersion7().ToString() |> VarName.Create)
          | xt :: xts ->
            do!
              xts
              |> Seq.map (ExprType.Unify vars typeBindings xt)
              |> sum.All
              |> Sum.map ignore

            xt
        }

      sum {
        // let! v = v |> ctx.fromValue

        match v with
        | List l ->
          let! tl = l |> List.map (typeCheckRootValue typeBindings vars) |> sum.All
          let! innerType = checkInnerType tl
          ListType innerType
        | Rest e -> return! typeCheckValueTail typeBindings vars e
      }

  let operatorEvalExtension
    (tailOperatorEvalExtensions: OperatorEvalExtensions<'ExprExtension, 'ValueExtension, 'ValueExtensionTail>)
    : OperatorEvalExtensions<
        'ExprExtension,
        'ValueExtension,
        ValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
       >
    =
    {| Apply =
        fun inputs ->
          match inputs.func with
          | ValueExtension.List _ -> co.Throw(Errors.Singleton "Nothing to apply in collections extension")
          | ValueExtension.Rest r -> tailOperatorEvalExtensions.Apply {| inputs with func = r |}
       GenericApply =
        fun inputs ->
          match inputs.typeFunc with
          | ValueExtension.List _ -> co.Throw(Errors.Singleton "Nothing to generic apply in collections extension")
          | ValueExtension.Rest r -> tailOperatorEvalExtensions.GenericApply {| inputs with typeFunc = r |}
       MatchCase =
        fun inputs ->
          match inputs.value with
          | ValueExtension.List _ -> co.Throw(Errors.Singleton "Nothing to match case in collections extension")
          | ValueExtension.Rest r -> tailOperatorEvalExtensions.MatchCase {| inputs with value = r |} |}
