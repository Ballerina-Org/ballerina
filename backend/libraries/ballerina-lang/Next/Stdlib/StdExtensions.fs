module Ballerina.DSL.Next.StdLib.Extensions

open Ballerina.DSL.Next.Types
open Ballerina.DSL.Next.StdLib.TimeSpan
open Ballerina.Lenses
open Ballerina.DSL.Next.Extensions
open Ballerina.DSL.Next.StdLib
open Ballerina.Collections.Sum
open Ballerina.Errors
open Ballerina.DSL.Next.Serialization.PocoObjects
open System
open Ballerina.DSL.Next.StdLib.List.Model
open Ballerina.DSL.Next.StdLib.Map.Model
open Ballerina.DSL.Next.StdLib.DB
open Ballerina.Data.Delta
open Ballerina.DSL.Next.StdLib.Email
open Ballerina.DSL.Next.StdLib.String
open Ballerina.DSL.Next.Types.TypeChecker.Model
open Ballerina.Cat.Collections.OrderedMap
open Ballerina.DSL.Next.StdLib.DB.Extension.DBRun
open Ballerina.DSL.Next.StdLib.View.Extension
open Ballerina.DSL.Next.StdLib.Coroutine.Extension
open Ballerina.DSL.Next.StdLib.WebApp.Extension

type ValueExt<'runtimeContext, 'db, 'customExtension
  when 'db: comparison and 'customExtension: comparison> =
  | VList of ListExt<'runtimeContext, 'db, 'customExtension>
  | VViewProps of ViewPropsOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | VCo of CoroutineOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | VPrimitive of PrimitiveExt<'runtimeContext, 'db, 'customExtension>
  | VComposite of CompositeTypeExt<'runtimeContext, 'db, 'customExtension>
  | VDB of DBExt<'runtimeContext, 'db, 'customExtension>
  | VMap of MapExt<'runtimeContext, 'db, 'customExtension>
  | VWebApp of WebAppOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | VCustom of 'customExtension

  override self.ToString() =
    match self with
    | VList ext -> ext.ToString()
    | VViewProps ext -> ext.ToString()
    | VCo ext -> ext.ToString()
    | VPrimitive ext -> ext.ToString()
    | VComposite ext -> ext.ToString()
    | VDB ext -> ext.ToString()
    | VMap ext -> ext.ToString()
    | VWebApp ext -> ext.ToString()
    | VCustom ext -> ext.ToString()

and [<NoComparison; NoEquality>] ValueExtDTO =
  { List: List.Model.ListValueDTO<ValueExtDTO>
    Map: Map.Model.MapValueDTO<ValueExtDTO> }

  static member Empty = { List = null; Map = null }

and DBExt<'runtimeContext, 'db, 'customExtension
  when 'db: comparison and 'customExtension: comparison> =
  | DBValues of
    DB.Model.DBValues<
      'runtimeContext,
      'db,
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >

  override self.ToString() : string =
    match self with
    | DBValues vals -> vals.ToString()

  static member inline ValueLens
    : PartialLens<
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        DBValues<
          'runtimeContext,
          'db,
          ValueExt<'runtimeContext, 'db, 'customExtension>
         >
       > =
    { Get =
        function
        | VDB(DBExt.DBValues x) -> Some x
        | _ -> None
      Set = DBExt.DBValues >> VDB }

and MapExt<'runtimeContext, 'db, 'customExtension
  when 'db: comparison and 'customExtension: comparison> =
  | MapOperations of
    Map.Model.MapOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | MapValues of
    Map.Model.MapValues<ValueExt<'runtimeContext, 'db, 'customExtension>>

  override self.ToString() : string =
    match self with
    | MapOperations ops -> ops.ToString()
    | MapValues vals -> vals.ToString()

  static member inline ValueLens
    : PartialLens<
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        MapValues<ValueExt<'runtimeContext, 'db, 'customExtension>>
       > =
    { Get =
        function
        | VMap(MapValues x) -> Some x
        | _ -> None
      Set = MapValues >> VMap }

  static member inline ValueDTOLens =
    { Get =
        fun valueExtDTO ->
          match valueExtDTO.Map with
          | null -> None
          | map -> Some map
      Set =
        fun mapValueDTO ->
          { ValueExtDTO.Empty with
              Map = mapValueDTO } }

and CompositeTypeExt<'runtimeContext, 'db, 'customExtension
  when 'db: comparison and 'customExtension: comparison> =
  | CompositeType of
    Choice<
      DateOnlyExt<'runtimeContext, 'db, 'customExtension>,
      DateTimeExt<'runtimeContext, 'db, 'customExtension>,
      GuidExt<'runtimeContext, 'db, 'customExtension>,
      TimeSpanExt<'runtimeContext, 'db, 'customExtension>,
      UpdaterExt<'runtimeContext, 'db, 'customExtension>
     >

  override self.ToString() : string =
    match self with
    | CompositeType(Choice1Of5 ct) -> ct.ToString()
    | CompositeType(Choice2Of5 ct) -> ct.ToString()
    | CompositeType(Choice3Of5 ct) -> ct.ToString()
    | CompositeType(Choice4Of5 ct) -> ct.ToString()
    | CompositeType(Choice5Of5 ct) -> ct.ToString()

