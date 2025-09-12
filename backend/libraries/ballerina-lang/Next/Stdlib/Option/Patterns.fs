namespace Ballerina.DSL.Next.StdLib.Option

[<AutoOpen>]
module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  type OptionOperations<'ext> with
    static member AsMap(op: OptionOperations<'ext>) : Sum<Core.Option<Value<TypeValue, 'ext>>, Errors> =
      match op with
      | Option_Map v -> v.f |> sum.Return


  type OptionValues<'ext> with
    static member AsOption(op: OptionValues<'ext>) : Sum<Core.Option<Value<TypeValue, 'ext>>, Errors> =
      match op with
      | Option v -> v |> sum.Return
