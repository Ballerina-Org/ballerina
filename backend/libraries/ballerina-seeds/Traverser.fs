namespace Ballerina.Seeds

open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Eval
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.LocalizedErrors
open Ballerina.Seeds.Fakes
open Ballerina.Reader.WithError

open Ballerina.State.WithError
open Ballerina.StdLib.Object
open Ballerina.DSL.Next.StdLib
open Ballerina.StdLib.OrderPreservingMap

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
  { TypeContext: TypeExprEvalState
    Label: SeedingClue
    InfinitiveVarNamesIndex: int
    InfinitiveNamesIndex: Map<string, int> }

module Traverser =

  let rec seed: TypeValue -> State<Value<TypeValue, ValueExt>, SeedingContext, SeedingState, Errors> =
    fun typeValue ->

      let (!) = seed

      let setLabel label =
        state.SetState(fun s -> { s with Label = FromContext label })

      let (!!) label (t: TypeValue) = setLabel label >>= fun () -> !t

      state {

        match typeValue with
        | TypeValue.Imported x when x.Id = LocalScope "List" && List.length x.Arguments = 1 ->
          let! values = [ 0..2 ] |> List.map (fun _ -> (!) x.Arguments.Head) |> state.All
          let listExtValue = ListValues >> Choice1Of3 >> ValueExt.ValueExt
          let lv = List.Model.ListValues.List values |> listExtValue
          return Value.Ext lv

        | TypeValue.Imported _ ->
          return! state.Throw(Errors.Singleton(Location.Unknown, "Imported seeds not implemented yet"))
        | TypeValue.Arrow _ ->
          return! state.Throw(Errors.Singleton(Location.Unknown, "Arrow seeds not implemented yet"))
        | TypeValue.Lambda _ ->
          return! state.Throw(Errors.Singleton(Location.Unknown, "Lambda seeds not implemented yet"))
        | TypeValue.Apply _ ->
          return! state.Throw(Errors.Singleton(Location.Unknown, "Apply seeds not implemented yet"))
        | TypeValue.Var _ ->
          do!
            state.SetState(fun s ->
              { s with
                  InfinitiveVarNamesIndex = s.InfinitiveVarNamesIndex + 1 })

          let! ctx = state.GetContext()
          let! s = state.GetState()

          return
            [ TypeSymbol.Create(Identifier.LocalScope "Guid"),
              ctx.Generator.Guid() |> PrimitiveValue.Guid |> Value.Primitive
              TypeSymbol.Create(Identifier.LocalScope "Name"),
              s.InfinitiveVarNamesIndex
              |> (VarName >> ctx.Generator.String >> PrimitiveValue.String >> Value.Primitive) ]
            |> Map.ofList
            |> Value.Record

        | TypeValue.Sum { value = elements } ->
          let! values = elements |> Seq.map (!) |> state.All
          return Value.Sum(0, values.Head)

        | TypeValue.Tuple { value = elements } ->
          let! values = elements |> Seq.map (!) |> state.All
          return Value.Tuple values

        | TypeValue.Map({ value = key, value }) ->
          let! k = !key
          let! v = !value

          return
            Value.Record(
              Map.ofList
                [ TypeSymbol.Create(Identifier.LocalScope "Key"), k
                  TypeSymbol.Create(Identifier.LocalScope "Value"), v ]
            )

        | TypeValue.Union cases ->
          let! ctx = state.GetContext()

          let sampled =
            match ctx.PickItemStrategy with
            | RandomItem -> cases.value |> OrderedMap.toList |> List.randomSample 1
            | First -> cases.value |> OrderedMap.toList |> List.head |> List.singleton
            | Last -> cases.value |> OrderedMap.toList |> List.last |> List.singleton

          let! cases =
            sampled
            |> List.map (fun (ts, tv) ->
              state {
                let! v = !! ts.Name.LocalName tv
                return ts, v
              })
            |> state.All

          return cases |> List.map Value.UnionCase |> Value.Tuple

        | TypeValue.Lookup id ->
          let! ctx = state.GetState()

          let! tv, _ =
            TypeExprEvalState.tryFindType (id, Location.Unknown)
            |> Reader.Run ctx.TypeContext
            |> state.OfSum

          return! (!!id.ToFSharpString) tv

        | TypeValue.Record fields ->
          let! fields =
            fields.value
            |> OrderedMap.toList
            |> List.map (fun (ts, tv) ->
              state {
                let! v = !! ts.Name.LocalName tv
                return ts, v
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
  static member Default(typeContext: TypeExprEvalState) =
    { TypeContext = typeContext
      Label = Absent
      InfinitiveVarNamesIndex = 0
      InfinitiveNamesIndex = Map.empty }
