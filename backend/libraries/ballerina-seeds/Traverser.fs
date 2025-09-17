namespace Ballerina.Seeds

open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Eval
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.Errors
open Ballerina.Seeds.Fakes
open Ballerina.Reader.WithError
open Ballerina.State.WithError
open Ballerina.StdLib.Object

type SeedingClue =
  | Absent
  | FromContext of string

type SeedTarget =
  | FullStructure
  | PrimitivesOnly

type SeedingContext<'T> =
  { WantedCount: int option
    TypeContext: TypeExprEvalState
    Label: SeedingClue
    Options: SeedTarget
    InfinitiveVarNamesIndex: int
    InfinitiveNamesIndex: Map<string, int>
    Generator: BogusDataGenerator<Value<'T, Unit>> }

module Traverser =

  let rec seed
    : TypeValue
        -> State<Value<TypeValue, 'valueExtension>, SeedingContext<TypeValue>, SeedingContext<TypeValue>, Errors> =
    fun typeValue ->

      let (!) = seed

      let setLabel label =
        state.SetState(fun s -> { s with Label = FromContext label })

      let (!!) label (t: TypeValue) = setLabel label >>= fun () -> !t

      state {
        let! ctx = state.GetContext()

        match typeValue with
        | TypeValue.Imported _ -> return! state.Throw(Errors.Singleton "Imported seeds not implemented yet")
        | TypeValue.Arrow _
        | TypeValue.Apply _
        | TypeValue.Lambda _ -> return! state.Throw(Errors.Singleton "Arrow/Lambda seeds not implemented yet")

        | TypeValue.Var _ ->
          do!
            state.SetState(fun s ->
              { s with
                  InfinitiveVarNamesIndex = s.InfinitiveVarNamesIndex + 1 })

          return
            [ TypeSymbol.Create(Identifier.LocalScope "Guid"),
              ctx.Generator.Guid() |> PrimitiveValue.Guid |> Value.Primitive
              TypeSymbol.Create(Identifier.LocalScope "Name"),
              ctx.InfinitiveVarNamesIndex
              |> (VarName >> ctx.Generator.String >> PrimitiveValue.String >> Value.Primitive) ]
            |> Map.ofList
            |> Value.Record

        | TypeValue.Sum elements ->
          let! values = elements |> Seq.map (!) |> state.All
          return Value.Sum(0, values.Head)

        | TypeValue.Tuple elements ->
          let! values = elements |> Seq.map (!) |> state.All
          return Value.Tuple values

        | TypeValue.Map(key, value) ->
          let! k = !key
          let! v = !value

          return
            Value.Record(
              Map.ofList
                [ TypeSymbol.Create(Identifier.LocalScope "Key"), k
                  TypeSymbol.Create(Identifier.LocalScope "Value"), v ]
            )

        | TypeValue.Union cases ->
          let sampled = cases |> Map.toList |> List.randomSample 1

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
          let! ctx = state.GetContext()
          let! tv, _ = TypeExprEvalState.tryFindType id |> Reader.Run ctx.TypeContext |> state.OfSum
          return! (!!id.ToFSharpString) tv

        | TypeValue.Record fields ->
          let! fields =
            fields
            |> Map.toList
            |> List.map (fun (ts, tv) ->
              state {
                let! v = !! ts.Name.LocalName tv
                return ts, v
              })
            |> state.All

          return Value.Record(Map.ofList fields)

        | TypeValue.Primitive p ->
          match ctx.Label with
          | Absent ->
            let value = FakeValue Unsupervised
            return ctx.Generator.PrimitiveValueCons p value

          | FromContext label ->
            do!
              state.SetState(fun s ->
                { s with
                    InfinitiveNamesIndex =
                      s.InfinitiveNamesIndex
                      |> Map.change label (function
                        | Some i -> Some(i + 1)
                        | None -> Some 0) })

            let! ctx = state.GetState()
            let value = FakeValue(Supervised(label, ctx.InfinitiveNamesIndex[label]))
            return ctx.Generator.PrimitiveValueCons p value

        | TypeValue.Set element ->
          let! element = !element
          return Value.Tuple [ element ]
      }

type SeedingContext<'T> with
  static member Default() =
    { WantedCount = None
      TypeContext = TypeExprEvalState.Empty
      Generator = Runner.en ()
      Label = Absent
      InfinitiveVarNamesIndex = 0
      InfinitiveNamesIndex = Map.empty
      Options = FullStructure }
