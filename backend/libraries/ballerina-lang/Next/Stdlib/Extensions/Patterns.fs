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
  open Ballerina.Data.Delta.Serialization

  type LanguageContext<'runtimeContext, 'ext, 'extDTO, 'deltaExt, 'deltaExtDTO
    when 'ext: comparison
    and 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct> with
    static member Empty: LanguageContext<'runtimeContext, 'ext, 'extDTO, 'deltaExt, 'deltaExtDTO> =
      { TypeCheckContext = TypeCheckContext.Empty("", "")
        TypeCheckState = TypeCheckState.Empty
        ExprEvalContext = id
        TypeCheckedPreludes = []
        SerializationContext = DeltaSerializationContext.Create SerializationContext.Empty
        ExtTypeChecker = None }
