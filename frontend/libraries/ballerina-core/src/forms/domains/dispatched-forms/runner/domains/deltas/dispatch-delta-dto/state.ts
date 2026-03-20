import { DispatchParsedType } from "src/forms/domains/dispatched-forms/deserializer/domains/specification/domains/types/state";
import { Unit } from "../../../../../../../fun/domains/unit/state";

export type DispatchTransferTuple2<a, b> = { Item1: a; Item2: b };

export type DispatchDeltaTransferNumberReplace = {
  Discriminator: "NumberReplace";
  Replace: any;
};
export type DispatchDeltaTransferStringReplace = {
  Discriminator: "StringReplace";
  Replace: any;
};
export type DispatchDeltaTransferBoolReplace = {
  Discriminator: "BoolReplace";
  Replace: any;
};
export type DispatchDeltaTransferTimeReplace = {
  Discriminator: "TimeReplace";
  Replace: any;
};
export type DispatchDeltaTransferGuidReplace = {
  Discriminator: "GuidReplace";
  Replace: any;
};
export type DispatchDeltaTransferPrimitive =
  | DispatchDeltaTransferNumberReplace
  | DispatchDeltaTransferStringReplace
  | DispatchDeltaTransferBoolReplace
  | DispatchDeltaTransferTimeReplace
  | DispatchDeltaTransferGuidReplace;

export type DispatchDeltaTransferUnit = {};
export type DispatchDeltaTransferOptionReplace = {
  Discriminator: "OptionReplace";
  Replace: any;
};
export type DispatchDeltaTransferOptionValue<DispatchDeltaTransferCustom> = {
  Discriminator: "OptionValue";
  Value: DeltaTransfer<DispatchDeltaTransferCustom>;
};
export type DispatchDeltaTransferOption<DispatchDeltaTransferCustom> =
  | DispatchDeltaTransferOptionReplace
  | DispatchDeltaTransferOptionValue<DispatchDeltaTransferCustom>;

export type DispatchDeltaTransferSumReplace = {
  Discriminator: "SumReplace";
  Replace: any;
};
export type DispatchDeltaTransferSumLeft<DispatchDeltaTransferCustom> = {
  Discriminator: "SumLeft";
  Left: DeltaTransfer<DispatchDeltaTransferCustom>;
};
export type DispatchDeltaTransferSumRight<DispatchDeltaTransferCustom> = {
  Discriminator: "SumRight";
  Right: DeltaTransfer<DispatchDeltaTransferCustom>;
};
export type DispatchDeltaTransferSum<DispatchDeltaTransferCustom> =
  | DispatchDeltaTransferSumReplace
  | DispatchDeltaTransferSumLeft<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferSumRight<DispatchDeltaTransferCustom>;

export type DispatchDeltaTransferArrayAdd = {
  Discriminator: "ArrayAdd";
  Add: any;
};
export type DispatchDeltaTransferArrayReplace = {
  Discriminator: "ArrayReplace";
  Replace: any;
};
export type DispatchDeltaTransferArrayValue<DispatchDeltaTransferCustom> = {
  Discriminator: "ArrayValue";
  Value: DispatchTransferTuple2<
    number,
    DeltaTransfer<DispatchDeltaTransferCustom>
  >;
};
export type DispatchDeltaTransferArrayValueAll<DispatchDeltaTransferCustom> = {
  Discriminator: "ArrayValueAll";
  ValueAll: DeltaTransfer<DispatchDeltaTransferCustom>;
};
export type DispatchDeltaTransferArrayAddAt = {
  Discriminator: "ArrayAddAt";
  AddAt: DispatchTransferTuple2<number, any>;
};
export type DispatchDeltaTransferArrayRemoveAt = {
  Discriminator: "ArrayRemoveAt";
  RemoveAt: number;
};
export type DispatchDeltaTransferArrayRemoveAll = {
  Discriminator: "ArrayRemoveAll";
  RemoveAll: Unit;
};
export type DispatchDeltaTransferArrayMoveFromTo = {
  Discriminator: "ArrayMoveFromTo";
  MoveFromTo: DispatchTransferTuple2<number, number>;
};
export type DispatchDeltaTransferArrayDuplicateAt = {
  Discriminator: "ArrayDuplicateAt";
  DuplicateAt: number;
};
export type DispatchDeltaTransferList<DispatchDeltaTransferCustom> =
  | DispatchDeltaTransferArrayAdd
  | DispatchDeltaTransferArrayReplace
  | DispatchDeltaTransferArrayValue<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferArrayValueAll<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferArrayAddAt
  | DispatchDeltaTransferArrayRemoveAt
  | DispatchDeltaTransferArrayRemoveAll
  | DispatchDeltaTransferArrayMoveFromTo
  | DispatchDeltaTransferArrayDuplicateAt;

