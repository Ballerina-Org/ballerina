namespace Ballerina.DSL.Next.Types.Json

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Json.Keys

[<AutoOpen>]
module WithSourceMapping =
  open FSharp.Data
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.StdLib.Json.Patterns

  type TypeCheckScope with
    static member FromJson: JsonValue -> Sum<TypeCheckScope, Errors<_>> =
      fun scope ->
        sum {
          let! fields = scope |> JsonValue.AsRecord
          let fields = Map.ofArray fields


          let! ty = fields |> Map.tryFindWithError "type" "TypeCheckScope" (fun () -> "type") ()
          let ty = JsonValue.AsString ty |> Sum.toOption
          let! md = fields |> Map.tryFindWithError "module" "TypeCheckScope" (fun () -> "module") ()
          let! md = JsonValue.AsString md

          let! assembly =
            fields
            |> Map.tryFindWithError "assembly" "TypeCheckScope" (fun () -> "assembly") ()

          let! assembly = JsonValue.AsString assembly

          return
            { Assembly = assembly
              Module = md
              Type = ty }
        }

    static member ToJson: TypeCheckScope -> JsonValue =
      fun scope ->
        let assembly = JsonValue.String scope.Assembly
        let md = JsonValue.String scope.Module

        let ty =
          match scope.Type with
          | Some t -> JsonValue.String t
          | None -> JsonValue.Null

        JsonValue.Record [| "assembly", assembly; "module", md; "type", ty |]

  type TypeExprSourceMapping<'valueExt> with
    static member FromJson
      (typeExprFromJson: JsonValue -> Sum<TypeExpr<'valueExt>, Errors<_>>)
      : JsonValue -> Sum<TypeExprSourceMapping<'valueExt>, Errors<_>> =
      fun mapping ->
        sum {
          let! mapping = JsonValue.AsRecord mapping |> sum.Map Map.ofArray

          let! ty =
            mapping
            |> Map.tryFindWithError "type" "TypeExprSourceMapping" (fun () -> "type") ()

          let! ty = JsonValue.AsString ty

          let! value =
            mapping
            |> Map.tryFindWithError "value" "TypeExprSourceMapping" (fun () -> "value") ()

          match ty with
          | "noSourceMapping" ->
            let! s = JsonValue.AsString value
            return NoSourceMapping s
          | "originTypeExpr" ->
            let! typeExpr = typeExprFromJson value
            return OriginTypeExpr typeExpr
          | "originTypeExprLet" ->
            let! fields = JsonValue.AsRecord value |> sum.Map Map.ofArray

            let! bindingName =
              fields
              |> Map.tryFindWithError "bindingName" "originTypeExprLet" (fun () -> "bindingName") ()
              |> Sum.bind JsonValue.AsString

            let! typeExpr =
              fields
              |> Map.tryFindWithError "typeExpr" "originTypeExprLet" (fun () -> "typeExpr") ()
              |> Sum.bind typeExprFromJson

            return OriginExprTypeLet(ExprTypeLetBindingName bindingName, typeExpr)
          | other ->
            return! sum.Throw(Errors.Singleton () (fun () -> $"Unexpected TypeExprSourceMapping type: {other}"))
        }

    static member ToJson
      (typeExprToJson: TypeExpr<'valueExt> -> JsonValue)
      : TypeExprSourceMapping<'valueExt> -> JsonValue =
      function
      | NoSourceMapping s ->
        JsonValue.Record [| "type", JsonValue.String "noSourceMapping"; "value", JsonValue.String s |]
      | OriginTypeExpr typeExpr ->
        JsonValue.Record
          [| "type", JsonValue.String "originTypeExpr"
             "value", typeExprToJson typeExpr |]

      | OriginExprTypeLet(ExprTypeLetBindingName bindingName, typeExpr) ->
        JsonValue.Record
          [| "type", JsonValue.String "originTypeExprLet"
             "value",
             JsonValue.Record
               [| "bindingName", JsonValue.String bindingName
                  "typeExpr", typeExprToJson typeExpr |] |]

  let private discriminator = "withSourceMapping"

  type WithSourceMapping<'v, 'valueExt> with
    static member FromJson
      (valueFromJson: JsonValue -> Sum<'v, Errors<_>>)
      (typeExprFromJson: JsonValue -> Sum<TypeExpr<'valueExt>, Errors<_>>)
      : JsonValue -> Sum<WithSourceMapping<'v, 'valueExt>, Errors<_>> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun withSourceMapping ->
        sum {
          let! withSourceMapping = withSourceMapping |> JsonValue.AsRecord |> sum.Map Map.ofArray

          let! scope =
            withSourceMapping
            |> Map.tryFindWithError "typeCheckScopeSource" "WithSourceMapping" (fun () -> "typeCheckScopeSource") ()

          let! scope = TypeCheckScope.FromJson scope

          let! typeExpr =
            withSourceMapping
            |> Map.tryFindWithError "typeExprSource" "WithSourceMapping" (fun () -> "typeExprSource") ()

          let! typeExpr = TypeExprSourceMapping.FromJson typeExprFromJson typeExpr

          let! value =
            withSourceMapping
            |> Map.tryFindWithError "value" "WithSourceMapping" (fun () -> "value") ()

          let! value = valueFromJson value

          return
            { value = value
              typeExprSource = typeExpr
              typeCheckScopeSource = scope }
        })

    static member ToJson
      (valueToJson: 'v -> JsonValue)
      (typeExprToJson: TypeExpr<'valueExt> -> JsonValue)
      (mapping: WithSourceMapping<'v, 'valueExt>)

      : JsonValue =
      let typeCheckScopeSource = TypeCheckScope.ToJson mapping.typeCheckScopeSource

      let typeExprSource =
        TypeExprSourceMapping.ToJson typeExprToJson mapping.typeExprSource

      let value = valueToJson mapping.value

      JsonValue.Record
        [| "typeCheckScopeSource", typeCheckScopeSource
           "typeExprSource", typeExprSource
           "value", value |]
      |> Json.discriminator discriminator
