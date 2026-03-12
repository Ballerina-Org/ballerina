import { Updater } from "../../../../../../../fun/domains/updater/state";
import { Unit } from "../../../../../../../fun/domains/unit/state";
import { ValueOrErrors } from "../../../../../../../collections/domains/valueOrErrors/state";
import { DispatchDelta, DispatchDeltaCustom } from "../dispatch-delta/state";
import { PredicateValue } from "../../../../../parser/domains/predicates/state";
import { List } from "immutable";
import { replaceWith } from "../../../../../../../fun/domains/updater/domains/replaceWith/state";
import { Sum } from "ballerina-core";

export const DispatchDeltaToUpdater =
  <Flags = Unit>(
    parseCustomDelta: (
      customDelta: DispatchDeltaCustom<Flags>,
    ) => ValueOrErrors<Updater<PredicateValue>, string>,
  ) =>
  (
    delta: DispatchDelta<Flags>,
  ): ValueOrErrors<Updater<PredicateValue>, string> => {
    const failCurrent = (current: PredicateValue, message: string): PredicateValue => {
      console.error("Error when applying an updater from a delta:\n", message);
      return current;
    };
    const rec = (
      nestedDelta: DispatchDelta<Flags>,
    ): ValueOrErrors<Updater<PredicateValue>, string> =>
      DispatchDeltaToUpdater(parseCustomDelta)(nestedDelta);

    if (delta.kind == "NumberReplace") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsNumber(current)
            ? delta.replace
            : failCurrent(
                current,
                `Delta NumberReplace expects current to be number, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (delta.kind == "StringReplace") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsString(current)
            ? delta.replace
            : failCurrent(
                current,
                `Delta StringReplace expects current to be string, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (delta.kind == "BoolReplace") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsBoolean(current)
            ? delta.replace
            : failCurrent(
                current,
                `Delta BoolReplace expects current to be boolean, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (delta.kind == "TimeReplace") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsDate(current)
            ? delta.replace
            : failCurrent(
                current,
                `Delta TimeReplace expects current to be date, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (delta.kind == "GuidReplace") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsString(current)
            ? delta.replace
            : failCurrent(
                current,
                `Delta GuidReplace expects current to be string, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (delta.kind == "UnitReplace") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsUnit(current)
            ? replaceWith(delta.replace)(current)
            : failCurrent(
                current,
                `Delta UnitReplace expects current to be unit, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (delta.kind == "OptionReplace") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsOption(current)
            ? replaceWith(delta.replace)(current)
            : failCurrent(
                current,
                `Delta OptionReplace expects current to be option, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (delta.kind == "SumReplace") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsSum(current)
            ? replaceWith(delta.replace)(current)
            : failCurrent(
                current,
                `Delta SumReplace expects current to be sum, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (delta.kind == "ArrayReplace") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsTuple(current)
            ? replaceWith(delta.replace)(current)
            : failCurrent(
                current,
                `Delta ArrayReplace expects current to be tuple, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (delta.kind == "RecordReplace") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsRecord(current)
            ? replaceWith(delta.replace)(current)
            : failCurrent(
                current,
                `Delta RecordReplace expects current to be record, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (delta.kind == "UnionReplace") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsUnionCase(current)
            ? replaceWith(delta.replace)(current)
            : failCurrent(
                current,
                `Delta UnionReplace expects current to be unionCase, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (delta.kind == "TupleReplace") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsTuple(current)
            ? replaceWith(delta.replace)(current)
            : failCurrent(
                current,
                `Delta TupleReplace expects current to be tuple, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (
      delta.kind == "SetReplace" ||
      delta.kind == "MapReplace" ||
      delta.kind == "OneReplace"
    ) {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        replaceWith(delta.replace),
      );
    }

    if (delta.kind == "SumLeft") {
      return rec(delta.value).Then((nestedUpdater) =>
        ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
          Updater((current) => {
            if (!PredicateValue.Operations.IsSum(current)) {
              return failCurrent(
                current,
                `Delta SumLeft expects current to be sum, got ${(current as { kind?: string }).kind ?? typeof current}`,
              );
            }
            if (current.value.kind !== "l") {
              return failCurrent(current, `SumLeft: current sum is right-valued`);
            }
            return {
              ...current,
              value: Sum.Updaters.left<PredicateValue, PredicateValue>(
                nestedUpdater,
              )(current.value),
            };
          }),
        ),
      );
    }

    if (delta.kind == "SumRight") {
      return rec(delta.value).Then((nestedUpdater) =>
        ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
          Updater((current) => {
            if (!PredicateValue.Operations.IsSum(current)) {
              return failCurrent(
                current,
                `Delta SumRight expects current to be sum, got ${(current as { kind?: string }).kind ?? typeof current}`,
              );
            }
            if (current.value.kind !== "r") {
              return failCurrent(current, `SumRight: current sum is left-valued`);
            }
            return {
              ...current,
              value: Sum.Updaters.right<PredicateValue, PredicateValue>(
                nestedUpdater,
              )(current.value),
            };
          }),
        ),
      );
    }

    if (delta.kind == "ArrayValue") {
      return rec(delta.value[1]).Then((nestedUpdater) =>
        ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
          Updater((current) => {
            if (!PredicateValue.Operations.IsTuple(current)) {
              return failCurrent(
                current,
                `Delta ArrayValue expects current to be tuple, got ${(current as { kind?: string }).kind ?? typeof current}`,
              );
            }
            const currentItem = current.values.get(delta.value[0]);
            if (currentItem === undefined) {
              return failCurrent(
                current,
                `ArrayValue: item at index ${delta.value[0]} does not exist`,
              );
            }
            return {
              ...current,
              values: current.values.update(delta.value[0], (item) =>
                item ? nestedUpdater(item) : item,
              ),
            };
          }),
        ),
      );
    }

    if (delta.kind == "ArrayAdd") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsTuple(current)
            ? {
                ...current,
                values: current.values.push(delta.value),
              }
            : failCurrent(
                current,
                `Delta ArrayAdd expects current to be tuple, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (delta.kind == "ArrayAddAt") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsTuple(current)
            ? {
                ...current,
                values: current.values.insert(delta.value[0], delta.value[1]),
              }
            : failCurrent(
                current,
                `Delta ArrayAddAt expects current to be tuple, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (delta.kind == "ArrayRemoveAt") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsTuple(current)
            ? {
                ...current,
                values: current.values.remove(delta.index),
              }
            : failCurrent(
                current,
                `Delta ArrayRemoveAt expects current to be tuple, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (delta.kind == "ArrayRemoveAll") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) =>
          PredicateValue.Operations.IsTuple(current)
            ? {
                ...current,
                values: List(),
              }
            : failCurrent(
                current,
                `Delta ArrayRemoveAll expects current to be tuple, got ${(current as { kind?: string }).kind ?? typeof current}`,
              ),
        ),
      );
    }

    if (delta.kind == "RecordField") {
      return rec(delta.field[1]).Then((nestedUpdater) =>
        ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
          Updater((current) => {
            if (!PredicateValue.Operations.IsRecord(current)) {
              return failCurrent(
                current,
                `Delta RecordField expects current to be record, got ${(current as { kind?: string }).kind ?? typeof current}`,
              );
            }
            const currentField = current.fields.get(delta.field[0]);
            if (currentField === undefined) {
              return failCurrent(
                current,
                `RecordField: field ${delta.field[0]} does not exist`,
              );
            }
            return {
              ...current,
              fields: current.fields.update(delta.field[0], (fieldValue) =>
                fieldValue ? nestedUpdater(fieldValue) : fieldValue,
              ),
            };
          }),
        ),
      );
    }

    if (delta.kind == "UnionCase") {
      return rec(delta.caseName[1]).Then((nestedUpdater) =>
        ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
          Updater((current) => {
            if (!PredicateValue.Operations.IsUnionCase(current)) {
              return failCurrent(
                current,
                `Delta UnionCase expects current to be unionCase, got ${(current as { kind?: string }).kind ?? typeof current}`,
              );
            }
            return {
              ...current,
              caseName: delta.caseName[0],
              fields: nestedUpdater(current.fields),
            };
          }),
        ),
      );
    }

    if (delta.kind == "TupleCase") {
      return rec(delta.item[1]).Then((nestedUpdater) =>
        ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
          Updater((current) => {
            if (!PredicateValue.Operations.IsTuple(current)) {
              return failCurrent(
                current,
                `Delta TupleCase expects current to be tuple, got ${(current as { kind?: string }).kind ?? typeof current}`,
              );
            }
            const currentItem = current.values.get(delta.item[0]);
            if (currentItem === undefined) {
              return failCurrent(
                current,
                `TupleCase: item at index ${delta.item[0]} does not exist`,
              );
            }
            return {
              ...current,
              values: current.values.update(delta.item[0], (item) =>
                item ? nestedUpdater(item) : item,
              ),
            };
          }),
        ),
      );
    }

    if (delta.kind == "TableValue") {
      return rec(delta.nestedDelta).Then((nestedUpdater) =>
        ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
          Updater((current) => {
            if (!PredicateValue.Operations.IsTable(current)) {
              return failCurrent(
                current,
                `Delta TableValue expects current to be table, got ${(current as { kind?: string }).kind ?? typeof current}`,
              );
            }
            const row = current.data.get(delta.id);
            if (row === undefined) {
              return failCurrent(current, `TableValue: row ${delta.id} does not exist`);
            }
            const updatedRow = nestedUpdater(row);
            if (!PredicateValue.Operations.IsRecord(updatedRow)) {
              return failCurrent(current, `TableValue: nested updater must return a record`);
            }
            return {
              ...current,
              data: current.data.set(delta.id, updatedRow),
            };
          }),
        ),
      );
    }

    if (delta.kind == "TableAdd") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) => {
          if (!PredicateValue.Operations.IsTable(current)) {
            return failCurrent(
              current,
              `Delta TableAdd expects current to be table, got ${(current as { kind?: string }).kind ?? typeof current}`,
            );
          }
          if (!PredicateValue.Operations.IsRecord(delta.value)) {
            return failCurrent(current, `TableAdd: value must be a record`);
          }
          const id = delta.value.fields.get("Id");
          if (typeof id !== "string") {
            return failCurrent(current, `TableAdd: id must be a string`);
          }
          return {
            ...current,
            data: current.data.set(id, delta.value),
          };
        }),
      );
    }

    if (delta.kind == "TableAddBatch") {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        Updater((current) => {
          if (!PredicateValue.Operations.IsTable(current)) {
            return failCurrent(
              current,
              `Delta TableAddBatch expects current to be table, got ${(current as { kind?: string }).kind ?? typeof current}`,
            );
          }
          const values: Array<[string, any]> = [];
          for (const row of delta.values.toArray()) {
            if (!PredicateValue.Operations.IsRecord(row)) {
              return failCurrent(current, `TableAddBatch: value must be a record`);
            }
            const id = row.fields.get("Id");
            if (typeof id !== "string") {
              return failCurrent(current, `TableAddBatch: id must be a string`);
            }
            values.push([id, row]);
          }
          return {
            ...current,
            data: current.data.concat(values),
          };
        }),
      );
    }

    if (delta.kind == "CustomDelta") {
      return parseCustomDelta(delta);
    }

    return ValueOrErrors.Default.throwOne<Updater<PredicateValue>, string>(
      `Unsupported delta kind: ${(delta as { kind: string }).kind}`,
    );
  };