export type DispatchDeltaTransferSetReplace = {
  Discriminator: "SetReplace";
  Replace: any;
};
export type DispatchDeltaTransferSetValue<DispatchDeltaTransferCustom> = {
  Discriminator: "SetValue";
  Value: DispatchTransferTuple2<
    any,
    DeltaTransfer<DispatchDeltaTransferCustom>
  >;
};
export type DispatchDeltaTransferSetAdd = {
  Discriminator: "SetAdd";
  Add: any;
};
export type DispatchDeltaTransferSetRemove = {
  Discriminator: "SetRemove";
  Remove: any;
};
export type DispatchDeltaTransferSet<DispatchDeltaTransferCustom> =
  | DispatchDeltaTransferSetReplace
  | DispatchDeltaTransferSetValue<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferSetAdd
  | DispatchDeltaTransferSetRemove;

export type DispatchDeltaTransferMapReplace = {
  Discriminator: "MapReplace";
  Replace: any;
};
export type DispatchDeltaTransferMapValue<DispatchDeltaTransferCustom> = {
  Discriminator: "MapValue";
  Value: DispatchTransferTuple2<
    number,
    DeltaTransfer<DispatchDeltaTransferCustom>
  >;
};
export type DispatchDeltaTransferMapKey<DispatchDeltaTransferCustom> = {
  Discriminator: "MapKey";
  Key: DispatchTransferTuple2<
    number,
    DeltaTransfer<DispatchDeltaTransferCustom>
  >;
};
export type DispatchDeltaTransferMapAdd = {
  Discriminator: "MapAdd";
  Add: DispatchTransferTuple2<any, any>;
};
export type DispatchDeltaTransferMapRemove = {
  Discriminator: "MapRemove";
  Remove: number;
};
export type DispatchDeltaTransferMap<DispatchDeltaTransferCustom> =
  | DispatchDeltaTransferMapReplace
  | DispatchDeltaTransferMapValue<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferMapKey<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferMapAdd
  | DispatchDeltaTransferMapRemove;

export type DispatchDeltaTransferRecordReplace = {
  Discriminator: string;
  Replace: any;
};
export type DispatchDeltaTransferRecordField<DispatchDeltaTransferCustom> = {
  Discriminator: string;
} & {
  [field: string]: DeltaTransfer<DispatchDeltaTransferCustom>;
};
export type DispatchDeltaTransferRecord<DispatchDeltaTransferCustom> =
  | DispatchDeltaTransferRecordReplace
  | DispatchDeltaTransferRecordField<DispatchDeltaTransferCustom>;

export type DispatchDeltaTransferUnionReplace = {
  Discriminator: "UnionReplace";
  Replace: any;
};
export type DispatchDeltaTransferUnionCase<DispatchDeltaTransferCustom> = {
  Discriminator: string;
} & {
  [caseName: string]: DeltaTransfer<DispatchDeltaTransferCustom>;
};
export type DispatchDeltaTransferUnion<DispatchDeltaTransferCustom> =
  | DispatchDeltaTransferUnionReplace
  | DispatchDeltaTransferUnionCase<DispatchDeltaTransferCustom>;

