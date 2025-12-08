namespace Ballerina.DSL.Next.StdLib.TimeSpan

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
  open System.Globalization

  let TimeSpanConstructorsExtension<'ext>
    (consLens: PartialLens<'ext, TimeSpanConstructors>)
    : list<(ResolvedIdentifier * TypeSymbol) * TypeCaseExtension<'ext, TimeSpanConstructors, PrimitiveValue>> =

    let timeSpanNewId = Identifier.FullyQualified([ "timeSpan" ], "new")
    let timeSpanNewSymbol = timeSpanNewId |> TypeSymbol.Create
    let timeSpanNewId = timeSpanNewId |> TypeCheckScope.Empty.Resolve

    let timeSpanNew =
      (timeSpanNewId, timeSpanNewSymbol),
      { CaseType = TypeExpr.Primitive PrimitiveType.String
        ConstructorType =
          TypeValue.CreateArrow(
            TypeValue.CreatePrimitive PrimitiveType.String,
            TypeValue.CreateSum
              [ TypeValue.CreatePrimitive PrimitiveType.Unit
                TypeValue.Lookup(Identifier.LocalScope "timeSpan") ]
          )
        Constructor = TimeSpan_New
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              match v with
              | Value.Primitive(PrimitiveValue.String v) ->
                return
                  try
                    Value.Sum(
                      { Case = 0; Count = 1 },
                      Value.Primitive(PrimitiveValue.TimeSpan(System.TimeSpan.Parse(v, CultureInfo.InvariantCulture)))
                    )
                  with _ ->
                    Value.Sum({ Case = 1; Count = 1 }, Value.Primitive(PrimitiveValue.Unit))
              | _ -> return! sum.Throw(Errors.Singleton(loc0, "Expected a string")) |> reader.OfSum
            }
        ConsLens =
          consLens
          |> PartialLens.BindGet (function
            | TimeSpan_New -> Some TimeSpan_New
            | _ -> None)
        ValueLens =
          { Get = fun _ -> None
            Set = fun _ -> failwith "Not supported" } }

    let timeSpanZeroId = Identifier.FullyQualified([ "timeSpan" ], "zero")
    let timeSpanZeroSymbol = timeSpanZeroId |> TypeSymbol.Create
    let timeSpanZeroId = timeSpanZeroId |> TypeCheckScope.Empty.Resolve

    let timeSpanZero =
      (timeSpanZeroId, timeSpanZeroSymbol),
      { CaseType = TypeExpr.Primitive PrimitiveType.Unit
        ConstructorType =
          TypeValue.CreateArrow(
            TypeValue.CreatePrimitive PrimitiveType.Unit,
            TypeValue.Lookup(Identifier.LocalScope "timeSpan")
          )
        Constructor = TimeSpan_Zero
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              match v with
              | Value.Primitive(PrimitiveValue.Unit) ->
                return Value.Primitive(PrimitiveValue.TimeSpan System.TimeSpan.Zero)
              | _ -> return! sum.Throw(Errors.Singleton(loc0, "Expected a unit")) |> reader.OfSum
            }
        ConsLens =
          consLens
          |> PartialLens.BindGet (function
            | TimeSpan_Zero -> Some TimeSpan_Zero
            | _ -> None)
        ValueLens =
          { Get = fun _ -> None
            Set = fun _ -> failwith "Not supported" } }

    [ timeSpanNew; timeSpanZero ]
