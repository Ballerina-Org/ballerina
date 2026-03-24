import { Map } from "immutable";
import { DispatchParsedType } from "../../../libraries/ballerina-core/src/forms/domains/dispatched-forms/deserializer/domains/specification/domains/types/state";
import {
  DispatchDeltaTransferOperations,
  type DeltaTransfer,
  type DispatchDeltaTransferArrayAdd,
  type DispatchDeltaTransferArrayAddAt,
  type DispatchDeltaTransferArrayDuplicateAt,
  type DispatchDeltaTransferArrayMoveFromTo,
  type DispatchDeltaTransferArrayRemoveAll,
  type DispatchDeltaTransferArrayRemoveAt,
  type DispatchDeltaTransferArrayReplace,
  type DispatchDeltaTransferArrayValue,
  type DispatchDeltaTransferArrayValueAll,
  type DispatchDeltaTransferBoolReplace,
  type DispatchDeltaTransferGuidReplace,
  type DispatchDeltaTransferMapAdd,
  type DispatchDeltaTransferMapKey,
  type DispatchDeltaTransferMapRemove,
  type DispatchDeltaTransferMapReplace,
  type DispatchDeltaTransferMapValue,
  type DispatchDeltaTransferNumberReplace,
  type DispatchDeltaTransferOneCreateValue,
  type DispatchDeltaTransferOneDeleteValue,
  type DispatchDeltaTransferOneReplace,
  type DispatchDeltaTransferOneValue,
  type DispatchDeltaTransferOptionReplace,
  type DispatchDeltaTransferOptionValue,
  type DispatchDeltaTransferRecordField,
  type DispatchDeltaTransferRecordReplace,
  type DispatchDeltaTransferSetAdd,
  type DispatchDeltaTransferSetRemove,
  type DispatchDeltaTransferSetReplace,
  type DispatchDeltaTransferSetValue,
  type DispatchDeltaTransferStringReplace,
  type DispatchDeltaTransferSumLeft,
  type DispatchDeltaTransferSumReplace,
  type DispatchDeltaTransferSumRight,
  type DispatchDeltaTransferTableActionOnAll,
  type DispatchDeltaTransferTableAdd,
  type DispatchDeltaTransferTableAddBatch,
  type DispatchDeltaTransferTableAddBatchEmpty,
  type DispatchDeltaTransferTableAddEmpty,
  type DispatchDeltaTransferTableDuplicateAt,
  type DispatchDeltaTransferTableMoveFromTo,
  type DispatchDeltaTransferTableRemoveAll,
  type DispatchDeltaTransferTableRemoveBatch,
  type DispatchDeltaTransferTableRemoveAt,
  type DispatchDeltaTransferTableValue,
  type DispatchDeltaTransferTableValueAll,
  type DispatchDeltaTransferTimeReplace,
  type DispatchDeltaTransferTupleReplace,
  type DispatchDeltaTransferTupleValue,
  type DispatchDeltaTransferUnionCase,
  type DispatchDeltaTransferUnionReplace,
} from "../../../libraries/ballerina-core/src/forms/domains/dispatched-forms/runner/domains/deltas/dispatch-delta-dto/state";

type Custom = never;

