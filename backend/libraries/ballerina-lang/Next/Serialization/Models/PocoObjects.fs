namespace Ballerina.DSL.Next.Serialization

module PocoObjects =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.LocalizedErrors
  open System
  open System.Collections.Generic

  type TypeParameterDiscriminator =
    | Symbol = 1
    | Star = 2
    | Schema = 3
    | Arrow = 4

  type ValueDiscriminator =
    | TypeLambda = 1
    | Lambda = 2
    | Record = 3
    | UnionCase = 4
    | RecordDes = 5
    | UnionCons = 6
    | Tuple = 7
    | Sum = 8
    | Primitive = 9
    | Var = 10
    | Ext = 11

  type TypeParameterArrowDTO =
    { Arg: TypeParameterDTO
      Body: TypeParameterDTO }

  and TypeParameterPlainDTO =
    { Kind: TypeParameterDiscriminator
      Name: string }

  and TypeParameterDTO =
    { Kind: TypeParameterDiscriminator
      Arrow: TypeParameterArrowDTO | null }

    static member CreateSymbol() =
      { Kind = TypeParameterDiscriminator.Symbol
        Arrow = null }

    static member CreateStar() =
      { Kind = TypeParameterDiscriminator.Star
        Arrow = null }

    static member CreateSchema() =
      { Kind = TypeParameterDiscriminator.Schema
        Arrow = null }

    static member CreateArrow(arg, body) =
      { Kind = TypeParameterDiscriminator.Arrow
        Arrow = { Arg = arg; Body = body } }

  type ResolvedIdentifierDTO =
    { Assembly: string
      Module: string
      Type: string
      Name: string }

  type TypeCheckScopeDTO =
    { Assembly: string
      Module: string
      Type: string }

  type PrimitiveValueDiscriminator =
    | Int32 = 1
    | Int64 = 2
    | Float32 = 3
    | Float64 = 4
    | Decimal = 5
    | Bool = 6
    | Guid = 7
    | String = 8
    | Date = 9
    | DateTime = 10
    | TimeSpan = 11
    | Unit = 12

  type ExprRecDiscriminator =
    | Primitive = 1
    | Lookup = 2
    | TypeLambda = 3
    | TypeApply = 4
    | TypeLet = 5
    | Lambda = 6
    | FromValue = 7
    | Apply = 8
    | Let = 9
    | If = 10
    | RecordCons = 11
    | RecordWith = 12
    | TupleCons = 13
    | SumCons = 14
    | RecordDes = 15
    | EntitiesDes = 16
    | RelationsDes = 17
    | EntityDes = 18
    | RelationDes = 19
    | RelationLookupDes = 20
    | UnionDes = 21
    | TupleDes = 22
    | SumDes = 23

  type PrimitiveValueDTO =
    { Kind: PrimitiveValueDiscriminator
      Int32: Nullable<int>
      Int64: Nullable<int64>
      Float32: Nullable<float32>
      Float64: Nullable<float>
      Decimal: Nullable<decimal>
      Bool: Nullable<bool>
      Guid: Nullable<Guid>
      String: string
      Date: Nullable<DateOnly>
      DateTime: Nullable<DateTime>
      TimeSpan: Nullable<TimeSpan> }

    static member Empty =
      { Kind = PrimitiveValueDiscriminator.Unit
        Int32 = Nullable()
        Int64 = Nullable()
        Float32 = Nullable()
        Float64 = Nullable()
        Decimal = Nullable()
        Bool = Nullable()
        Guid = Nullable()
        String = null
        Date = Nullable()
        DateTime = Nullable()
        TimeSpan = Nullable() }

    static member CreateInt32 int32 =
      { PrimitiveValueDTO.Empty with
          Kind = PrimitiveValueDiscriminator.Int32
          Int32 = Nullable int32 }

    static member CreateInt64 int64 =
      { PrimitiveValueDTO.Empty with
          Kind = PrimitiveValueDiscriminator.Int64
          Int64 = Nullable int64 }

    static member CreateFloat32 float32 =
      { PrimitiveValueDTO.Empty with
          Kind = PrimitiveValueDiscriminator.Float32
          Float32 = Nullable float32 }

    static member CreateFloat64 float64 =
      { PrimitiveValueDTO.Empty with
          Kind = PrimitiveValueDiscriminator.Float64
          Float64 = Nullable float64 }

    static member CreateDecimal decimal =
      { PrimitiveValueDTO.Empty with
          Kind = PrimitiveValueDiscriminator.Decimal
          Decimal = Nullable decimal }

    static member CreateBool bool =
      { PrimitiveValueDTO.Empty with
          Kind = PrimitiveValueDiscriminator.Bool
          Bool = Nullable bool }

    static member CreateGuid guid =
      { PrimitiveValueDTO.Empty with
          Kind = PrimitiveValueDiscriminator.Guid
          Guid = Nullable guid }

    static member CreateString string =
      { PrimitiveValueDTO.Empty with
          Kind = PrimitiveValueDiscriminator.String
          String = string }

    static member CreateDate date =
      { PrimitiveValueDTO.Empty with
          Kind = PrimitiveValueDiscriminator.Date
          Date = Nullable date }

    static member CreateDateTime dateTime =
      { PrimitiveValueDTO.Empty with
          Kind = PrimitiveValueDiscriminator.DateTime
          DateTime = Nullable dateTime }

    static member CreateTimeSpan span =
      { PrimitiveValueDTO.Empty with
          Kind = PrimitiveValueDiscriminator.TimeSpan
          TimeSpan = Nullable span }

  and ExprTypeLambdaDTO<'T, 'Id, 'valueExtDTO
    when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { Body: ExprDTO<'T, 'Id, 'valueExtDTO>
      Param: TypeParameterPlainDTO }

  and ExprTypeApplyDTO<'T, 'Id, 'valueExtDTO
    when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { Func: ExprDTO<'T, 'Id, 'valueExtDTO>
      TypeArg: 'T }

  and ExprTypeLetDTO<'T, 'Id, 'valueExtDTO when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct>
    =
    { Name: string
      TypeDef: 'T
      Body: ExprDTO<'T, 'Id, 'valueExtDTO> }

  and ExprLambdaDTO<'T, 'Id, 'valueExtDTO when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct>
    =
    { Param: Var
      ParamType: 'T
      Body: ExprDTO<'T, 'Id, 'valueExtDTO> }

  and ExprFromValueDTO<'T, 'Id, 'valueExtDTO
    when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { Value: ValueDTO<'valueExtDTO>
      ValueType: 'T
      ValueKind: Kind }

  and ExprApplyDTO<'T, 'Id, 'valueExtDTO when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { F: ExprDTO<'T, 'Id, 'valueExtDTO>
      Arg: ExprDTO<'T, 'Id, 'valueExtDTO> }

  and ExprLetDTO<'T, 'Id, 'valueExtDTO when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { Var: Var
      Type: 'T
      Val: ExprDTO<'T, 'Id, 'valueExtDTO>
      Rest: ExprDTO<'T, 'Id, 'valueExtDTO> }

  and ExprIfDTO<'T, 'Id, 'valueExtDTO when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { Cond: ExprDTO<'T, 'Id, 'valueExtDTO>
      Then: ExprDTO<'T, 'Id, 'valueExtDTO>
      Else: ExprDTO<'T, 'Id, 'valueExtDTO> }

  and ExprRecordConsDTO<'T, 'Id, 'valueExtDTO
    when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { Fields: List<'Id * ExprDTO<'T, 'Id, 'valueExtDTO>> }

  and ExprRecordWithDTO<'T, 'Id, 'valueExtDTO
    when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { Record: ExprDTO<'T, 'Id, 'valueExtDTO>
      Fields: List<'Id * ExprDTO<'T, 'Id, 'valueExtDTO>> }

  and ExprTupleConsDTO<'T, 'Id, 'valueExtDTO
    when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { Items: List<ExprDTO<'T, 'Id, 'valueExtDTO>> }

  and ExprSumConsDTO<'T, 'Id, 'valueExtDTO when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct>
    = { Selector: SumConsSelector }

  and ExprRecordDesDTO<'T, 'Id, 'valueExtDTO
    when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { Expr: ExprDTO<'T, 'Id, 'valueExtDTO>
      Field: 'Id }

  and ExprEntitiesDesDTO<'T, 'Id, 'valueExtDTO
    when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { Expr: ExprDTO<'T, 'Id, 'valueExtDTO> }

  and ExprRelationsDesDTO<'T, 'Id, 'valueExtDTO
    when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { Expr: ExprDTO<'T, 'Id, 'valueExtDTO> }

  and ExprEntityDesDTO<'T, 'Id, 'valueExtDTO
    when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { Expr: ExprDTO<'T, 'Id, 'valueExtDTO>
      EntityName: SchemaEntityName }

  and ExprRelationDesDTO<'T, 'Id, 'valueExtDTO
    when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { Expr: ExprDTO<'T, 'Id, 'valueExtDTO>
      RelationName: SchemaRelationName }

  and ExprRelationLookupDesDTO<'T, 'Id, 'valueExtDTO
    when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { Expr: ExprDTO<'T, 'Id, 'valueExtDTO>
      RelationName: SchemaRelationName
      Direction: RelationLookupDirection } //TODO <- this is union

  and ExprUnionDesDTO<'T, 'Id, 'valueExtDTO when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct>
    =
    { Handlers: Map<'Id, CaseHandler<'T, 'Id, 'valueExtDTO>>
      Fallback: ExprDTO<'T, 'Id, 'valueExtDTO> }

  and ExprTupleDesDTO<'T, 'Id, 'valueExtDTO when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct>
    =
    { Tuple: ExprDTO<'T, 'Id, 'valueExtDTO>
      Item: TupleDesSelector }

  and ExprSumDesDTO<'T, 'Id, 'valueExtDTO when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct>
    =
    { Handlers: Map<SumConsSelector, CaseHandler<'T, 'Id, 'valueExtDTO>> }

  and ExprRecDTO<'T, 'Id, 'valueExtDTO when 'Id: comparison and 'valueExtDTO: not null and 'valueExtDTO: not struct> =
    { Kind: ExprRecDiscriminator
      Primitive: PrimitiveValueDTO | null
      Lookup: ExprLookup<'T, 'Id, 'valueExtDTO> | null
      TypeLambda: ExprTypeLambdaDTO<'T, 'Id, 'valueExtDTO> | null
      TypeApply: ExprTypeApplyDTO<'T, 'Id, 'valueExtDTO> | null
      TypeLet: ExprTypeLetDTO<'T, 'Id, 'valueExtDTO> | null
      Lambda: ExprLambdaDTO<'T, 'Id, 'valueExtDTO> | null
      FromValue: ExprFromValueDTO<'T, 'Id, 'valueExtDTO> | null
      Apply: ExprApplyDTO<'T, 'Id, 'valueExtDTO> | null
      Let: ExprLetDTO<'T, 'Id, 'valueExtDTO> | null
      If: ExprIfDTO<'T, 'Id, 'valueExtDTO> | null
      RecordCons: ExprRecordConsDTO<'T, 'Id, 'valueExtDTO> | null
      RecordWith: ExprRecordWithDTO<'T, 'Id, 'valueExtDTO> | null
      TupleCons: ExprTupleConsDTO<'T, 'Id, 'valueExtDTO> | null
      SumCons: ExprSumConsDTO<'T, 'Id, 'valueExtDTO> | null
      RecordDes: ExprRecordDesDTO<'T, 'Id, 'valueExtDTO> | null
      EntitiesDes: ExprEntitiesDesDTO<'T, 'Id, 'valueExtDTO> | null
      RelationsDes: ExprRelationsDesDTO<'T, 'Id, 'valueExtDTO> | null
      EntityDes: ExprEntityDesDTO<'T, 'Id, 'valueExtDTO> | null
      RelationDes: ExprRelationDesDTO<'T, 'Id, 'valueExtDTO> | null
      RelationLookupDes: ExprRelationLookupDesDTO<'T, 'Id, 'valueExtDTO> | null
      UnionDes: ExprUnionDesDTO<'T, 'Id, 'valueExtDTO> | null
      TupleDes: ExprTupleDesDTO<'T, 'Id, 'valueExtDTO> | null
      SumDes: ExprSumDesDTO<'T, 'Id, 'valueExtDTO> | null }

    static member private Empty: ExprRecDTO<'T, 'Id, 'valueExtDTO> =
      { Kind = ExprRecDiscriminator.Primitive
        Primitive = null
        Lookup = null
        TypeLambda = null
        TypeApply = null
        TypeLet = null
        Lambda = null
        FromValue = null
        Apply = null
        If = null
        Let = null
        RecordCons = null
        RecordWith = null
        TupleCons = null
        SumCons = null
        RecordDes = null
        EntitiesDes = null
        RelationsDes = null
        EntityDes = null
        RelationDes = null
        RelationLookupDes = null
        UnionDes = null
        TupleDes = null
        SumDes = null

      }

    static member CreatePrimitive primitive =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.Primitive
          Primitive = primitive }

    static member CreateLookup lookup =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.Lookup
          Lookup = lookup }

    static member CreateTypeLambda typeLambda =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.TypeLambda
          TypeLambda = typeLambda }

    static member CreateTypeApply typeApply =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.TypeApply
          TypeApply = typeApply }

    static member CreateTypeLet typeLet =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.TypeLet
          TypeLet = typeLet }

    static member CreateLambda lambda =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.Lambda
          Lambda = lambda }

    static member CreateFromValue fromValue =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.FromValue
          FromValue = fromValue }

    static member CreateApply apply =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.Apply
          Apply = apply }

    static member CreateIf ``if`` =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.If
          If = ``if`` }

    static member CreateLet ``let`` =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.Let
          Let = ``let`` }

    static member CreateRecordCons recCons =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.RecordCons
          RecordCons = recCons }

    static member CreateRecordWith recWith =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.RecordWith
          RecordWith = recWith }

    static member CreateTupleCons tupleCons =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.TupleCons
          TupleCons = tupleCons }

    static member CreateRecordDes recDes =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.RecordDes
          RecordDes = recDes }

    static member CreateEntitiesDes entitiesDes =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.EntitiesDes
          EntitiesDes = entitiesDes }

    static member CreateRelationsDes relationsDes =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.RelationsDes
          RelationsDes = relationsDes }

    static member CreateEntityDes entityDes =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.EntityDes
          EntityDes = entityDes }

    static member CreateRelationDes relationDes =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.RelationDes
          RelationDes = relationDes }

    static member CreateRelationLookupDes relationLookupDes =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.RelationLookupDes
          RelationLookupDes = relationLookupDes }

    static member CreateUnionDes unionDes =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.UnionDes
          UnionDes = unionDes }

    static member CreateTupleDes tupleDes =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.TupleDes
          TupleDes = tupleDes }

    static member CreateSumDes sumDes =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.SumDes
          SumDes = sumDes }

    static member CreateSumCons sumCons =
      { ExprRecDTO<'T, 'Id, 'valueExtDTO>.Empty with
          Kind = ExprRecDiscriminator.SumCons
          SumCons = sumCons }

  and TypeLambdaDTO<'T, 'valueExt when 'valueExt: not null and 'valueExt: not struct> =
    { TypeParameter: TypeParameterDTO
      Expr: ExprDTO<'T, ResolvedIdentifierDTO, 'valueExt> }

  and LambdaDTO<'T, 'valueExt when 'valueExt: not struct and 'valueExt: not null> =
    { Var: Var
      Expr: ExprDTO<'T, ResolvedIdentifierDTO, 'valueExt>
      Closure: Map<ResolvedIdentifierDTO, ValueDTO<'valueExt>>
      TypeCheckScope: TypeCheckScopeDTO }

  and UnionCaseDTO<'valueExt when 'valueExt: not struct and 'valueExt: not null> =
    { Case: ResolvedIdentifierDTO
      Value: ValueDTO<'valueExt> }

  and SumDTO<'valueExt when 'valueExt: not struct and 'valueExt: not null> =
    { Selector: SumConsSelector
      Value: ValueDTO<'valueExt> }

  and RecordKeyValueDTO<'valueExt when 'valueExt: not null and 'valueExt: not struct> =
    { Key: ResolvedIdentifierDTO
      Value: ValueDTO<'valueExt> }

  and ExtDTO<'valueExt> =
    { Value: 'valueExt
      ApplicableId: ResolvedIdentifierDTO | null }

  and ValueDTO<'valueExt when 'valueExt: not null and 'valueExt: not struct> =
    { Kind: ValueDiscriminator
      Record: RecordKeyValueDTO<'valueExt>[] | null
      UnionCase: UnionCaseDTO<'valueExt> | null
      RecordDes: ResolvedIdentifierDTO | null
      UnionCons: ResolvedIdentifierDTO | null
      Tuple: ValueDTO<'valueExt>[] | null
      Sum: SumDTO<'valueExt> | null
      Primitive: PrimitiveValueDTO | null
      Var: Var | null
      Ext: ExtDTO<'valueExt> | null }

    static member private Empty =
      { Kind = ValueDiscriminator.Primitive
        Record = null
        UnionCase = null
        RecordDes = null
        UnionCons = null
        Tuple = null
        Sum = null
        Primitive = null
        Var = null
        Ext = null }

    static member CreateRecord(record: RecordKeyValueDTO<'valueExt>[]) =
      { ValueDTO<'valueExt>.Empty with
          Kind = ValueDiscriminator.Record
          Record = record }

    static member CreateUnionCase case =
      { ValueDTO<'valueExt>.Empty with
          Kind = ValueDiscriminator.UnionCase
          UnionCase = case }

    static member CreateRecordDes des =
      { ValueDTO<'valueExt>.Empty with
          Kind = ValueDiscriminator.RecordDes
          RecordDes = des }

    static member CreateUnionCons cons =
      { ValueDTO<'valueExt>.Empty with
          Kind = ValueDiscriminator.UnionCons
          UnionCons = cons }

    static member CreateTuple tuple =
      { ValueDTO<'valueExt>.Empty with
          Kind = ValueDiscriminator.Tuple
          Tuple = tuple }

    static member CreateSum sum =
      { ValueDTO<'valueExt>.Empty with
          Kind = ValueDiscriminator.Sum
          Sum = sum }

    static member CreatePrimitive primitive =
      { ValueDTO<'valueExt>.Empty with
          Kind = ValueDiscriminator.Primitive
          Primitive = primitive }

    static member CreateVar var =
      { ValueDTO<'valueExt>.Empty with
          Kind = ValueDiscriminator.Var
          Var = var }

    static member CreateExt (applicableId: Option<ResolvedIdentifierDTO>) (ext: 'valueExt) =
      { ValueDTO<'valueExt>.Empty with
          Kind = ValueDiscriminator.Ext
          Ext =
            { Value = ext
              ApplicableId =
                match applicableId with
                | Some id -> id
                | None -> null } }



  and ExprDTO<'T, 'Id, 'valueExt when 'Id: comparison and 'valueExt: not struct and 'valueExt: not null> =
    { Expr: ExprRecDTO<'T, 'Id, 'valueExt>
      Location: Location
      Scope: TypeCheckScopeDTO }
