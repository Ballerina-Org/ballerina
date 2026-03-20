namespace Codegen.Python

module Model =
  open Codegen.Python.Syntax
  open Ballerina.Errors
  open Ballerina.Collections.Sum

  type Import = { Source: string; Target: string }

  type PythonCodeGenState =
    { UsedImports: Set<Import> }

    static member Updaters =
      {| UsedImports = fun u -> fun s -> { s with UsedImports = u s.UsedImports } |}


  type PythonCodeGenConfig =
    { Int: CodegenConfigTypeDef
      Decimal: CodegenConfigTypeDef
      Bool: CodegenConfigTypeDef
      String: CodegenConfigTypeDef
      Unit: CodegenConfigTypeDef
      List: CodegenConfigListDef
      Sum: List<CodegenConfigSumDef>
      Tuple: List<CodegenConfigTupleDef> }

  and CodegenConfigListDef =
    { GeneratedTypeName: string
      RequiredImports: Set<Import>
      Parser: Import
      Serializer: Import }

  and CodegenConfigSumDef =
    { Arity: int
      GeneratedTypeName: string
      RequiredImports: Set<Import>
      Parser: Import
      Serializer: Import }

    static member FindArity (config: List<CodegenConfigSumDef>) (arity: int) : Sum<CodegenConfigSumDef, Errors<unit>> =
      config
      |> List.tryFind (fun c -> c.Arity = arity)
      |> Sum.fromOption (fun () -> Errors.Singleton () (fun () -> $"Error: missing sum config for arity {arity}"))

  and CodegenConfigTypeDef =
    { GeneratedTypeName: TypeAnnotation
      RequiredImports: Set<Import>
      Parser: Import
      Serializer: Import }

  and CodegenConfigOptionDef =
    { GeneratedTypeName: string
      RequiredImports: Set<Import>
      Parser: Import
      Serializer: Import }

  and CodegenConfigTupleDef =
    { Arity: int
      GeneratedTypeName: string
      RequiredImports: Set<Import>
      Parser: Import
      Serializer: Import }

    static member FindArity
      (config: List<CodegenConfigTupleDef>)
      (arity: int)
      : Sum<CodegenConfigTupleDef, Errors<unit>> =
      config
      |> List.tryFind (fun c -> c.Arity = arity)
      |> Sum.fromOption (fun () -> Errors.Singleton () (fun () -> $"Error: missing tuple config for arity {arity}"))

module Serialization =
  open System.Text.Json
  open System.Text.Json.Serialization
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Model

  // NOTE: Model types are used directly for serialization
  type PythonCodeGenConfig with
    static member Deserialize(serializedConfig: string) : Sum<PythonCodeGenConfig, Errors<unit>> =
      try
        Left(
          JsonSerializer.Deserialize<PythonCodeGenConfig>(
            serializedConfig,
            JsonFSharpOptions.Default().ToJsonSerializerOptions()
          )
        )
      with err ->
        Right(Errors.Singleton () (fun () -> sprintf "Error when reading Python codegen config: %s" err.Message))
