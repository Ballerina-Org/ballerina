namespace Ballerina.DSL.Expr

module Extension =
  open Ballerina.DSL.Expr.Model
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.Coroutines.Model
  open Ballerina.DSL.Expr.Eval

  type Parse<'ExprExtension, 'ValueExtension> = JsonValue -> Sum<Expr<'ExprExtension, 'ValueExtension>, Errors>

  type ParserExtension<'ExprExtension, 'ValueExtension> =
    { parseExtension: Parse<'ExprExtension, 'ValueExtension> -> Parse<'ExprExtension, 'ValueExtension> }

  type TypeCheck<'ExprExtension, 'ValueExtension> =
    (TypeBindings) -> (VarTypes) -> (Expr<'ExprExtension, 'ValueExtension>) -> Sum<ExprType, Errors>

  type TypeCheckerExtension<'ExprExtension, 'ValueExtension> =
    { typeCheck: TypeCheck<'ExprExtension, 'ValueExtension> -> TypeCheck<'ExprExtension, 'ValueExtension> }

  type Eval<'ExprExtension, 'ValueExtension> =
    (Expr<'ExprExtension, 'ValueExtension>)
      -> Coroutine<
        Value<'ExprExtension, 'ValueExtension>,
        ExprEvalState,
        ExprEvalContext<'ExprExtension, 'ValueExtension>,
        Unit,
        Errors
       >

  type EvaluatorExtension<'ExprExtension, 'ValueExtension> =
    { eval: Eval<'ExprExtension, 'ValueExtension> -> Eval<'ExprExtension, 'ValueExtension> }

  type LanguageExtension<'ExprExtension, 'ValueExtension> =
    { parse: EvaluatorExtension<'ExprExtension, 'ValueExtension>
      typeCheck: EvaluatorExtension<'ExprExtension, 'ValueExtension>
      eval: EvaluatorExtension<'ExprExtension, 'ValueExtension> }
