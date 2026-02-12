namespace Ballerina.Cat.Tests.BusinessRuleEngine.Next.SyntheticData

module ImportedTypesConfig =
  open Ballerina.DSL.Next.SyntheticData.Generator
  open System

  let GENERATED_MAX_COLLECTION_LENGTH = 4

  type ListConfig =
    { MaxLength: int }

    static member Default = { MaxLength = GENERATED_MAX_COLLECTION_LENGTH }

  let configWithRandom<'importedCfg> seed (imported: 'importedCfg option) : GeneratorConfig<'importedCfg> =
    let emptyConfig = GeneratorConfig<'importedCfg>.Empty

    GeneratorConfig<'importedCfg>.Updaters.Random (fun _ -> Random(seed)) emptyConfig
    |> GeneratorConfig<'importedCfg>.Updaters.ImportedConfig(fun _ -> imported)
