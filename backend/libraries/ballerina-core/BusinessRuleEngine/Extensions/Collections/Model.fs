namespace Ballerina.DSL.Expr.Extensions

module Collections =
  open Ballerina.Core.Json
  open Ballerina.DSL.Expr.Model
  open FSharp.Data
  open Ballerina.Fun
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.Coroutines.Model
  open Ballerina.DSL.Expr.Eval
  open Ballerina.DSL.Parser.Expr
  open Ballerina.DSL.Parser.Patterns
  open Ballerina.DSL.Expr.Types.TypeCheck
  open Ballerina.DSL.Expr.Types.Unification
  open Ballerina.Collections.NonEmptyList
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
    : CollectionsExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
        -> (ExprEval<'ExprExtension, 'ValueExtension> -> EvalFrom<'ExprExtension, 'ValueExtension, 'ExprExtensionTail>)
        -> ExprEval<'ExprExtension, 'ValueExtension>
        -> EvalFrom<
          'ExprExtension,
          'ValueExtension,
          CollectionsExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
         > =
    fun ctx evalTail evalRoot e ->
      // let (!) = eval

      co {
        match e with
        | CollectionsExprExtension.List l ->
          let! vs = l |> List.map evalRoot |> co.All
          return CollectionsValueExtension.List vs |> ctx.toValue
        | CollectionsExprExtension.Rest tail -> return! evalTail evalRoot tail
      }

  let toJsonCollectionsValue
    : ('ExprExtensionTail -> Sum<JsonValue, Errors>)
        -> ('ValueExtensionTail -> Sum<JsonValue, Errors>)
        -> (Expr<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
        -> (Value<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
        -> CollectionsValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
        -> Sum<JsonValue, Errors> =
    fun _toJsonExprTail toJsonValueTail _toJsonRootExpr toJsonRootValue value ->
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
    : ('ExprExtensionTail -> Sum<JsonValue, Errors>)
        -> ('ValueExtensionTail -> Sum<JsonValue, Errors>)
        -> (Expr<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
        -> (Value<'ExprExtension, 'ValueExtension> -> Sum<JsonValue, Errors>)
        -> CollectionsExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
        -> Sum<JsonValue, Errors> =
    fun toJsonExprTail _toJsonValueTail toJsonRootExpr _toJsonRootValue expr ->
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
    : CollectionsExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
        -> TypeChecker<'ExprExtensionTail>
        -> TypeChecker<'ValueExtensionTail>
        -> TypeChecker<Expr<'ExprExtension, 'ValueExtension>>
        -> TypeChecker<Value<'ExprExtension, 'ValueExtension>>
        -> TypeChecker<
          CollectionsExprExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
         > =
    fun _ctx typeCheckExprTail _typeCheckValueTail typeCheckRootExpr _typeCheckRootValue typeBindings vars e ->
      let _notImplementedError exprName =
        sum.Throw(Errors.Singleton $"Error: not implemented Expr type checker for expression {exprName}")

      sum {
        match e with
        | CollectionsExprExtension.List l ->
          let! tl = l |> List.map (typeCheckRootExpr typeBindings vars) |> sum.All

          match tl with
          | [] -> return ListType(ExprType.VarType(Guid.CreateVersion7().ToString() |> VarName.Create))
          | xt :: xts ->
            do!
              xts
              |> Seq.map (ExprType.Unify vars typeBindings xt)
              |> sum.All
              |> Sum.map ignore

            return xt // ListType tl
        | CollectionsExprExtension.Rest t -> return! typeCheckExprTail typeBindings vars t
      }

  let typeCheckValueCollections
    : CollectionsExtensionContext<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
        -> TypeChecker<'ExprExtensionTail>
        -> TypeChecker<'ValueExtensionTail>
        -> TypeChecker<Expr<'ExprExtension, 'ValueExtension>>
        -> TypeChecker<Value<'ExprExtension, 'ValueExtension>>
        -> TypeChecker<
          CollectionsValueExtension<'ExprExtension, 'ValueExtension, 'ExprExtensionTail, 'ValueExtensionTail>
         > =
    fun _ctx _typeCheckExprTail typeCheckValueTail _typeCheckRootExpr typeCheckRootValue typeBindings vars v ->
      // let (!) = typeCheckRoot vars

      sum {
        // let! v = v |> ctx.fromValue

        match v with
        | List l ->
          let! tl = l |> List.map (typeCheckRootValue typeBindings vars) |> sum.All

          match tl with
          | [] -> return ListType(ExprType.VarType(Guid.CreateVersion7().ToString() |> VarName.Create))
          | xt :: xts ->
            do!
              xts
              |> Seq.map (ExprType.Unify vars typeBindings xt)
              |> sum.All
              |> Sum.map ignore

            return xt // ListType tl
        | Rest e -> return! typeCheckValueTail typeBindings vars e
      }
