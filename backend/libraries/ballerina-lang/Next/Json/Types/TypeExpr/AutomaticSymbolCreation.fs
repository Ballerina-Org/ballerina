namespace Ballerina.DSL.Next.Types.Json

module AutomaticSymbolCreation =
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Errors

  let wrapWithLet (typeExpr: TypeExpr, lookupsRepresentingSymbols: List<TypeExpr>) : Sum<TypeExpr, Errors> =
    sum {
      let! symbolNames =
        lookupsRepresentingSymbols
        |> List.map (fun (symbol: TypeExpr) ->
          sum {
            match symbol with
            | TypeExpr.Lookup(Identifier.LocalScope name) -> name
            | _ ->
              return!
                sum.Throw(Errors.Singleton $"Expected a lookup representing a local scope symbol but got {symbol}")
          })
        |> sum.All

      let wrappedTypeExpr =
        List.foldBack
          (fun (symbolName: string) (accExpr: TypeExpr) ->
            TypeExpr.Let(symbolName, TypeExpr.NewSymbol symbolName, accExpr))
          symbolNames
          typeExpr

      return wrappedTypeExpr
    }
