import {
  BasicUpdater,
  Updater,
} from "../../../../../../../fun/domains/updater/state";
import { Unit } from "../../../../../../../fun/domains/unit/state";
import { ValueOrErrors } from "../../../../../../../collections/domains/valueOrErrors/state";
import { DispatchDelta, DispatchDeltaCustom } from "../dispatch-delta/state";
import {
  PredicateValue,
  ValueRecord,
  ValueSum,
  ValueTable,
  ValueTuple,
  ValueUnionCase,
} from "../../../../../parser/domains/predicates/state";
import { List, OrderedMap } from "immutable";
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
    const rec = (
      nestedDelta: DispatchDelta<Flags>,
    ): ValueOrErrors<Updater<PredicateValue>, string> =>
      DispatchDeltaToUpdater(parseCustomDelta)(nestedDelta);

    if (
      delta.kind == "NumberReplace" ||
      delta.kind == "StringReplace" ||
      delta.kind == "BoolReplace" ||
      delta.kind == "TimeReplace" ||
      delta.kind == "GuidReplace" ||
      delta.kind == "UnitReplace" ||
      delta.kind == "OptionReplace" ||
      delta.kind == "SumReplace" ||
      delta.kind == "ArrayReplace" ||
      delta.kind == "SetReplace" ||
      delta.kind == "MapReplace" ||
      delta.kind == "RecordReplace" ||
      delta.kind == "UnionReplace" ||
      delta.kind == "TupleReplace" ||
      delta.kind == "OneReplace"
    ) {
      return ValueOrErrors.Default.return<Updater<PredicateValue>, string>(
        replaceWith(delta.replace),
      );
    }

    if (delta.kind == "SumLeft") {
      return rec(delta.value).Then(
        (nestedUpdater) =>
          ValueOrErrors.Default.return<Updater<ValueSum>, string>(
            Updater((current) => ({
              ...current,
              value: Sum.Updaters.left<PredicateValue, PredicateValue>(
                nestedUpdater,
              )(current.value),
            })),
          ) as ValueOrErrors<Updater<PredicateValue>, string>,
      );
    }

    if (delta.kind == "SumRight") {
      return rec(delta.value).Then(
        (nestedUpdater) =>
          ValueOrErrors.Default.return<Updater<ValueSum>, string>(
            Updater((current) => ({
              ...current,
              value: Sum.Updaters.right<PredicateValue, PredicateValue>(
                nestedUpdater,
              )(current.value),
            })),
          ) as ValueOrErrors<Updater<PredicateValue>, string>,
      );
    }

    if (delta.kind == "ArrayValue") {
      return rec(delta.value[1]).Then((nestedUpdater) =>
        ValueOrErrors.Default.return<Updater<ValueTuple>, string>(
          Updater((current) => ({
            ...current,
            values: current.values.update(delta.value[0], (value) =>
              value ? nestedUpdater(value) : value,
            ),
          })),
        ),
      ) as ValueOrErrors<Updater<PredicateValue>, string>;
    }

    if (delta.kind == "ArrayAdd") {
      return ValueOrErrors.Default.return<Updater<ValueTuple>, string>(
        Updater((current) => {
          return {
            ...current,
            values: current.values.push(delta.value),
          };
        }),
      ) as ValueOrErrors<Updater<PredicateValue>, string>;
    }

    if (delta.kind == "ArrayAddAt") {
      return ValueOrErrors.Default.return<Updater<ValueTuple>, string>(
        Updater((current) => {
          return {
            ...current,
            values: current.values.insert(delta.value[0], delta.value[1]),
          };
        }),
      ) as ValueOrErrors<Updater<PredicateValue>, string>;
    }

    if (delta.kind == "ArrayRemoveAt") {
      return ValueOrErrors.Default.return<Updater<ValueTuple>, string>(
        Updater((current) => {
          return {
            ...current,
            values: current.values.remove(delta.index),
          };
        }),
      ) as ValueOrErrors<Updater<PredicateValue>, string>;
    }

    if (delta.kind == "ArrayRemoveAll") {
      return ValueOrErrors.Default.return<Updater<ValueTuple>, string>(
        Updater((current) => {
          return {
            ...current,
            values: List(),
          };
        }),
      ) as ValueOrErrors<Updater<PredicateValue>, string>;
    }

    if (delta.kind == "RecordField") {
      return rec(delta.field[1]).Then((nestedUpdater) =>
        ValueOrErrors.Default.return<Updater<ValueRecord>, string>(
          Updater((current) => {
            return {
              ...current,
              fields: current.fields.update(delta.field[0], (value) =>
                value ? nestedUpdater(value) : value,
              ),
            };
          }),
        ),
      ) as ValueOrErrors<Updater<PredicateValue>, string>;
    }

    if (delta.kind == "UnionCase") {
      return rec(delta.caseName[1]).Then(
        (nestedUpdater) =>
          ValueOrErrors.Default.return<Updater<ValueUnionCase>, string>(
            Updater((current) => {
              return {
                ...current,
                caseName: delta.caseName[0],
                fields: nestedUpdater(current.fields),
              };
            }),
          ) as ValueOrErrors<Updater<PredicateValue>, string>,
      );
    }

    if (delta.kind == "TupleCase") {
      return rec(delta.item[1]).Then(
        (nestedUpdater) =>
          ValueOrErrors.Default.return<Updater<ValueTuple>, string>(
            Updater((current) => {
              return {
                ...current,
                values: current.values.update(delta.item[0], (value) =>
                  value ? nestedUpdater(value) : value,
                ),
              };
            }),
          ) as ValueOrErrors<Updater<PredicateValue>, string>,
      );
    }

    if (delta.kind == "TableValue") {
      return rec(delta.nestedDelta).Then((nestedUpdater) =>
        ValueOrErrors.Default.return<Updater<ValueTable>, string>(
          Updater((current) => {
            return {
              ...current,
              data: current.data.update(delta.id, (value) =>
                value
                  ? (nestedUpdater as BasicUpdater<ValueRecord>)(value)
                  : value,
              ),
            };
          }),
        ),
      ) as ValueOrErrors<Updater<PredicateValue>, string>;
    }

    if (delta.kind == "TableAdd") {
      const value = delta.value;
      if (!PredicateValue.Operations.IsRecord(value)) {
        return ValueOrErrors.Default.throwOne<Updater<PredicateValue>, string>(
          `TableAdd: value must be a record`,
        );
      }
      const id = value.fields.get("Id");
      if (typeof id !== "string") {
        return ValueOrErrors.Default.throwOne<Updater<PredicateValue>, string>(
          `TableAdd: id must be a string`,
        );
      }
      return ValueOrErrors.Default.return<Updater<ValueTable>, string>(
        Updater((current) => {
          return {
            ...current,
            data: current.data.set(id, value),
          };
        }),
      ) as ValueOrErrors<Updater<PredicateValue>, string>;
    }

    if (delta.kind == "TableAddBatch") {
      return ValueOrErrors.Operations.All(
        delta.values.map((value) => {
          if (!PredicateValue.Operations.IsRecord(value)) {
            return ValueOrErrors.Default.throwOne<
              [string, ValueRecord],
              string
            >(`TableAddBatch: value must be a record`);
          }
          const id = value.fields.get("Id");
          if (typeof id !== "string") {
            return ValueOrErrors.Default.throwOne<
              [string, ValueRecord],
              string
            >(`TableAddBatch: id must be a string`);
          }
          return ValueOrErrors.Default.return<[string, ValueRecord], string>([
            id,
            value,
          ]);
        }),
      ).Then((values) => {
        const newData = OrderedMap(values);
        return ValueOrErrors.Default.return<Updater<ValueTable>, string>(
          Updater((current) => {
            return {
              ...current,
              data: current.data.concat(newData),
            };
          }),
        ) as ValueOrErrors<Updater<PredicateValue>, string>;
      });
    }

    if (delta.kind == "CustomDelta") {
      return parseCustomDelta(delta);
    }

    return ValueOrErrors.Default.throwOne<Updater<PredicateValue>, string>(
      `Unsupported delta kind: ${(delta as { kind: string }).kind}`,
    );
  };
