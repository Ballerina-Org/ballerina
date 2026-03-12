import { ValueOrErrors } from "../../../libraries/ballerina-core/src/collections/domains/valueOrErrors/state";
import { DispatchDeltaTransfer } from "../../../libraries/ballerina-core/src/forms/domains/dispatched-forms/runner/domains/deltas/state";
import { type DispatchDelta } from "../../../libraries/ballerina-core/src/forms/domains/dispatched-forms/runner/domains/deltas/dispatch-delta/state";
import { DispatchParsedType } from "../../../libraries/ballerina-core/src/forms/domains/dispatched-forms/deserializer/domains/specification/domains/types/state";
import {
  PredicateValue,
  type PredicateValue as PredicateValueType,
} from "../../../libraries/ballerina-core/src/forms/domains/parser/domains/predicates/state";
import { Updater } from "../../../libraries/ballerina-core/src/fun/domains/updater/state";
import { Sum } from "ballerina-core";
import { List, Map, OrderedMap } from "immutable";
import { DispatchCategory } from "../utils/category";

describe("DispatchDeltaTransfer.Default.ToUpdater", () => {
  it("updates a simple predicate value using a matching delta", () => {
    const initialValue: PredicateValueType = "old value";

    const delta: DispatchDelta = {
      kind: "StringReplace",
      replace: "new value",
      state: undefined,
      type: {
        kind: "primitive",
        name: "string",
      },
      flags: undefined,
      sourceAncestorLookupTypeNames: [],
    };

    const updaterResult = DispatchDeltaTransfer.Default.ToUpdater(() =>
      ValueOrErrors.Default.throwOne("Unexpected custom delta"),
    )(delta);

    expect(updaterResult.kind).toBe("value");
    if (updaterResult.kind !== "value") {
      throw new Error("Expected updater result to be a value");
    }

    const updatedValue = updaterResult.value(initialValue);
    expect(updatedValue).toBe("new value");
  });

  it("updates a nested predicate value for StringReplace in RecordField in UnionCase", () => {
    const stringType = DispatchParsedType.Default.primitive("string");
    const personCaseType = DispatchParsedType.Default.record(
      Map([["name", stringType]]),
    );

    const initialValue = PredicateValue.Default.unionCase(
      "Person",
      PredicateValue.Default.record(OrderedMap([["name", "Bob"]])),
    );

    const delta: DispatchDelta = {
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
              state: undefined,
              type: stringType,
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

    const updaterResult = DispatchDeltaTransfer.Default.ToUpdater(() =>
      ValueOrErrors.Default.throwOne("Unexpected custom delta"),
    )(delta);

    expect(updaterResult.kind).toBe("value");
    if (updaterResult.kind !== "value") {
      throw new Error("Expected updater result to be a value");
    }

    const updatedValue = updaterResult.value(initialValue);
    expect(updatedValue).toMatchObject(
      PredicateValue.Default.unionCase(
        "Person",
        PredicateValue.Default.record(OrderedMap([["name", "Alice"]])),
      ),
    );
  });

  it("updates a custom category predicate value through CustomDelta", () => {
    const injectedCategoryType =
      DispatchParsedType.Default.primitive("injectedCategory");
    const initialValue: DispatchCategory = {
      kind: "custom",
      value: {
        kind: "child",
        extraSpecial: false,
      },
    };
    const updatedCategory: DispatchCategory = {
      kind: "custom",
      value: {
        kind: "adult",
        extraSpecial: true,
      },
    };

    const delta: DispatchDelta = {
      kind: "CustomDelta",
      value: {
        kind: "CategoryReplace",
        replace: updatedCategory,
        type: injectedCategoryType,
        state: {
          commonFormState: {},
          customFormState: {
            likelyOutdated: false,
          },
        },
      },
      flags: undefined,
      sourceAncestorLookupTypeNames: [],
    };

    const updaterResult = DispatchDeltaTransfer.Default.ToUpdater(
      (customDelta) => {
        if (customDelta.value.kind !== "CategoryReplace") {
          return ValueOrErrors.Default.throwOne("Unsupported custom delta");
        }
        return ValueOrErrors.Default.return(
          Updater(() => customDelta.value.replace),
        );
      },
    )(delta);

    expect(updaterResult.kind).toBe("value");
    if (updaterResult.kind !== "value") {
      throw new Error("Expected updater result to be a value");
    }

    const updatedValue = updaterResult.value(initialValue);
    expect(updatedValue).toMatchObject(updatedCategory);
  });

  it("does not update when SumRight delta is applied to a left-valued sum", () => {
    const consoleErrorSpy = jest
      .spyOn(console, "error")
      .mockImplementation(() => undefined);

    const initialValue = PredicateValue.Default.sum(
      Sum.Default.left<PredicateValueType, PredicateValueType>("left-value"),
    );

    const delta: DispatchDelta = {
      kind: "SumRight",
      value: {
        kind: "StringReplace",
        replace: "updated-right-value",
        state: undefined,
        type: DispatchParsedType.Default.primitive("string"),
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      },
      flags: undefined,
      sourceAncestorLookupTypeNames: [],
    };

    const updaterResult = DispatchDeltaTransfer.Default.ToUpdater(() =>
      ValueOrErrors.Default.throwOne("Unexpected custom delta"),
    )(delta);

    expect(updaterResult.kind).toBe("value");
    if (updaterResult.kind !== "value") {
      throw new Error("Expected updater result to be a value");
    }

    const updatedValue = updaterResult.value(initialValue);
    expect(updatedValue).toEqual(initialValue);
    expect(consoleErrorSpy).toHaveBeenCalled();
    expect(consoleErrorSpy.mock.calls[0]?.join(" ")).toContain(
      "SumRight: current sum is left-valued",
    );
    consoleErrorSpy.mockRestore();
  });

  it("updates a table value using TableAdd inside a RecordField delta", () => {
    const row = PredicateValue.Default.record(
      OrderedMap([
        ["Id", "row-1"],
        ["Name", "Alice"],
      ]),
    );
    const initialValue = PredicateValue.Default.record(
      OrderedMap([
        [
          "entries",
          PredicateValue.Default.table(
            0,
            0,
            OrderedMap(),
            false,
            PredicateValue.Default.record(OrderedMap()),
          ),
        ],
      ]),
    );
    const tableType = DispatchParsedType.Default.table(
      DispatchParsedType.Default.lookup("PersonRow"),
    );
    const containerType = DispatchParsedType.Default.record(
      Map([["entries", tableType]]),
    );

    const delta: DispatchDelta = {
      kind: "RecordField",
      field: [
        "entries",
        {
          kind: "TableAdd",
          value: row,
          flags: undefined,
          sourceAncestorLookupTypeNames: [],
        },
      ],
      recordType: containerType,
      flags: undefined,
      sourceAncestorLookupTypeNames: [],
    };

    const updaterResult = DispatchDeltaTransfer.Default.ToUpdater(() =>
      ValueOrErrors.Default.throwOne("Unexpected custom delta"),
    )(delta);

    expect(updaterResult.kind).toBe("value");
    if (updaterResult.kind !== "value") {
      throw new Error("Expected updater result to be a value");
    }

    const updatedValue = updaterResult.value(initialValue);
    expect(PredicateValue.Operations.IsRecord(updatedValue)).toBe(true);
    if (!PredicateValue.Operations.IsRecord(updatedValue)) {
      throw new Error("Expected updated value to be a record");
    }
    const updatedTableValue = updatedValue.fields.get("entries");
    expect(PredicateValue.Operations.IsTable(updatedTableValue as any)).toBe(
      true,
    );
    if (!PredicateValue.Operations.IsTable(updatedTableValue as any)) {
      throw new Error("Expected entries to be a table");
    }
    const updatedTable = updatedTableValue as any;
    expect(updatedTable.data.has("row-1")).toBe(true);
    expect(updatedTable.data.get("row-1")?.fields.get("Name")).toBe("Alice");
  });

  it("updates a table value using TableAddBatch inside a RecordField delta", () => {
    const row1 = PredicateValue.Default.record(
      OrderedMap([
        ["Id", "row-1"],
        ["Name", "Alice"],
      ]),
    );
    const row2 = PredicateValue.Default.record(
      OrderedMap([
        ["Id", "row-2"],
        ["Name", "Bob"],
      ]),
    );
    const initialValue = PredicateValue.Default.record(
      OrderedMap([
        [
          "entries",
          PredicateValue.Default.table(
            0,
            0,
            OrderedMap(),
            false,
            PredicateValue.Default.record(OrderedMap()),
          ),
        ],
      ]),
    );
    const tableType = DispatchParsedType.Default.table(
      DispatchParsedType.Default.lookup("PersonRow"),
    );
    const containerType = DispatchParsedType.Default.record(
      Map([["entries", tableType]]),
    );

    const delta: DispatchDelta = {
      kind: "RecordField",
      field: [
        "entries",
        {
          kind: "TableAddBatch",
          values: List([row1, row2]),
          flags: undefined,
          sourceAncestorLookupTypeNames: [],
        },
      ],
      recordType: containerType,
      flags: undefined,
      sourceAncestorLookupTypeNames: [],
    };

    const updaterResult = DispatchDeltaTransfer.Default.ToUpdater(() =>
      ValueOrErrors.Default.throwOne("Unexpected custom delta"),
    )(delta);

    expect(updaterResult.kind).toBe("value");
    if (updaterResult.kind !== "value") {
      throw new Error("Expected updater result to be a value");
    }

    const updatedValue = updaterResult.value(initialValue);
    expect(PredicateValue.Operations.IsRecord(updatedValue)).toBe(true);
    if (!PredicateValue.Operations.IsRecord(updatedValue)) {
      throw new Error("Expected updated value to be a record");
    }
    const updatedTableValue = updatedValue.fields.get("entries");
    expect(PredicateValue.Operations.IsTable(updatedTableValue as any)).toBe(
      true,
    );
    if (!PredicateValue.Operations.IsTable(updatedTableValue as any)) {
      throw new Error("Expected entries to be a table");
    }
    const updatedTable = updatedTableValue as any;
    expect(updatedTable.data.has("row-1")).toBe(true);
    expect(updatedTable.data.has("row-2")).toBe(true);
    expect(updatedTable.data.get("row-1")?.fields.get("Name")).toBe("Alice");
    expect(updatedTable.data.get("row-2")?.fields.get("Name")).toBe("Bob");
  });

  describe("base branch coverage", () => {
    const toUpdater = (
      delta: DispatchDelta,
      parseCustomDelta: Parameters<typeof DispatchDeltaTransfer.Default.ToUpdater>[0] = () =>
        ValueOrErrors.Default.throwOne("Unexpected custom delta"),
    ) => DispatchDeltaTransfer.Default.ToUpdater(parseCustomDelta)(delta);

    const expectIdentityWithConsoleError = (
      delta: DispatchDelta,
      current: PredicateValueType,
      expectedMessage?: string,
    ) => {
      const consoleErrorSpy = jest
        .spyOn(console, "error")
        .mockImplementation(() => undefined);
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") {
        throw new Error("Expected updater result to be a value");
      }
      const updated = updaterResult.value(current);
      expect(updated).toEqual(current);
      expect(consoleErrorSpy).toHaveBeenCalled();
      if (expectedMessage) {
        expect(consoleErrorSpy.mock.calls[0]?.join(" ")).toContain(expectedMessage);
      }
      consoleErrorSpy.mockRestore();
    };

    it("NumberReplace is strict (positive + mismatch fallback)", () => {
      const delta: DispatchDelta = {
        kind: "NumberReplace",
        replace: 42,
        state: undefined,
        type: DispatchParsedType.Default.primitive("number"),
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") {
        throw new Error("Expected updater result to be a value");
      }
      expect(updaterResult.value(0)).toBe(42);
      expectIdentityWithConsoleError(
        delta,
        PredicateValue.Default.record(OrderedMap()),
        "Delta NumberReplace expects current to be number",
      );
    });

    it("StringReplace is strict (positive + mismatch fallback)", () => {
      const delta: DispatchDelta = {
        kind: "StringReplace",
        replace: "updated",
        state: undefined,
        type: DispatchParsedType.Default.primitive("string"),
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") {
        throw new Error("Expected updater result to be a value");
      }
      expect(updaterResult.value("before")).toBe("updated");
      expectIdentityWithConsoleError(
        delta,
        PredicateValue.Default.record(OrderedMap()),
        "Delta StringReplace expects current to be string",
      );
    });

    it("BoolReplace is strict (positive + mismatch fallback)", () => {
      const delta: DispatchDelta = {
        kind: "BoolReplace",
        replace: true,
        state: undefined,
        type: DispatchParsedType.Default.primitive("string"),
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") {
        throw new Error("Expected updater result to be a value");
      }
      expect(updaterResult.value(false)).toBe(true);
      expectIdentityWithConsoleError(
        delta,
        PredicateValue.Default.record(OrderedMap()),
        "Delta BoolReplace expects current to be boolean",
      );
    });

    it("TimeReplace is strict (positive + mismatch fallback)", () => {
      const delta: DispatchDelta = {
        kind: "TimeReplace",
        replace: "2026-01-01T00:00:00.000Z",
        state: undefined,
        type: DispatchParsedType.Default.primitive("string"),
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") {
        throw new Error("Expected updater result to be a value");
      }
      expect(updaterResult.value(new Date("2025-01-01T00:00:00.000Z"))).toBe(
        "2026-01-01T00:00:00.000Z",
      );
      expectIdentityWithConsoleError(
        delta,
        0,
        "Delta TimeReplace expects current to be date",
      );
    });

    it("GuidReplace is strict (positive + mismatch fallback)", () => {
      const delta: DispatchDelta = {
        kind: "GuidReplace",
        replace: "8f2f2d14-2a43-4cda-a3cb-e8b487dc5c8f",
        state: undefined,
        type: DispatchParsedType.Default.primitive("string"),
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") {
        throw new Error("Expected updater result to be a value");
      }
      expect(updaterResult.value("00000000-0000-0000-0000-000000000000")).toBe(
        "8f2f2d14-2a43-4cda-a3cb-e8b487dc5c8f",
      );
      expectIdentityWithConsoleError(
        delta,
        PredicateValue.Default.record(OrderedMap()),
        "Delta GuidReplace expects current to be string",
      );
    });

    it("handles SumLeft positive and negative", () => {
      const delta: DispatchDelta = {
        kind: "SumLeft",
        value: {
          kind: "StringReplace",
          replace: "left-updated",
          state: undefined,
          type: DispatchParsedType.Default.primitive("string"),
          flags: undefined,
          sourceAncestorLookupTypeNames: [],
        },
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") {
        throw new Error("Expected updater result to be a value");
      }
      const positive = updaterResult.value(
        PredicateValue.Default.sum(
          Sum.Default.left<PredicateValueType, PredicateValueType>("left"),
        ),
      );
      expect(positive).toEqual(
        PredicateValue.Default.sum(
          Sum.Default.left<PredicateValueType, PredicateValueType>("left-updated"),
        ),
      );

      expectIdentityWithConsoleError(
        delta,
        PredicateValue.Default.sum(
          Sum.Default.right<PredicateValueType, PredicateValueType>("right"),
        ),
        "SumLeft: current sum is right-valued",
      );
    });

    it("handles SumRight positive and negative", () => {
      const delta: DispatchDelta = {
        kind: "SumRight",
        value: {
          kind: "StringReplace",
          replace: "right-updated",
          state: undefined,
          type: DispatchParsedType.Default.primitive("string"),
          flags: undefined,
          sourceAncestorLookupTypeNames: [],
        },
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") {
        throw new Error("Expected updater result to be a value");
      }
      const positive = updaterResult.value(
        PredicateValue.Default.sum(
          Sum.Default.right<PredicateValueType, PredicateValueType>("right"),
        ),
      );
      expect(positive).toEqual(
        PredicateValue.Default.sum(
          Sum.Default.right<PredicateValueType, PredicateValueType>(
            "right-updated",
          ),
        ),
      );

      expectIdentityWithConsoleError(
        delta,
        PredicateValue.Default.sum(
          Sum.Default.left<PredicateValueType, PredicateValueType>("left"),
        ),
        "SumRight: current sum is left-valued",
      );
    });

    it("handles ArrayValue positive and negative", () => {
      const delta: DispatchDelta = {
        kind: "ArrayValue",
        value: [
          1,
          {
            kind: "StringReplace",
            replace: "updated",
            state: undefined,
            type: DispatchParsedType.Default.primitive("string"),
            flags: undefined,
            sourceAncestorLookupTypeNames: [],
          },
        ],
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") throw new Error("Expected value");
      const positive = updaterResult.value(
        PredicateValue.Default.tuple(List(["a", "b"])),
      );
      expect(positive).toEqual(PredicateValue.Default.tuple(List(["a", "updated"])));

      expectIdentityWithConsoleError(
        delta,
        PredicateValue.Default.record(OrderedMap([["x", "y"]])),
      );
    });

    it("handles ArrayAdd positive and negative", () => {
      const delta: DispatchDelta = {
        kind: "ArrayAdd",
        value: "b",
        state: undefined,
        type: DispatchParsedType.Default.primitive("string"),
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") throw new Error("Expected value");
      expect(updaterResult.value(PredicateValue.Default.tuple(List(["a"])))).toEqual(
        PredicateValue.Default.tuple(List(["a", "b"])),
      );
      expectIdentityWithConsoleError(
        delta,
        PredicateValue.Default.record(OrderedMap()),
      );
    });

    it("handles ArrayAddAt positive and negative", () => {
      const delta: DispatchDelta = {
        kind: "ArrayAddAt",
        value: [0, "b"],
        elementState: undefined,
        elementType: DispatchParsedType.Default.primitive("string"),
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") throw new Error("Expected value");
      expect(updaterResult.value(PredicateValue.Default.tuple(List(["a"])))).toEqual(
        PredicateValue.Default.tuple(List(["b", "a"])),
      );
      expectIdentityWithConsoleError(
        delta,
        PredicateValue.Default.record(OrderedMap()),
      );
    });

    it("handles ArrayRemoveAt positive and negative", () => {
      const delta: DispatchDelta = {
        kind: "ArrayRemoveAt",
        index: 0,
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") throw new Error("Expected value");
      expect(
        updaterResult.value(PredicateValue.Default.tuple(List(["a", "b"]))),
      ).toEqual(PredicateValue.Default.tuple(List(["b"])));
      expectIdentityWithConsoleError(
        delta,
        PredicateValue.Default.record(OrderedMap()),
      );
    });

    it("handles ArrayRemoveAll positive and negative", () => {
      const delta: DispatchDelta = {
        kind: "ArrayRemoveAll",
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") throw new Error("Expected value");
      const cleared = updaterResult.value(
        PredicateValue.Default.tuple(List(["a", "b"])),
      );
      expect(PredicateValue.Operations.IsTuple(cleared)).toBe(true);
      if (!PredicateValue.Operations.IsTuple(cleared)) {
        throw new Error("Expected cleared tuple value");
      }
      expect(cleared.values.size).toBe(0);
      expectIdentityWithConsoleError(
        delta,
        PredicateValue.Default.record(OrderedMap()),
      );
    });

    it("handles RecordField positive and negative", () => {
      const delta: DispatchDelta = {
        kind: "RecordField",
        field: [
          "name",
          {
            kind: "StringReplace",
            replace: "Alice",
            state: undefined,
            type: DispatchParsedType.Default.primitive("string"),
            flags: undefined,
            sourceAncestorLookupTypeNames: [],
          },
        ],
        recordType: DispatchParsedType.Default.record(
          Map([["name", DispatchParsedType.Default.primitive("string")]]),
        ),
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") throw new Error("Expected value");
      expect(
        updaterResult.value(
          PredicateValue.Default.record(OrderedMap([["name", "Bob"]])),
        ),
      ).toEqual(PredicateValue.Default.record(OrderedMap([["name", "Alice"]])));

      expectIdentityWithConsoleError(
        delta,
        PredicateValue.Default.record(OrderedMap()),
        "RecordField: field name does not exist",
      );
    });

    it("handles UnionCase positive and negative", () => {
      const delta: DispatchDelta = {
        kind: "UnionCase",
        caseName: [
          "Person",
          {
            kind: "StringReplace",
            replace: "Alice",
            state: undefined,
            type: DispatchParsedType.Default.primitive("string"),
            flags: undefined,
            sourceAncestorLookupTypeNames: [],
          },
        ],
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") throw new Error("Expected value");
      expect(
        updaterResult.value(PredicateValue.Default.unionCase("Person", "Bob")),
      ).toEqual(PredicateValue.Default.unionCase("Person", "Alice"));

      expectIdentityWithConsoleError(
        delta,
        PredicateValue.Default.record(OrderedMap([["x", "y"]])),
      );
    });

    it("handles TupleCase positive and negative", () => {
      const delta: DispatchDelta = {
        kind: "TupleCase",
        item: [
          1,
          {
            kind: "StringReplace",
            replace: "c",
            state: undefined,
            type: DispatchParsedType.Default.primitive("string"),
            flags: undefined,
            sourceAncestorLookupTypeNames: [],
          },
        ],
        tupleType: DispatchParsedType.Default.tuple([
          DispatchParsedType.Default.primitive("string"),
          DispatchParsedType.Default.primitive("string"),
        ]),
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") throw new Error("Expected value");
      expect(
        updaterResult.value(PredicateValue.Default.tuple(List(["a", "b"]))),
      ).toEqual(PredicateValue.Default.tuple(List(["a", "c"])));

      expectIdentityWithConsoleError(
        delta,
        PredicateValue.Default.tuple(List(["a"])),
        "TupleCase: item at index 1 does not exist",
      );
    });

    it("handles TableValue positive and negative", () => {
      const initialTable = PredicateValue.Default.table(
        0,
        0,
        OrderedMap([
          [
            "row-1",
            PredicateValue.Default.record(OrderedMap([["Name", "Bob"]])),
          ],
        ]),
        false,
        PredicateValue.Default.record(OrderedMap()),
      );
      const delta: DispatchDelta = {
        kind: "TableValue",
        id: "row-1",
        nestedDelta: {
          kind: "RecordField",
          field: [
            "Name",
            {
              kind: "StringReplace",
              replace: "Alice",
              state: undefined,
              type: DispatchParsedType.Default.primitive("string"),
              flags: undefined,
              sourceAncestorLookupTypeNames: [],
            },
          ],
          recordType: DispatchParsedType.Default.record(
            Map([["Name", DispatchParsedType.Default.primitive("string")]]),
          ),
          flags: undefined,
          sourceAncestorLookupTypeNames: [],
        },
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") throw new Error("Expected value");
      const positive = updaterResult.value(initialTable);
      expect(PredicateValue.Operations.IsTable(positive)).toBe(true);
      if (!PredicateValue.Operations.IsTable(positive)) {
        throw new Error("Expected table");
      }
      expect(positive.data.get("row-1")?.fields.get("Name")).toBe("Alice");

      expectIdentityWithConsoleError(
        { ...delta, id: "row-missing" },
        initialTable,
        "TableValue: row row-missing does not exist",
      );
    });

    it("handles TableAdd positive and negative", () => {
      const table = PredicateValue.Default.table(
        0,
        0,
        OrderedMap(),
        false,
        PredicateValue.Default.record(OrderedMap()),
      );
      const row = PredicateValue.Default.record(
        OrderedMap([
          ["Id", "row-1"],
          ["Name", "Alice"],
        ]),
      );
      const delta: DispatchDelta = {
        kind: "TableAdd",
        value: row,
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") throw new Error("Expected value");
      const positive = updaterResult.value(table);
      expect(PredicateValue.Operations.IsTable(positive)).toBe(true);
      if (!PredicateValue.Operations.IsTable(positive)) {
        throw new Error("Expected table");
      }
      expect(positive.data.has("row-1")).toBe(true);

      expectIdentityWithConsoleError(
        delta,
        PredicateValue.Default.record(OrderedMap()),
      );
    });

    it("handles TableAddBatch positive and negative", () => {
      const table = PredicateValue.Default.table(
        0,
        0,
        OrderedMap(),
        false,
        PredicateValue.Default.record(OrderedMap()),
      );
      const row1 = PredicateValue.Default.record(
        OrderedMap([
          ["Id", "row-1"],
          ["Name", "Alice"],
        ]),
      );
      const row2 = PredicateValue.Default.record(
        OrderedMap([
          ["Id", "row-2"],
          ["Name", "Bob"],
        ]),
      );
      const delta: DispatchDelta = {
        kind: "TableAddBatch",
        values: List([row1, row2]),
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      const updaterResult = toUpdater(delta);
      expect(updaterResult.kind).toBe("value");
      if (updaterResult.kind !== "value") throw new Error("Expected value");
      const positive = updaterResult.value(table);
      expect(PredicateValue.Operations.IsTable(positive)).toBe(true);
      if (!PredicateValue.Operations.IsTable(positive)) {
        throw new Error("Expected table");
      }
      expect(positive.data.has("row-1")).toBe(true);
      expect(positive.data.has("row-2")).toBe(true);

      const missingIdRow = PredicateValue.Default.record(
        OrderedMap([["Name", "No Id"]]),
      );
      const invalidDelta: DispatchDelta = {
        kind: "TableAddBatch",
        values: List([missingIdRow]),
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };
      expectIdentityWithConsoleError(
        invalidDelta,
        table,
        "TableAddBatch: id must be a string",
      );
    });

    it("handles CustomDelta positive and negative", () => {
      const delta: DispatchDelta = {
        kind: "CustomDelta",
        value: { kind: "AnyCustom" },
        flags: undefined,
        sourceAncestorLookupTypeNames: [],
      };

      const positiveResult = toUpdater(delta, () =>
        ValueOrErrors.Default.return(
          Updater(() => "custom-updated" as PredicateValueType),
        ),
      );
      expect(positiveResult.kind).toBe("value");
      if (positiveResult.kind !== "value") throw new Error("Expected value");
      expect(positiveResult.value("before")).toBe("custom-updated");

      const negativeResult = toUpdater(delta, () =>
        ValueOrErrors.Default.throwOne("Custom parse failed"),
      );
      expect(negativeResult.kind).toBe("errors");
    });

    it("returns errors for unsupported delta kinds", () => {
      const result = toUpdater({
        kind: "NotSupportedYet",
      } as unknown as DispatchDelta);
      expect(result.kind).toBe("errors");
    });
  });
});
