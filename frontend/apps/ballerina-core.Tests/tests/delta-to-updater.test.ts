import { ValueOrErrors } from "../../../libraries/ballerina-core/src/collections/domains/valueOrErrors/state";
import { DispatchDeltaTransfer } from "../../../libraries/ballerina-core/src/forms/domains/dispatched-forms/runner/domains/deltas/state";
import { type DispatchDelta } from "../../../libraries/ballerina-core/src/forms/domains/dispatched-forms/runner/domains/deltas/dispatch-delta/state";
import { DispatchParsedType } from "../../../libraries/ballerina-core/src/forms/domains/dispatched-forms/deserializer/domains/specification/domains/types/state";
import {
  PredicateValue,
  type PredicateValue as PredicateValueType,
} from "../../../libraries/ballerina-core/src/forms/domains/parser/domains/predicates/state";
import { Updater } from "../../../libraries/ballerina-core/src/fun/domains/updater/state";
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
});
