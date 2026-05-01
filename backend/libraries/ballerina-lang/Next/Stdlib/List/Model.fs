namespace Ballerina.DSL.Next.StdLib.List

module Model =
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Serialization.PocoObjects
  open Ballerina.Data.Delta
  open Ballerina.Data.Delta.Serialization.DeltaDTO
  open System

  // type ListConstructors<'ext> =
  //   | List_Cons of {| Closure: Option<Value<TypeValue<'ext>, 'ext>> |}
  //   | List_Nil

  type ListOperations<'ext> =
    | List_Cons
    | List_Nil
    | List_Map of {| f: Option<Value<TypeValue<'ext>, 'ext>> |}
    | List_Filter of {| f: Option<Value<TypeValue<'ext>, 'ext>> |}
    | List_Append of {| l: Option<Value<TypeValue<'ext>, 'ext>> |}
    | List_Fold of
      {| f: Option<Value<TypeValue<'ext>, 'ext>>
         acc: Option<Value<TypeValue<'ext>, 'ext>> |}
    | List_Length
    | List_Decompose
    | List_OrderBy of {| f: Option<Value<TypeValue<'ext>, 'ext>> |}
    | List_Any of {| f: Option<Value<TypeValue<'ext>, 'ext>> |}

    override self.ToString() : string =
      match self with
      | List_Cons -> "List::Cons"
      | List_Nil -> "List::Nil"
      | List_Map _ -> "List::map"
      | List_Filter _ -> "List::filter"
      | List_Append _ -> "List::append"
      | List_Fold _ -> "List::fold"
      | List_Length -> "List::length"
      | List_Decompose -> "List::decompose"
      | List_OrderBy _ -> "List::orderBy"
      | List_Any _ -> "List::any"

  type ListValues<'ext> =
    | List of List<Value<TypeValue<'ext>, 'ext>>

    override self.ToString() : string =
      match self with
      | List values ->
        let valueStr =
          values |> List.map (fun v -> v.ToString()) |> String.concat "; "

        $"[{valueStr}]"


  type ListValueDTO<'extDTO when 'extDTO: not null and 'extDTO: not struct> =
    ValueDTO<'extDTO>[]

  type ListDeltaExt<'valueExt, 'deltaExt> =
    | UpdateElement of index: int * delta: Delta<'valueExt, 'deltaExt>
    | AppendElement of value: Model.Value<Model.TypeValue<'valueExt>, 'valueExt>
    | RemoveElement of index: int
    | InsertElement of
      index: int *
      value: Model.Value<Model.TypeValue<'valueExt>, 'valueExt>
    | DuplicateElement of index: int
    | SetAllElements of
      value: Model.Value<Model.TypeValue<'valueExt>, 'valueExt>
    | RemoveAllElements
    | MoveElement of fromIndex: int * toIndex: int

  [<NoComparison; NoEquality>]
  type UpdateElementDTO<'extDTO, 'deltaExtDTO
    when 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct> =
    { Index: int
      Value: DeltaDTO<'extDTO, 'deltaExtDTO> }

  [<NoComparison; NoEquality>]
  type InsertElementDTO<'extDTO when 'extDTO: not null and 'extDTO: not struct>
    =
    { Index: int; Value: ValueDTO<'extDTO> }

  type DuplicateElementDTO<'extDTO
    when 'extDTO: not null and 'extDTO: not struct> = { Index: int }

  type MoveElementDTO<'extDTO when 'extDTO: not null and 'extDTO: not struct> =
    { From: int; To: int }

  [<NoComparison; NoEquality>]
  type ListDeltaExtDTO<'extDTO, 'deltaExtDTO
    when 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct> =
    { UpdateElement:
        System.Collections.Generic.Dictionary<
          int,
          DeltaDTO<'extDTO, 'deltaExtDTO>
         >
      AppendElement: ValueDTO<'extDTO> | null
      RemoveElement: Nullable<int>
      InsertElement:
        System.Collections.Generic.Dictionary<int, ValueDTO<'extDTO>>
      DuplicateElement: Nullable<int>
      SetAllElements: ValueDTO<'extDTO> | null
      RemoveAllElements: Nullable<bool> // not sure about this
      MoveElement: MoveElementDTO<'extDTO> | null }

    static member Empty =
      { UpdateElement = null
        AppendElement = null
        RemoveElement = Nullable()
        InsertElement = null
        DuplicateElement = Nullable()
        SetAllElements = null
        RemoveAllElements = Nullable()
        MoveElement = null }

    static member CreateUpdate
      index
      delta
      : ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      let updateElement =
        new System.Collections.Generic.Dictionary<
          int,
          DeltaDTO<'extDTO, 'deltaExtDTO>
         >()

      updateElement.Add(index, delta)

      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          UpdateElement = updateElement }

    static member CreateAppend value : ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          AppendElement = value }

    static member CreateRemove
      (index: int)
      : ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          RemoveElement = Nullable index }

    static member CreateInsert
      index
      value
      : ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      let insertElement =
        new System.Collections.Generic.Dictionary<int, ValueDTO<'extDTO>>()

      insertElement.Add(index, value)

      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          InsertElement = insertElement }

    static member CreateDuplicate
      index
      : ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          DuplicateElement = Nullable index }

    static member CreateSetAllElements
      value
      : ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          SetAllElements = value }

    static member CreateRemoveAllElements
      : ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          RemoveAllElements = Nullable true }

    static member CreateMoveElement
      fromIndex
      toIndex
      : ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          MoveElement = { From = fromIndex; To = toIndex } }
