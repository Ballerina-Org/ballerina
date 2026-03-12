import { ValueOrErrors } from "../../../libraries/ballerina-core/src/collections/domains/valueOrErrors/state";
import { Map } from "immutable";
import { DispatchDeltaDTOToDelta } from "../../../libraries/ballerina-core/src/forms/domains/dispatched-forms/runner/domains/deltas/dto-to-delta/state";
import { DispatchParsedType } from "../../../libraries/ballerina-core/src/forms/domains/dispatched-forms/deserializer/domains/specification/domains/types/state";
import { dispatchFromAPIRawValue } from "../../../libraries/ballerina-core/src/forms/domains/dispatched-forms/built-ins/state";
import { DispatchFieldTypeConverters } from "../utils/converters";
import { DispatchCategory, DispatchPassthroughFormInjectedTypes } from "utils/category";
import { DispatchDeltaTransferPrimitive, DispatchDeltaTransferRecord, DispatchDeltaTransferTable, DispatchDeltaTransferUnion } from "src/forms/domains/dispatched-forms/runner/domains/deltas/dispatch-delta-dto/state";
import { DispatchDelta } from "ballerina-core";
import { PredicateValue } from "../../../libraries/ballerina-core/src/forms/domains/parser/domains/predicates/state";

describe("DispatchDeltaDTOToDelta", () => {
  it("correctly generates a delta from a NumberReplace DTO", () => {
    const numberType = DispatchParsedType.Default.primitive("number");

    const fromApiRaw = (
      raw: unknown,
    ) => ValueOrErrors.Default.return<number, string>(raw as number);

    const parseCustomDeltaDTO = () => () =>
      ValueOrErrors.Default.throwOne<any, string>("Unexpected custom delta");

    const dto = {
      Discriminator: "NumberReplace" as const,
      Replace: 123,
    };

    const result = DispatchDeltaDTOToDelta(fromApiRaw, parseCustomDeltaDTO)(numberType)(
      dto,
    );

    expect(result.kind).toBe("value");
    if (result.kind !== "value") {
      throw new Error("Expected value result");
    }

    expect(result.value).toMatchObject({
      kind: "NumberReplace",
      replace: 123,
      type: numberType,
      state: undefined,
      flags: undefined,
      sourceAncestorLookupTypeNames: [],
    });
  });

  it("correctly generates a nested delta for StringReplace in RecordField in UnionCase", () => {
    const stringType = DispatchParsedType.Default.primitive("string");
    const personCaseType = DispatchParsedType.Default.record(
      Map([["name", stringType]]),
    );
    const unionType = DispatchParsedType.Default.union(
      Map([["Person", personCaseType]]),
    );



    const fromApiRaw = (raw: unknown, type: DispatchParsedType<DispatchPassthroughFormInjectedTypes>) =>
      dispatchFromAPIRawValue(type, Map(), DispatchFieldTypeConverters)(raw);

    const parseCustomDeltaDTO = () => () =>
      ValueOrErrors.Default.throwOne<any, string>("Unexpected custom delta");

    const StringReplaceDTO: DispatchDeltaTransferPrimitive = {
      Discriminator: "StringReplace",
      Replace: "Alice",
    };

    const RecordFieldDTO: DispatchDeltaTransferRecord<DispatchPassthroughFormInjectedTypes> = {
      Discriminator: "name",
      name: StringReplaceDTO,
    };

    const UnionCaseDTO: DispatchDeltaTransferUnion<DispatchPassthroughFormInjectedTypes> = {
      Discriminator: "Person",
      Person: RecordFieldDTO,
    };

    const result = DispatchDeltaDTOToDelta(fromApiRaw, parseCustomDeltaDTO)(unionType)(
      UnionCaseDTO,
    );

    const expected: DispatchDelta<DispatchPassthroughFormInjectedTypes> = {
      kind: "UnionCase",
      caseName: [
        "Person",
        {
          kind: "RecordField",
          field: [
            "name",
            {
              kind: "StringReplace",
              replace: "Alice",
              type: stringType,
              state: undefined,
              flags: undefined,
              sourceAncestorLookupTypeNames: [],
            },
          ],
          recordType: personCaseType,
          flags: undefined,
          sourceAncestorLookupTypeNames: [],
        },
      ],
      flags: undefined,
      sourceAncestorLookupTypeNames: [],
    };

    expect(result.kind).toBe("value");
    if (result.kind !== "value") {
      throw new Error("Expected value result");
    }

    expect(result.value).toMatchObject(expected);
  });

  it("correctly generates a delta for a custom category type", () => {
    const injectedCategoryType =
      DispatchParsedType.Default.primitive<DispatchPassthroughFormInjectedTypes>(
        "injectedCategory",
      );

    const fromApiRaw = (
      raw: unknown,
      type: DispatchParsedType<DispatchPassthroughFormInjectedTypes>,
    ) => dispatchFromAPIRawValue(type, Map(), DispatchFieldTypeConverters)(raw);

    const parseCustomDeltaDTO =
      (
        fromApiRawForType: (raw: any) => ValueOrErrors<any, string>,
      ) =>
      (customDeltaDTO: { kind: "CategoryReplace"; replace: any }) =>
        fromApiRawForType(customDeltaDTO.replace).Then((replace) =>
          ValueOrErrors.Default.return<any, string>({
            kind: "CategoryReplace",
            replace,
            type: injectedCategoryType,
            state: {
              commonFormState: {},
              customFormState: {
                likelyOutdated: false,
              },
            },
          }),
        );

    const dto = {
      kind: "CategoryReplace" as const,
      replace: {
        kind: "adult" as const,
        extraSpecial: true,
      },
    };

    const result = DispatchDeltaDTOToDelta(fromApiRaw, parseCustomDeltaDTO)(
      injectedCategoryType,
    )(dto as any);

    expect(result.kind).toBe("value");
    if (result.kind !== "value") {
      throw new Error("Expected value result");
    }

    const expectedReplace: DispatchCategory = {
      kind: "custom",
      value: {
        kind: "adult",
        extraSpecial: true,
      },
    };

    expect(result.value).toMatchObject({
      kind: "CustomDelta",
      value: {
        kind: "CategoryReplace",
        replace: expectedReplace,
        type: injectedCategoryType,
      },
      state: undefined,
      flags: undefined,
      sourceAncestorLookupTypeNames: [],
    });
  });

  it("correctly generates a TableAdd delta inside a record field", () => {
    const rowType = DispatchParsedType.Default.record(
      Map([
        ["Id", DispatchParsedType.Default.primitive("string")],
        ["Name", DispatchParsedType.Default.primitive("string")],
      ]),
    );
    const tableType = DispatchParsedType.Default.table(
      DispatchParsedType.Default.lookup("PersonRow"),
    );
    const containerType = DispatchParsedType.Default.record(
      Map([["entries", tableType]]),
    );
    const typesMap = Map([["PersonRow", rowType]]);

    const fromApiRaw = (
      raw: unknown,
      type: DispatchParsedType<DispatchPassthroughFormInjectedTypes>,
    ) => dispatchFromAPIRawValue(type, typesMap as any, DispatchFieldTypeConverters)(raw);

    const parseCustomDeltaDTO = () => () =>
      ValueOrErrors.Default.throwOne<any, string>("Unexpected custom delta");

    const tableAddDTO: Extract<
      DispatchDeltaTransferTable<DispatchPassthroughFormInjectedTypes>,
      { Discriminator: "TableAdd" }
    > = {
      Discriminator: "TableAdd",
      Add: { Id: "row-1", Name: "Alice" },
    };

    const recordFieldDTO: DispatchDeltaTransferRecord<DispatchPassthroughFormInjectedTypes> = {
      Discriminator: "entries",
      entries: tableAddDTO,
    };

    const result = DispatchDeltaDTOToDelta(fromApiRaw, parseCustomDeltaDTO)(containerType)(
      recordFieldDTO,
    );

    expect(result.kind).toBe("value");
    if (result.kind !== "value") {
      throw new Error("Expected value result");
    }

    expect(result.value.kind).toBe("RecordField");
    if (result.value.kind !== "RecordField") {
      throw new Error("Expected RecordField");
    }
    expect(result.value.field[0]).toBe("entries");
    expect(result.value.field[1].kind).toBe("TableAdd");
    if (result.value.field[1].kind !== "TableAdd") {
      throw new Error("Expected nested TableAdd");
    }
    expect(PredicateValue.Operations.IsRecord(result.value.field[1].value)).toBe(true);
    if (!PredicateValue.Operations.IsRecord(result.value.field[1].value)) {
      throw new Error("Expected TableAdd value to be record");
    }
    expect(result.value.field[1].value.fields.get("Id")).toBe("row-1");
    expect(result.value.field[1].value.fields.get("Name")).toBe("Alice");
  });

  it("correctly generates a TableAddBatch delta inside a record field", () => {
    const rowType = DispatchParsedType.Default.record(
      Map([
        ["Id", DispatchParsedType.Default.primitive("string")],
        ["Name", DispatchParsedType.Default.primitive("string")],
      ]),
    );
    const tableType = DispatchParsedType.Default.table(
      DispatchParsedType.Default.lookup("PersonRow"),
    );
    const containerType = DispatchParsedType.Default.record(
      Map([["entries", tableType]]),
    );
    const typesMap = Map([["PersonRow", rowType]]);

    const fromApiRaw = (
      raw: unknown,
      type: DispatchParsedType<DispatchPassthroughFormInjectedTypes>,
    ) => dispatchFromAPIRawValue(type, typesMap as any, DispatchFieldTypeConverters)(raw);

    const parseCustomDeltaDTO = () => () =>
      ValueOrErrors.Default.throwOne<any, string>("Unexpected custom delta");

    const tableAddBatchDTO: Extract<
      DispatchDeltaTransferTable<DispatchPassthroughFormInjectedTypes>,
      { Discriminator: "TableAddBatch" }
    > = {
      Discriminator: "TableAddBatch",
      AddBatch: [
        { Id: "row-1", Name: "Alice" },
        { Id: "row-2", Name: "Bob" },
      ],
    };

    const recordFieldDTO: DispatchDeltaTransferRecord<DispatchPassthroughFormInjectedTypes> = {
      Discriminator: "entries",
      entries: tableAddBatchDTO,
    };

    const result = DispatchDeltaDTOToDelta(fromApiRaw, parseCustomDeltaDTO)(containerType)(
      recordFieldDTO,
    );

    expect(result.kind).toBe("value");
    if (result.kind !== "value") {
      throw new Error("Expected value result");
    }

    expect(result.value.kind).toBe("RecordField");
    if (result.value.kind !== "RecordField") {
      throw new Error("Expected RecordField");
    }
    expect(result.value.field[0]).toBe("entries");
    expect(result.value.field[1].kind).toBe("TableAddBatch");
    if (result.value.field[1].kind !== "TableAddBatch") {
      throw new Error("Expected nested TableAddBatch");
    }
    expect(result.value.field[1].values.size).toBe(2);
    const first = result.value.field[1].values.get(0);
    const second = result.value.field[1].values.get(1);
    expect(PredicateValue.Operations.IsRecord(first as any)).toBe(true);
    expect(PredicateValue.Operations.IsRecord(second as any)).toBe(true);
    if (
      !PredicateValue.Operations.IsRecord(first as any) ||
      !PredicateValue.Operations.IsRecord(second as any)
    ) {
      throw new Error("Expected batch values to be records");
    }
    const firstRecord = first as any;
    const secondRecord = second as any;
    expect(firstRecord.fields.get("Id")).toBe("row-1");
    expect(firstRecord.fields.get("Name")).toBe("Alice");
    expect(secondRecord.fields.get("Id")).toBe("row-2");
    expect(secondRecord.fields.get("Name")).toBe("Bob");
  });
});
