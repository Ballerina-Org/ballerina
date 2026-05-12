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
      | _ ->
        sum.Throw(Errors.Singleton () (fun () -> "Expected BriefToPlan operation"))

    static member AsCmsStage
      (op: AIConfiguratorOperations<'ext>)
      : Sum<unit, Errors<Unit>> =
      match op with
      | AIConfiguratorOperations.CmsStage -> sum.Return()
      | _ ->
        sum.Throw(Errors.Singleton () (fun () -> "Expected CmsStage operation"))

    static member AsProductsStage
      (op: AIConfiguratorOperations<'ext>)
      : Sum<unit, Errors<Unit>> =
      match op with
      | AIConfiguratorOperations.ProductsStage -> sum.Return()
      | _ ->
        sum.Throw(Errors.Singleton () (fun () -> "Expected ProductsStage operation"))