and ListExt<'runtimeContext, 'db, 'customExtension
  when 'db: comparison and 'customExtension: comparison> =
  | ListOperations of
    List.Model.ListOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | ListValues of
    List.Model.ListValues<ValueExt<'runtimeContext, 'db, 'customExtension>>

  override self.ToString() : string =
    match self with
    | ListOperations ops -> ops.ToString()
    | ListValues vals -> vals.ToString()

  static member inline ValueLens
    : PartialLens<
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        ListValues<ValueExt<'runtimeContext, 'db, 'customExtension>>
       > =
    { Get =
        fun (v: ValueExt<'runtimeContext, 'db, 'customExtension>) ->
          match v with
          | VList(ListValues x) -> Some x
          | _ -> None
      Set = ListValues >> VList }


  static member inline ValueDTOLens =
    { Get =
        fun valueExtDTO ->
          match valueExtDTO.List with
          | null -> None
          | list -> Some list
      Set =
        fun valueExtDTO ->
          { ValueExtDTO.Empty with
              List = valueExtDTO } }

and DateOnlyExt<'runtimeContext, 'db, 'customExtension
  when 'db: comparison and 'customExtension: comparison> =
  | DateOnlyOperations of
    DateOnly.Model.DateOnlyOperations<
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >

  override self.ToString() : string =
    match self with
    | DateOnlyOperations ops -> ops.ToString()

and DateTimeExt<'runtimeContext, 'db, 'customExtension
  when 'db: comparison and 'customExtension: comparison> =
  | DateTimeOperations of
    DateTime.Model.DateTimeOperations<
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >

  override self.ToString() : string =
    match self with
    | DateTimeOperations ops -> ops.ToString()

and GuidExt<'runtimeContext, 'db, 'customExtension
  when 'db: comparison and 'customExtension: comparison> =
  | GuidOperations of
    Guid.Model.GuidOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>

  override self.ToString() : string =
    match self with
    | GuidOperations ops -> ops.ToString()

and TimeSpanExt<'runtimeContext, 'db, 'customExtension
  when 'db: comparison and 'customExtension: comparison> =
  | TimeSpanOperations of
    TimeSpan.Model.TimeSpanOperations<
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >

  override self.ToString() : string =
    match self with
    | TimeSpanOperations ops -> ops.ToString()

and UpdaterExt<'runtimeContext, 'db, 'customExtension
  when 'db: comparison and 'customExtension: comparison> =
  | UpdaterOperations of
    Updater.Model.UpdaterOperations<
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >

and PrimitiveExt<'runtimeContext, 'db, 'customExtension
  when 'db: comparison and 'customExtension: comparison> =
  | BoolOperations of
    Bool.Model.BoolOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | Int32Operations of
    Int32.Model.Int32Operations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | Int64Operations of
    Int64.Model.Int64Operations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | Float32Operations of
    Float32.Model.Float32Operations<
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >
  | Float64Operations of
    Float64.Model.Float64Operations<
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >
  | DecimalOperations of
    Decimal.Model.DecimalOperations<
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >
  | EmailOperations of
    Email.Model.EmailOperations<
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >
  | StringOperations of
    String.Model.StringOperations<
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >

  override self.ToString() : string =
    match self with
    | BoolOperations ops -> ops.ToString()
    | Int32Operations ops -> ops.ToString()
    | Int64Operations ops -> ops.ToString()
    | Float32Operations ops -> ops.ToString()
    | Float64Operations ops -> ops.ToString()
    | DecimalOperations ops -> ops.ToString()
    | EmailOperations ops -> ops.ToString()
    | StringOperations ops -> ops.ToString()

and DeltaExt<'runtimeContext, 'db, 'customExtension
  when 'db: comparison and 'customExtension: comparison> =
  | DeltaExtension of
    Choice<
      ListDeltaExt<
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        DeltaExt<'runtimeContext, 'db, 'customExtension>
       >,
      TupleDeltaExt<'runtimeContext, 'db, 'customExtension>,
      OptionDeltaExt,
      Map.Model.MapDeltaExt<
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        DeltaExt<'runtimeContext, 'db, 'customExtension>
       >
     >

  static member inline ListDeltaLens
    : PartialLens<
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        ListDeltaExt<
          ValueExt<'runtimeContext, 'db, 'customExtension>,
          DeltaExt<'runtimeContext, 'db, 'customExtension>
         >
       > =
    { Get =
        (function
        | DeltaExtension(Choice1Of4 listDelta) -> Some listDelta
        | _ -> None)
      Set = Choice1Of4 >> DeltaExtension }

  static member inline ListDeltaDTOLens =
    { Get =
        fun deltaExtDTO ->
          match deltaExtDTO.ListDelta with
          | null -> None
          | listDelta -> Some listDelta
      Set =
        fun deltaExtDTO ->
          { DeltaExtDTO.Empty with
              ListDelta = deltaExtDTO } }

  static member inline MapDeltaLens
    : PartialLens<
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        Map.Model.MapDeltaExt<
          ValueExt<'runtimeContext, 'db, 'customExtension>,
          DeltaExt<'runtimeContext, 'db, 'customExtension>
         >
       > =
    { Get =
        (function
        | DeltaExtension(Choice4Of4 mapDelta) -> Some mapDelta
        | _ -> None)
      Set = Choice4Of4 >> DeltaExtension }

  static member inline MapDeltaDTOLens =
    { Get =
        fun deltaExtDTO ->
          match deltaExtDTO.MapDelta with
          | null -> None
          | mapDelta -> Some mapDelta
      Set =
        fun deltaExtDTO ->
          { DeltaExtDTO.Empty with
              MapDelta = deltaExtDTO } }

  static member Getters = {| DeltaExt = fun (DeltaExtension e) -> e |}

  static member ToUpdater
    : (DeltaExt<'runtimeContext, 'db, 'customExtension>
        -> Model.Value<
          Model.TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
          ValueExt<'runtimeContext, 'db, 'customExtension>
         >
        -> Sum<
          Model.Value<
            Model.TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
            ValueExt<'runtimeContext, 'db, 'customExtension>
           >,
          Errors<unit>
         >) =
    fun (delta: DeltaExt<'runtimeContext, 'db, 'customExtension>) ->
      fun
          (value:
            Model.Value<
              Model.TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
              ValueExt<'runtimeContext, 'db, 'customExtension>
             >) ->
        sum {
          match value with
          | Value.Ext(VList(ListValues(List.Model.ListValues.List l)),
                      _) ->
            match delta with
            | DeltaExt.DeltaExtension(Choice1Of4(UpdateElement(i, delta))) ->
              let! updater =
                Delta.ToUpdater
                  DeltaExt<'runtimeContext, 'db, 'customExtension>.ToUpdater
                  delta

              let currentElement = List.item i l
              let! updatedElement = updater currentElement
              let next = List.updateAt i updatedElement l

              return
                (VList(ListValues(List.Model.ListValues.List next)),
                 None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.AppendElement(v))) ->
              let next = List.append l [ v ]

              return
                (VList(ListValues(List.Model.ListValues.List next)),
                 None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.RemoveElement(i))) ->
              let next = List.removeAt i l

              return
                (VList(ListValues(List.Model.ListValues.List next)),
                 None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.InsertElement(i, v))) ->
              let next = List.insertAt (i + 1) v l

              return
                (VList(ListValues(List.Model.ListValues.List next)),
                 None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.DuplicateElement(i))) ->
              let next = List.insertAt (i + 1) (List.item i l) l

              return
                (VList(ListValues(List.Model.ListValues.List next)),
                 None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.SetAllElements(v))) ->
              let next = List.map (fun _ -> v) l

              return
                (VList(ListValues(List.Model.ListValues.List next)),
                 None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.RemoveAllElements)) ->
              let next = List.empty

              return
                (VList(ListValues(List.Model.ListValues.List next)),
                 None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.MoveElement(fromIndex,
                                                                          toIndex))) ->
              let len = List.length l

              let next =
                if fromIndex < 0 || fromIndex >= len then
                  l
                elif toIndex < 0 || toIndex >= len then
                  l
                elif fromIndex = toIndex then
                  l
                else
                  let element = List.item fromIndex l
                  let withoutElement = List.removeAt fromIndex l
                  List.insertAt toIndex element withoutElement

              return
                (VList(ListValues(List.Model.ListValues.List next)),
                 None)
                |> Value.Ext
            | other ->
              return!
                sum.Throw(
                  Errors.Singleton () (fun () ->
                    $"Unimplemented delta ext toUpdater for {other}")
                )
          | Value.Ext(VMap(MapExt.MapValues(Map.Model.MapValues.Map m)),
                      _) ->
            match delta with
            | DeltaExt.DeltaExtension(Choice4Of4(Map.Model.MapDeltaExt.UpdateKey(oldKey,
                                                                                 newKey))) ->
              match Map.tryFind oldKey m with
              | Some value ->
                let next = m |> Map.remove oldKey |> Map.add newKey value

                return
                  (VMap(MapExt.MapValues(Map.Model.MapValues.Map next)),
                   None)
                  |> Value.Ext
              | None ->
                return!
                  sum.Throw(
                    Errors.Singleton () (fun () ->
                      $"Key not found in map for UpdateKey delta")
                  )
            | DeltaExt.DeltaExtension(Choice4Of4(Map.Model.MapDeltaExt.UpdateValue(key,
                                                                                   delta))) ->
              match Map.tryFind key m with
              | Some currentValue ->
                let! updater =
                  Delta.ToUpdater
                    DeltaExt<'runtimeContext, 'db, 'customExtension>.ToUpdater
                    delta

                let! updatedValue = updater currentValue
                let next = m |> Map.add key updatedValue

                return
                  (VMap(MapExt.MapValues(Map.Model.MapValues.Map next)),
                   None)
                  |> Value.Ext
              | None ->
                return!
                  sum.Throw(
                    Errors.Singleton () (fun () ->
                      $"Key not found in map for UpdateValue delta")
                  )
            | DeltaExt.DeltaExtension(Choice4Of4(Map.Model.MapDeltaExt.AddItem(key,
                                                                               value))) ->
              let next = m |> Map.add key value

              return
                (VMap(MapExt.MapValues(Map.Model.MapValues.Map next)),
                 None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice4Of4(Map.Model.MapDeltaExt.RemoveItem(key))) ->
              let next = m |> Map.remove key

              return
                (VMap(MapExt.MapValues(Map.Model.MapValues.Map next)),
                 None)
                |> Value.Ext
            | other ->
              return!
                sum.Throw(
                  Errors.Singleton () (fun () ->
                    $"Unimplemented delta ext toUpdater for map op: {other}")
                )
          | Value.Tuple elements ->
            match delta with
            | DeltaExt.DeltaExtension(Choice2Of4(TupleDeltaExt.RemoveElement(i))) ->
              let next = List.removeAt i elements
              return Value.Tuple next
            | DeltaExt.DeltaExtension(Choice2Of4(TupleDeltaExt.AppendElement(value))) ->
              let next = List.append elements [ value ]
              return Value.Tuple next
            | other ->
              return!
                sum.Throw(
                  Errors.Singleton () (fun () ->
                    $"Unimplemented delta ext toUpdater for tuple op: {other}")
                )
          | other ->
            return!
              sum.Throw(
                Errors.Singleton () (fun () ->
                  $"Expected value to be a list or map for Delta ext, got {other}")
              )
        }