export type DispatchDeltaTransferTupleReplace = {
  Discriminator: "TupleReplace";
  Replace: any;
};
export type DispatchDeltaTransferTupleValue<DispatchDeltaTransferCustom> = {
  Discriminator: string;
} & {
  [item: string]: DeltaTransfer<DispatchDeltaTransferCustom>;
};
export type DispatchDeltaTransferTuple<DispatchDeltaTransferCustom> =
  | DispatchDeltaTransferTupleReplace
  | DispatchDeltaTransferTupleValue<DispatchDeltaTransferCustom>;

export type DispatchDeltaTransferTableValue<DispatchDeltaTransferCustom> = {
  Discriminator: "TableValue";
  Value: DispatchTransferTuple2<
    string,
    DeltaTransfer<DispatchDeltaTransferCustom>
  >;
};
export type DispatchDeltaTransferTableValueAll<DispatchDeltaTransferCustom> = {
  Discriminator: "TableValueAll";
  ValueAll: DeltaTransfer<DispatchDeltaTransferCustom>;
};
export type DispatchDeltaTransferTableAddEmpty = {
  Discriminator: "TableAddEmpty";
};
export type DispatchDeltaTransferTableAdd = {
  Discriminator: "TableAdd";
  Add: any;
};
export type DispatchDeltaTransferTableAddBatch = {
  Discriminator: "TableAddBatch";
  AddBatch: any[];
};
export type DispatchDeltaTransferTableAddBatchEmpty = {
  Discriminator: "TableAddBatchEmpty";
  AddBatchEmpty: number;
};
export type DispatchDeltaTransferTableRemoveAt = {
  Discriminator: "TableRemoveAt";
  RemoveAt: string;
};
export type DispatchDeltaTransferTableRemoveBatch = {
  Discriminator: "TableRemoveBatch";
  RemoveBatch: string[];
};
export type DispatchDeltaTransferTableRemoveAll = {
  Discriminator: "TableRemoveAll";
  RemoveAll: Unit;
};
export type DispatchDeltaTransferTableDuplicateAt = {
  Discriminator: "TableDuplicateAt";
  DuplicateAt: string;
};
export type DispatchDeltaTransferTableActionOnAll = {
  Discriminator: "TableActionOnAll";
  ActionOnAll: string;
};
export type DispatchDeltaTransferTableMoveFromTo = {
  Discriminator: "TableMoveFromTo";
  MoveFromTo: [string, string];
};
export type DispatchDeltaTransferTable<DispatchDeltaTransferCustom> =
  | DispatchDeltaTransferTableValue<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferTableValueAll<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferTableAddEmpty
  | DispatchDeltaTransferTableAdd
  | DispatchDeltaTransferTableAddBatch
  | DispatchDeltaTransferTableAddBatchEmpty
  | DispatchDeltaTransferTableRemoveAt
  | DispatchDeltaTransferTableRemoveBatch
  | DispatchDeltaTransferTableRemoveAll
  | DispatchDeltaTransferTableDuplicateAt
  | DispatchDeltaTransferTableActionOnAll
  | DispatchDeltaTransferTableMoveFromTo;

export type DispatchDeltaTransferOneValue<DispatchDeltaTransferCustom> = {
  Discriminator: "OneValue";
  Value: DeltaTransfer<DispatchDeltaTransferCustom>;
};
export type DispatchDeltaTransferOneReplace = {
  Discriminator: "OneReplace";
  Replace: any;
};
export type DispatchDeltaTransferOneCreateValue = {
  Discriminator: "OneCreateValue";
  CreateValue: any;
};
export type DispatchDeltaTransferOneDeleteValue = {
  Discriminator: "OneDeleteValue";
  DeleteValue: Unit;
};
export type DispatchDeltaTransferOne<DispatchDeltaTransferCustom> =
  | DispatchDeltaTransferOneValue<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferOneReplace
  | DispatchDeltaTransferOneCreateValue
  | DispatchDeltaTransferOneDeleteValue;

