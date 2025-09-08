namespace Ballerina.Seeds

open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Eval
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.Errors
open Ballerina.Seeds.Fakes
open Ballerina.Reader.WithError
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
    Generator: BogusDataGenerator<Value<'T, Unit>> }

module Traverser =

  let rec seed: TypeValue -> Reader<Value<TypeValue, Unit>, SeedingContext<TypeValue>, Errors> =
    fun typeValue ->

      let (!) = seed

      let (!!) (label: string) (t: TypeValue) =
        seed t |> reader.MapContext(fun ctx -> { ctx with Label = FromContext label })

      reader {
        let! ctx = reader.GetContext()

        match typeValue with
        | TypeValue.Arrow _
        | TypeValue.Apply _
        | TypeValue.Lambda _ -> return! reader.Throw(Errors.Singleton "Arrow/Lambda seeds not implemented yet")

        | TypeValue.Var _ ->
          let! ctx =
            reader.GetContext()
            |> reader.MapContext(fun ctx ->
              { ctx with
                  InfinitiveVarNamesIndex = ctx.InfinitiveVarNamesIndex + 1 })

          return
            [ TypeSymbol.Create(Identifier.LocalScope "Guid"),
              ctx.Generator.Guid() |> PrimitiveValue.Guid |> Value.Primitive
              TypeSymbol.Create(Identifier.LocalScope "Name"),
              ctx.InfinitiveVarNamesIndex
              |> (VarName >> ctx.Generator.String >> PrimitiveValue.String >> Value.Primitive) ]
            |> Map.ofList
            |> Value.Record

        | TypeValue.Sum elements ->
          let! values = elements |> Seq.map (!) |> reader.All
          return Value.Sum(0, values.Head)

        | TypeValue.Tuple elements ->
          let! values = elements |> Seq.map (!) |> reader.All
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
          let! cases =
            cases
            |> Map.toList
            |> List.randomSample 1
            |> List.map (fun (ts, tv) ->
              reader {
                let! value = !! ts.Name.LocalName tv
                return ts, value
              })
            |> reader.All

          return cases |> List.map Value.UnionCase |> Value.Tuple

        | TypeValue.Lookup id ->
          let! tv, _ = TypeExprEvalState.tryFindType id |> reader.MapContext _.TypeContext
          return! (!!id.ToFSharpString) tv

        | TypeValue.Record fields ->
          let! fieldValues =
            fields
            |> Map.toList
            |> List.map (fun (ts, tv) ->
              reader {
                let! tv = !! ts.Name.LocalName tv
                return ts, tv
              })
            |> reader.All

          return Value.Record(Map.ofList fieldValues)

        | TypeValue.Primitive p ->
          let value =
            match ctx.Label with
            | Absent -> FakeValue Unsupervised
            | FromContext label -> FakeValue(Supervised(label, 0))

          return ctx.Generator.PrimitiveValueCons p value

        | TypeValue.List element
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
      Options = FullStructure }
