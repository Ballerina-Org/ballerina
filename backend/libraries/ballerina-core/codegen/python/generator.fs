namespace Ballerina.Codegen.Python.Generator

module Main =
  open Ballerina.Core.StringBuilder
  open Ballerina.Errors
  open Ballerina.Collections.Sum

  // TODO: Call with Expr
  type Generator() =
    static member ToPython() : Sum<StringBuilder, Errors> =
      Left (One "dummy")