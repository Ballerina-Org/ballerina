import { Map } from "immutable";
import { Identifiable } from "../../../../../baseEntity/domains/identifiable/state";
import { unit, Unit } from "../../../../../fun/domains/unit/state";
import {
  BasicUpdater,
  Updater,
} from "../../../../../fun/domains/updater/state";
import { BasicFun } from "../../../../../fun/state";
import { Option, Sum } from "../../../sum/state";
import { ValueOrErrors } from "../../../valueOrErrors/state";

export const MapRepo = {
  Default: {
    fromIdentifiables: <T extends Identifiable>(array: T[]): Map<T["Id"], T> =>
      Map(
        array.reduce<Record<string, T>>((acc, item) => {
          acc[item.Id] = item;
          return acc;
        }, {}),
      ),
  },
  Updaters: {
    set<K, V>(key: K, value: V): Updater<Map<K, V>> {
      return Updater((_) => _.set(key, value));
    },
    remove<K, V>(key: K): Updater<Map<K, V>> {
      return Updater((_) => _.remove(key));
    },
    update: <k, v>(k: k, _: BasicUpdater<v>): Updater<Map<k, v>> =>
      Updater((current) =>
        current.has(k) ? current.set(k, _(current.get(k)!)) : current,
      ),
    upsert: <k, v>(
      k: k,
      defaultValue: BasicFun<Unit, v>,
      _: BasicUpdater<v>,
    ): Updater<Map<k, v>> =>
      Updater((current) =>
        current.has(k)
          ? current.set(k, _(current.get(k)!))
          : current.set(k, _(defaultValue({}))),
      ),
  },
  Operations: {
    tryFirstWithError: <k, v, e>(
      m: Map<k, v>,
      e: () => e,
    ): ValueOrErrors<v, e> =>
      ValueOrErrors.Default.ofOption(
        m.size != 0 ? Sum.Default.right(m.first()!) : Sum.Default.left(unit),
        e,
      ),
    tryFind: <k, v>(k: k, m: Map<k, v>): Option<v> =>
      m.has(k) ? Sum.Default.right(m.get(k)!) : Sum.Default.left(unit),
    tryFindWithError: <k, v, e>(
      k: k,
      m: Map<k, v>,
      e: () => e,
    ): ValueOrErrors<v, e> =>
      ValueOrErrors.Default.ofOption(
        m.has(k) ? Sum.Default.right(m.get(k)!) : Sum.Default.left(unit),
        e,
      ),
  },
};
