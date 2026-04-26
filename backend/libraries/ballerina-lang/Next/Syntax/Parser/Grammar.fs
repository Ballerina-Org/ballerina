namespace Ballerina.DSL.Next.Syntax.Parser

open Ballerina.Grammar

module Grammar =

  let allRules: NamedRule list =
    Common.grammarRules
    @ Kind.grammarRules
    @ TypeHooksAndProperties.grammarRules
    @ TypeSchema.grammarRules
    @ Type.grammarRules
    @ Query.grammarRules
    @ View.grammarRules
    @ Expr.grammarRules
