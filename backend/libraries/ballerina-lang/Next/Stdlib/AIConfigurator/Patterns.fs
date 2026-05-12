namespace Ballerina.DSL.Next.StdLib.AIConfigurator

[<AutoOpen>]
module Patterns =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors

  type AIConfiguratorOperations<'ext> with
    static member AsBriefToPlan
      (op: AIConfiguratorOperations<'ext>)
      : Sum<unit, Errors<Unit>> =
      match op with
      | AIConfiguratorOperations.BriefToPlan -> sum.Return()
