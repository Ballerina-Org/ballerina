namespace Ballerina.DSL.Next.Extensions

[<AutoOpen>]
module Patterns =
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Next.Serialization

  type LanguageContext<'ext, 'extDTO when 'ext: comparison and 'extDTO: not null and 'extDTO: not struct> with
    static member Empty: LanguageContext<'ext, 'extDTO> =
      { TypeCheckContext = TypeCheckContext.Empty("", "")
        TypeCheckState = TypeCheckState.Empty
        ExprEvalContext = ExprEvalContext.Empty
        TypeCheckedPreludes = []
        SerializationContext = SerializationContext.Empty }
