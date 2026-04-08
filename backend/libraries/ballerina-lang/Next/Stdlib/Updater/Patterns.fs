namespace Ballerina.DSL.Next.StdLib.Updater

[<AutoOpen>]
module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors

  type UpdaterOperations<'ext> with
    static member AsApply(op: UpdaterOperations<'ext>) : Sum<UpdaterType<'ext>, Errors<unit>> =
      match op with
      | UpdaterOperations.Apply v -> v.Updater |> sum.Return
