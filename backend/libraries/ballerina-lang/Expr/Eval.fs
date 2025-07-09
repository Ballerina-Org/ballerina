namespace Ballerina.DSL.Expr

module Eval =
  open Ballerina.Fun
  open Ballerina.Coroutines.Model
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.Errors

  type ExprEvalContext<'ExprExtension, 'ValueExtension> =
    { Vars: Vars<'ExprExtension, 'ValueExtension>
      Types: TypeBindings }

    static member Update =
      {| Vars =
          fun
              (f: Updater<Vars<'ExprExtension, 'ValueExtension>>)
              (ctx: ExprEvalContext<'ExprExtension, 'ValueExtension>) -> { ctx with Vars = f ctx.Vars }
         Types =
          fun (f: Updater<TypeBindings>) (ctx: ExprEvalContext<'ExprExtension, 'ValueExtension>) ->
            { ctx with Types = f ctx.Types } |}

  type ExprEvalState = unit

  type EvalCoroutine<'ExprExtension, 'ValueExtension> =
    Coroutine<
      Value<'ExprExtension, 'ValueExtension>,
      ExprEvalState,
      ExprEvalContext<'ExprExtension, 'ValueExtension>,
      Unit,
      Errors
     >

  type ExprEval<'ExprExtension, 'ValueExtension> =
    (Expr<'ExprExtension, 'ValueExtension>) -> EvalCoroutine<'ExprExtension, 'ValueExtension>

  type EvalFrom<'ExprExtension, 'ValueExtension, 'ExprExtensionTail> =
    'ExprExtensionTail -> EvalCoroutine<'ExprExtension, 'ValueExtension>

  type OperatorEvalExtensions<'ExprExtension, 'ValueExtension, 'ValueExtensionTail> =
    {| Apply:
         {| func: 'ValueExtensionTail
            arg: Value<'ExprExtension, 'ValueExtension> |}
           -> EvalCoroutine<'ExprExtension, 'ValueExtension>
       GenericApply:
         {| typeFunc: 'ValueExtensionTail
            typeArg: ExprType
            types: TypeBindings |}
           -> EvalCoroutine<'ExprExtension, 'ValueExtension>
       MatchCase:
         {| value: 'ValueExtensionTail
            caseHandlers: Map<string, (VarName * Expr<'ExprExtension, 'ValueExtension>)> |}
           -> EvalCoroutine<'ExprExtension, 'ValueExtension> |}

  module OperatorEvalExtensions =
    let preprocessTail<'ExprExtension, 'ValueExtension, 'ValueExtensionTail, 'ValueExtensionTail2>
      (f: 'ValueExtensionTail2 -> 'ValueExtensionTail)
      (i: OperatorEvalExtensions<'ExprExtension, 'ValueExtension, 'ValueExtensionTail>)
      : OperatorEvalExtensions<'ExprExtension, 'ValueExtension, 'ValueExtensionTail2> =
      {| Apply = fun inputs -> i.Apply {| inputs with func = f inputs.func |}
         GenericApply =
          fun inputs ->
            i.GenericApply
              {| inputs with
                  typeFunc = f inputs.typeFunc |}
         MatchCase = fun inputs -> i.MatchCase {| inputs with value = f inputs.value |} |}

  type Expr<'ExprExtension, 'ValueExtension> with
    static member eval
      (operatorEvalExtensions: OperatorEvalExtensions<'ExprExtension, 'ValueExtension, 'ValueExtension>)
      (evalExtension:
        ExprEval<'ExprExtension, 'ValueExtension> -> EvalFrom<'ExprExtension, 'ValueExtension, 'ExprExtension>)
      : ExprEval<'ExprExtension, 'ValueExtension> =
      fun e ->
        let (!) = Expr.eval operatorEvalExtensions evalExtension

        co {
          match e with
          | Apply(f, arg) ->
            let! fValue = !f
            let! arg = !arg

            match fValue with
            | Value.Lambda(v, _, _, b) -> return! !b |> co.mapContext (ExprEvalContext.Update.Vars(Map.add v arg))
            | Value.Extension extensionFunc ->
              return! operatorEvalExtensions.Apply {| func = extensionFunc; arg = arg |}
            | _ ->
              return!
                $"runtime error: {fValue} should be a function because it is applied"
                |> Errors.Singleton
                |> co.Throw

          | VarLookup varName ->
            let! ctx = co.GetContext()
            let! varValue = ctx.Vars |> Map.tryFindWithError varName "var" varName.VarName |> co.ofSum

            return varValue
          | Value v -> return v
          | LetType(typeName, typeDef, in_) ->
            return! !in_ |> co.mapContext (ExprEvalContext.Update.Types(Map.add typeName typeDef))
          | Let(x, expr, in_) ->
            let! expr = !expr
            return! !in_ |> co.mapContext (ExprEvalContext.Update.Vars(Map.add x expr))

          | MakeRecord fields ->
            let! evaluatedFields =
              fields
              |> Map.toSeq
              |> Seq.map (fun (fieldName, expr) ->
                co {
                  let! value = !expr
                  return (fieldName, value)
                })
              |> Seq.toList
              |> co.All

            return Value.Record(Map.ofSeq evaluatedFields)

          | MatchCase(value, caseHandlers) ->
            let! (value: Value<'ExprExtension, 'ValueExtension>) = !value

            match value with
            | Value.CaseCons(caseName, caseValue) ->
              let! varName, handler = caseHandlers |> Map.tryFindWithError caseName "case name" caseName |> co.ofSum

              return!
                !handler
                |> co.mapContext (ExprEvalContext.Update.Vars(Map.add varName caseValue))
            | Value.Extension value ->
              return!
                operatorEvalExtensions.MatchCase
                  {| value = value
                     caseHandlers = caseHandlers |}
            | _ ->
              return!
                $"runtime error: {value} should be a case constructor because it is matched"
                |> Errors.Singleton
                |> co.Throw

          | MakeCase(caseName, value) ->
            let! value = !value
            return Value.CaseCons(caseName, value)

          | RecordFieldLookup(e, fieldName) ->
            let! e = !e

            match e with
            | Value.Record fields ->
              match fields |> Map.tryFind fieldName with
              | Some v -> return v
              | None ->
                return!
                  $"runtime error: {fieldName} not found in record {e}"
                  |> Errors.Singleton
                  |> co.Throw
            | _ ->
              return!
                $"runtime error: {e} should be a record because it is field looked up"
                |> Errors.Singleton
                |> co.Throw

          | Expr.GenericApply(typeF, typeArg) ->
            let! typeF = !typeF

            match typeF with
            | Value.GenericLambda(_, b) -> return! !b
            | Value.Extension extensionTypeFunc ->
              let! ctx = co.GetContext()

              return!
                operatorEvalExtensions.GenericApply
                  {| typeFunc = extensionTypeFunc
                     typeArg = typeArg
                     types = ctx.Types |}
            | _ ->
              return!
                $"runtime error: {e} should be a generic lambda because it is applied"
                |> Errors.Singleton
                |> co.Throw
          | Expr.Extension e -> return! evalExtension (!) e
          | e -> return! $"runtime error: eval({e}) not implemented" |> Errors.Singleton |> co.Throw
        }
