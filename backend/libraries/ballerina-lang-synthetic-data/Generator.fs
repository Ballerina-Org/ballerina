namespace Ballerina.DSL.Next.SyntheticData

module Generator =
  open System
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.DSL.Next.Types.TypeChecker.Model

  type ImportedGenerator<'valueExt, 'importedCfg> =
    { Id: ResolvedIdentifier
      Generate:
        GeneratorConfig<'importedCfg>
          -> (TypeValue<'valueExt>
            -> seq<Sum<Value<TypeValue<'valueExt>, 'valueExt>, Errors<Unit>>>)
          -> ImportedTypeValue<'valueExt>
          -> seq<Sum<Value<TypeValue<'valueExt>, 'valueExt>, Errors<Unit>>> }

  and GeneratorConfig<'importedCfg> =
    { Random: Random
      ImportedConfig: 'importedCfg option }

    static member Empty =
      { Random = Random()
        ImportedConfig = Option<'importedCfg>.None }

    static member Updaters =
      {| Random =
          fun u (c: GeneratorConfig<'importedCfg>) ->
            { c with
                GeneratorConfig.Random = u c.Random }
         ImportedConfig =
          fun u (c: GeneratorConfig<'importedCfg>) ->
            { c with
                GeneratorConfig.ImportedConfig = u c.ImportedConfig } |}

  let private fail msg =
    Errors.Singleton () (fun () -> msg) |> sum.Throw

  let private randomString (rng: Random) (minLength: int) (maxLength: int) =
    let length = rng.Next(minLength, maxLength + 1)

    // ASCII characters (only numbers and letters)
    Array.init length (fun _ -> char (rng.Next(48, 123))) |> System.String

  let private generatePrimitive (rng: Random) (p: PrimitiveType) =
    match p with
    | PrimitiveType.Unit -> PrimitiveValue.Unit
    | PrimitiveType.Guid -> PrimitiveValue.Guid(Guid.CreateVersion7())
    | PrimitiveType.Int32 -> PrimitiveValue.Int32(rng.Next(-1000, 1001))
    | PrimitiveType.Int64 ->
      PrimitiveValue.Int64(rng.NextInt64(-100000L, 100001L))
    | PrimitiveType.Float32 ->
      PrimitiveValue.Float32(float32 (rng.NextDouble() * 1000.0))
    | PrimitiveType.Float64 -> PrimitiveValue.Float64(rng.NextDouble() * 1000.0)
    | PrimitiveType.Decimal ->
      PrimitiveValue.Decimal(decimal (rng.NextDouble() * 1000.0))
    | PrimitiveType.Bool -> PrimitiveValue.Bool(rng.Next(0, 2) = 0)
    | PrimitiveType.String -> PrimitiveValue.String(randomString rng 4 12)
    | PrimitiveType.DateTime ->
      let baseDate = System.DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)

      PrimitiveValue.DateTime(
        baseDate
          .AddDays(float (rng.Next(0, 365 * 10)))
          .AddSeconds(float (rng.Next(0, 60 * 60 * 24)))
      )
    | PrimitiveType.DateOnly ->
      let baseDate = System.DateOnly(2020, 1, 1)
      PrimitiveValue.Date(baseDate.AddDays(rng.Next(0, 365 * 10)))
    | PrimitiveType.TimeSpan ->
      PrimitiveValue.TimeSpan(
        TimeSpan.FromSeconds(float (rng.Next(0, 60 * 60 * 24)))
      )
    | PrimitiveType.Vector -> failwith "Not Implemented"

  /// Combine all elements from all sequences into a single sequence of lists
  let private cartesianProduct<'a>
    (seqs: list<seq<Sum<'a, Errors<Unit>>>>)
    : seq<list<'a>> =
    let rec go (seqs: list<seq<Sum<'a, Errors<Unit>>>>) : seq<list<'a>> =
      match seqs with
      | [] -> seq { yield [] }
      | first :: rest ->
        seq {
          for s in first do
            match s with
            | Sum.Right _ -> ignore ()
            | Sum.Left x ->
              for restResult in go rest do
                yield x :: restResult
        }

    go seqs

  let rec private localGenerate<'valueExt, 'importedCfg
    when 'valueExt: comparison>
    (config: GeneratorConfig<'importedCfg>)
    (typeContext: TypeCheckContext<'valueExt>)
    (typeState: TypeCheckState<'valueExt>)
    (importedGenerators:
      Map<ResolvedIdentifier, ImportedGenerator<'valueExt, 'importedCfg>>)
    (t: TypeValue<'valueExt>)
    : seq<Sum<Value<TypeValue<'valueExt>, 'valueExt>, Errors<Unit>>> =
    match t with
    | TypeValue.Primitive p ->
      generatePrimitive config.Random p.value
      |> Value.Primitive
      |> sum.Return
      |> Seq.singleton
    | TypeValue.Var v ->
      match Map.tryFind v.Name typeContext.TypeVariables with
      | Some(bound, _kind) ->
        localGenerate config typeContext typeState importedGenerators bound
      | None -> fail $"Missing type variable binding for {v}" |> Seq.singleton
    | TypeValue.Lookup id ->
      let resolved = typeContext.Scope.Resolve id

      let binding =
        typeState.Bindings
        |> Map.tryFind resolved
        |> Option.orElseWith (fun () -> Map.tryFind resolved typeContext.Values)

      match binding with
      | Some(bound, _kind) ->
        localGenerate config typeContext typeState importedGenerators bound
      | None -> fail $"Missing lookup binding for {resolved}" |> Seq.singleton
    | TypeValue.Record fields ->
      let scope = fields.typeCheckScopeSource

      let fieldSeqs =
        fields.value
        |> OrderedMap.toList
        |> List.map (fun (symbol, (fieldType, _kind)) ->
          localGenerate
            config
            typeContext
            typeState
            importedGenerators
            fieldType
          |> Seq.map (fun s ->
            sum {
              let! value = s
              let fieldId = scope.Resolve symbol.Name
              return fieldId, value
            }))

      cartesianProduct fieldSeqs
      |> Seq.map (Map.ofList >> Value.Record >> sum.Return)
    | TypeValue.Tuple items ->
      let itemSeqs =
        items.value
        |> List.map (
          localGenerate config typeContext typeState importedGenerators
        )

      cartesianProduct itemSeqs |> Seq.map (Value.Tuple >> sum.Return)
    | TypeValue.Union cases ->
      let caseList = cases.value |> OrderedMap.toList

      match caseList with
      | [] -> fail $"Union type {t} has no cases" |> Seq.singleton
      | _ ->
        seq {
          for symbol, caseType in caseList do
            let caseId = cases.typeCheckScopeSource.Resolve symbol.Name

            for s in
              localGenerate
                config
                typeContext
                typeState
                importedGenerators
                caseType do
              yield
                sum {
                  let! value = s
                  return Value.UnionCase(caseId, value)
                }
        }
    | TypeValue.Sum variants ->
      match variants.value with
      | [] -> fail $"Sum type {t} has no cases" |> Seq.singleton
      | cases ->
        seq {
          for index, caseType in List.indexed cases do
            let selector =
              { SumConsSelector.Case = index + 1
                Count = cases.Length }

            for s in
              localGenerate
                config
                typeContext
                typeState
                importedGenerators
                caseType do
              yield
                sum {
                  let! value = s
                  return Value.Sum(selector, value)
                }
        }
    | TypeValue.Imported imported ->
      match Map.tryFind imported.Id importedGenerators with
      | None ->
        fail $"Missing imported generator for {imported.Id}" |> Seq.singleton
      | Some generator ->
        generator.Generate
          config
          (localGenerate config typeContext typeState importedGenerators)
          imported
    | _ ->
      fail $"Synthetic generator does not support type {t}" |> Seq.singleton

  let Generate<'valueExt, 'importedCfg when 'valueExt: comparison>
    (config: GeneratorConfig<'importedCfg>)
    (context: TypeCheckContext<'valueExt> * TypeCheckState<'valueExt>)
    (importedGenerators:
      Map<ResolvedIdentifier, ImportedGenerator<'valueExt, 'importedCfg>>)
    (t: TypeValue<'valueExt>)
    : seq<Sum<Value<TypeValue<'valueExt>, 'valueExt>, Errors<Unit>>> =
    let typeContext, typeState = context
    localGenerate config typeContext typeState importedGenerators t
