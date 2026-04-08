namespace Ballerina.API

module SyntheticData =
  open Ballerina.DSL.Next.Types.TypeChecker
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.SyntheticData.Generator
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.StdLib.MutableMemoryDB

  let GENERATED_MANY_ITEMS_LENGTH = 4

  type ListConfig =
    { ManyItemsLength: int }

    static member Default = { ManyItemsLength = GENERATED_MANY_ITEMS_LENGTH }

  let configWithRandom<'importedCfg> seed (imported: 'importedCfg option) : GeneratorConfig<'importedCfg> =
    let emptyConfig = GeneratorConfig<'importedCfg>.Empty

    GeneratorConfig<'importedCfg>.Updaters.Random (fun _ -> Random(seed)) emptyConfig
    |> GeneratorConfig<'importedCfg>.Updaters.ImportedConfig(fun _ -> imported)

  let private emptyContext<'valueExt when 'valueExt: comparison> () : TypeCheckContext<'valueExt> =
    TypeCheckContext.Empty("", "")

  let private emptyState<'valueExt when 'valueExt: comparison> () : TypeCheckState<'valueExt> = TypeCheckState.Empty

  let listImportedGenerators<'runtimeContext, 'db, 'customExtension
    when 'customExtension: comparison and 'db: comparison>
    ()
    : Map<ResolvedIdentifier, ImportedGenerator<ValueExt<'runtimeContext, 'db, 'customExtension>, ListConfig>> =
    let stdlib, _ = db_ops () |> bootstrapStdExtensions
    let listTypeId = stdlib.List.TypeName |> fst

    let generator =
      { ImportedGenerator.Id = listTypeId
        Generate =
          fun (config: GeneratorConfig<ListConfig>) generate imported ->
            match imported.Arguments with
            | [ elementType ] ->
              let maxLength =
                config.ImportedConfig
                |> Option.map (fun c -> c.ManyItemsLength)
                |> Option.defaultValue GENERATED_MANY_ITEMS_LENGTH

              let elementSeq = generate elementType

              let rec cartesian (n: int) : seq<Sum<list<_>, _>> =
                if n = 0 then
                  seq { yield sum.Return [] }
                else
                  seq {
                    for s in elementSeq do
                      match s with
                      | Sum.Right e -> yield Sum.Right e
                      | Sum.Left head ->
                        for tailResult in cartesian (n - 1) do
                          match tailResult with
                          | Sum.Right e -> yield Sum.Right e
                          | Sum.Left tail -> yield sum.Return(head :: tail)
                  }

              seq {
                for length in [ 0; 1; maxLength ] do
                  for s in cartesian length do
                    yield
                      s
                      |> sum.Map(fun values ->
                        let listValues = Ballerina.DSL.Next.StdLib.List.Model.ListValues.List values
                        let extValue = ValueExt.ValueExt(Choice1Of7(ListExt.ListValues listValues))
                        Value.Ext(extValue, None))
              }
            | _ ->
              seq {
                yield
                  (fun () -> $"Expected 1 list type argument, got {imported.Arguments.Length}")
                  |> Errors.Singleton()
                  |> sum.Throw
              } }

    Map.ofList [ listTypeId, generator ]
