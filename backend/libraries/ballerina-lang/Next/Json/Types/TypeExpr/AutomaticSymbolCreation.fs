namespace Ballerina.DSL.Next.Types.Json

open Ballerina.LocalizedErrors

module AutomaticSymbolCreation =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Errors

  let wrapWithLet
    (typeExpr: TypeExpr<'valueExt>, lookupsRepresentingSymbols: List<TypeExpr<'valueExt>>, symbolsKind: SymbolsKind)
    : Sum<TypeExpr<'valueExt>, Errors<Location>> =
    sum {
      let! symbolNames =
        lookupsRepresentingSymbols
        |> List.map (fun (symbol: TypeExpr<'valueExt>) ->
          sum {
            match symbol with
            | TypeExpr.Lookup(Identifier.LocalScope name) -> name
            | _ ->
              return!
                sum.Throw(
                  Errors.Singleton Location.Unknown (fun () ->
                    $"Expected a lookup representing a local scope symbol but got {symbol}")
                )
          })
        |> sum.All

      let wrappedTypeExpr = TypeExpr.LetSymbols(symbolNames, symbolsKind, typeExpr)

      return wrappedTypeExpr
    }
