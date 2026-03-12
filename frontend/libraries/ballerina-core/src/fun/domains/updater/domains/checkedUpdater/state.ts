import { BasicFun, Fun } from "../../../../state";
import { ValueOrErrors } from "../../../../../collections/domains/valueOrErrors/state";
import { Updater } from "../../state";

export type BasicCheckedUpdater<e> = BasicFun<
  e,
  ValueOrErrors<Updater<e>, string>
>;

export type CheckedUpdater<e> = BasicCheckedUpdater<e> & {
  fun: Fun<e, ValueOrErrors<Updater<e>, string>>;
  then(other: BasicCheckedUpdater<e>): CheckedUpdater<e>;
  thenMany(others: Array<BasicCheckedUpdater<e>>): CheckedUpdater<e>;
  apply(current: e): ValueOrErrors<e, string>;
};

export const CheckedUpdater = <e>(u: BasicCheckedUpdater<e>): CheckedUpdater<e> => {
  return Object.assign(u, {
    fun: Fun(u),
    then: function (
      this: CheckedUpdater<e>,
      other: BasicCheckedUpdater<e>,
    ): CheckedUpdater<e> {
      return CheckedUpdater<e>((current) =>
        this(current).Then((leftUpdater) => {
          const valueAfterLeft = leftUpdater(current);
          return other(valueAfterLeft).Then((rightUpdater) =>
            ValueOrErrors.Default.return<Updater<e>, string>(
              Updater<e>((value) => rightUpdater(leftUpdater(value))),
            ),
          );
        }),
      );
    },
    thenMany: function (
      this: CheckedUpdater<e>,
      others: Array<BasicCheckedUpdater<e>>,
    ): CheckedUpdater<e> {
      return others
        .map((_) => CheckedUpdater(_))
        .reduce((acc, next) => acc.then(next), this);
    },
    apply: function (
      this: CheckedUpdater<e>,
      current: e,
    ): ValueOrErrors<e, string> {
      return this(current).Then((updater) =>
        ValueOrErrors.Default.return<e, string>(updater(current)),
      );
    },
  });
};

export const CheckedUpdaterOperations = {
  Return: <e>(updater: Updater<e>): CheckedUpdater<e> =>
    CheckedUpdater((_) =>
      ValueOrErrors.Default.return<Updater<e>, string>(updater),
    ),
  ThrowOne: <e>(error: string): CheckedUpdater<e> =>
    CheckedUpdater((_) => ValueOrErrors.Default.throwOne<Updater<e>, string>(error)),
};