namespace Ballerina.DSL.Next.Extensions

[<AutoOpen>]
module Patterns =
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns

  type LanguageContext<'ext when 'ext: comparison> with
    static member Empty: LanguageContext<'ext> =
      { TypeCheckContext = TypeCheckContext.Empty("", "")
        TypeCheckState = TypeCheckState.Empty
        ExprEvalContext = ExprEvalContext.Empty
        TypeCheckedPreludes = [] }
