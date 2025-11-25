namespace Ballerina.DSL.Next.StdLib.Option

[<AutoOpen>]
module Model =
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  type OptionConstructors =
    | Option_Some
    | Option_None

    override self.ToString() : string =
      match self with
      | Option_Some -> "Option::Some"
      | Option_None -> "Option::None"

  type OptionOperations<'ext> =
    | Option_Map of {| f: Option<Value<TypeValue, 'ext>> |}

    override self.ToString() : string =
      match self with
      | Option_Map _ -> "Option::map"

  type OptionValues<'ext> =
    | Option of Option<Value<TypeValue, 'ext>>

    override self.ToString() : string =
      match self with
      | Option(Some v) -> $"Some({v.ToString()})"
      | Option(None) -> "None"
