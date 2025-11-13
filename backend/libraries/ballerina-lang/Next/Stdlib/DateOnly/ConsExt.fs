namespace Ballerina.DSL.Next.StdLib.DateOnly

[<AutoOpen>]
module ConsExt =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open Ballerina.StdLib.Formats

  let DateOnlyConstructorsExtension<'ext>
    (consLens: PartialLens<'ext, DateOnlyConstructors>)
    : list<(ResolvedIdentifier * TypeSymbol) * TypeCaseExtension<'ext, DateOnlyConstructors, PrimitiveValue>> =

    let dateOnlyNewId = Identifier.FullyQualified([ "dateOnly" ], "new")
    let dateOnlyNewSymbol = dateOnlyNewId |> TypeSymbol.Create
    let dateOnlyNewId = dateOnlyNewId |> TypeCheckScope.Empty.Resolve

    let dateOnlyNew: (ResolvedIdentifier * TypeSymbol) * TypeCaseExtension<'ext, DateOnlyConstructors, PrimitiveValue> =
      (dateOnlyNewId, dateOnlyNewSymbol),
      { CaseType = TypeExpr.Primitive PrimitiveType.String
        ConstructorType =
          TypeExpr.Arrow(
            TypeExpr.Primitive PrimitiveType.String,
            TypeExpr.Sum
              [ TypeExpr.Primitive PrimitiveType.Unit
                TypeExpr.Lookup(Identifier.LocalScope "dateOnly") ]
          )
        Constructor = DateOnly_New
        Apply =
          fun loc0 (_, v) ->
            reader {
              match v with
              | Value.Primitive(PrimitiveValue.String v) ->
                return
                  match Iso8601.DateOnly.tryParse v with
                  | Some date -> Value.Sum({ Case = 0; Count = 1 }, date |> PrimitiveValue.Date |> Value.Primitive)
                  | None -> Value.Sum({ Case = 1; Count = 1 }, Value.Primitive(PrimitiveValue.Unit))
              | _ -> return! sum.Throw(Errors.Singleton(loc0, "Expected a string")) |> reader.OfSum
            }
        ConsLens =
          consLens
          |> PartialLens.BindGet (function
            | DateOnly_New -> Some DateOnly_New
            | _ -> None)
        ValueLens =
          { Get = fun _ -> None
            Set = fun _ -> failwith "Not supported" } }

    let dateOnlyNowId = Identifier.FullyQualified([ "dateOnly" ], "now")
    let dateOnlyNowSymbol = dateOnlyNowId |> TypeSymbol.Create
    let dateOnlyNowId = dateOnlyNowId |> TypeCheckScope.Empty.Resolve

    let dateOnlyNow =
      (dateOnlyNowId, dateOnlyNowSymbol),
      { CaseType = TypeExpr.Primitive PrimitiveType.Unit
        ConstructorType =
          TypeExpr.Arrow(TypeExpr.Primitive PrimitiveType.Unit, TypeExpr.Lookup(Identifier.LocalScope "dateOnly"))
        Constructor = DateOnly_Now
        Apply =
          fun loc0 (_, v) ->
            reader {
              match v with
              | Value.Primitive(PrimitiveValue.Unit) ->
                let date = System.DateTime.Now
                let dateOnly = System.DateOnly(date.Year, date.Month, date.Day)
                return Value.Primitive(PrimitiveValue.Date dateOnly)
              | _ -> return! sum.Throw(Errors.Singleton(loc0, "Expected a unit (now)")) |> reader.OfSum
            }
        ConsLens =
          consLens
          |> PartialLens.BindGet (function
            | DateOnly_Now -> Some DateOnly_Now
            | _ -> None)
        ValueLens =
          { Get = fun _ -> None
            Set = fun _ -> failwith "Not supported" } }

    let dateOnlyUTCNowId = Identifier.FullyQualified([ "dateOnly" ], "utcNow")
    let dateOnlyUTCNowSymbol = dateOnlyUTCNowId |> TypeSymbol.Create
    let dateOnlyUTCNowId = dateOnlyUTCNowId |> TypeCheckScope.Empty.Resolve

    let dateOnlyUTCNow =
      (dateOnlyUTCNowId, dateOnlyUTCNowSymbol),
      { CaseType = TypeExpr.Primitive PrimitiveType.Unit
        ConstructorType =
          TypeExpr.Arrow(TypeExpr.Primitive PrimitiveType.Unit, TypeExpr.Lookup(Identifier.LocalScope "dateOnly"))
        Constructor = DateOnly_UTCNow
        Apply =
          fun loc0 (_, v) ->
            reader {
              match v with
              | Value.Primitive(PrimitiveValue.Unit) ->
                let date = System.DateTime.UtcNow
                let dateOnly = System.DateOnly(date.Year, date.Month, date.Day)
                return Value.Primitive(PrimitiveValue.Date dateOnly)
              | _ -> return! sum.Throw(Errors.Singleton(loc0, "Expected a unit (UTC now)")) |> reader.OfSum
            }
        ConsLens =
          consLens
          |> PartialLens.BindGet (function
            | DateOnly_UTCNow -> Some DateOnly_UTCNow
            | _ -> None)
        ValueLens =
          { Get = fun _ -> None
            Set = fun _ -> failwith "Not supported" } }

    [ dateOnlyNew; dateOnlyNow; dateOnlyUTCNow ]