and TupleDeltaExt<'runtimeContext, 'db, 'customExtension
  when 'db: comparison and 'customExtension: comparison> =
  | RemoveElement of index: int
  | AppendElement of
    value:
      Model.Value<
        Model.TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >

and OptionDeltaExt = OptionDeltaExt

and [<NoComparison; NoEquality>] DeltaExtDTO =
  { ListDelta: ListDeltaExtDTO<ValueExtDTO, DeltaExtDTO> | null
    MapDelta: Map.Model.MapDeltaExtDTO<ValueExtDTO, DeltaExtDTO> | null }

  static member Empty = { ListDelta = null; MapDelta = null }



[<NoComparison; NoEquality>]
type StdExtensions<'runtimeContext, 'valueExt, 'valueExtDTO, 'deltaExt, 'deltaExtDTO
  when 'valueExt: comparison
  and 'deltaExt: comparison
  and 'valueExtDTO: not null
  and 'valueExtDTO: not struct
  and 'deltaExtDTO: not null
  and 'deltaExtDTO: not struct> =
  { List:
      TypeExtension<
        'runtimeContext,
        'valueExt,
        'valueExtDTO,
        'deltaExt,
        'deltaExtDTO,
        Unit,
        List.Model.ListValues<'valueExt>,
        List.Model.ListOperations<'valueExt>
       >
    Bool:
      OperationsExtension<
        'runtimeContext,
        'valueExt,
        Bool.Model.BoolOperations<'valueExt>
       >
    Int32:
      OperationsExtension<
        'runtimeContext,
        'valueExt,
        Int32.Model.Int32Operations<'valueExt>
       >
    Int64:
      OperationsExtension<
        'runtimeContext,
        'valueExt,
        Int64.Model.Int64Operations<'valueExt>
       >
    Float32:
      OperationsExtension<
        'runtimeContext,
        'valueExt,
        Float32.Model.Float32Operations<'valueExt>
       >
    Float64:
      OperationsExtension<
        'runtimeContext,
        'valueExt,
        Float64.Model.Float64Operations<'valueExt>
       >
    Decimal:
      OperationsExtension<
        'runtimeContext,
        'valueExt,
        Decimal.Model.DecimalOperations<'valueExt>
       >
    DateOnly:
      OperationsExtension<
        'runtimeContext,
        'valueExt,
        DateOnly.Model.DateOnlyOperations<'valueExt>
       >
    DateTime:
      OperationsExtension<
        'runtimeContext,
        'valueExt,
        DateTime.Model.DateTimeOperations<'valueExt>
       >
    String:
      OperationsExtension<
        'runtimeContext,
        'valueExt,
        String.Model.StringOperations<'valueExt>
       >
    Guid:
      OperationsExtension<
        'runtimeContext,
        'valueExt,
        Guid.Model.GuidOperations<'valueExt>
       >
    TimeSpan:
      OperationsExtension<
        'runtimeContext,
        'valueExt,
        TimeSpan.Model.TimeSpanOperations<'valueExt>
       >
    Updater:
      OperationsExtension<
        'runtimeContext,
        'valueExt,
        Updater.Model.UpdaterOperations<'valueExt>
       >
    Map:
      TypeExtension<
        'runtimeContext,
        'valueExt,
        'valueExtDTO,
        'deltaExt,
        'deltaExtDTO,
        Unit,
        Map.Model.MapValues<'valueExt>,
        Map.Model.MapOperations<'valueExt>
       > }

