namespace Ballerina.DSL.Expr.Extensions

module Collections =
  open Ballerina.StdLib.Json
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

  type CollectionsExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail> =
    | List of List<Expr<'ExprExtension, 'ValueExtension>>
    | Rest of 'ExprExtensionTail

    override e.ToString() =
      match e with
      | List es ->
        let formattedValues = es |> Seq.map (fun e -> e.ToString())
        $"""[{String.Join(", ", formattedValues)}]"""
      | Rest t -> t.ToString()


  type CollectionsValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail> =
    | List of List<Value<'ExprExtension, 'ValueExtension>>
    | Rest of 'ValueExtensionTail

    override v.ToString() =
      match v with
      | List vs ->
        let formattedValues = vs |> Seq.map (fun v -> v.ToString())
        $"""[{String.Join(", ", formattedValues)}]"""
      | Rest e -> e.ToString()

  type CollectionsExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail> =
    { fromExpr:
        Expr<'ExprExtension, 'ValueExtension>
          -> Sum<
            CollectionsExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>,
            Errors
           >
      toExpr:
        CollectionsExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
          -> Expr<'ExprExtension, 'ValueExtension>
      fromValue:
        Value<'ExprExtension, 'ValueExtension>
          -> Sum<
            CollectionsValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>,
            Errors
           >
      toValue:
        CollectionsValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
          -> Value<'ExprExtension, 'ValueExtension> }


  type CollectionsExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail> with
    static member AsList
      (ctx: CollectionsExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (v: Expr<'ExprExtension, 'ValueExtension>)
      : Sum<List<Expr<'ExprExtension, 'ValueExtension>>, Errors> =
      sum {
        let! v = ctx.fromExpr v

        match v with
        | CollectionsExprExtension.List l -> return l
        | _ -> return! sum.Throw(Errors.Singleton $"Error: expected list, found {v.ToString()}")
      }

  type CollectionsValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail> with
    static member AsList
      (ctx: CollectionsExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
      (v: Value<'ExprExtension, 'ValueExtension>)
      : Sum<List<Value<'ExprExtension, 'ValueExtension>>, Errors> =
      sum {
        let! v = ctx.fromValue v

        match v with
        | CollectionsValueExtension.List v -> return v
        | _ -> return! sum.Throw(Errors.Singleton $"Error: expected list, found {v.ToString()}")
      }

  type Expr<'ExprExtension, 'ValueExtension> with
    static member private ParseList
      (ctx: CollectionsExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
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
            return CollectionsExprExtension.List elements |> ctx.toExpr
          }
          |> sum.MapError(Errors.WithPriority ErrorPriority.High)
      }

  let parseCollections
    : CollectionsExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
        -> ExprParser<'ExprExtension, 'ValueExtension>
        -> JsonValue
        -> Sum<Expr<'ExprExtension, 'ValueExtension>, Errors> =
    fun ctx parseRootExpr jsonValue -> Expr.ParseList ctx parseRootExpr jsonValue

  let evalCollections
    (ctx: CollectionsExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
    (evalTail:
      ExprEval<'ExprExtension, 'ValueExtension> -> EvalFrom<'ExprExtension, 'ValueExtension, 'ExprExtensionTail>)
    (evalRoot: ExprEval<'ExprExtension, 'ValueExtension>)
    : EvalFrom<
        'ExprExtension,
        'ValueExtension,
        CollectionsExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
       >
    =
    fun e ->
      // let (!) = eval

      co {
        match e with
        | CollectionsExprExtension.List l ->
          let! vs = l |> List.map evalRoot |> co.All
          return CollectionsValueExtension.List vs |> ctx.toValue
        | CollectionsExprExtension.Rest tail -> return! evalTail evalRoot tail
      }

  let toJsonCollectionsValue
    (_toJsonExprTail: 'ExprExtensionTail -> Sum<JsonValue, Errors>)
    (toJsonValueTail: 'ValueExtensionTail -> Sum<JsonValue, Errors>)
    (_toJsonRootExpr: Expr<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
    (toJsonRootValue: Value<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
    (value: CollectionsValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
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

  let toJsonCollectionsExpr
    (toJsonExprTail: 'ExprExtensionTail -> Sum<JsonValue, Errors>)
    (_toJsonValueTail: 'ValueExtensionTail -> Sum<JsonValue, Errors>)
    (toJsonRootExpr: Expr<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
    (_toJsonRootValue: Value<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
    (expr: CollectionsExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
    : Sum<JsonValue, Errors> =
    sum {
      match expr with
      | CollectionsExprExtension.List elements ->
        let! jsonElements = elements |> List.map toJsonRootExpr |> sum.All

        return
          JsonValue.Record
            [| "kind", JsonValue.String "list"
               "elements", jsonElements |> Array.ofList |> JsonValue.Array |]
      | CollectionsExprExtension.Rest t -> return! toJsonExprTail t
    }

  let typeCheckExprCollections
    (_ctx: CollectionsExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
    (typeCheckExprTail: TypeChecker<'ExprExtensionTail>)
    (_typeCheckValueTail: TypeChecker<'ValueExtensionTail>)
    (typeCheckRootExpr: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
    (_typeCheckRootValue: TypeChecker<Value<'ExprExtension, 'ValueExtension>>)
    : TypeChecker<CollectionsExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>> =
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
        | CollectionsExprExtension.List l ->
          let! tl = l |> List.map (typeCheckRootExpr typeBindings vars) |> sum.All
          let! innerType = checkInnerType tl
          ListType innerType
        | CollectionsExprExtension.Rest t -> return! typeCheckExprTail typeBindings vars t
      }

  let typeCheckValueCollections
    (_ctx: CollectionsExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>)
    (_typeCheckExprTail: TypeChecker<'ExprExtensionTail>)
    (typeCheckValueTail: TypeChecker<'ValueExtensionTail>)
    (_typeCheckRootExpr: TypeChecker<Expr<'ExprExtension, 'ValueExtension>>)
    (typeCheckRootValue: TypeChecker<Value<'ExprExtension, 'ValueExtension>>)
    : TypeChecker<CollectionsValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>> =
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

  let operatorEvalCollectionsExtension
    (tailOperatorEvalExtensions: OperatorEvalExtensions<'ExprExtension, 'ValueExtension, 'ValueExtensionTail>)
    : OperatorEvalExtensions<
        'ExprExtension,
        'ValueExtension,
        CollectionsValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
       >
    =
    {| Apply =
        fun inputs ->
          match inputs.func with
          | CollectionsValueExtension.List _ -> co.Throw(Errors.Singleton "Nothing to apply in collections extension")
          | CollectionsValueExtension.Rest r -> tailOperatorEvalExtensions.Apply {| inputs with func = r |}
       GenericApply =
        fun inputs ->
          match inputs.typeFunc with
          | CollectionsValueExtension.List _ ->
            co.Throw(Errors.Singleton "Nothing to generic apply in collections extension")
          | CollectionsValueExtension.Rest r -> tailOperatorEvalExtensions.GenericApply {| inputs with typeFunc = r |}
       MatchCase =
        fun inputs ->
          match inputs.value with
          | CollectionsValueExtension.List _ ->
            co.Throw(Errors.Singleton "Nothing to match case in collections extension")
          | CollectionsValueExtension.Rest r -> tailOperatorEvalExtensions.MatchCase {| inputs with value = r |} |}
