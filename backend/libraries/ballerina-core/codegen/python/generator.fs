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
      (typeDefinition: (TypeId * ExprType))
      (otherTypes: (TypeId * ExprType) list)
      : Sum<StringBuilder, Errors> =
      let hardcodedConfig: PythonCodeGenConfig =
        { Int =
            { GeneratedTypeName = "int"
              RequiredImport = None }
          Float =
            { GeneratedTypeName = "Decimal"
              RequiredImport = Some "from decimal import Decimal" }
          Bool =
            { GeneratedTypeName = "bool"
              RequiredImport = None }
          String =
            { GeneratedTypeName = "str"
              RequiredImport = None }
          Date =
            { GeneratedTypeName = "date"
              RequiredImport = Some "from datetime import date" }
          DateTime =
            { GeneratedTypeName = "datetime"
              RequiredImport = Some "from datetime import datetime" }
          Guid =
            { GeneratedTypeName = "UUID"
              RequiredImport = Some "from uuid import UUID" }
          Unit =
            { GeneratedTypeName = "None"
              RequiredImport = None }
          Option =
            { GeneratedTypeName = "Option"
              RequiredImport = Some "from ballerina_core.primitives import Option" }
          Set =
            { GeneratedTypeName = "frozenset"
              RequiredImport = None }
          List =
            { GeneratedTypeName = "Sequence"
              RequiredImport = Some "from collections.abc import Sequence" }
          Tuple =
            { GeneratedTypeName = "tuple"
              RequiredImport = None }
          Map =
            { GeneratedTypeName = "Mapping"
              RequiredImport = Some "from collections.abc import Mapping" }
          Sum =
            { GeneratedTypeName = "Sum"
              RequiredImport = Some "from ballerina_core.primitives import Sum" } }

      let typesToGenerate =
        [ typeDefinition ]
        |> List.append otherTypes
        |> Seq.map (fun (typeId, typeDefinition) ->
          { TypeName = typeId.TypeName
            Type = typeDefinition })
        |> List.ofSeq

      let result =
        state {
          let! generatedTypes = PythonGeneratedType.Generate hardcodedConfig typesToGenerate


          let! s = state.GetState()

          return
            StringBuilder.Many(
              seq {
                yield Header.Generate hardcodedConfig s.UsedImports
                yield generatedTypes
              }
            )
        }
        |> state.WithErrorContext $"...when generating Python code"


      match result.run (hardcodedConfig, { UsedImports = Set.empty }) with
      | Right(e, _) -> Right e
      | Left(res, s') -> Left res
