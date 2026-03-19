namespace Codegen.Python

module Syntax =
  open Ballerina.Collections.NonEmptyList
  let indent = (+) "    "

  let appendNewline s = s + "\n"

  type TypeAnnotation = TypeAnnotation of string

  let genericApplication (typeFunction: string) (typeArguments: NonEmptyList<TypeAnnotation>) : TypeAnnotation =
    let typeArguments: string list =
      typeArguments |> NonEmptyList.ToList |> List.map (fun (TypeAnnotation t) -> t)

    TypeAnnotation(sprintf "%s[%s]" typeFunction (String.concat ", " typeArguments))

  let applyFunction (functionName: string) (typeArguments: List<string>) : string =
    sprintf "%s(%s)" functionName (String.concat ", " typeArguments)