export type DeltaTransfer<DispatchDeltaTransferCustom> =
  | DispatchDeltaTransferPrimitive
  | DispatchDeltaTransferUnit
  | DispatchDeltaTransferOption<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferSum<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferList<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferSet<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferMap<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferRecord<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferUnion<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferTuple<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferTable<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferOne<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferCustom;

export const DispatchDeltaTransferOperations = {
  isNumberReplace: (
    delta: unknown,
  ): delta is { Discriminator: "NumberReplace"; Replace: number } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "NumberReplace" &&
    "Replace" in delta &&
    typeof delta.Replace === "number",
  isStringReplace: (
    delta: unknown,
  ): delta is { Discriminator: "StringReplace"; Replace: string } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "StringReplace" &&
    "Replace" in delta &&
    typeof delta.Replace === "string",
  isBoolReplace: (
    delta: unknown,
  ): delta is { Discriminator: "BoolReplace"; Replace: boolean } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "BoolReplace" &&
    "Replace" in delta &&
    typeof delta.Replace === "boolean",
  isTimeReplace: (
    delta: unknown,
  ): delta is { Discriminator: "TimeReplace"; Replace: number } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "TimeReplace" &&
    "Replace" in delta &&
    typeof delta.Replace === "number",
  isGuidReplace: (
    delta: unknown,
  ): delta is { Discriminator: "GuidReplace"; Replace: string } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "GuidReplace" &&
    "Replace" in delta &&
    typeof delta.Replace === "string",
  isInt32Replace: (
    delta: unknown,
  ): delta is { Discriminator: "Int32Replace"; Replace: bigint } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "Int32Replace" &&
    "Replace" in delta &&
    typeof delta.Replace === "bigint",
  isFloat32Replace: (
    delta: unknown,
  ): delta is { Discriminator: "Float32Replace"; Replace: number } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "Float32Replace" &&
    "Replace" in delta &&
    typeof delta.Replace === "number",
  isUnit: (delta: unknown): delta is {} =>
    typeof delta === "object" &&
    delta !== null &&
    Object.keys(delta).length === 0,
  isOptionReplace: (
    delta: unknown,
  ): delta is { Discriminator: "OptionReplace"; Replace: any } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "OptionReplace" &&
    "Replace" in delta &&
    typeof delta.Replace === "object",
  isOptionValue: (
    delta: unknown,
  ): delta is {
    Discriminator: "OptionValue";
    Value: object;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "OptionValue" &&
    "Value" in delta &&
    typeof delta.Value === "object",
  isSumReplace: (
    delta: unknown,
  ): delta is { Discriminator: "SumReplace"; Replace: any } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "SumReplace" &&
    "Replace" in delta &&
    typeof delta.Replace === "object",
  isSumLeft: (
    delta: unknown,
  ): delta is {
    Discriminator: "SumLeft";
    Left: object;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "SumLeft" &&
    "Left" in delta &&
    typeof delta.Left === "object",
  isSumRight: (
    delta: unknown,
  ): delta is {
    Discriminator: "SumRight";
    Right: object;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "SumRight" &&
    "Right" in delta &&
    typeof delta.Right === "object",
  isArrayAdd: (
    delta: unknown,
  ): delta is { Discriminator: "ArrayAdd"; Add: object } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "ArrayAdd" &&
    "Add" in delta &&
    typeof delta.Add === "object",
  isArrayReplace: (
    delta: unknown,
  ): delta is { Discriminator: "ArrayReplace"; Replace: object } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "ArrayReplace" &&
    "Replace" in delta &&
    typeof delta.Replace === "object",
  isArrayValue: (
    delta: unknown,
  ): delta is {
    Discriminator: "ArrayValue";
    Value: DispatchTransferTuple2<number, object>;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "ArrayValue" &&
    "Value" in delta &&
    typeof delta.Value === "object" &&
    delta.Value !== null &&
    "Item1" in delta.Value &&
    typeof delta.Value.Item1 !== null &&
    typeof delta.Value.Item1 === "number" &&
    "Item2" in delta.Value &&
    typeof delta.Value.Item2 !== null &&
    typeof delta.Value.Item2 === "object",
  isArrayValueAll: (
    delta: unknown,
  ): delta is {
    Discriminator: "ArrayValueAll";
    ValueAll: object;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "ArrayValueAll" &&
    "ValueAll" in delta &&
    typeof delta.ValueAll === "object" &&
    delta.ValueAll !== null,
  isArrayAddAt: (
    delta: unknown,
  ): delta is {
    Discriminator: "ArrayAddAt";
    AddAt: DispatchTransferTuple2<number, any>;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "ArrayAddAt" &&
    "AddAt" in delta &&
    typeof delta.AddAt === "object",
  isArrayRemoveAt: (
    delta: unknown,
  ): delta is { Discriminator: "ArrayRemoveAt"; RemoveAt: number } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "ArrayRemoveAt" &&
    "RemoveAt" in delta &&
    typeof delta.RemoveAt === "number",
  isArrayRemoveAll: (
    delta: unknown,
  ): delta is { Discriminator: "ArrayRemoveAll"; RemoveAll: Unit } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "ArrayRemoveAll" &&
    "RemoveAll" in delta &&
    typeof delta.RemoveAll === "object" &&
    delta.RemoveAll !== null &&
    Object.keys(delta.RemoveAll).length === 0,
  isArrayMoveFromTo: (
    delta: unknown,
  ): delta is {
    Discriminator: "ArrayMoveFromTo";
    MoveFromTo: DispatchTransferTuple2<number, number>;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "ArrayMoveFromTo" &&
    "MoveFromTo" in delta &&
    typeof delta.MoveFromTo === "object",
  isArrayDuplicateAt: (
    delta: unknown,
  ): delta is {
    Discriminator: "ArrayDuplicateAt";
    DuplicateAt: number;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "ArrayDuplicateAt" &&
    "DuplicateAt" in delta &&
    typeof delta.DuplicateAt === "number",
  isSetReplace: (
    delta: unknown,
  ): delta is {
    Discriminator: "SetReplace";
    Replace: object;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "SetReplace" &&
    "Replace" in delta &&
    typeof delta.Replace === "object",
  isSetValue: (
    delta: unknown,
  ): delta is {
    Discriminator: "SetValue";
    Value: DispatchTransferTuple2<any, object>;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "SetValue" &&
    "Value" in delta &&
    typeof delta.Value === "object" &&
    delta.Value !== null &&
    "Item1" in delta.Value &&
    "Item2" in delta.Value &&
    typeof delta.Value.Item2 !== null &&
    typeof delta.Value.Item2 === "object",
  isSetAdd: (
    delta: unknown,
  ): delta is {
    Discriminator: "SetAdd";
    Add: object;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "SetAdd" &&
    "Add" in delta &&
    typeof delta.Add === "object",
  isSetRemove: (
    delta: unknown,
  ): delta is {
    Discriminator: "SetRemove";
    Remove: object;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "SetRemove" &&
    "Remove" in delta &&
    typeof delta.Remove === "object",
  isMapReplace: (
    delta: unknown,
  ): delta is {
    Discriminator: "MapReplace";
    Replace: object;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "MapReplace" &&
    "Replace" in delta &&
    typeof delta.Replace === "object",
  isMapValue: (
    delta: unknown,
  ): delta is {
    Discriminator: "MapValue";
    Value: DispatchTransferTuple2<number, object>;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "MapValue" &&
    "Value" in delta &&
    typeof delta.Value === "object" &&
    delta.Value !== null &&
    "Item1" in delta.Value &&
    typeof delta.Value.Item1 !== null &&
    typeof delta.Value.Item1 === "number" &&
    "Item2" in delta.Value &&
    typeof delta.Value.Item2 !== null &&
    typeof delta.Value.Item2 === "object",
  isMapKey: (
    delta: unknown,
  ): delta is {
    Discriminator: "MapKey";
    Key: DispatchTransferTuple2<number, object>;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "MapKey" &&
    "Key" in delta &&
    typeof delta.Key === "object" &&
    delta.Key !== null &&
    "Item1" in delta.Key &&
    typeof delta.Key.Item1 !== null &&
    typeof delta.Key.Item1 === "number" &&
    "Item2" in delta.Key &&
    typeof delta.Key.Item2 !== null &&
    typeof delta.Key.Item2 === "object",
  isMapAdd: (
    delta: unknown,
  ): delta is {
    Discriminator: "MapAdd";
    Add: DispatchTransferTuple2<any, object>;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "MapAdd" &&
    "Add" in delta &&
    typeof delta.Add === "object" &&
    delta.Add !== null &&
    "Item1" in delta.Add &&
    "Item2" in delta.Add &&
    typeof delta.Add.Item2 !== null &&
    typeof delta.Add.Item2 === "object",
  isMapRemove: (
    delta: unknown,
  ): delta is {
    Discriminator: "MapRemove";
    Remove: number;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "MapRemove" &&
    "Remove" in delta &&
    typeof delta.Remove === "number",
  isRecordReplace: (
    delta: unknown,
    type: DispatchParsedType<any>,
  ): delta is { Discriminator: string; Replace: object } =>
    type.kind == "record" &&
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    typeof delta.Discriminator === "string" &&
    "Replace" in delta &&
    typeof delta.Replace === "object",
  isRecordField: (
    delta: unknown,
    type: DispatchParsedType<any>,
  ): delta is {
    Discriminator: string;
  } & {
    [field: string]: object;
  } =>
    type.kind == "record" &&
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    typeof delta.Discriminator === "string" &&
    Object.keys(delta).includes(delta.Discriminator),
  isUnionReplace: (
    delta: unknown,
  ): delta is { Discriminator: "UnionReplace"; Replace: object } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "UnionReplace" &&
    "Replace" in delta &&
    typeof delta.Replace === "object",
  isUnionCase: (
    delta: unknown,
    type: DispatchParsedType<any>,
  ): delta is { Discriminator: string } & {
    [caseName: string]: object;
  } =>
    type.kind == "union" &&
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    typeof delta.Discriminator === "string" &&
    Object.keys(delta).includes(delta.Discriminator),
  isTupleReplace: (
    delta: unknown,
  ): delta is { Discriminator: "TupleReplace"; Replace: object } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "TupleReplace" &&
    "Replace" in delta &&
    typeof delta.Replace === "object",
  isTupleValue: (
    delta: unknown,
    type: DispatchParsedType<any>,
  ): delta is {
    Discriminator: string;
    [item: string]: unknown;
  } =>
    type.kind == "tuple" &&
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    typeof delta.Discriminator === "string" &&
    (
      (() => {
        const match = /^Tuple\d+Item(\d+)$/.exec(delta.Discriminator);
        return match != null && Object.keys(delta).includes(`Item${match[1]}`);
    })()),
isTableValue: (
    delta: unknown,
  ): delta is {
    Discriminator: "TableValue";
    Value: DispatchTransferTuple2<string, object>;
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "TableValue" &&
    "Value" in delta &&
    typeof delta.Value === "object",
  isTableValueAll: (
    delta: unknown,
  ): delta is { Discriminator: "TableValueAll"; ValueAll: object } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "TableValueAll" &&
    "ValueAll" in delta &&
    typeof delta.ValueAll === "object" &&
    delta.ValueAll !== null,
  isTableAddEmpty: (
    delta: unknown,
  ): delta is { Discriminator: "TableAddEmpty" } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "TableAddEmpty",
  isTableAdd: (
    delta: unknown,
  ): delta is { Discriminator: "TableAdd"; Add: object } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "TableAdd" &&
    "Add" in delta &&
    typeof delta.Add === "object",
  isTableAddBatch: (
    delta: unknown,
  ): delta is { Discriminator: "TableAddBatch"; AddBatch: object[] } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "TableAddBatch" &&
    "AddBatch" in delta &&
    Array.isArray(delta.AddBatch),
  isTableAddBatchEmpty: (
    delta: unknown,
  ): delta is { Discriminator: "TableAddBatchEmpty"; AddBatchEmpty: number } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "TableAddBatchEmpty" &&
    "AddBatchEmpty" in delta &&
    typeof delta.AddBatchEmpty === "number",
  isTableRemoveAt: (
    delta: unknown,
  ): delta is { Discriminator: "TableRemoveAt"; RemoveAt: string } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "TableRemoveAt" &&
    "RemoveAt" in delta &&
    typeof delta.RemoveAt === "string",
  isTableRemoveBatch: (
    delta: unknown,
  ): delta is { Discriminator: "TableRemoveBatch"; RemoveBatch: string[] } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "TableRemoveBatch" &&
    "RemoveBatch" in delta &&
    Array.isArray(delta.RemoveBatch) &&
    delta.RemoveBatch.every((x) => typeof x === "string"),
  isTableRemoveAll: (
    delta: unknown,
  ): delta is { Discriminator: "TableRemoveAll"; RemoveAll: Unit } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "TableRemoveAll" &&
    "RemoveAll" in delta &&
    typeof delta.RemoveAll === "object" &&
    delta.RemoveAll !== null &&
    Object.keys(delta.RemoveAll).length === 0,
  isTableDuplicateAt: (
    delta: unknown,
  ): delta is { Discriminator: "TableDuplicateAt"; DuplicateAt: string } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "TableDuplicateAt" &&
    "DuplicateAt" in delta &&
    typeof delta.DuplicateAt === "string",
  isTableActionOnAll: (
    delta: unknown,
  ): delta is { Discriminator: "TableActionOnAll"; ActionOnAll: string } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "TableActionOnAll" &&
    "ActionOnAll" in delta &&
    typeof delta.ActionOnAll === "string",
  isTableMoveFromTo: (
    delta: unknown,
  ): delta is {
    Discriminator: "TableMoveFromTo";
    MoveFromTo: [string, string];
  } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "TableMoveFromTo" &&
    "MoveFromTo" in delta &&
    Array.isArray(delta.MoveFromTo) &&
    delta.MoveFromTo.length === 2 &&
    typeof delta.MoveFromTo[0] === "string" &&
    typeof delta.MoveFromTo[1] === "string",
  isOneValue: (
    delta: unknown,
  ): delta is { Discriminator: "OneValue"; Value: object } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "OneValue" &&
    "Value" in delta &&
    typeof delta.Value === "object",
  isOneReplace: (
    delta: unknown,
  ): delta is { Discriminator: "OneReplace"; Replace: object } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "OneReplace" &&
    "Replace" in delta &&
    typeof delta.Replace === "object",
  isOneCreateValue: (
    delta: unknown,
  ): delta is { Discriminator: "OneCreateValue"; CreateValue: object } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "OneCreateValue" &&
    "CreateValue" in delta &&
    typeof delta.CreateValue === "object",
  isOneDeleteValue: (
    delta: unknown,
  ): delta is { Discriminator: "OneDeleteValue"; DeleteValue: Unit } =>
    typeof delta === "object" &&
    delta !== null &&
    "Discriminator" in delta &&
    delta.Discriminator === "OneDeleteValue" &&
    "DeleteValue" in delta &&
    typeof delta.DeleteValue === "object" &&
    delta.DeleteValue !== null &&
    Object.keys(delta.DeleteValue).length === 0,
};

export type DispatchDeltaTransferComparand = string;
