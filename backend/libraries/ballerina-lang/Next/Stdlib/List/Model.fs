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

  type ListValues<'ext> =
    | List of List<Value<TypeValue<'ext>, 'ext>>

    override self.ToString() : string =
      match self with
      | List values ->
        let valueStr = values |> List.map (fun v -> v.ToString()) |> String.concat "; "
        $"[{valueStr}]"

  type ListKind =
    | Empty = 1
    | Cons = 2

  type ListValueDTO<'extDTO when 'extDTO: not null and 'extDTO: not struct> =
    { Kind: ListKind
      List: ValueDTO<'extDTO>[] | null }

    static member CreateEmpty: ListValueDTO<'extDTO> = { Kind = ListKind.Empty; List = null }

    static member CreateList l : ListValueDTO<'extDTO> =
      { Kind = ListKind.Cons
        List = l |> List.toArray }

  type ListDeltaExt<'valueExt, 'deltaExt> =
    | UpdateElement of index: int * delta: Delta<'valueExt, 'deltaExt>
    | AppendElement of value: Model.Value<Model.TypeValue<'valueExt>, 'valueExt>
    | RemoveElement of index: int
    | InsertElement of index: int * value: Model.Value<Model.TypeValue<'valueExt>, 'valueExt>
    | DuplicateElement of index: int
    | SetAllElements of value: Model.Value<Model.TypeValue<'valueExt>, 'valueExt>
    | RemoveAllElements
    | MoveElement of fromIndex: int * toIndex: int


  type ListDeltaExtDiscriminator =
    | UpdateElement = 1
    | AppendElement = 2
    | RemoveElement = 3
    | InsertElement = 4
    | DuplicateElement = 5
    | SetAllElements = 6
    | RemoveAllElements = 7
    | MoveElement = 8

  type UpdateElementDTO<'extDTO, 'deltaExtDTO
    when 'extDTO: not null and 'extDTO: not struct and 'deltaExtDTO: not null and 'deltaExtDTO: not struct> =
    { Index: int
      Value: DeltaDTO<'extDTO, 'deltaExtDTO> }

  type InsertElementDTO<'extDTO when 'extDTO: not null and 'extDTO: not struct> =
    { Index: int; Value: ValueDTO<'extDTO> }

  type DuplicateElementDTO<'extDTO when 'extDTO: not null and 'extDTO: not struct> = { Index: int }

  type MoveElementDTO<'extDTO when 'extDTO: not null and 'extDTO: not struct> = { From: int; To: int }

  type ListDeltaExtDTO<'extDTO, 'deltaExtDTO
    when 'extDTO: not null and 'extDTO: not struct and 'deltaExtDTO: not null and 'deltaExtDTO: not struct> =
    { Discriminator: ListDeltaExtDiscriminator
      UpdateElement: UpdateElementDTO<'extDTO, 'deltaExtDTO> | null
      AppendElement: ValueDTO<'extDTO> | null
      RemoveElement: Nullable<int>
      InsertElement: InsertElementDTO<'extDTO> | null
      DuplicateElement: DuplicateElementDTO<'extDTO> | null
      SetAllElements: ValueDTO<'extDTO> | null
      RemoveAllElements: Nullable<bool> // not sure about this
      MoveElement: MoveElementDTO<'extDTO> | null }

    static member Empty =
      { Discriminator = ListDeltaExtDiscriminator.UpdateElement
        UpdateElement = null
        AppendElement = null
        RemoveElement = Nullable()
        InsertElement = null
        DuplicateElement = null
        SetAllElements = null
        RemoveAllElements = Nullable()
        MoveElement = null }

    static member CreateUpdate index delta : ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          Discriminator = ListDeltaExtDiscriminator.UpdateElement
          UpdateElement = { Index = index; Value = delta } }

    static member CreateAppend value : ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          Discriminator = ListDeltaExtDiscriminator.AppendElement
          AppendElement = value }

    static member CreateRemove(index: int) : ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          Discriminator = ListDeltaExtDiscriminator.RemoveElement
          RemoveElement = Nullable index }

    static member CreateInsert index value : ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          Discriminator = ListDeltaExtDiscriminator.InsertElement
          InsertElement = { Index = index; Value = value } }

    static member CreateDuplicate index : ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          Discriminator = ListDeltaExtDiscriminator.DuplicateElement
          DuplicateElement = { Index = index } }

    static member CreateSetAllElements value : ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          Discriminator = ListDeltaExtDiscriminator.SetAllElements
          SetAllElements = value }

    static member CreateRemoveAllElements: ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          Discriminator = ListDeltaExtDiscriminator.RemoveAllElements
          RemoveAllElements = Nullable true }

    static member CreateMoveElement fromIndex toIndex : ListDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { ListDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          Discriminator = ListDeltaExtDiscriminator.MoveElement
          MoveElement = { From = fromIndex; To = toIndex } }
