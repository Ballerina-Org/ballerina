import { List } from "immutable";
import { Updater } from "../../../../../fun/domains/updater/state";
import { BasicFun } from "../../../../../fun/state";
import { unit } from "../../../../../fun/domains/unit/state";
import { Option, Sum } from "../../../sum/state";
import { ValueOrErrors } from "../../../valueOrErrors/state";

export const ListRepo = {
  Default: {},
  Updaters: {
    remove<V>(elementIndex: number): Updater<List<V>> {
      return Updater((_) => _.remove(elementIndex));
    },
    push<V>(v: V): Updater<List<V>> {
      return Updater((_) => _.push(v));
    },
    insert<V>(elementIndex: number, v: V): Updater<List<V>> {
      return Updater((_) => _.insert(elementIndex, v));
    },
    update<V>(elementIndex: number, upd: Updater<V>): Updater<List<V>> {
      return Updater((_) => {
        const element = _.get(elementIndex);
        if (element == undefined) return _;
        return _.set(elementIndex, upd(element));
      });
    },
    filter<V>(predicate: BasicFun<V, boolean>): Updater<List<V>> {
      return Updater((_) => _.filter(predicate));
    },
    move<V>(elementIndex: number, to: number): Updater<List<V>> {
      return Updater((_) => {
        const element = _.get(elementIndex);
        if (element == undefined) return _;
        return _.remove(elementIndex).insert(to, element);
      });
    },
    duplicate<V>(elementIndex: number): Updater<List<V>> {
      return Updater((_) => {
        const element = _.get(elementIndex);
        if (element == undefined) return _;
        return _.insert(elementIndex + 1, element);
      });
    },
  },
  Operations: {
    tryFind: <V>(elementIndex: number, list: List<V>): Option<V> =>
      list.has(elementIndex)
        ? Sum.Default.right(list.get(elementIndex)!)
        : Sum.Default.left(unit),
    tryFindWithError: <v, e>(
      i: number,
      m: List<v>,
      e: () => e,
    ): ValueOrErrors<v, e> =>
      ValueOrErrors.Default.ofOption(
        m.has(i) ? Sum.Default.right(m.get(i)!) : Sum.Default.left(unit),
        e,
      ),
  },
};