let makeExtensions<'runtimeContext, 'db, 'customExtension
  when 'db: comparison and 'customExtension: comparison>
  (string_ops: StringTypeClass<ValueExt<'runtimeContext, 'db, 'customExtension>>)
  (email_ops: EmailTypeClass<'runtimeContext>)
  (db_ops:
    DBTypeClass<
      'runtimeContext,
      'db,
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >)
  (typeCheckingConfig:
    Option<TypeCheckingConfig<ValueExt<'runtimeContext, 'db, 'customExtension>>>)
  : StdExtensions<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO,
      DeltaExt<'runtimeContext, 'db, 'customExtension>,
      DeltaExtDTO
     > *
    LanguageContext<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO,
      DeltaExt<'runtimeContext, 'db, 'customExtension>,
      DeltaExtDTO
     > *
    TypeCheckingConfig<ValueExt<'runtimeContext, 'db, 'customExtension>>
  =

  let registerDBExtensions, query_sym, mk_query =
    Ballerina.DSL.Next.StdLib.DB.Extension.CUD.registerDBExtensions
      db_ops
      { Get =
          function
          | VList(ListExt.ListValues(List.Model.ListValues.List values)) ->
            Some values
          | _ -> None
        Set =
          List.Model.ListValues.List
          >> ListExt.ListValues
          >> VList }
      { Get =
          function
          | VMap(MapValues(Map.Model.MapValues.Map x)) -> Some x
          | _ -> None
        Set =
          Map.Model.MapValues.Map
          >> MapValues
          >> VMap }
      DBExt<_, _, _>.ValueLens
      typeCheckingConfig

  let listExtension, list_sym, mk_list_type =
    List.Extension.ListExtension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO,
      DeltaExt<'runtimeContext, 'db, 'customExtension>,
      DeltaExtDTO
     >
      ListExt<_, _, _>.ValueLens
      { Get =
          function
          | VList(ListOperations x) -> Some x
          | _ -> None
        Set = ListOperations >> VList }
      ListExt<_, _, _>.ValueDTOLens
      DeltaExt<_, _, _>.ListDeltaLens
      DeltaExt<_, _, _>.ListDeltaDTOLens
      (typeCheckingConfig |> Option.map (fun cfg -> cfg.ListTypeSymbol))

  let viewExtension, viewPropsExtension, reactNodeExtension, reactComponentExtension, view_sym, view_props_sym, react_node_sym, react_component_sym, mk_view_type, mk_view_props_type, _react_node_type, _mk_react_component_type =
    View.Extension.ViewExtension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO,
      DeltaExt<'runtimeContext, 'db, 'customExtension>,
      DeltaExtDTO
     >
      { Get = function | VViewProps x -> Some x | _ -> None
        Set = VViewProps }
      (typeCheckingConfig |> Option.map (fun cfg -> cfg.ViewTypeSymbol))
      (typeCheckingConfig |> Option.map (fun cfg -> cfg.ViewPropsTypeSymbol))
      (typeCheckingConfig |> Option.map (fun cfg -> cfg.ReactNodeTypeSymbol))
      (typeCheckingConfig |> Option.map (fun cfg -> cfg.ReactComponentTypeSymbol))

  let coroutineExtension, co_sym, mk_co_type =
    Coroutine.Extension.CoroutineExtension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO,
      DeltaExt<'runtimeContext, 'db, 'customExtension>,
      DeltaExtDTO
     >
      { Get = function | VCo x -> Some x | _ -> None
        Set = VCo }
      (typeCheckingConfig |> Option.map (fun cfg -> cfg.CoTypeSymbol))
      (Identifier.FullyQualified([ "Frontend" ], "View"))

  let webAppIOExtension, webAppRunExtension, _webapp_sym, _mk_webapp_io_type =
    WebApp.Extension.WebAppExtension<
      'runtimeContext,
      'db,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO,
      DeltaExt<'runtimeContext, 'db, 'customExtension>,
      DeltaExtDTO
     >
      { Get = function | VWebApp x -> Some x | _ -> None
        Set = VWebApp }
      DBExt<'runtimeContext, 'db, 'customExtension>.ValueLens
      None
      (Identifier.FullyQualified([ "Frontend" ], "View"))
      (Identifier.LocalScope "Co")
      (Identifier.LocalScope "DBIO")

  let dateOnlyExtension =
    DateOnly.Extension.DateOnlyExtension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO
     >
      { Get =
          function
          | VComposite(CompositeType(Choice1Of5(DateOnlyOperations x))) ->
            Some x
          | _ -> None
        Set =
          DateOnlyOperations
          >> Choice1Of5
          >> CompositeType
          >> VComposite }

  let boolExtension =
    Bool.Extension.BoolExtension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >
      { Get =
          function
          | VPrimitive(BoolOperations x) -> Some x
          | _ -> None
        Set = BoolOperations >> VPrimitive }

  let int32Extension =
    Int32.Extension.Int32Extension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >
      { Get =
          function
          | VPrimitive(Int32Operations x) -> Some x
          | _ -> None
        Set = Int32Operations >> VPrimitive }

  let int64Extension =
    Int64.Extension.Int64Extension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >
      { Get =
          function
          | VPrimitive(Int64Operations x) -> Some x
          | _ -> None
        Set = Int64Operations >> VPrimitive }

  let float32Extension =
    Float32.Extension.Float32Extension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >
      { Get =
          function
          | VPrimitive(Float32Operations x) -> Some x
          | _ -> None
        Set = Float32Operations >> VPrimitive }

  let float64Extension =
    Float64.Extension.Float64Extension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >
      { Get =
          function
          | VPrimitive(Float64Operations x) -> Some x
          | _ -> None
        Set = Float64Operations >> VPrimitive }

  let decimalExtension =
    Decimal.Extension.DecimalExtension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >
      { Get =
          function
          | VPrimitive(DecimalOperations x) -> Some x
          | _ -> None
        Set = DecimalOperations >> VPrimitive }

  let dateTimeExtension =
    DateTime.Extension.DateTimeExtension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO
     >
      { Get =
          function
          | VComposite(CompositeType(Choice2Of5(DateTimeOperations x))) ->
            Some x
          | _ -> None
        Set =
          DateTimeOperations
          >> Choice2Of5
          >> CompositeType
          >> VComposite }

  let timeSpanExtension =
    TimeSpanExtension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO
     >
      { Get =
          function
          | VComposite(CompositeType(Choice4Of5(TimeSpanOperations x))) ->
            Some x
          | _ -> None
        Set =
          TimeSpanOperations
          >> Choice4Of5
          >> CompositeType
          >> VComposite }

  let stringExtension =
    String.Extension.StringExtension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >
      string_ops
      { Get =
          function
          | VPrimitive(StringOperations x) -> Some x
          | _ -> None
        Set = StringOperations >> VPrimitive }

  let emailExtension =
    Email.Extension.EmailExtension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >
      email_ops
      { Get =
          function
          | VPrimitive(EmailOperations x) -> Some x
          | _ -> None
        Set = EmailOperations >> VPrimitive }

  let guidExtension =
    Guid.Extension.GuidExtension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO
     >
      { Get =
          function
          | VComposite(CompositeType(Choice3Of5(GuidOperations x))) ->
            Some x
          | _ -> None
        Set =
          GuidOperations
          >> Choice3Of5
          >> CompositeType
          >> VComposite }

  let updaterExtension =
    Updater.Extension.UpdaterExtension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>
     >
      { Get =
          function
          | VComposite(CompositeType(Choice5Of5(UpdaterOperations x))) ->
            Some x
          | _ -> None
        Set =
          UpdaterOperations
          >> Choice5Of5
          >> CompositeType
          >> VComposite }

  let mapExtension =
    Map.Extension.MapExtension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO,
      DeltaExt<'runtimeContext, 'db, 'customExtension>,
      DeltaExtDTO
     >
      MapExt<_, _, _>.ValueLens
      { Get =
          function
          | VMap(MapOperations x) -> Some x
          | _ -> None
        Set = MapOperations >> VMap }
      (Some ListExt<'runtimeContext, 'db, 'customExtension>.ValueLens)
      MapExt<_, _, _>.ValueDTOLens
      DeltaExt<_, _, _>.MapDeltaLens
      DeltaExt<_, _, _>.MapDeltaDTOLens

  let context
    : LanguageContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        ValueExtDTO,
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        DeltaExtDTO
       > =
    LanguageContext<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO,
      DeltaExt<'runtimeContext, 'db, 'customExtension>,
      DeltaExtDTO
     >.Empty

  let context =
    context
    |> registerDBExtensions
    |> (listExtension |> TypeExtension.RegisterLanguageContext)
    |> (viewExtension |> TypeExtension.RegisterLanguageContext)
    |> (viewPropsExtension |> TypeExtension.RegisterLanguageContext)
    |> (reactNodeExtension |> TypeExtension.RegisterLanguageContext)
    |> (reactComponentExtension |> TypeExtension.RegisterLanguageContext)
    |> (coroutineExtension |> TypeExtension.RegisterLanguageContext)
    |> (webAppIOExtension |> TypeExtension.RegisterLanguageContext)
    |> (webAppRunExtension |> TypeLambdaExtension.RegisterLanguageContext)
    |> (dateOnlyExtension |> OperationsExtension.RegisterLanguageContext)
    |> (dateTimeExtension |> OperationsExtension.RegisterLanguageContext)
    |> (guidExtension |> OperationsExtension.RegisterLanguageContext)
    |> (timeSpanExtension |> OperationsExtension.RegisterLanguageContext)
    |> (boolExtension |> OperationsExtension.RegisterLanguageContext)
    |> (int32Extension |> OperationsExtension.RegisterLanguageContext)
    |> (int64Extension |> OperationsExtension.RegisterLanguageContext)
    |> (float32Extension |> OperationsExtension.RegisterLanguageContext)
    |> (float64Extension |> OperationsExtension.RegisterLanguageContext)
    |> (decimalExtension |> OperationsExtension.RegisterLanguageContext)
    |> (emailExtension |> OperationsExtension.RegisterLanguageContext)
    |> (stringExtension |> OperationsExtension.RegisterLanguageContext)
    |> (updaterExtension |> OperationsExtension.RegisterLanguageContext)
    |> (mapExtension |> TypeExtension.RegisterLanguageContext)

  // -- HTML View Attribute Schemas --
  let stringType = TypeValue.CreatePrimitive PrimitiveType.String
  let boolType = TypeValue.CreatePrimitive PrimitiveType.Bool
  let unitType = TypeValue.CreatePrimitive PrimitiveType.Unit

  let globalAttrs: Map<string, List<TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>>> =
    Map.ofList
      [ "class", [ stringType ]
        "id", [ stringType ]
        "style", [ stringType ]
        "title", [ stringType ]
        "hidden", [ boolType ]
        "tabIndex", [ stringType ]
        "role", [ stringType ]
        "key", [ stringType ] ]

  let eventHandlerAttrs: Map<string, List<TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>>> =
    Map.ofList
      [ "onClick", [ unitType ]
        "onChange", [ unitType ]
        "onSubmit", [ unitType ]
        "onInput", [ unitType ]
        "onFocus", [ unitType ]
        "onBlur", [ unitType ]
        "onKeyDown", [ unitType ]
        "onKeyUp", [ unitType ]
        "onKeyPress", [ unitType ]
        "onMouseDown", [ unitType ]
        "onMouseUp", [ unitType ]
        "onMouseEnter", [ unitType ]
        "onMouseLeave", [ unitType ]
        "onMouseOver", [ unitType ]
        "onMouseOut", [ unitType ]
        "onDoubleClick", [ unitType ]
        "onContextMenu", [ unitType ]
        "onDragStart", [ unitType ]
        "onDrag", [ unitType ]
        "onDragEnd", [ unitType ]
        "onDragOver", [ unitType ]
        "onDrop", [ unitType ]
        "onScroll", [ unitType ]
        "onTouchStart", [ unitType ]
        "onTouchEnd", [ unitType ]
        "onTouchMove", [ unitType ] ]

  let mergeAttrs
    (maps: List<Map<string, List<TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>>>>)
    : Map<string, List<TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>>> =
    maps
    |> List.fold
      (fun acc m -> m |> Map.fold (fun a k v -> Map.add k v a) acc)
      Map.empty

  let baseAttrs = mergeAttrs [ globalAttrs; eventHandlerAttrs ]

  let inputAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList
          [ "type", [ stringType ]
            "value", [ stringType ]
            "checked", [ boolType ]
            "placeholder", [ stringType ]
            "disabled", [ boolType ]
            "readOnly", [ boolType ]
            "required", [ boolType ]
            "autoFocus", [ boolType ]
            "autoComplete", [ stringType ]
            "name", [ stringType ]
            "min", [ stringType ]
            "max", [ stringType ]
            "step", [ stringType ]
            "pattern", [ stringType ]
            "maxLength", [ stringType ]
            "minLength", [ stringType ]
            "accept", [ stringType ]
            "multiple", [ boolType ] ] ]

  let anchorAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList
          [ "href", [ stringType ]
            "target", [ stringType ]
            "rel", [ stringType ]
            "download", [ stringType; boolType ] ] ]

  let imgAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList
          [ "src", [ stringType ]
            "alt", [ stringType ]
            "width", [ stringType ]
            "height", [ stringType ]
            "loading", [ stringType ] ] ]

  let formAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList
          [ "action", [ stringType ]
            "method", [ stringType ]
            "encType", [ stringType ]
            "noValidate", [ boolType ] ] ]

  let buttonAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList
          [ "type", [ stringType ]
            "disabled", [ boolType ]
            "name", [ stringType ]
            "value", [ stringType ] ] ]

  let selectAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList
          [ "value", [ stringType ]
            "disabled", [ boolType ]
            "name", [ stringType ]
            "required", [ boolType ]
            "multiple", [ boolType ] ] ]

  let optionAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList
          [ "value", [ stringType ]
            "disabled", [ boolType ]
            "selected", [ boolType ] ] ]

  let textareaAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList
          [ "value", [ stringType ]
            "placeholder", [ stringType ]
            "disabled", [ boolType ]
            "readOnly", [ boolType ]
            "required", [ boolType ]
            "rows", [ stringType ]
            "cols", [ stringType ]
            "name", [ stringType ]
            "maxLength", [ stringType ]
            "autoFocus", [ boolType ] ] ]

  let labelAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList [ "for", [ stringType ] ] ]

  let iframeAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList
          [ "src", [ stringType ]
            "width", [ stringType ]
            "height", [ stringType ]
            "sandbox", [ stringType ]
            "allow", [ stringType ]
            "name", [ stringType ] ] ]

  let videoAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList
          [ "src", [ stringType ]
            "controls", [ boolType ]
            "autoPlay", [ boolType ]
            "loop", [ boolType ]
            "muted", [ boolType ]
            "poster", [ stringType ]
            "width", [ stringType ]
            "height", [ stringType ] ] ]

  let sourceAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList
          [ "src", [ stringType ]
            "type", [ stringType ] ] ]

  let metaAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList
          [ "name", [ stringType ]
            "content", [ stringType ]
            "charset", [ stringType ]
            "httpEquiv", [ stringType ]
            "property", [ stringType ] ] ]

  let linkAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList
          [ "href", [ stringType ]
            "rel", [ stringType ]
            "type", [ stringType ]
            "media", [ stringType ] ] ]

  let scriptAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList
          [ "src", [ stringType ]
            "type", [ stringType ]
            "async", [ boolType ]
            "defer", [ boolType ] ] ]

  let int32Type = TypeValue.CreatePrimitive PrimitiveType.Int32

  let tableCellAttrs =
    mergeAttrs
      [ baseAttrs
        Map.ofList
          [ "colSpan", [ int32Type; stringType ]
            "rowSpan", [ int32Type; stringType ]
            "headers", [ stringType ]
            "scope", [ stringType ] ] ]

  let viewAttributeSchemas
    : ViewAttributeSchemas<ValueExt<'runtimeContext, 'db, 'customExtension>> =
    Map.ofList
      [ // Container & layout
        "div", baseAttrs
        "span", baseAttrs
        "p", baseAttrs
        "section", baseAttrs
        "article", baseAttrs
        "aside", baseAttrs
        "header", baseAttrs
        "footer", baseAttrs
        "main", baseAttrs
        "nav", baseAttrs
        // Headings
        "h1", baseAttrs
        "h2", baseAttrs
        "h3", baseAttrs
        "h4", baseAttrs
        "h5", baseAttrs
        "h6", baseAttrs
        // Text
        "strong", baseAttrs
        "em", baseAttrs
        "small", baseAttrs
        "b", baseAttrs
        "i", baseAttrs
        "u", baseAttrs
        "code", baseAttrs
        "pre", baseAttrs
        "blockquote", baseAttrs
        "abbr", baseAttrs
        "cite", baseAttrs
        "mark", baseAttrs
        "sub", baseAttrs
        "sup", baseAttrs
        // Lists
        "ul", baseAttrs
        "ol", baseAttrs
        "li", baseAttrs
        "dl", baseAttrs
        "dt", baseAttrs
        "dd", baseAttrs
        // Table
        "table", baseAttrs
        "thead", baseAttrs
        "tbody", baseAttrs
        "tfoot", baseAttrs
        "tr", baseAttrs
        "th", tableCellAttrs
        "td", tableCellAttrs
        "caption", baseAttrs
        "colgroup", baseAttrs
        "col", baseAttrs
        // Form
        "form", formAttrs
        "input", inputAttrs
        "textarea", textareaAttrs
        "select", selectAttrs
        "option", optionAttrs
        "button", buttonAttrs
        "label", labelAttrs
        "fieldset", baseAttrs
        "legend", baseAttrs
        // Media
        "img", imgAttrs
        "video", videoAttrs
        "audio", videoAttrs
        "source", sourceAttrs
        // Links
        "a", anchorAttrs
        // Embedded
        "iframe", iframeAttrs
        // Separators
        "hr", baseAttrs
        "br", baseAttrs
        // Head
        "meta", metaAttrs
        "link", linkAttrs
        "script", scriptAttrs
        // Other
        "details", baseAttrs
        "summary", baseAttrs
        "dialog", baseAttrs
        "canvas", baseAttrs
        "svg", baseAttrs
        "path", baseAttrs ]

  let context =
    { context with
        TypeCheckContext =
          { context.TypeCheckContext with
              ViewAttributeSchemas = viewAttributeSchemas } }

  let extensions =
    { List = listExtension
      Int32 = int32Extension
      Bool = boolExtension
      Int64 = int64Extension
      Float32 = float32Extension
      Float64 = float64Extension
      Decimal = decimalExtension
      DateOnly = dateOnlyExtension
      DateTime = dateTimeExtension
      String = stringExtension
      Guid = guidExtension
      TimeSpan = timeSpanExtension
      Map = mapExtension
      Updater = updaterExtension }

  let typeCheckingConfig =
    { QueryTypeSymbol = query_sym
      ListTypeSymbol = list_sym
      ViewTypeSymbol = view_sym
      ViewPropsTypeSymbol = view_props_sym
      ReactNodeTypeSymbol = react_node_sym
      ReactComponentTypeSymbol = react_component_sym
      CoTypeSymbol = co_sym
      MkQueryType = mk_query
      MkListType = mk_list_type
      MkViewType = mk_view_type
      MkViewPropsType = mk_view_props_type
      MkCoType = mk_co_type
      ImportedTypesWithFields =
        Map.ofList
          [ view_props_sym,
            fun args ->
              match args with
              | [ schema; ctx; st ] ->
                OrderedMap.ofList
                  [ TypeSymbol.Create(Identifier.LocalScope "schema"), (schema, Kind.Schema)
                    TypeSymbol.Create(Identifier.LocalScope "context"), (ctx, Kind.Star)
                    TypeSymbol.Create(Identifier.LocalScope "state"), (st, Kind.Star)
                    TypeSymbol.Create(Identifier.LocalScope "setState"),
                    (TypeValue.CreateArrow(
                       TypeValue.CreateArrow(st, st),
                       TypeValue.CreatePrimitive PrimitiveType.Unit
                     ),
                     Kind.Star) ]
              | _ -> OrderedMap.empty ] }

  extensions, context, typeCheckingConfig

let stdExtensions<'runtimeContext, 'db when 'db: comparison> =
  fun str_ops email_ops db_ops typeCheckingConfig ->
    makeExtensions<'runtimeContext, 'db, unit>
      str_ops
      email_ops
      db_ops
      (Some typeCheckingConfig)

let bootstrapStdExtensions<'runtimeContext, 'db when 'db: comparison> =
  fun str_ops email_ops db_ops ->
    makeExtensions<'runtimeContext, 'db, unit> str_ops email_ops db_ops None
