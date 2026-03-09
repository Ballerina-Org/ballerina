namespace Ballerina.DSL.Next.StdLib.DB.Extension

[<AutoOpen>]
module CommonHelpers =
  open Ballerina.Collections.Option
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.StdLib.DB

  /// Extract entity ref from extension value in the first step of operation application
  let extractEntityRefFromValue<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (loc0: Location)
    (v: Value<TypeValue<'ext>, 'ext>)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    : Reader<EntityRef<'db, 'ext>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Location>> =

    reader {
      let! v, _ =
        v
        |> Value.AsExt
        |> sum.MapError(Errors.MapContext(replaceWith loc0))
        |> reader.OfSum

      let! v =
        v
        |> valueLens.Get
        |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Cannot get value from extension"))
        |> reader.OfSum

      let! v =
        v
        |> DBValues.AsEntityRef
        |> sum.MapError(Errors.MapContext(replaceWith loc0))
        |> reader.OfSum

      return v
    }

  /// Extract relation ref from extension value in the first step of operation application
  let extractRelationRefFromValue<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (loc0: Location)
    (v: Value<TypeValue<'ext>, 'ext>)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    (fieldName: string)
    : Reader<RelationRef<'db, 'ext>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Location>> =
    reader {
      let! v =
        v
        |> Value.AsRecord
        |> sum.MapError(Errors.MapContext(replaceWith loc0))
        |> reader.OfSum

      let! v =
        v
        |> Map.tryFind (ResolvedIdentifier.Create fieldName)
        |> sum.OfOption(Errors.Singleton loc0 (fun () -> $"Cannot find '{fieldName}' field in operation"))
        |> reader.OfSum

      let! v, _ =
        v
        |> Value.AsExt
        |> sum.MapError(Errors.MapContext(replaceWith loc0))
        |> reader.OfSum

      let! v =
        v
        |> valueLens.Get
        |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Cannot get value from extension"))
        |> reader.OfSum

      let! v =
        v
        |> DBValues.AsRelationRef
        |> sum.MapError(Errors.MapContext(replaceWith loc0))
        |> reader.OfSum

      return v
    }
