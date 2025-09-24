namespace Ballerina.DSL.Next.Extensions

[<AutoOpen>]
module Model =
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Json

  type LanguageContext<'ext> =
    { TypeCheckContext: TypeCheckContext
      TypeCheckState: TypeCheckState
      ExprEvalContext: ExprEvalContext<'ext> }

  type OperationsExtension<'ext, 'extOperations> =
    { TypeVars: List<TypeVar * Kind> // example: [ ("a", Star) ]
      // WrapTypeVars: TypeExpr -> TypeValue
      Operations:
        Map<
          Identifier,  // example: ("Int.+")
          OperationExtension<'ext, 'extOperations>
         > }

  and OperationExtension<'ext, 'extOperations> =
    { Type: TypeValue // "Int -> Int -> Int"
      Kind: Kind // *
      Operation: 'extOperations
      OperationsLens: PartialLens<'ext, 'extOperations> // lens to access the value inside the extension value
      Apply: 'extOperations * Value<TypeValue, 'ext> -> ExprEvaluator<'ext, Value<TypeValue, 'ext>> }

  and TypeExtension<'ext, 'extConstructors, 'extValues, 'extOperations> =
    { TypeName: Identifier * TypeSymbol // example: "Option"
      TypeVars: List<TypeVar * Kind> // example: [ ("a", Star) ]
      WrapTypeVars: TypeExpr -> TypeValue
      Deconstruct: 'extValues -> Value<TypeValue, 'ext> // function to extract the underlying value from a value
      Cases:
        Map<
          Identifier * TypeSymbol,  // example: ("Option.Some", "OptionSome")
          TypeCaseExtension<'ext, 'extConstructors, 'extValues>
         >
      Operations:
        Map<
          Identifier,  // example: ("Option.Some", "OptionSome")
          TypeOperationExtension<'ext, 'extConstructors, 'extValues, 'extOperations>
         >
      Parser: ValueParserLayer<TypeValue, 'ext>
      Encoder: ValueEncoderLayer<TypeValue, 'ext> }

  and TypeOperationExtension<'ext, 'extConstructors, 'extValues, 'extOperations> =
    { Type: TypeValue // "a => b => (a -> b) -> Option a -> Option b"
      Kind: Kind // * => * => *
      Operation: 'extOperations
      OperationsLens: PartialLens<'ext, 'extOperations> // lens to access the value inside the extension value
      Apply: 'extOperations * Value<TypeValue, 'ext> -> ExprEvaluator<'ext, 'ext> }

  and TypeCaseExtension<'ext, 'extConstructors, 'extValues> =
    { CaseType: TypeExpr // "a"
      ConstructorType: TypeExpr // "a => Option a"
      Constructor: 'extConstructors
      ValueLens: PartialLens<'ext, 'extValues> // lens to access the value inside the extension value
      ConsLens: PartialLens<'ext, 'extConstructors> // lens to access the constructor inside the extension value
      Apply: 'extConstructors * Value<TypeValue, 'ext> -> ExprEvaluator<'ext, Value<TypeValue, 'ext>> }
