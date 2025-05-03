namespace Ballerina.Codegen.Python.Generator

module Main =
  open Ballerina.Core.StringBuilder
  open Ballerina.Errors
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.State.WithError
  open Ballerina.DSL.Codegen.Python.LanguageConstructs.Model
  open Ballerina.DSL.Codegen.Python.LanguageConstructs.GeneratedTypes
  open Ballerina.DSL.Codegen.Python.LanguageConstructs

  type Generator() =
    static member ToPython
      (codegenConfig: PythonCodeGenConfig)
      (typeDefinition: (TypeId * ExprType))
      (otherTypes: (TypeId * ExprType) list)
      : Sum<StringBuilder, Errors> =

      let typesToGenerate =
        [ typeDefinition ]
        |> List.append otherTypes
        |> Seq.map (fun (typeId, typeDefinition) ->
          { TypeName = typeId.TypeName
            Type = typeDefinition })
        |> List.ofSeq

      let result =
        state {
          let! generatedTypes = PythonGeneratedType.Generate typesToGenerate
          let! s = state.GetState()

          StringBuilder.Many(
            seq {
              yield Header.Generate codegenConfig s.UsedImports
              yield generatedTypes
            }
          )
        }
        |> state.WithErrorContext $"...when generating Python code"


      match result.run (codegenConfig, { UsedImports = Set.empty }) with
      | Right(e, _) -> Right e
      | Left(res, s') -> Left res
