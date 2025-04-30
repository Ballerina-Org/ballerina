import {
  ApiConverters,
  CollectionSelection,
  DispatchApiConverters,
  fromAPIRawValue,
  Sum,
  toAPIRawValue,
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
      fromAPIRawValue: (_) => (typeof _ == "string" ? _ : ""),
      toAPIRawValue: ([_, __]) => _,
    },
    number: {
      fromAPIRawValue: (_) => (typeof _ == "number" ? _ : 0),
      toAPIRawValue: ([_, __]) => _,
    },
    boolean: {
      fromAPIRawValue: (_) => (typeof _ == "boolean" ? _ : false),
      toAPIRawValue: ([_, __]) => _,
    },
    base64File: {
      fromAPIRawValue: (_) => (typeof _ == "string" ? _ : ""),
      toAPIRawValue: ([_, __]) => _,
    },
    secret: {
      fromAPIRawValue: (_) => (typeof _ == "string" ? _ : ""),
      toAPIRawValue: ([_, isModified]) => (isModified ? _ : undefined),
    },
    Date: {
      fromAPIRawValue: (_) =>
        typeof _ == "string"
          ? new Date(Date.parse(_))
          : typeof _ == "number"
            ? new Date(_)
            : new Date(Date.now()),
      toAPIRawValue: ([_, __]) => _,
    },
    union: { fromAPIRawValue: (_) => _, toAPIRawValue: ([_, __]) => _ },
    SingleSelection: {
      fromAPIRawValue: (_) =>
        _.IsSome == false
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
      fromAPIRawValue: (_) => (_ == undefined ? List() : List(_)),
      toAPIRawValue: ([_, __]) => _.valueSeq().toArray(),
    },
    Map: {
      fromAPIRawValue: (_) =>
        _ == undefined
          ? List()
          : List(_.map((_: { Key: any; Value: any }) => [_.Key, _.Value])),
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
        _.IsRight ? Sum.Default.right(_.Value) : Sum.Default.left(_.Value),
      toAPIRawValue: ([_, __]) => ({
        IsRight: _.kind == "r",
        Value: _.value,
      }),
    },
    SumUnitDate: {
      fromAPIRawValue: (_: any) =>
        _.IsRight ? Sum.Default.right(_.Value) : Sum.Default.left(_.Value),
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
      toAPIRawValue: ([_, __]) => _,
    },
  };
