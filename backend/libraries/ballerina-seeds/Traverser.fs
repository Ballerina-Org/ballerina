namespace Ballerina.Seeds

open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.TypeChecker.Eval
open Ballerina.DSL.Next.Types.TypeChecker.Model
open Ballerina.DSL.Next.Types.TypeChecker.Patterns
open Ballerina.DSL.Next.Types.TypeChecker
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.Data.Schema.Model
open Ballerina.Data.Schema.ActivePatterns
open Ballerina.LocalizedErrors
open Ballerina.Seeds.Fakes
open Ballerina.Reader.WithError

open Ballerina.State.WithError
open Ballerina.StdLib.Object
open Ballerina.DSL.Next.StdLib
open Ballerina.StdLib.OrderPreservingMap
open Ballerina.Cat.Collections.OrderedMap

type SeedingClue =
  | Absent
  | FromContext of string

type SeedTarget =
  | FullStructure
  | PrimitivesOnly

type PickItemStrategy =
  | First
  | Last
  | RandomItem

type SeedingContext =
  { WantedCount: int option
    Options: SeedTarget
    PickItemStrategy: PickItemStrategy
    Generator: BogusDataGenerator }

type SeedingState =
  { TypeContext: TypeCheckState
    Label: SeedingClue
    InfinitiveVarNamesIndex: int
    InfinitiveNamesIndex: Map<string, int> }

module Traverser =

  let isSupported =
    function
    | TypeValue.Imported x when x.Id.Name <> "List" || List.length x.Arguments <> 1 -> false
    | TypeValue.Arrow _
    | TypeValue.Lambda _
    | TypeValue.Application _ -> false
    | _ -> true


  let rec seed
    (entity: EntityName)
    : TypeValue -> State<Value<TypeValue, ValueExt>, SeedingContext, SeedingState, Errors> =
    fun typeValue ->

      let (!) = seed entity

      let setLabel label =
        state.SetState(fun s -> { s with Label = FromContext label })

      let (!!) label (t: TypeValue) = setLabel label >>= fun () -> !t

      state {

        match typeValue with
        | TypeValue.Imported x when x.Id.Name = "List" && List.length x.Arguments = 1 ->
          let! values = [ 0..2 ] |> List.map (fun _ -> (!) x.Arguments.Head) |> state.All
          let listExtValue = ListValues >> Choice1Of4 >> ValueExt.ValueExt
          let lv = List.Model.ListValues.List values |> listExtValue
          return Value.Ext lv
        | TypeValue.Imported x when x.Id.Name = "Option" && List.length x.Arguments = 1 ->
          let! value = (!) x.Arguments.Head
          let ext = OptionValues >> Choice2Of4 >> ValueExt.ValueExt
          let valueExt = Option.Model.OptionValues.Option(Some value) |> ext
          return Value.Ext valueExt
        | TypeValue.Imported _ ->
          return! state.Throw(Errors.Singleton(Location.Unknown, "Imported seeds not implemented yet"))
        | TypeValue.Arrow _ ->
          return! state.Throw(Errors.Singleton(Location.Unknown, "Arrow seeds not implemented yet"))
        | TypeValue.Lambda _ ->
          return! state.Throw(Errors.Singleton(Location.Unknown, "Lambda seeds not implemented yet"))
        | TypeValue.Application _ ->
          return! state.Throw(Errors.Singleton(Location.Unknown, "Apply seeds not implemented yet"))
        | TypeValue.Var _ ->
          do!
            state.SetState(fun s ->
              { s with
                  InfinitiveVarNamesIndex = s.InfinitiveVarNamesIndex + 1 })

          let! ctx = state.GetContext()
          let! s = state.GetState()

          return
            [ "Guid" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve,
              ctx.Generator.Guid() |> PrimitiveValue.Guid |> Value.Primitive
              "Name" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve,
              s.InfinitiveVarNamesIndex
              |> (VarName >> ctx.Generator.String >> PrimitiveValue.String >> Value.Primitive) ]
            |> Map.ofList
            |> Value.Record

        | TypeValue.Sum { value = elements } ->
          match elements with
          | [] ->
            return! state.Throw(Errors.Singleton(Location.Unknown, "TypeValue.Sum with no elements can't be seeded"))
          | elements ->
            let! values = elements |> Seq.map (!) |> state.All

            return
              Value.Sum(
                { Case = elements.Length - 1
                  Count = elements.Length },
                values |> List.head
              )
        | TypeValue.Tuple { value = elements } ->
          let! values = elements |> Seq.map (!) |> state.All
          return Value.Tuple values

        | TypeValue.Map({ value = key, value }) ->
          let! key = (!) key
          let! value = (!) value

          return
            Value.Record(
              Map.ofList
                [ "Key" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve, key
                  "Value" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve, value ]
            )
            |> List.singleton
            |> Value.Tuple

        | TypeValue.Union cases ->
          let! ctx = state.GetContext()

          let ts, tv =
            match ctx.PickItemStrategy with
            | RandomItem -> cases.value |> OrderedMap.toList |> List.randomSample 1
            | First -> cases.value |> OrderedMap.toList |> List.head |> List.singleton
            | Last -> cases.value |> OrderedMap.toList |> List.last |> List.singleton
            |> List.head

          let! v = !! ts.Name.LocalName tv
          let case = ts.Name |> TypeCheckScope.Empty.Resolve, v
          return Value.UnionCase case

        | TypeValue.Lookup id ->
          let! ctx = state.GetState()

          let! tv, _ =
            TypeCheckState.tryFindType (id |> TypeCheckScope.Empty.Resolve, Location.Unknown)
            |> Reader.Run ctx.TypeContext
            |> state.OfSum

          return! (!!id.AsFSharpString) tv
        | TypeValue.Record(CollectionReference fields) ->
          let! fields =
            fields
            |> OrderedMap.toList
            |> List.map (fun (ts, (tv, _kind)) ->
              state {
                let! v = !! entity.EntityName tv
                return ts.Name |> TypeCheckScope.Empty.Resolve, v
              })
            |> state.All

          return Value.Record(Map.ofList fields)
        | TypeValue.Record fields ->
          let! fields =
            fields.value
            |> OrderedMap.toList
            |> List.map (fun (ts, (tv, _kind)) ->
              state {
                let! v = !! ts.Name.LocalName tv
                return ts.Name |> TypeCheckScope.Empty.Resolve, v
              })
            |> state.All

          return Value.Record(Map.ofList fields)

        | TypeValue.Primitive p ->
          let! ctx = state.GetContext()
          let! s = state.GetState()

          match s.Label with
          | Absent ->
            let value = FakeValue Unsupervised
            return ctx.Generator.PrimitiveValueCons p.value value

          | FromContext label ->

            do!
              state.SetState(fun current ->
                { current with
                    InfinitiveNamesIndex =
                      s.InfinitiveNamesIndex
                      |> Map.change label (function
                        | Some i -> Some(i + 1)
                        | None -> Some 0) })

            let! s = state.GetState()
            let value = FakeValue(Supervised(label, s.InfinitiveNamesIndex[label]))
            return ctx.Generator.PrimitiveValueCons p.value value

        | TypeValue.Set element ->
          let! element = !element.value
          return Value.Tuple [ element ]
      }

type SeedingContext with
  static member Default() =
    { WantedCount = None
      PickItemStrategy = RandomItem
      Generator = Runner.en ()
      Options = FullStructure }

type SeedingState with
  static member Default(typeContext: TypeCheckState) =
    { TypeContext = typeContext
      Label = Absent
      InfinitiveVarNamesIndex = 0
      InfinitiveNamesIndex = Map.empty }
