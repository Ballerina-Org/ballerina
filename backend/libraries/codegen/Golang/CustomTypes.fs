namespace Codegen.Golang

open Ballerina.StdLib
open Codegen.Golang

type GolangCustomType =
  { TypeName: string
    GeneratedTypeName: string }

  static member Generate (_: GolangContext) (customTypes: List<GolangCustomType>) =
    customTypes
    |> Seq.map (fun t ->
      StringBuilder.Many(
        seq {
          yield StringBuilder.One "\n"
          yield StringBuilder.One $"type {t.TypeName} = {t.GeneratedTypeName}"
          yield StringBuilder.One "\n"
        }
      ))
    |> StringBuilder.Many
