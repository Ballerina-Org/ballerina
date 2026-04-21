namespace Ballerina.DSL.Next.StdLib.View

module Extension =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Extensions

  let ViewExtension<'runtimeContext, 'ext, 'extDTO, 'deltaExt, 'deltaExtDTO
    when 'ext: comparison
    and 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct>
    (viewTypeSymbol: Option<TypeSymbol>)
    (viewPropsTypeSymbol: Option<TypeSymbol>)
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
      TypeExtension<
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
      TypeSymbol *
      (TypeValue<'ext> -> TypeValue<'ext> -> TypeValue<'ext> -> TypeValue<'ext>) *
      (TypeValue<'ext> -> TypeValue<'ext> -> TypeValue<'ext> -> TypeValue<'ext>)
    =
    // --- View ---
    let viewId = Identifier.FullyQualified([ "Frontend" ], "View")

    let viewSymbolId =
      viewTypeSymbol |> Option.defaultWith (fun () -> viewId |> TypeSymbol.Create)

    let viewResolvedId = viewId |> TypeCheckScope.Empty.Resolve

    let schemaVar, schemaKind = TypeVar.Create("schema"), Kind.Schema
    let ctxVar, ctxKind = TypeVar.Create("context"), Kind.Star
    let stVar, stKind = TypeVar.Create("state"), Kind.Star

    let make_viewType (schema: TypeValue<'ext>) (ctx: TypeValue<'ext>) (st: TypeValue<'ext>) =
      TypeValue.Imported
        { Id = viewResolvedId
          Sym = viewSymbolId
          Parameters = []
          Arguments = [ schema; ctx; st ] }

    let viewExtension =
      { TypeName = viewResolvedId, viewSymbolId
        TypeVars = [ (schemaVar, schemaKind); (ctxVar, ctxKind); (stVar, stKind) ]
        Cases = Map.empty
        Operations = Map.empty
        Serialization = None
        ExtTypeChecker = None }

    // --- View::Props ---
    let viewPropsId = Identifier.FullyQualified([ "View" ], "Props")

    let viewPropsSymbolId =
      viewPropsTypeSymbol |> Option.defaultWith (fun () -> viewPropsId |> TypeSymbol.Create)

    let viewPropsResolvedId = viewPropsId |> TypeCheckScope.Empty.Resolve

    let make_viewPropsType (schema: TypeValue<'ext>) (ctx: TypeValue<'ext>) (st: TypeValue<'ext>) =
      TypeValue.Imported
        { Id = viewPropsResolvedId
          Sym = viewPropsSymbolId
          Parameters = []
          Arguments = [ schema; ctx; st ] }

    let viewPropsExtension =
      { TypeName = viewPropsResolvedId, viewPropsSymbolId
        TypeVars = [ (schemaVar, schemaKind); (ctxVar, ctxKind); (stVar, stKind) ]
        Cases = Map.empty
        Operations = Map.empty
        Serialization = None
        ExtTypeChecker = None }

    viewExtension, viewPropsExtension, viewSymbolId, viewPropsSymbolId, make_viewType, make_viewPropsType
