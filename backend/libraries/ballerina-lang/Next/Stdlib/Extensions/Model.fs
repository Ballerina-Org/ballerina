namespace Ballerina.DSL.Next.Extensions

[<AutoOpen>]
module Model =
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms
  open Ballerina.Lenses
  open Ballerina.Collections.NonEmptyList
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.Cat.Collections.OrderedMap

  type LanguageContext<'ext when 'ext: comparison> =
    { TypeCheckContext: TypeCheckContext<'ext>
      TypeCheckState: TypeCheckState<'ext>
      ExprEvalContext: ExprEvalContext<'ext>
      TypeCheckedPreludes: List<Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>> }

  type OperationsExtension<'ext, 'extOperations> =
    { TypeVars: List<TypeVar * Kind> // example: [ ("a", Star) ]
      // WrapTypeVars: TypeExpr -> TypeValue
      Operations:
        Map<
          ResolvedIdentifier,  // example: ("Int.+")
          OperationExtension<'ext, 'extOperations>
         > }

  and OperationExtension<'ext, 'extOperations> =
    { PublicIdentifiers: Option<TypeValue<'ext> * Kind * 'extOperations>
      OperationsLens: PartialLens<'ext, 'extOperations> // lens to access the value inside the extension value
      Apply:
        Location
          -> List<Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>>
          -> 'extOperations * Value<TypeValue<'ext>, 'ext>
          -> ExprEvaluator<'ext, Value<TypeValue<'ext>, 'ext>> }

  and TypeExtension<'ext, 'extConstructors, 'extValues, 'extOperations> =
    { TypeName: ResolvedIdentifier * TypeSymbol // example: "Option"
      TypeVars: List<TypeVar * Kind> // example: [ ("a", Star) ]
      // Deconstruct: 'extValues -> Value<TypeValue<'ext>, 'ext> // function to extract the underlying value from a value
      Cases:
        Map<
          ResolvedIdentifier * TypeSymbol,  // example: ("Option.Some", "OptionSome")
          TypeCaseExtension<'ext, 'extConstructors, 'extValues>
         >
      Operations:
        Map<
          ResolvedIdentifier,  // example: ("Option.Some", "OptionSome")
          TypeOperationExtension<'ext, 'extConstructors, 'extValues, 'extOperations>
         > }

    static member ToImportedTypeValue
      (typeExt: TypeExtension<'ext, 'extConstructors, 'extValues, 'extOperations>)
      : ImportedTypeValue<'ext> =
      { Id = typeExt.TypeName |> fst
        Sym = typeExt.TypeName |> snd
        Parameters = typeExt.TypeVars |> List.map (fun (tv, k) -> TypeParameter.Create(tv.Name, k))
        Arguments = []
        UnionLike =
          if typeExt.Cases |> Map.isEmpty then
            None
          else
            typeExt.Cases
            |> Map.toSeq
            |> Seq.map (fun ((_, sym), caseExt) -> (sym, caseExt.CaseType))
            |> OrderedMap.ofSeq
            |> Some
        RecordLike = None }

  and TypeOperationExtension<'ext, 'extConstructors, 'extValues, 'extOperations> =
    { Type: TypeValue<'ext> // "a => b => (a -> b) -> Option a -> Option b"
      Kind: Kind // * => * => *
      Operation: 'extOperations
      OperationsLens: PartialLens<'ext, 'extOperations> // lens to access the value inside the extension value
      Apply:
        Location
          -> List<Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>>
          -> 'extOperations * Value<TypeValue<'ext>, 'ext>
          -> ExprEvaluator<'ext, Value<TypeValue<'ext>, 'ext>> }

  and TypeCaseExtension<'ext, 'extConstructors, 'extValues> =
    { CaseType: TypeExpr<'ext> // "a"
      ConstructorType: TypeValue<'ext> // "a => Option a"
      Constructor: 'extConstructors
      ValueLens: PartialLens<'ext, 'extValues> // lens to access the value inside the extension value
      ConsLens: PartialLens<'ext, 'extConstructors> // lens to access the constructor inside the extension value
      Apply:
        Location
          -> List<Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>>
          -> 'extConstructors * Value<TypeValue<'ext>, 'ext>
          -> ExprEvaluator<'ext, Value<TypeValue<'ext>, 'ext>> }

  and TypeLambdaExtension<'ext, 'extTypeLambda> =
    { ExtensionType: ResolvedIdentifier * TypeValue<'ext> * Kind
      ExtraBindings: Map<ResolvedIdentifier, TypeValue<'ext> * Kind>
      Value: 'extTypeLambda // eval value bindings will contain an entry from the extension identifier to this value (modulo DU packaging)
      ValueLens: PartialLens<'ext, 'extTypeLambda> // lens to handle wrapping and upwrapping between the extension value and the core value
      EvalToTypeApplicable: ExtensionEvaluator<'ext> // implementation of what happens at runtime when the extension is type applied (instantiation)
      EvalToApplicable: ExtensionEvaluator<'ext> } // implementation of what happens at runtime when the extension is applied

  type ExtensionPrelude = ExtensionPrelude of string
