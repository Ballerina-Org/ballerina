namespace Ballerina.DSL.Next.StdLib.Updater

[<AutoOpen>]
module Extension =
  open Ballerina
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions

  let UpdaterExtension<'runtimeContext, 'ext>
    (operationLens: PartialLens<'ext, UpdaterOperations<'ext>>)
    : OperationsExtension<'runtimeContext, 'ext, UpdaterOperations<'ext>> =

    let updaterApply =
      Identifier.FullyQualified([ "@updater" ], "apply")
      |> TypeCheckScope.Empty.Resolve

    let applyOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, UpdaterOperations<'ext>> =
      updaterApply,
      { PublicIdentifiers = None
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | UpdaterOperations.Apply v -> Some(UpdaterOperations.Apply v))
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! updater =
                op
                |> UpdaterOperations.AsApply
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return!
                updater v
                |> reader.OfSum
                |> reader.MapError(Errors.MapContext(replaceWith loc0))
            } }

    { TypeVars = []
      Operations = [ applyOperation ] |> Map.ofList }