describe("DispatchDeltaTransferOperations", () => {
  it("returns the expected guard result for each typed DTO case", () => {
    const numberReplace: DispatchDeltaTransferNumberReplace = {
      Discriminator: "NumberReplace",
      Replace: 42,
    };
    const stringReplace: DispatchDeltaTransferStringReplace = {
      Discriminator: "StringReplace",
      Replace: "hello",
    };
    const boolReplace: DispatchDeltaTransferBoolReplace = {
      Discriminator: "BoolReplace",
      Replace: true,
    };
    const timeReplace: DispatchDeltaTransferTimeReplace = {
      Discriminator: "TimeReplace",
      Replace: 1710000000000,
    };
    const guidReplace: DispatchDeltaTransferGuidReplace = {
      Discriminator: "GuidReplace",
      Replace: "8f2f2d14-2a43-4cda-a3cb-e8b487dc5c8f",
    };
    const int32Replace: { Discriminator: "Int32Replace"; Replace: bigint } = {
      Discriminator: "Int32Replace",
      Replace: 42n,
    };
    const float32Replace: { Discriminator: "Float32Replace"; Replace: number } =
      {
        Discriminator: "Float32Replace",
        Replace: 42.5,
      };
    const unit: {} = {};

    const optionReplace: DispatchDeltaTransferOptionReplace = {
      Discriminator: "OptionReplace",
      Replace: { some: true },
    };
    const optionValue: DispatchDeltaTransferOptionValue<Custom> = {
      Discriminator: "OptionValue",
      Value: numberReplace,
    };

    const sumReplace: DispatchDeltaTransferSumReplace = {
      Discriminator: "SumReplace",
      Replace: { payload: 1 },
    };
    const sumLeft: DispatchDeltaTransferSumLeft<Custom> = {
      Discriminator: "SumLeft",
      Left: stringReplace,
    };
    const sumRight: DispatchDeltaTransferSumRight<Custom> = {
      Discriminator: "SumRight",
      Right: boolReplace,
    };

    const arrayAdd: DispatchDeltaTransferArrayAdd = {
      Discriminator: "ArrayAdd",
      Add: { value: "x" },
    };
    const arrayReplace: DispatchDeltaTransferArrayReplace = {
      Discriminator: "ArrayReplace",
      Replace: { all: [] },
    };
    const arrayValue: DispatchDeltaTransferArrayValue<Custom> = {
      Discriminator: "ArrayValue",
      Value: { Item1: 1, Item2: stringReplace },
    };
    const arrayValueAll: DispatchDeltaTransferArrayValueAll<Custom> = {
      Discriminator: "ArrayValueAll",
      ValueAll: boolReplace,
    };
    const arrayAddAt: DispatchDeltaTransferArrayAddAt = {
      Discriminator: "ArrayAddAt",
      AddAt: { Item1: 0, Item2: { a: 1 } },
    };
    const arrayRemoveAt: DispatchDeltaTransferArrayRemoveAt = {
      Discriminator: "ArrayRemoveAt",
      RemoveAt: 2,
    };
    const arrayRemoveAll: DispatchDeltaTransferArrayRemoveAll = {
      Discriminator: "ArrayRemoveAll",
      RemoveAll: {},
    };
    const arrayMoveFromTo: DispatchDeltaTransferArrayMoveFromTo = {
      Discriminator: "ArrayMoveFromTo",
      MoveFromTo: { Item1: 0, Item2: 2 },
    };
    const arrayDuplicateAt: DispatchDeltaTransferArrayDuplicateAt = {
      Discriminator: "ArrayDuplicateAt",
      DuplicateAt: 3,
    };

    const setReplace: DispatchDeltaTransferSetReplace = {
      Discriminator: "SetReplace",
      Replace: { values: [] },
    };
    const setValue: DispatchDeltaTransferSetValue<Custom> = {
      Discriminator: "SetValue",
      Value: { Item1: "k", Item2: numberReplace },
    };
    const setAdd: DispatchDeltaTransferSetAdd = {
      Discriminator: "SetAdd",
      Add: { value: 1 },
    };
    const setRemove: DispatchDeltaTransferSetRemove = {
      Discriminator: "SetRemove",
      Remove: { value: 1 },
    };

    const mapReplace: DispatchDeltaTransferMapReplace = {
      Discriminator: "MapReplace",
      Replace: { entries: [] },
    };
    const mapValue: DispatchDeltaTransferMapValue<Custom> = {
      Discriminator: "MapValue",
      Value: { Item1: 1, Item2: stringReplace },
    };
    const mapKey: DispatchDeltaTransferMapKey<Custom> = {
      Discriminator: "MapKey",
      Key: { Item1: 1, Item2: numberReplace },
    };
    const mapAdd: DispatchDeltaTransferMapAdd = {
      Discriminator: "MapAdd",
      Add: { Item1: "k", Item2: { v: 1 } },
    };
    const mapRemove: DispatchDeltaTransferMapRemove = {
      Discriminator: "MapRemove",
      Remove: 1,
    };

    const recordReplace: DispatchDeltaTransferRecordReplace = {
      Discriminator: "AnyRecordReplace",
      Replace: { name: "Alice" },
    };
    const recordField: DispatchDeltaTransferRecordField<Custom> = {
      Discriminator: "name",
      name: stringReplace,
    };

    const unionReplace: DispatchDeltaTransferUnionReplace = {
      Discriminator: "UnionReplace",
      Replace: { kind: "A" },
    };
    const unionCase: DispatchDeltaTransferUnionCase<Custom> = {
      Discriminator: "CaseA",
      CaseA: recordField,
    };

    const tupleReplace: DispatchDeltaTransferTupleReplace = {
      Discriminator: "TupleReplace",
      Replace: { item1: "a" },
    };
    const tupleValue: DispatchDeltaTransferTupleValue<Custom> = {
      Discriminator: "Tuple2Item1",
      Item1: stringReplace,
    };

    const tableValue: DispatchDeltaTransferTableValue<Custom> = {
      Discriminator: "TableValue",
      Value: { Item1: "row-1", Item2: recordField },
    };
    const tableValueAll: DispatchDeltaTransferTableValueAll<Custom> = {
      Discriminator: "TableValueAll",
      ValueAll: recordField,
    };
    const tableAddEmpty: DispatchDeltaTransferTableAddEmpty = {
      Discriminator: "TableAddEmpty",
    };
    const tableAdd: DispatchDeltaTransferTableAdd = {
      Discriminator: "TableAdd",
      Add: { id: "row-2" },
    };
    const tableAddBatch: DispatchDeltaTransferTableAddBatch = {
      Discriminator: "TableAddBatch",
      AddBatch: [{ id: "row-3" }],
    };
    const tableAddBatchEmpty: DispatchDeltaTransferTableAddBatchEmpty = {
      Discriminator: "TableAddBatchEmpty",
      AddBatchEmpty: 2,
    };
    const tableRemoveAt: DispatchDeltaTransferTableRemoveAt = {
      Discriminator: "TableRemoveAt",
      RemoveAt: "row-1",
    };
    const tableRemoveBatch: DispatchDeltaTransferTableRemoveBatch = {
      Discriminator: "TableRemoveBatch",
      RemoveBatch: ["row-1", "row-2"],
    };
    const tableRemoveAll: DispatchDeltaTransferTableRemoveAll = {
      Discriminator: "TableRemoveAll",
      RemoveAll: {},
    };
    const tableDuplicateAt: DispatchDeltaTransferTableDuplicateAt = {
      Discriminator: "TableDuplicateAt",
      DuplicateAt: "row-1",
    };
    const tableActionOnAll: DispatchDeltaTransferTableActionOnAll = {
      Discriminator: "TableActionOnAll",
      ActionOnAll: "archive",
    };
    const tableMoveFromTo: DispatchDeltaTransferTableMoveFromTo = {
      Discriminator: "TableMoveFromTo",
      MoveFromTo: ["row-1", "row-2"],
    };

    const oneValue: DispatchDeltaTransferOneValue<Custom> = {
      Discriminator: "OneValue",
      Value: recordField,
    };
    const oneReplace: DispatchDeltaTransferOneReplace = {
      Discriminator: "OneReplace",
      Replace: { id: "one-1" },
    };
    const oneCreateValue: DispatchDeltaTransferOneCreateValue = {
      Discriminator: "OneCreateValue",
      CreateValue: { id: "one-2" },
    };
    const oneDeleteValue: DispatchDeltaTransferOneDeleteValue = {
      Discriminator: "OneDeleteValue",
      DeleteValue: {},
    };

    const recordType = DispatchParsedType.Default.record(
      Map([["name", DispatchParsedType.Default.primitive("string")]]),
    );
    const unionType = DispatchParsedType.Default.union(
      Map([["CaseA", recordType]]),
    );
    const tupleType = DispatchParsedType.Default.tuple([
      DispatchParsedType.Default.primitive("string"),
    ]);

    expect(DispatchDeltaTransferOperations.isNumberReplace(numberReplace)).toBe(
      true,
    );
    expect(DispatchDeltaTransferOperations.isStringReplace(stringReplace)).toBe(
      true,
    );
    expect(DispatchDeltaTransferOperations.isBoolReplace(boolReplace)).toBe(
      true,
    );
    expect(DispatchDeltaTransferOperations.isTimeReplace(timeReplace)).toBe(
      true,
    );
    expect(DispatchDeltaTransferOperations.isGuidReplace(guidReplace)).toBe(
      true,
    );
    expect(DispatchDeltaTransferOperations.isInt32Replace(int32Replace)).toBe(
      true,
    );
    expect(
      DispatchDeltaTransferOperations.isFloat32Replace(float32Replace),
    ).toBe(true);
    expect(DispatchDeltaTransferOperations.isUnit(unit)).toBe(true);
    expect(DispatchDeltaTransferOperations.isOptionReplace(optionReplace)).toBe(
      true,
    );
    expect(DispatchDeltaTransferOperations.isOptionValue(optionValue)).toBe(
      true,
    );
    expect(DispatchDeltaTransferOperations.isSumReplace(sumReplace)).toBe(true);
    expect(DispatchDeltaTransferOperations.isSumLeft(sumLeft)).toBe(true);
    expect(DispatchDeltaTransferOperations.isSumRight(sumRight)).toBe(true);
    expect(DispatchDeltaTransferOperations.isArrayAdd(arrayAdd)).toBe(true);
    expect(DispatchDeltaTransferOperations.isArrayReplace(arrayReplace)).toBe(
      true,
    );
    expect(DispatchDeltaTransferOperations.isArrayValue(arrayValue)).toBe(true);
    expect(DispatchDeltaTransferOperations.isArrayValueAll(arrayValueAll)).toBe(
      true,
    );
    expect(DispatchDeltaTransferOperations.isArrayAddAt(arrayAddAt)).toBe(true);
    expect(DispatchDeltaTransferOperations.isArrayRemoveAt(arrayRemoveAt)).toBe(
      true,
    );
    expect(
      DispatchDeltaTransferOperations.isArrayRemoveAll(arrayRemoveAll),
    ).toBe(true);
    expect(
      DispatchDeltaTransferOperations.isArrayMoveFromTo(arrayMoveFromTo),
    ).toBe(true);
    expect(
      DispatchDeltaTransferOperations.isArrayDuplicateAt(arrayDuplicateAt),
    ).toBe(true);
    expect(DispatchDeltaTransferOperations.isSetReplace(setReplace)).toBe(true);
    expect(DispatchDeltaTransferOperations.isSetValue(setValue)).toBe(true);
    expect(DispatchDeltaTransferOperations.isSetAdd(setAdd)).toBe(true);
    expect(DispatchDeltaTransferOperations.isSetRemove(setRemove)).toBe(true);
    expect(DispatchDeltaTransferOperations.isMapReplace(mapReplace)).toBe(true);
    expect(DispatchDeltaTransferOperations.isMapValue(mapValue)).toBe(true);
    expect(DispatchDeltaTransferOperations.isMapKey(mapKey)).toBe(true);
    expect(DispatchDeltaTransferOperations.isMapAdd(mapAdd)).toBe(true);
    expect(DispatchDeltaTransferOperations.isMapRemove(mapRemove)).toBe(true);
    expect(
      DispatchDeltaTransferOperations.isRecordReplace(
        recordReplace,
        recordType,
      ),
    ).toBe(true);
    expect(
      DispatchDeltaTransferOperations.isRecordField(recordField, recordType),
    ).toBe(true);
    expect(DispatchDeltaTransferOperations.isUnionReplace(unionReplace)).toBe(
      true,
    );
    expect(
      DispatchDeltaTransferOperations.isUnionCase(unionCase, unionType),
    ).toBe(true);
    expect(DispatchDeltaTransferOperations.isTupleReplace(tupleReplace)).toBe(
      true,
    );
    expect(
      DispatchDeltaTransferOperations.isTupleValue(tupleValue, tupleType),
    ).toBe(true);
    expect(DispatchDeltaTransferOperations.isTableValue(tableValue)).toBe(true);
    expect(DispatchDeltaTransferOperations.isTableValueAll(tableValueAll)).toBe(
      true,
    );
    expect(DispatchDeltaTransferOperations.isTableAddEmpty(tableAddEmpty)).toBe(
      true,
    );
    expect(DispatchDeltaTransferOperations.isTableAdd(tableAdd)).toBe(true);
    expect(DispatchDeltaTransferOperations.isTableAddBatch(tableAddBatch)).toBe(
      true,
    );
    expect(
      DispatchDeltaTransferOperations.isTableAddBatchEmpty(tableAddBatchEmpty),
    ).toBe(true);
    expect(DispatchDeltaTransferOperations.isTableRemoveAt(tableRemoveAt)).toBe(
      true,
    );
    expect(
      DispatchDeltaTransferOperations.isTableRemoveBatch(tableRemoveBatch),
    ).toBe(true);
    expect(
      DispatchDeltaTransferOperations.isTableRemoveAll(tableRemoveAll),
    ).toBe(true);
    expect(
      DispatchDeltaTransferOperations.isTableDuplicateAt(tableDuplicateAt),
    ).toBe(true);
    expect(
      DispatchDeltaTransferOperations.isTableActionOnAll(tableActionOnAll),
    ).toBe(true);
    expect(
      DispatchDeltaTransferOperations.isTableMoveFromTo(tableMoveFromTo),
    ).toBe(true);
    expect(DispatchDeltaTransferOperations.isOneValue(oneValue)).toBe(true);
    expect(DispatchDeltaTransferOperations.isOneReplace(oneReplace)).toBe(true);
    expect(
      DispatchDeltaTransferOperations.isOneCreateValue(oneCreateValue),
    ).toBe(true);
    expect(
      DispatchDeltaTransferOperations.isOneDeleteValue(oneDeleteValue),
    ).toBe(true);
  });
});
