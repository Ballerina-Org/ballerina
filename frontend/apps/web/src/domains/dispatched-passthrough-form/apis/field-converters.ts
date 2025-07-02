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

export const PersonDispatchFieldTypeConverters: DispatchApiConverters<DispatchPassthroughFormInjectedTypes> =
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
  };
export const DispatchFieldTypeConverters2: DispatchApiConverters<DispatchPassthroughFormInjectedTypes> =
  {
    injectedCategory: {
      fromAPIRawValue: (_) => {
        if (_ == undefined || _?.fields?.kind == "custom") {
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
              kind: _.fields.kind,
              extraSpecial: _.fields.extraSpecial,
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
      fromAPIRawValue: (_) => {
        return _;
      },
      toAPIRawValue: ([_, __]) => _,
    },
    number: {
      fromAPIRawValue: (_) => {
        return _.kind == "int" ? parseInt(_.value): parseFloat(_.Values)
      },
      toAPIRawValue: ([_, __]) => _,
    },
    boolean: {
      fromAPIRawValue: (_) =>  {
        return _;
      },
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
      fromAPIRawValue: (_) => typeof _ == "string" ? new Date(Date.parse(_)) : _,
      toAPIRawValue: ([_, __]) => _,
    },
    union: {
      fromAPIRawValue: (_) => {

        if (_ == undefined) {
          return _;
        }
        if (
          _.fields.Case == undefined ||
          typeof _.fields.Case != "string"
        ) {
          return _;
        }
        return {
          caseName: _.fields.Case,
          fields: _.fields.Value, //.fields,
        };
      },
      toAPIRawValue: ([_, __]) => _,
    },
    SingleSelection: {
      fromAPIRawValue: (_) => {
    
        return _.fields?.IsSome == false
          ? CollectionSelection().Default.right("no selection")
          : CollectionSelection().Default.left(_.fields.Value)},
      toAPIRawValue: ([_, __]) =>
        _.kind == "r"
          ? { IsSome: false, Value: null }
          : { IsSome: true, Value: _.value },
    },
    MultiSelection: {
      
      fromAPIRawValue: (_) =>{
   
        const x = "elements" in _
          ? _["elements"]
          : _;
        return x == undefined
          ? OrderedMap()
          : OrderedMap(
            x.map((i: any) =>
              { 
             
                return("Value" in i.fields? [i.fields.Value, i] : [i.fields.Id, i])}),
          )},
      toAPIRawValue: ([_, __]) => _.valueSeq().toArray(),
    },
    List: {
      fromAPIRawValue: (_) =>
      { 
     
        return (Array.isArray(_.elements) ? List(_.elements) : _)},
      toAPIRawValue: ([_, __]) => _.valueSeq().toArray(),
    },
    Map: {
      fromAPIRawValue: (_) =>{
     
        const x = "elements" in _
          ? _["elements"]
          : _;
        return Array.isArray(x)
          ? List(x.map((_: { fields: { Key: any; Value: any } }) => [_.fields.Key, _.fields.Value]))
          : _},
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
      fromAPIRawValue: (_: any) =>{
    
        return _.fields?.IsRight ? Sum.Default.right(_.fields?.Value) : Sum.Default.left(_.fields?.Value)},
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
    // One: {
    //   fromAPIRawValue: (_) =>{
    //     return _.fields.isRight
    //       ? PredicateValue.Default.option(true, _.fields.right)
    //       : PredicateValue.Default.option(false, PredicateValue.Default.unit())},
    //   toAPIRawValue: ([_, __]) => _,
    // },
    One: {
      fromAPIRawValue: (_) =>{
      
        return _.fields.One
          ? PredicateValue.Default.option(true, _.fields.Value)
          : PredicateValue.Default.option(false, PredicateValue.Default.unit())},
      toAPIRawValue: ([_, __]) => _,
    },
  };

function convertStructuredValue(arg: any): any {
  debugger
  if (arg == null || typeof arg !== "object") {
    return arg;
  }

  if ("kind" in arg && ("fields" in arg || "value" in arg) || "elements" in arg) {
    const kind = arg.kind;
    const fields = arg.fields;
    const elements = arg.elements;
    const value = arg.value;

    switch (kind) {
      case "int": return parseFloat(value);
      case "float": return parseFloat(value);
      case "unit": return {};
      case "tuple": {
        const tmp = elements.map(convertStructuredValue);
        debugger 
        return List(tmp);}
      case "record":
        if ("Value" in fields && "Case" in fields) {
          return convertStructuredValue(fields.Value)
        } 
        else if ("kind" in fields) {
          return convertStructuredValue(fields)
        }
        else if (Array.isArray(fields) && fields.every(item =>
        item && typeof item === "object" && "Key" in item && "Value" in item)
      ) {
          return  Map(fields.map((field: { Key: any; Value: any }) => [
            convertStructuredValue(field.Key),
            convertStructuredValue(field.Value),
          ]))
        }
        return  convertStructuredValue(fields);

      default:
        //TODO: rather fail that allow unexpected
        return value
    }
  }
}

// export const DispatchFieldTypeConverters: DispatchApiConverters<DispatchPassthroughFormInjectedTypes> = {
//   injectedCategory: {
//     fromAPIRawValue: (_) => convertStructuredValue(_),
//     toAPIRawValue: ([_, __]) => ({ kind: _.value.kind, extraSpecial: _.value.extraSpecial }),
//   },
//   string: {
//     fromAPIRawValue: (_) => convertStructuredValue(_),
//     toAPIRawValue: ([_, __]) => _,
//   },
//   number: {
//     fromAPIRawValue: (_) => convertStructuredValue(_),
//     toAPIRawValue: ([_, __]) => _,
//   },
//   boolean: {
//     fromAPIRawValue: (_) => convertStructuredValue(_),
//     toAPIRawValue: ([_, __]) => _,
//   },
//   base64File: {
//     fromAPIRawValue: (_) => convertStructuredValue(_),
//     toAPIRawValue: ([_, __]) => _,
//   },
//   secret: {
//     fromAPIRawValue: (_) => convertStructuredValue(_),
//     toAPIRawValue: ([_, isModified]) => (isModified ? _ : undefined),
//   },
//   Date: {
//     fromAPIRawValue: (_) => convertStructuredValue(_),
//     toAPIRawValue: ([_, __]) => _,
//   },
//   union: {
//     fromAPIRawValue: (_) =>{
//       if ("Value" in _.fields && "Case" in _.fields)
//         return { caseName: _.fields.Case, fields: _.fields.Value };
//       else if ("fields" in _ && "kind" in _) 
//         return convertStructuredValue(_.fields);
//       else convertStructuredValue(_);},
//     toAPIRawValue: ([_, __]) => _,
//   },
//   SingleSelection: {
//     fromAPIRawValue: (_) => {
//       debugger 
//       convertStructuredValue(_) },
//     toAPIRawValue: ([_, __]) => _.kind == "r" ? { IsSome: false, Value: null } : { IsSome: true, Value: _.value },
//   },
//   MultiSelection: {
//     fromAPIRawValue: (_) => {
//       debugger
//      
//       return convertStructuredValue(_)},
//     toAPIRawValue: ([_, __]) => _.valueSeq().toArray(),
//   },
//   List: {
//     fromAPIRawValue: (_) => {
//       debugger
//       return convertStructuredValue(_)},
//     toAPIRawValue: ([_, __]) => _.valueSeq().toArray(),
//   },
//   Map: {
//     fromAPIRawValue: (_) => {
//       debugger
//       return convertStructuredValue(_)
//     },
//     toAPIRawValue: ([_, __]) =>
//       _.valueSeq()
//         .toArray()
//         .map((_: any) => ({ Key: _[0], Value: _[1] })),
//   },
//   Tuple: {
//     fromAPIRawValue: (_) =>{
//       debugger 
//       return convertStructuredValue(_)},
//     toAPIRawValue: ([_, __]) =>
//       _.valueSeq()
//         .toArray()
//         .reduce((acc, value, index) => ({ ...acc, [`Item${index + 1}`]: value }), {}),
//   },
//   Sum: {
//     fromAPIRawValue: (_) => convertStructuredValue(_),
//     toAPIRawValue: ([_, __]) => ({ IsRight: _.kind == "r", Value: _.value }),
//   },
//   SumUnitDate: {
//     fromAPIRawValue: (_) => convertStructuredValue(_),
//     toAPIRawValue: ([_, __]) => ({ IsRight: _.kind == "r", Value: _.value }),
//   },
//   Table: {
//     fromAPIRawValue: (_) => convertStructuredValue(_),
//     toAPIRawValue: ([_, __]) => ({ From: 0, To: 0, HasMore: false, Values: [] }),
//   },
//   One: {
//     fromAPIRawValue: (_) => convertStructuredValue(_),
//     toAPIRawValue: ([_, __]) => _,
//   },
// };

export const DispatchFieldTypeConverters: DispatchApiConverters<DispatchPassthroughFormInjectedTypes> =
  {
    injectedCategory: {
      fromAPIRawValue: (_) => {
        debugger
        if (_.fields == undefined) {
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
              kind: _.fields.kind,
              extraSpecial: _.fields.value,
            },
          };
        }
      },
      toAPIRawValue: ([_, __]) => ({
        kind: _.value.kind,
        extraSpecial: _.value.value,
      }),
    },
    string: {
      fromAPIRawValue: (_) => convertStructuredValue(_),
      toAPIRawValue: ([_, __]) => _,
    },
    number: {
      fromAPIRawValue: (_) => convertStructuredValue(_),
      toAPIRawValue: ([_, __]) => _,
    },
    boolean: {
      fromAPIRawValue: (_) => convertStructuredValue(_),
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
      fromAPIRawValue: (_) =>{
        debugger
        return typeof _ == "string" ? new Date(Date.parse(_)) : _ },
      toAPIRawValue: ([_, __]) => _,
    },
    union: {
      fromAPIRawValue: (_) => {
        debugger
        if (_ == undefined) {
          return _;
        }
        if (
          _.fields.Case == undefined ||
          typeof _.fields.Case != "string"
        ) {
          return _;
        }
        return {
          caseName: _.fields.Case,
          fields: convertStructuredValue(_.fields.Value),
        };
      },
      toAPIRawValue: ([_, __]) => _,
    },
    SingleSelection: {
      fromAPIRawValue: (_) =>{
        debugger
        return _.fields.IsSome == false
          ? CollectionSelection().Default.right("no selection")
          : CollectionSelection().Default.left(_.fields.Value)},
      toAPIRawValue: ([_, __]) =>
        _.kind == "r"
          ? { IsSome: false, Value: null }
          : { IsSome: true, Value: _.value },
    },
    MultiSelection: {
      fromAPIRawValue: (_) => convertStructuredValue(_),
      toAPIRawValue: ([_, __]) => _.valueSeq().toArray(),
    },
    List: {
      fromAPIRawValue: (_) => {
        debugger
        return (Array.isArray(_) ? List(_) : _)},
      toAPIRawValue: ([_, __]) => _.valueSeq().toArray(),
    },
    Map: {
      fromAPIRawValue: (_) =>{
        debugger
        return Array.isArray(_)
          ? List(_.map((_: { Key: any; Value: any }) => [_.Key, _.Value]))
          : _},
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
        debugger
        if (_.fields == undefined) {
          return List();
        }
        const prefix = "Item";
        let index = 1;
        const result: any[] = [];
        for (const __ in Object.keys(_.fields)) {
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
      fromAPIRawValue: (_: any) =>{
        debugger
        return _?.fields.IsRight ? Sum.Default.right(_.fields.Value) : Sum.Default.left(_.fields.Value)},
      toAPIRawValue: ([_, __]) => ({
        IsRight: _.kind == "r",
        Value: _.value,
      }),
    },
    SumUnitDate: {
      fromAPIRawValue: (_: any) =>{
        debugger
        return _?.IsRight ? Sum.Default.right(_.Value) : Sum.Default.left(_.Value)},
      toAPIRawValue: ([_, __]) => ({
        IsRight: _.kind == "r",
        Value: _.value,
      }),
    },
    Table: {
      fromAPIRawValue: (_) => {
        debugger
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
      fromAPIRawValue: (_) =>{
        debugger
        return _.fields.One
          ? PredicateValue.Default.option(true, _.fields.Value)
          : PredicateValue.Default.option(false, PredicateValue.Default.unit())},
      toAPIRawValue: ([_, __]) => _,
    },
  };