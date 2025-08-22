import {
  ApiConverters,
  CollectionSelection,
  DispatchApiConverters,
  fromAPIRawValue,
  PredicateValue,
  Sum,
  toAPIRawValue,
  ValueFilterContains,
  ValueFilterEqualsTo,
  ValueFilterGreaterThan,
  ValueFilterGreaterThanOrEqualsTo,
  ValueFilterIsNotNull,
  ValueFilterIsNull,
  ValueFilterNotEqualsTo,
  ValueFilterSmallerThanOrEqualsTo,
  ValueFilterSmallerThan,
  ValueFilterStartsWith,
  ValueOption,
  ValueSumN,
} from "ballerina-core";
import { List, OrderedMap, Map } from "immutable";
import { DispatchPassthroughFormInjectedTypes } from "../injected-forms/category";

export const DispatchFieldTypeConverters: DispatchApiConverters<DispatchPassthroughFormInjectedTypes> =
  {
    injectedCategory: {
      fromAPIRawValue: (_) => {
        if (_ == undefined) {
          return {
            kind: "custom",
            value: {
              kind: "adult",
              extraSpecial: false,
            },
          };
        } else {
          return {
            kind: "custom",
            value: {
              kind: _.kind,
              extraSpecial: _.extraSpecial,
            },
          };
        }
      },
      toAPIRawValue: ([_, __]) => ({
        kind: _.value.kind,
        extraSpecial: _.value.extraSpecial,
      }),
    },
    string: {
      fromAPIRawValue: (_) => _,
      toAPIRawValue: ([_, __]) => _,
    },
    number: {
      fromAPIRawValue: (_) => _,
      toAPIRawValue: ([_, __]) => _,
    },
    boolean: {
      fromAPIRawValue: (_) => _,
      toAPIRawValue: ([_, __]) => _,
    },
    base64File: {
      fromAPIRawValue: (_) => _,
      toAPIRawValue: ([_, __]) => _,
    },
    secret: {
      fromAPIRawValue: (_) => _,
      toAPIRawValue: ([_, isModified]) => (isModified ? _ : undefined),
    },
    Date: {
      fromAPIRawValue: (_) =>
        typeof _ == "string" ? new Date(Date.parse(_)) : _,
      toAPIRawValue: ([_, __]) => _,
    },
    union: {
      fromAPIRawValue: (_) => {
        if (_ == undefined) {
          return _;
        }
        if (
          _.Discriminator == undefined ||
          typeof _.Discriminator != "string"
        ) {
          return _;
        }
        return {
          caseName: _.Discriminator,
          fields: _[_.Discriminator],
        };
      },
      toAPIRawValue: ([_, __]) => _,
    },
    SingleSelection: {
      fromAPIRawValue: (_) =>
        _?.IsSome == false
          ? CollectionSelection().Default.right("no selection")
          : CollectionSelection().Default.left(_.Value),
      toAPIRawValue: ([_, __]) =>
        _.kind == "r"
          ? { IsSome: false, Value: null }
          : { IsSome: true, Value: _.value },
    },
    MultiSelection: {
      fromAPIRawValue: (_) =>
        _ == undefined
          ? OrderedMap()
          : OrderedMap(
              _.map((_: any) => ("Value" in _ ? [_.Value, _] : [_.Id, _])),
            ),
      toAPIRawValue: ([_, __]) => _.valueSeq().toArray(),
    },
    List: {
      fromAPIRawValue: (_) => (Array.isArray(_) ? List(_) : _),
      toAPIRawValue: ([_, __]) => _.valueSeq().toArray(),
    },
    Map: {
      fromAPIRawValue: (_) =>
        Array.isArray(_)
          ? List(_.map((_: { Key: any; Value: any }) => [_.Key, _.Value]))
          : _,
      toAPIRawValue: ([_, __]) =>
        _.valueSeq()
          .toArray()
          .map((_: any) => ({
            Key: _[0],
            Value: _[1],
          })),
    },
    Tuple: {
      fromAPIRawValue: (_) => {
        if (_ == undefined) {
          return List();
        }
        const prefix = "Item";
        let index = 1;
        const result: any[] = [];
        for (const __ in Object.keys(_)) {
          const key = `${prefix}${index}`;
          if (key in _) {
            result.push(_[key]);
          }
          index++;
        }
        return List(result);
      },
      toAPIRawValue: ([_, __]) =>
        _.valueSeq()
          .toArray()
          .reduce(
            (acc, value, index) => ({
              ...acc,
              [`Item${index + 1}`]: value,
            }),
            {},
          ),
    },
    Sum: {
      fromAPIRawValue: (_: any) =>
        _?.IsRight ? Sum.Default.right(_?.Value) : Sum.Default.left(_?.Value),
      toAPIRawValue: ([_, __]) => ({
        IsRight: _.kind == "r",
        Value: _.value,
      }),
    },
    SumN: {
      fromAPIRawValue: (_: any) => {
        const caseIndex =
          parseInt(_.Discriminator.split("of")[0].split("case")[1]) - 1;
        const arity = parseInt(_.Discriminator.split("of")[1]);
        const discriminator = `Case${caseIndex + 1}`;

        return ValueSumN.Default(caseIndex, arity, _[discriminator]);
      },
      toAPIRawValue: ([_, __]) => {
        return {
          Discriminator: `case${_.caseIndex + 1}of${_.arity}`,
          [`Case${_.caseIndex + 1}`]: _.value,
        };
      },
    },
    SumUnitDate: {
      fromAPIRawValue: (_: any) =>
        _?.IsRight ? Sum.Default.right(_.Value) : Sum.Default.left(_.Value),
      toAPIRawValue: ([_, __]) => ({
        IsRight: _.kind == "r",
        Value: _.value,
      }),
    },
    Table: {
      fromAPIRawValue: (_) => {
        if (_ == undefined) {
          return { data: Map(), hasMoreValues: false, from: 0, to: 0 };
        }
        return {
          data: OrderedMap(_.Values),
          hasMoreValues: _.HasMore,
          from: _.From,
          to: _.To,
        };
      },
      toAPIRawValue: ([_, __]) => ({
        From: 0,
        To: 0,
        HasMore: false,
        Values: [],
      }),
    },
    One: {
      fromAPIRawValue: (_) =>
        _.isRight
          ? PredicateValue.Default.option(true, _.right)
          : PredicateValue.Default.option(false, PredicateValue.Default.unit()),
      toAPIRawValue: ([_, __]) => _,
    },
    ReadOnly: {
      fromAPIRawValue: (_) => {
        // Extract value from ReadOnly field structure
        if (typeof _ !== "object" || _ === null || !("ReadOnly" in _)) {
          throw new Error(
            `ReadOnly type requires value to be wrapped in ReadOnly field, but got ${JSON.stringify(_)}`,
          );
        }
        return _;
      },
      toAPIRawValue: ([_, __]) => {
        // Wrap value in ReadOnly field structure
        return _;
      },
    },
    // Filters
    Contains: {
      fromAPIRawValue: (_) => ValueFilterContains.Default(_.Contains),
      toAPIRawValue: ([_, __]) => ({
        Contains: _.contains,
      }),
    },
    "=": {
      fromAPIRawValue: (_) => ValueFilterEqualsTo.Default(_.EqualsTo),
      toAPIRawValue: ([_, __]) => ({
        EqualsTo: _.equalsTo,
      }),
    },
    "!=": {
      fromAPIRawValue: (_) => ValueFilterNotEqualsTo.Default(_.NotEqualsTo),
      toAPIRawValue: ([_, __]) => ({
        NotEqualsTo: _.notEqualsTo,
      }),
    },
    ">=": {
      fromAPIRawValue: (_) =>
        ValueFilterGreaterThanOrEqualsTo.Default(_.GreaterThanOrEqualsTo),
      toAPIRawValue: ([_, __]) => ({
        GreaterThanOrEqualsTo: _.greaterThanOrEqualsTo,
      }),
    },
    ">": {
      fromAPIRawValue: (_) => ValueFilterGreaterThan.Default(_.GreaterThan),
      toAPIRawValue: ([_, __]) => ({
        GreaterThan: _.greaterThan,
      }),
    },
    "!=null": {
      fromAPIRawValue: (_) => ValueFilterIsNotNull.Default(),
      toAPIRawValue: ([_, __]) => ({
        IsNotNull: {},
      }),
    },
    "=null": {
      fromAPIRawValue: (_) => ValueFilterIsNull.Default(),
      toAPIRawValue: ([_, __]) => ({
        IsNull: {},
      }),
    },
    "<=": {
      fromAPIRawValue: (_) =>
        ValueFilterSmallerThanOrEqualsTo.Default(_.SmallerThanOrEqualsTo),
      toAPIRawValue: ([_, __]) => ({
        SmallerThanOrEqualsTo: _.smallerThanOrEqualsTo,
      }),
    },
    "<": {
      fromAPIRawValue: (_) => ValueFilterSmallerThan.Default(_.SmallerThan),
      toAPIRawValue: ([_, __]) => ({
        SmallerThan: _.smallerThan,
      }),
    },
    StartsWith: {
      fromAPIRawValue: (_) => ValueFilterStartsWith.Default(_.StartsWith),
      toAPIRawValue: ([_, __]) => ({
        StartsWith: _.startsWith,
      }),
    },
  };
