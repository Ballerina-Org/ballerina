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
    | List_Fold of
      {| f: Option<Value<TypeValue, 'ext>>
         acc: Option<Value<TypeValue, 'ext>> |}
    | List_Length

    override self.ToString() : string =
      match self with
      | List_Cons -> "List::Cons"
      | List_Nil -> "List::Nil"
      | List_Map _ -> "List::map"
      | List_Filter _ -> "List::filter"
      | List_Fold _ -> "List::fold"
      | List_Length -> "List::length"

  type ListValues<'ext> =
    | List of List<Value<TypeValue, 'ext>>

    override self.ToString() : string =
      match self with
      | List values ->
        let valueStr = values |> List.map (fun v -> v.ToString()) |> String.concat "; "
        $"[{valueStr}]"
