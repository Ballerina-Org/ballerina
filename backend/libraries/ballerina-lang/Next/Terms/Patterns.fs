namespace Ballerina.DSL.Next.Terms

module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Terms.Model

  type Var with
    static member Create(name: string) : Var = { Name = name }
