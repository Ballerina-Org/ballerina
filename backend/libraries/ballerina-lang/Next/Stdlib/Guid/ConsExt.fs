namespace Ballerina.DSL.Next.StdLib.Guid

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

  let GuidConstructorsExtension<'ext>
    (consLens: PartialLens<'ext, GuidConstructors>)
    : list<(ResolvedIdentifier * TypeSymbol) * TypeCaseExtension<'ext, GuidConstructors, PrimitiveValue>> =

    let guidNewId = Identifier.FullyQualified([ "guid" ], "new")
    let guidNewSymbol = guidNewId |> TypeSymbol.Create
    let guidNewId = guidNewId |> TypeCheckScope.Empty.Resolve

    let guidNew =
      (guidNewId, guidNewSymbol),
      { CaseType = TypeExpr.Primitive PrimitiveType.String
        ConstructorType =
          TypeExpr.Arrow(
            TypeExpr.Primitive PrimitiveType.String,
            TypeExpr.Sum
              [ TypeExpr.Primitive PrimitiveType.Unit
                TypeExpr.Lookup(Identifier.LocalScope "guid") ]
          )
        Constructor = Guid_New
        Apply =
          fun loc0 (_, v) ->
            reader {
              match v with
              | Value.Primitive(PrimitiveValue.String v) ->
                return
                  try
                    Value.Sum({ Case = 0; Count = 1 }, Value.Primitive(PrimitiveValue.Guid(System.Guid.Parse(v))))
                  with _ ->
                    Value.Sum({ Case = 1; Count = 1 }, Value.Primitive(PrimitiveValue.Unit))
              | _ -> return! sum.Throw(Errors.Singleton(loc0, "Expected a string")) |> reader.OfSum
            }
        ConsLens =
          consLens
          |> PartialLens.BindGet (function
            | Guid_New -> Some Guid_New
            | _ -> None)
        ValueLens =
          { Get = fun _ -> None
            Set = fun _ -> failwith "Not supported" } }

    let guidV4Id = Identifier.FullyQualified([ "guid" ], "v4")
    let guidV4Symbol = guidV4Id |> TypeSymbol.Create
    let guidV4Id = guidV4Id |> TypeCheckScope.Empty.Resolve

    let guidV4 =
      (guidV4Id, guidV4Symbol),
      { CaseType = TypeExpr.Primitive PrimitiveType.Unit
        ConstructorType =
          TypeExpr.Arrow(TypeExpr.Primitive PrimitiveType.Unit, TypeExpr.Lookup(Identifier.LocalScope "guid"))
        Constructor = Guid_V4
        Apply =
          fun loc0 (_, v) ->
            reader {
              match v with
              | Value.Primitive PrimitiveValue.Unit ->
                return Value.Primitive(PrimitiveValue.Guid(System.Guid.NewGuid()))
              | _ -> return! sum.Throw(Errors.Singleton(loc0, "Expected a unit")) |> reader.OfSum
            }
        ConsLens =
          consLens
          |> PartialLens.BindGet (function
            | Guid_V4 -> Some Guid_V4
            | _ -> None)
        ValueLens =
          { Get = fun _ -> None
            Set = fun _ -> failwith "Not supported" } }

    [ guidNew; guidV4 ]
