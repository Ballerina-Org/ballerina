namespace Ballerina.DSL.Next.Extensions

[<AutoOpen>]
module Model =
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms
  open Ballerina.Lenses
  open Ballerina.Collections.NonEmptyList
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.TypeChecker
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Serialization
  open Ballerina.DSL.Next.Serialization.PocoObjects
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Data.Delta.Serialization
  open Ballerina.Fun


  type LanguageContext<'runtimeContext, 'ext, 'extDTO, 'deltaExt, 'deltaExtDTO
    when 'ext: comparison
    and 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct> =
    { TypeCheckContext: TypeCheckContext<'ext>
      TypeCheckState: TypeCheckState<'ext>
      ExprEvalContext: Updater<ExprEvalContext<'runtimeContext, 'ext>>
      TypeCheckedPreludes: List<TypeCheckedExpr<'ext>>
      SerializationContext:
        DeltaSerializationContext<'ext, 'extDTO, 'deltaExt, 'deltaExtDTO>
      ExtTypeChecker: Option<IsExtInstanceOf<'ext>> }

  type OperationsExtension<'runtimeContext, 'ext, 'extOperations> =
    { TypeVars: List<TypeVar * Kind> // example: [ ("a", Star) ]
      // WrapTypeVars: TypeExpr -> TypeValue
      Operations:
        Map<
          ResolvedIdentifier,  // example: ("Int.+")
          OperationExtension<'runtimeContext, 'ext, 'extOperations>
         > }

  and OperationExtension<'runtimeContext, 'ext, 'extOperations> =
    { PublicIdentifiers: Option<TypeValue<'ext> * Kind * 'extOperations>
      OperationsLens: PartialLens<'ext, 'extOperations> // lens to access the value inside the extension value
      Apply:
        Location
          -> List<TypeCheckedExpr<'ext>>
          -> 'extOperations * Value<TypeValue<'ext>, 'ext>
          -> ExprEvaluator<'runtimeContext, 'ext, Value<TypeValue<'ext>, 'ext>> }

  and TypeExtension<'runtimeContext, 'ext, 'extDTO, 'deltaExt, 'deltaExtDTO, 'extConstructors, 'extValues, 'extOperations
    when 'ext: comparison
    and 'extDTO: not struct
    and 'extDTO: not null
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct> =
    { TypeName: ResolvedIdentifier * TypeSymbol // example: "Option"
      TypeVars: List<TypeVar * Kind> // example: [ ("a", Star) ]
      // Deconstruct: 'extValues -> Value<TypeValue<'ext>, 'ext> // function to extract the underlying value from a value
      Cases:
        Map<
          ResolvedIdentifier * TypeSymbol,  // example: ("Option.Some", "OptionSome")
          TypeCaseExtension<'runtimeContext, 'ext, 'extConstructors, 'extValues>
         >
      Operations:
        Map<
          ResolvedIdentifier,  // example: ("Option.Some", "OptionSome")
          TypeOperationExtension<
            'runtimeContext,
            'ext,
            'extConstructors,
            'extValues,
            'extOperations
           >
         >
      Serialization:
        Option<DeltaSerializationContext<'ext, 'extDTO, 'deltaExt, 'deltaExtDTO>>
      ExtTypeChecker: Option<IsExtInstanceOf<'ext>> }

    static member ToImportedTypeValue
      (typeExt:
        TypeExtension<
          'runtimeContext,
          'ext,
          'extDTO,
          'deltaExt,
          'deltaExtDTO,
          'extConstructors,
          'extValues,
          'extOperations
         >)
      : ImportedTypeValue<'ext> =
      { Id = typeExt.TypeName |> fst
        Sym = typeExt.TypeName |> snd
        Parameters =
          typeExt.TypeVars
          |> List.map (fun (tv, k) -> TypeParameter.Create(tv.Name, k))
        Arguments = [] }

  and TypeOperationExtension<'runtimeContext, 'ext, 'extConstructors, 'extValues, 'extOperations>
    =
    { Type: TypeValue<'ext> // "a => b => (a -> b) -> Option a -> Option b"
      Kind: Kind // * => * => *
      Operation: 'extOperations
      OperationsLens: PartialLens<'ext, 'extOperations> // lens to access the value inside the extension value
      Apply:
        Location
          -> List<TypeCheckedExpr<'ext>>
          -> 'extOperations * Value<TypeValue<'ext>, 'ext>
          -> ExprEvaluator<'runtimeContext, 'ext, Value<TypeValue<'ext>, 'ext>> }

  and TypeCaseExtension<'runtimeContext, 'ext, 'extConstructors, 'extValues> =
    { CaseType: TypeExpr<'ext> // "a"
      ConstructorType: TypeValue<'ext> // "a => Option a"
      Constructor: 'extConstructors
      ValueLens: PartialLens<'ext, 'extValues> // lens to access the value inside the extension value
      ConsLens: PartialLens<'ext, 'extConstructors> // lens to access the constructor inside the extension value
      Apply:
        Location
          -> List<TypeCheckedExpr<'ext>>
          -> 'extConstructors * Value<TypeValue<'ext>, 'ext>
          -> ExprEvaluator<'runtimeContext, 'ext, Value<TypeValue<'ext>, 'ext>> }

  and TypeLambdaExtension<'runtimeContext, 'ext, 'extDTO, 'extTypeLambda
    when 'extDTO: not null and 'extDTO: not struct> =
    { ExtensionType: ResolvedIdentifier * TypeValue<'ext> * Kind
      ExtraBindings: Map<ResolvedIdentifier, TypeValue<'ext> * Kind>
      Value: 'extTypeLambda // eval value bindings will contain an entry from the extension identifier to this value (modulo DU packaging)
      ValueLens: PartialLens<'ext, 'extTypeLambda> // lens to handle wrapping and upwrapping between the extension value and the core value
      EvalToTypeApplicable: ExtensionEvaluator<'runtimeContext, 'ext> // implementation of what happens at runtime when the extension is type applied (instantiation)
      EvalToApplicable: ExtensionEvaluator<'runtimeContext, 'ext> } // implementation of what happens at runtime when the extension is applied

  type ExtensionPrelude = ExtensionPrelude of string
