namespace Ballerina.DSL.Next.StdLib.Coroutine

module Extension =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Extensions

  let CoroutineExtension<'runtimeContext, 'ext, 'extDTO, 'deltaExt, 'deltaExtDTO
    when 'ext: comparison
    and 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct>
    (typeSymbol: Option<TypeSymbol>)
    : TypeExtension<
        'runtimeContext,
        'ext,
        'extDTO,
        'deltaExt,
        'deltaExtDTO,
        Unit,
        Unit,
        Unit
       > *
      TypeSymbol *
      (TypeValue<'ext>
        -> TypeValue<'ext>
        -> TypeValue<'ext>
        -> TypeValue<'ext>
        -> TypeValue<'ext>)
    =
    let coId = Identifier.LocalScope "Co"

    let coSymbolId =
      typeSymbol |> Option.defaultWith (fun () -> coId |> TypeSymbol.Create)

    let coResolvedId = coId |> TypeCheckScope.Empty.Resolve

    let schemaVar, schemaKind = TypeVar.Create("schema"), Kind.Schema
    let ctxVar, ctxKind = TypeVar.Create("context"), Kind.Star
    let stVar, stKind = TypeVar.Create("state"), Kind.Star
    let resVar, resKind = TypeVar.Create("result"), Kind.Star

    let make_coType
      (schema: TypeValue<'ext>)
      (ctx: TypeValue<'ext>)
      (st: TypeValue<'ext>)
      (res: TypeValue<'ext>)
      =
      TypeValue.Imported
        { Id = coResolvedId
          Sym = coSymbolId
          Parameters = []
          Arguments = [ schema; ctx; st; res ] }

    let coExtension =
      { TypeName = coResolvedId, coSymbolId
        TypeVars =
          [ (schemaVar, schemaKind)
            (ctxVar, ctxKind)
            (stVar, stKind)
            (resVar, resKind) ]
        Cases = Map.empty
        Operations = Map.empty
        Serialization = None
        ExtTypeChecker = None }

    coExtension, coSymbolId, make_coType
