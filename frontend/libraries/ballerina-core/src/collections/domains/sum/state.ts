import { id } from "../../../fun/domains/id/state";
import { unit, Unit } from "../../../fun/domains/unit/state";
import { BasicUpdater, Updater } from "../../../fun/domains/updater/state";
import { BasicFun, Fun } from "../../../fun/state";
export type LeftValue<a> = { value: a; kind: "l" };
export const LeftValue = {
  Updaters: {
    value: <a>(_: BasicUpdater<a>): Updater<LeftValue<a>> =>
      Updater((v) => ({ ...v, value: _(v.value) })),
  },
};
export type RightValue<b> = { value: b; kind: "r" };
export const RightValue = {
  Updaters: {
    value: <b>(_: BasicUpdater<b>): Updater<RightValue<b>> =>
      Updater((v) => ({ ...v, value: _(v.value) })),
  },
};
export type Sum<a, b> = { value: a; kind: "l" } | { value: b; kind: "r" };
export type Either<a, b> = Sum<a, b>;
export type Option<a> = Sum<Unit, a>;
export const Sum = Object.assign(
  <a, b>() => ({
    Default: {
      left: Fun((_: a): Sum<a, b> => ({ value: _, kind: "l" })),
      right: Fun((_: b): Sum<a, b> => ({ value: _, kind: "r" })),
    },
    Updaters: {
      left: Fun(
        (_: BasicUpdater<a>): Updater<Sum<a, b>> =>
          Updater(Sum<a, b>().Updaters.map2(_, id)),
      ),
      right: Fun(
        (_: BasicUpdater<b>): Updater<Sum<a, b>> =>
          Updater(Sum<a, b>().Updaters.map2(id, _)),
      ),
      map2: <a, b, a1, b1>(
        l: BasicFun<a, a1>,
        r: BasicFun<b, b1>,
      ): Fun<Sum<a, b>, Sum<a1, b1>> =>
        Sum<a, b>().Operations.fold<a, b, Sum<a1, b1>>(
          Fun(l).then(Sum<a1, b1>().Default.left),
          Fun(r).then(Sum<a1, b1>().Default.right),
        ),
    },
    Operations: {
      fold: <a, b, c>(
        l: BasicFun<a, c>,
        r: BasicFun<b, c>,
      ): Fun<Sum<a, b>, c> =>
        Fun((_) => (_.kind == "l" ? l(_.value) : r(_.value))),
    },
  }),
  {
    Default: {
      left: <a, b>(_: a): Sum<a, b> => ({ value: _, kind: "l" }),
      right: <a, b>(_: b): Sum<a, b> => ({ value: _, kind: "r" }),
    },
    Updaters: {
      left: <a, b>(_: BasicUpdater<a>): Updater<Sum<a, b>> =>
        Updater(Sum.Updaters.map2(_, id)),
      right: <a, b>(_: BasicUpdater<b>): Updater<Sum<a, b>> =>
        Updater(Sum.Updaters.map2(id, _)),
      map2: <a, b, a1, b1>(
        l: BasicFun<a, a1>,
        r: BasicFun<b, b1>,
      ): Fun<Sum<a, b>, Sum<a1, b1>> =>
        Sum.Operations.fold<a, b, Sum<a1, b1>>(
          Fun(l).then(Sum.Default.left<a1, b1>),
          Fun(r).then(Sum.Default.right<a1, b1>),
        ),
    },
    Operations: {
      fold: <a, b, c>(
        l: BasicFun<a, c>,
        r: BasicFun<b, c>,
      ): Fun<Sum<a, b>, c> =>
        Fun((_) => (_.kind == "l" ? l(_.value) : r(_.value))),
      ofOption: <a, b>(
        onNone: BasicFun<Unit, b>,
      ): BasicFun<Option<a>, Sum<b, a>> =>
        Sum.Updaters.map2<Unit, a, b, a>(onNone, id),
    },
  },
);
export const Either = Sum;

export const Option = {
  Default: {
    none: <a>(): Option<a> => Sum.Default.left<Unit, a>(unit),
    some: <a>(_: a): Option<a> => Sum.Default.right<Unit, a>(_),
  },
  Updaters: {
    none: <a>(): Updater<Option<a>> => Sum.Updaters.left<Unit, a>((_) => unit),
    some: <a>(_: BasicUpdater<a>): Updater<Option<a>> =>
      Sum.Updaters.right<Unit, a>(_),
    map2: <a, a1>(some: BasicFun<a, a1>): Fun<Option<a>, Option<a1>> =>
      Sum.Updaters.map2<Unit, a, Unit, a1>(id, some),
  },
};
