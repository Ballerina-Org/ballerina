namespace Ballerina.DSL.Next.StdLib.List

[<AutoOpen>]
module Model =
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  // type ListConstructors<'ext> =
  //   | List_Cons of {| Closure: Option<Value<TypeValue, 'ext>> |}
  //   | List_Nil

  type ListOperations<'ext> =
    | List_Cons
    | List_Nil
    | List_Map of {| f: Option<Value<TypeValue, 'ext>> |}
    | List_Filter of {| f: Option<Value<TypeValue, 'ext>> |}

  type ListValues<'ext> = List of List<Value<TypeValue, 'ext>>
