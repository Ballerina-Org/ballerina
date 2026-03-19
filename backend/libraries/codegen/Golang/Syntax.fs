namespace Codegen.Golang

module Syntax =
  open Ballerina.StdLib.StringBuilder

  let appendNewline s = s + "\n"
  let indent (s: string) : string = "  " + s

  type TypeAnnotation = TypeAnnotation of string

  let genericApplication (typeFunction: string) (typeArguments: TypeAnnotation list) : TypeAnnotation =
    let typeArguments: string list =
      typeArguments |> List.map (fun (TypeAnnotation t) -> t)

    TypeAnnotation(sprintf "%s[%s]" typeFunction (String.concat ", " typeArguments))

  let errorType = TypeAnnotation "error"

  type TypeAlias = { Name: string; Type: TypeAnnotation }

  type TypeAlias with
    static member Generate(typeAlias: TypeAlias) : StringBuilder =
      let (TypeAnnotation typeAnnotation) = typeAlias.Type

      StringBuilder.One(sprintf "type %s = %s" typeAlias.Name typeAnnotation)
      |> StringBuilder.Map appendNewline

  let applyFunction (functionName: string) (typeArguments: List<string>) : string =
    sprintf "%s(%s)" functionName (String.concat ", " typeArguments)
