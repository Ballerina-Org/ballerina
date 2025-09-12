namespace Ballerina.DSL.Next.StdLib.Option

[<AutoOpen>]
module Model =
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  type OptionConstructors =
    | Option_Some
    | Option_None

  type OptionOperations<'ext> = Option_Map of {| f: Option<Value<TypeValue, 'ext>> |}

  type OptionValues<'ext> = Option of Option<Value<TypeValue, 'ext>>
