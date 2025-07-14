import {
  ApiConverters,
  CollectionSelection,
  DispatchApiConverters,
  fromAPIRawValue,
  PredicateValue,
  Sum,
  toAPIRawValue,
  ValueOption,
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
        return _.ReadOnly;
      },
      toAPIRawValue: ([_, __]) => {
        // Wrap value in ReadOnly field structure
        return {
          ReadOnly: _,
        };
      },
    },
  };
