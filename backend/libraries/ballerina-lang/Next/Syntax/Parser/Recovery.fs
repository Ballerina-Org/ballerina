namespace Ballerina.DSL.Next.Syntax.Parser

[<AutoOpen>]
module Recovery =

  open Ballerina.Parser
  open Ballerina
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Model
  open Common

  /// Create a RecoveredSyntaxError expression node.
  /// Use this in the parser whenever a required token is missing but parsing
  /// can still meaningfully continue.  The typechecker will surface the error
  /// when it reaches the node.
  let createRecoveredError
    (errorMessage: string)
    (errorLocation: Location)
    (recoveryContext: string)
    : Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>
    =
    let errorData: ExprRecoveredSyntaxError<TypeExpr<'valueExt>, Identifier, 'valueExt> =
      { ErrorMessage = errorMessage
        ErrorLocation = errorLocation
        RecoveryContext = recoveryContext }

    { Expr = ExprRec.RecoveredSyntaxError(errorData)
      Scope = TypeCheckScope.Empty
      Location = errorLocation }


