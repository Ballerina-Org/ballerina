namespace Ballerina.DSL.Next.Extensions

[<AutoOpen>]
module Patterns =
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms

  type LanguageContext<'ext> with
    static member Empty: LanguageContext<'ext> =
      { TypeCheckContext = TypeCheckContext.Empty
        TypeCheckState = TypeCheckState.Empty
        ExprEvalContext = ExprEvalContext.Empty }
