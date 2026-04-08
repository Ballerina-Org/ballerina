namespace Ballerina.DSL.Next.Serialization

module PocoObjects =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.LocalizedErrors
  open System
  open System.Collections.Generic
  open System.Text.Json.Serialization
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.TypeChecker
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.State.WithError
  open Ballerina

  type ResolvedIdentifierDTO = string
  type SumCaseDTO = string

  type SumConsSelector with
    member this.ToDTO = $"{this.Case}Of{this.Count}"

    static member TryParse(sumCaseDTO: string) : Sum<SumConsSelector, Errors<unit>> =
      sum {
        let tryParseInt (intString: string) (error: Errors<unit>) =
          sum {
            let intRef = ref Int32.MinValue

            if Int32.TryParse(intString, intRef) then
              return intRef.Value
            else
              return! sum.Throw error
          }

        let split = sumCaseDTO.Split "Of"

        if split.Length <> 2 then
          return! sum.Throw(Errors.Singleton () (fun _ -> "Failed to parse sum selector."))
        else
          let! case = tryParseInt split.[0] (Errors.Singleton () (fun _ -> "Failed to parse case of sum selector."))
          let! count = tryParseInt split.[1] (Errors.Singleton () (fun _ -> "Failed to parse count of sum selector."))
          return { Case = case; Count = count }
      }

  type ResolvedIdentifier with
    member this.ToDTO: ResolvedIdentifierDTO =
      let assemblyName =
        if this.Assembly |> String.IsNullOrEmpty |> not then
          $"{this.Assembly}::"
        else
          ""

      let moduleName =
        if this.Module |> String.IsNullOrEmpty |> not then
          $"{this.Module}::"
        else
          ""

      let typeName =
        match this.Type with
        | None -> ""
        | Some t -> $"{t}::"

      $"{assemblyName}{moduleName}{typeName}{this.Name}"

    static member TryParse(resolvedIdentifierDTO: string) : Sum<ResolvedIdentifier, Errors<unit>> =
      sum {

        let split = resolvedIdentifierDTO.Split("::") |> Array.rev

        match split.Length with
        | 1 ->
          return
            { Name = split.[0]
              Type = None
              Module = ""
              Assembly = "" }
        | 2 ->
          return
            { Name = split.[0]
              Type = Some split.[1]
              Module = ""
              Assembly = "" }
        | 3 ->
          return
            { Name = split.[0]
              Type = Some split.[1]
              Module = split.[2]
              Assembly = "" }
        | 4 ->
          return
            { Name = split.[0]
              Type = Some split.[1]
              Module = split.[2]
              Assembly = split.[3] }
        | _ -> return! sum.Throw(Errors.Singleton () (fun _ -> $"Failed to parse resolved identifier DTO."))
      }

  [<RequireQualifiedAccess>]
  module PrimitiveValueDtoJsonPropertyNames =
    [<Literal>]
    let Int32 = "Int32"

    [<Literal>]
    let Int64 = "Int64"

    [<Literal>]
    let Float32 = "Float32"

    [<Literal>]
    let Float64 = "Float64"

    [<Literal>]
    let Decimal = "Decimal"

    [<Literal>]
    let Bool = "Bool"

    [<Literal>]
    let Guid = "Guid"

    [<Literal>]
    let String = "String"

    [<Literal>]
    let Date = "Date"

    [<Literal>]
    let DateTime = "DateTime"

    [<Literal>]
    let TimeSpan = "TimeSpan"

  type PrimitiveValueDTO() =
    [<JsonPropertyName(PrimitiveValueDtoJsonPropertyNames.Int32)>]
    member val Int32: Nullable<int> = Nullable() with get, set

    [<JsonPropertyName(PrimitiveValueDtoJsonPropertyNames.Int64)>]
    member val Int64: Nullable<int64> = Nullable() with get, set

    [<JsonPropertyName(PrimitiveValueDtoJsonPropertyNames.Float32)>]
    member val Float32: Nullable<float32> = Nullable() with get, set

    [<JsonPropertyName(PrimitiveValueDtoJsonPropertyNames.Float64)>]
    member val Float64: Nullable<float> = Nullable() with get, set

    [<JsonPropertyName(PrimitiveValueDtoJsonPropertyNames.Decimal)>]
    member val Decimal: Nullable<decimal> = Nullable() with get, set

    [<JsonPropertyName(PrimitiveValueDtoJsonPropertyNames.Bool)>]
    member val Bool: Nullable<bool> = Nullable() with get, set

    [<JsonPropertyName(PrimitiveValueDtoJsonPropertyNames.Guid)>]
    member val Guid: Nullable<Guid> = Nullable() with get, set

    [<JsonPropertyName(PrimitiveValueDtoJsonPropertyNames.String)>]
    member val String: string = null with get, set

    [<JsonPropertyName(PrimitiveValueDtoJsonPropertyNames.Date)>]
    member val Date: Nullable<DateOnly> = Nullable() with get, set

    [<JsonPropertyName(PrimitiveValueDtoJsonPropertyNames.DateTime)>]
    member val DateTime: Nullable<DateTime> = Nullable() with get, set

    [<JsonPropertyName(PrimitiveValueDtoJsonPropertyNames.TimeSpan)>]
    member val TimeSpan: Nullable<TimeSpan> = Nullable() with get, set

    new(int32: int) as this =
      PrimitiveValueDTO()

      then this.Int32 <- Nullable int32

    new(int64: int64) as this =
      PrimitiveValueDTO()

      then this.Int64 <- Nullable int64

    new(float32: float32) as this =
      PrimitiveValueDTO()

      then this.Float32 <- Nullable float32

    new(float64: float) as this =
      PrimitiveValueDTO()

      then this.Float64 <- Nullable float64

    new(decimal: decimal) as this =
      PrimitiveValueDTO()

      then this.Decimal <- Nullable decimal

    new(bool: bool) as this =
      PrimitiveValueDTO()

      then this.Bool <- Nullable bool

    new(guid: Guid) as this =
      PrimitiveValueDTO()

      then this.Guid <- Nullable guid

    new(string: string) as this =
      PrimitiveValueDTO()

      then this.String <- string

    new(date: DateOnly) as this =
      PrimitiveValueDTO()

      then this.Date <- Nullable date

    new(dateTime: DateTime) as this =
      PrimitiveValueDTO()

      then this.DateTime <- Nullable dateTime

    new(span: TimeSpan) as this =
      PrimitiveValueDTO()

      then this.TimeSpan <- Nullable span

  and ExtDTO<'valueExt when 'valueExt: not null and 'valueExt: not struct> [<JsonConstructor>] (value: 'valueExt) =
    member val Value: 'valueExt = value with get, set
    member val ApplicableId: ResolvedIdentifierDTO = null with get, set

    new(applicableId: ResolvedIdentifierDTO, value: 'valueExt) as this =
      ExtDTO<'valueExt>(value)
      then this.ApplicableId <- applicableId

  and ValueDTO<'valueExt when 'valueExt: not null and 'valueExt: not struct>() =
    member val Record: Dictionary<ResolvedIdentifierDTO, ValueDTO<'valueExt>> = null with get, set
    member val UnionCase: Dictionary<ResolvedIdentifierDTO, ValueDTO<'valueExt>> = null with get, set
    member val RecordDes: ResolvedIdentifierDTO | null = null with get, set
    member val UnionCons: ResolvedIdentifierDTO | null = null with get, set
    member val Tuple: ValueDTO<'valueExt>[] | null = null with get, set
    member val Sum: Dictionary<SumCaseDTO, ValueDTO<'valueExt>> = null with get, set
    member val Primitive: PrimitiveValueDTO | null = null with get, set
    member val Var: Var | null = null with get, set
    member val Ext: 'valueExt | null = null with get, set
    member val ExtWithId: Dictionary<ResolvedIdentifierDTO, 'valueExt> = null with get, set

    member this.GetRecordFieldPositions
      (typeValue: TypeValue<'ext>)
      : State<Map<TypeSymbol, int>, TypeCheckContext<'ext>, TypeCheckState<'ext>, Errors<Location>> =
      state {
        let! record =
          TypeValue.AsRecord typeValue
          |> state.OfSum
          |> state.MapError(Errors.MapContext(replaceWith Location.Unknown))

        if isNull this.Record |> not then
          return!
            record.data
            |> Map.toList
            |> List.map (fun (typeSymbol, _) ->
              state {
                let! typeSymbolResolvedIdentifierDTO =
                  TypeCheckState.TryResolveIdentifier(typeSymbol, Location.Unknown)
                  |> state.Map(fun identifier -> identifier.ToDTO)


                let! index =
                  this.Record.Keys
                  |> Seq.tryFindIndex (fun recordField -> recordField = typeSymbolResolvedIdentifierDTO)
                  |> sum.OfOption(
                    Errors.Singleton Location.Unknown (fun _ ->
                      $"The field {typeSymbol.Name} from the type value was not found in the dto.")
                  )
                  |> state.OfSum

                return typeSymbol, index
              })
            |> state.All
            |> state.Map Map.ofList
        else
          return! state.Throw(Errors.Singleton Location.Unknown (fun _ -> "The given value dto is not a record."))
      }

    new(applicableId: Option<ResolvedIdentifierDTO>, ext: 'valueExt) as this =
      ValueDTO<'valueExt>()

      then
        match applicableId with
        | None -> this.Ext <- ext
        | Some id ->
          let extDictionary = new Dictionary<ResolvedIdentifierDTO, 'valueExt>()
          extDictionary.Add(id, ext)
          this.ExtWithId <- extDictionary

    new(record: System.Collections.Generic.Dictionary<ResolvedIdentifierDTO, ValueDTO<'valueExt>>) as this =
      ValueDTO<'valueExt>()

      then this.Record <- record

    new(case: Dictionary<ResolvedIdentifierDTO, ValueDTO<'valueExt>>, isSum: bool) as this =
      ValueDTO<'valueExt>()

      then if isSum then this.Sum <- case else this.UnionCase <- case

    new(des: ResolvedIdentifierDTO) as this =
      ValueDTO<'valueExt>()

      then this.RecordDes <- des

    new(tuple: ValueDTO<'valueExt> array) as this =
      ValueDTO<'valueExt>()

      then this.Tuple <- tuple

    new(primitive: PrimitiveValueDTO) as this =
      ValueDTO<'valueExt>()

      then this.Primitive <- primitive

    new(var: Var) as this =
      ValueDTO<'valueExt>()

      then this.Var <- var
