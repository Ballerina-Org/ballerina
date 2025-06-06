import { List } from "immutable";
import { BasicFun, BasicFun2, Fun } from "../../../fun/state";
import { Value } from "../../../value/state";
import { Errors } from "../errors/state";
import { unit, Unit, Updater, Option, Sum } from "../../../../main";

export type ValueOrErrors<v, e> = (
  | (Value<v> & { kind: "value" })
  | (Errors<e> & { kind: "errors" })
) & {
  Map: <a, b, e>(
    this: ValueOrErrors<a, e>,
    f: BasicFun<a, b>,
  ) => ValueOrErrors<b, e>;
  MapErrors: <a, e, e2>(
    this: ValueOrErrors<a, e>,
    f: BasicFun<List<e>, List<e2>>,
  ) => ValueOrErrors<a, e2>;
  Flatten: <a, e>(
    this: ValueOrErrors<ValueOrErrors<a, e>, e>,
  ) => ValueOrErrors<a, e>;
  Then: <a, b, e>(
    this: ValueOrErrors<a, e>,
    k: BasicFun<a, ValueOrErrors<b, e>>,
  ) => ValueOrErrors<b, e>;
};

const operations = {
  Map: function <a, b, e>(
    this: ValueOrErrors<a, e>,
    f: BasicFun<a, b>,
  ): ValueOrErrors<b, e> {
    if (this.kind == "errors") {
      return this;
    }
    return ValueOrErrors.Default.return(f(this.value));
  },
  MapErrors: function <a, e, e2>(
    this: ValueOrErrors<a, e>,
    f: BasicFun<List<e>, List<e2>>,
  ): ValueOrErrors<a, e2> {
    if (this.kind == "errors") {
      return ValueOrErrors.Default.throw(f(this.errors));
    }
    return this;
  },
  Flatten: function <a, e>(
    this: ValueOrErrors<ValueOrErrors<a, e>, e>,
  ): ValueOrErrors<a, e> {
    if (this.kind == "errors") {
      return this;
    } else if (this.value.kind == "errors") {
      return ValueOrErrors.Default.throw(this.value.errors);
    } else {
      return this.value;
    }
  },
  Then: function <a, b, e>(
    this: ValueOrErrors<a, e>,
    k: BasicFun<a, ValueOrErrors<b, e>>,
  ): ValueOrErrors<b, e> {
    return this.Map(k).Flatten();
  },
};

export const ValueOrErrors = {
  Default: {
    return: <v, e>(_: v): ValueOrErrors<v, e> => ({
      ...Value.Default(_),
      kind: "value",
      ...operations,
    }),
    throwOne: <v, e>(_: e): ValueOrErrors<v, e> => ({
      errors: List([_]),
      kind: "errors",
      ...operations,
    }),
    throw: <v, e>(_: List<e>): ValueOrErrors<v, e> => ({
      errors: _,
      kind: "errors",
      ...operations,
    }),
    ofOption: <v, e>(
      v: Option<v>,
      e: BasicFun<Unit, e>,
    ): ValueOrErrors<v, e> =>
      v.kind == "l"
        ? ValueOrErrors.Default.throwOne(e(unit))
        : ValueOrErrors.Default.return(v.value),
  },
  Operations: {
    Return: <v, e>(_: v): ValueOrErrors<v, e> =>
      ValueOrErrors.Default.return(_),
    Throw: <v, e>(_: List<e>): ValueOrErrors<v, e> =>
      ValueOrErrors.Default.throw(_),
    Map: <a, b, e>(
      f: BasicFun<a, b>,
    ): Fun<ValueOrErrors<a, e>, ValueOrErrors<b, e>> =>
      Fun((_) =>
        _.kind == "errors" ? _ : ValueOrErrors.Default.return(f(_.value)),
      ),
    MapErrors: <a, e, e2>(
      f: BasicFun<List<e>, List<e2>>,
    ): BasicFun<ValueOrErrors<a, e>, ValueOrErrors<a, e2>> =>
      Fun((_) =>
        _.kind == "errors" ? ValueOrErrors.Default.throw(f(_.errors)) : _,
      ),
    Flatten: <a, e>(): Fun<
      ValueOrErrors<ValueOrErrors<a, e>, e>,
      ValueOrErrors<a, e>
    > =>
      Fun((_) =>
        _.kind == "errors"
          ? _
          : _.value.kind == "errors"
            ? ValueOrErrors.Default.throw(_.value.errors)
            : _.value,
      ),
    Then: <a, b, e>(
      k: BasicFun<a, ValueOrErrors<b, e>>,
    ): Fun<ValueOrErrors<a, e>, ValueOrErrors<b, e>> =>
      Fun((_: ValueOrErrors<a, e>) =>
        ValueOrErrors.Operations.Flatten<b, e>()(
          ValueOrErrors.Operations.Map<a, ValueOrErrors<b, e>, e>(k)(_),
        ),
      ),
    Catch: <v, e>(_: ValueOrErrors<v, e>): ValueOrErrors<Sum<v, List<e>>, e> =>
      _.kind == "errors"
        ? ValueOrErrors.Default.return(Sum.Default.right(_.errors))
        : ValueOrErrors.Default.return(Sum.Default.left(_.value)),
    Fold: <v, e, c>(
      l: BasicFun<v, c>,
      r: BasicFun<List<e>, c>,
    ): Fun<ValueOrErrors<v, e>, c> =>
      Fun((_) => (_.kind == "value" ? l(_.value) : r(_.errors))),
    All: <v, e>(_: List<ValueOrErrors<v, e>>): ValueOrErrors<List<v>, e> =>
      _.size == 0
        ? ValueOrErrors.Default.return(List())
        : ValueOrErrors.Operations.Catch(_.first()!).Then((res_p) =>
            res_p.kind == "l"
              ? ValueOrErrors.Operations.All(_.slice(1).toList()).Then(
                  (values) =>
                    ValueOrErrors.Default.return(
                      List<v>().push(res_p.value).concat(values),
                    ),
                )
              : ValueOrErrors.Default.throw(res_p.value),
          ),
  },
};
