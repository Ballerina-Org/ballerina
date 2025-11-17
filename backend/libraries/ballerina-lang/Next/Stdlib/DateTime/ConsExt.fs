namespace Ballerina.DSL.Next.StdLib.DateTime

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

  let DateTimeConstructorsExtension<'ext>
    (consLens: PartialLens<'ext, DateTimeConstructors>)
    : list<(ResolvedIdentifier * TypeSymbol) * TypeCaseExtension<'ext, DateTimeConstructors, PrimitiveValue>> =

    let dateTimeNewId = Identifier.FullyQualified([ "dateTime" ], "new")
    let dateTimeNewSymbol = dateTimeNewId |> TypeSymbol.Create
    let dateTimeNewId = dateTimeNewId |> TypeCheckScope.Empty.Resolve

    let dateTimeNew: (ResolvedIdentifier * TypeSymbol) * TypeCaseExtension<'ext, DateTimeConstructors, PrimitiveValue> =
      (dateTimeNewId, dateTimeNewSymbol),
      { CaseType = TypeExpr.Primitive PrimitiveType.String
        ConstructorType =
          TypeExpr.Arrow(
            TypeExpr.Primitive PrimitiveType.String,
            TypeExpr.Sum
              [ TypeExpr.Primitive PrimitiveType.Unit
                TypeExpr.Lookup(Identifier.LocalScope "dateTime") ]
          )
        Constructor = DateTime_New
        Apply =
          fun loc0 (_, v) ->
            reader {
              match v with
              | Value.Primitive(PrimitiveValue.String v) ->
                return
                  match Iso8601.DateTime.tryParse v with
                  | Some date -> Value.Sum({ Case = 0; Count = 1 }, date |> PrimitiveValue.DateTime |> Value.Primitive)
                  | None -> Value.Sum({ Case = 1; Count = 1 }, Value.Primitive(PrimitiveValue.Unit))
              | _ -> return! sum.Throw(Errors.Singleton(loc0, "Expected a string")) |> reader.OfSum
            }
        ConsLens =
          consLens
          |> PartialLens.BindGet (function
            | DateTime_New -> Some DateTime_New
            | _ -> None)
        ValueLens =
          { Get = fun _ -> None
            Set = fun _ -> failwith "Not supported" } }

    let dateTimeNowId = Identifier.FullyQualified([ "dateTime" ], "now")
    let dateTimeNowSymbol = dateTimeNowId |> TypeSymbol.Create
    let dateTimeNowId = dateTimeNowId |> TypeCheckScope.Empty.Resolve

    let dateTimeNow =
      (dateTimeNowId, dateTimeNowSymbol),
      { CaseType = TypeExpr.Primitive PrimitiveType.Unit
        ConstructorType =
          TypeExpr.Arrow(TypeExpr.Primitive PrimitiveType.Unit, TypeExpr.Lookup(Identifier.LocalScope "dateTime"))
        Constructor = DateTime_Now
        Apply =
          fun loc0 (_, v) ->
            reader {
              match v with
              | Value.Primitive(PrimitiveValue.Unit) ->
                return Value.Primitive(PrimitiveValue.DateTime System.DateTime.Now)
              | _ -> return! sum.Throw(Errors.Singleton(loc0, "Expected a unit (now)")) |> reader.OfSum
            }
        ConsLens =
          consLens
          |> PartialLens.BindGet (function
            | DateTime_Now -> Some DateTime_Now
            | _ -> None)
        ValueLens =
          { Get = fun _ -> None
            Set = fun _ -> failwith "Not supported" } }

    let dateTimeUTCNowId = Identifier.FullyQualified([ "dateTime" ], "utcNow")
    let dateTimeUTCNowSymbol = dateTimeUTCNowId |> TypeSymbol.Create
    let dateTimeUTCNowId = dateTimeUTCNowId |> TypeCheckScope.Empty.Resolve

    let dateTimeUTCNow =
      (dateTimeUTCNowId, dateTimeUTCNowSymbol),
      { CaseType = TypeExpr.Primitive PrimitiveType.Unit
        ConstructorType =
          TypeExpr.Arrow(TypeExpr.Primitive PrimitiveType.Unit, TypeExpr.Lookup(Identifier.LocalScope "dateTime"))
        Constructor = DateTime_UTCNow
        Apply =
          fun loc0 (_, v) ->
            reader {
              match v with
              | Value.Primitive(PrimitiveValue.Unit) ->
                return Value.Primitive(PrimitiveValue.DateTime System.DateTime.UtcNow)
              | _ -> return! sum.Throw(Errors.Singleton(loc0, "Expected a unit (UTC now)")) |> reader.OfSum
            }
        ConsLens =
          consLens
          |> PartialLens.BindGet (function
            | DateTime_UTCNow -> Some DateTime_UTCNow
            | _ -> None)
        ValueLens =
          { Get = fun _ -> None
            Set = fun _ -> failwith "Not supported" } }

    [ dateTimeNew; dateTimeNow; dateTimeUTCNow ]
