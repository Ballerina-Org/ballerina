namespace Ballerina.Cat.Tests.BusinessRuleEngine.Next.SyntheticData

module ImportedTypesConfig =
  open Ballerina.DSL.Next.SyntheticData.Generator
  open System

  let GENERATED_MANY_ITEMS_LENGTH = 4

  type ListConfig =
    { ManyItemsLength: int }

    static member Default = { ManyItemsLength = GENERATED_MANY_ITEMS_LENGTH }

  let configWithRandom<'importedCfg>
    seed
    (imported: 'importedCfg option)
    : GeneratorConfig<'importedCfg> =
    let emptyConfig = GeneratorConfig<'importedCfg>.Empty

    GeneratorConfig<'importedCfg>.Updaters.Random
      (fun _ -> Random(seed))
      emptyConfig
    |> GeneratorConfig<'importedCfg>.Updaters.ImportedConfig(fun _ -> imported)
