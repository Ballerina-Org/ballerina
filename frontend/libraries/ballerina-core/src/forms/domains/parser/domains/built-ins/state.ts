import { Map, List, Set, OrderedMap } from "immutable";
import {
  CollectionReference,
  EnumReference,
} from "../../../collection/domains/reference/state";
import { CollectionSelection } from "../../../collection/domains/selection/state";
import { BasicFun } from "../../../../../fun/state";
import {
  BooleanFormState,
  CommonFormState,
  DateFormState,
  InjectedPrimitives,
  NumberFormState,
  ParsedType,
  PredicateValue,
  replaceWith,
  StringFormState,
  Sum,
  TypeName,
  unit,
  UnitFormState,
  ValueRecord,
  ValueTuple,
  Base64FileFormState,
  SecretFormState,
  ListFieldState,
  MapFieldState,
  TupleFormState,
  SumFormState,
  AbstractTableRendererState,
  StreamPosition,
  Chunk,
  Unit,
} from "../../../../../../main";
import { ValueOrErrors } from "../../../../../collections/domains/valueOrErrors/state";

const sortObjectKeys = (obj: Record<string, any>) =>
  Object.keys(obj)
    .sort()
    .reduce((sortedObj, key) => {
      sortedObj[key] = obj[key]!;
      return sortedObj;
    }, {} as any);

const simpleMapKeyToIdentifer = (key: any): string => {
  if (typeof key == "object") return JSON.stringify(sortObjectKeys(key));
  return JSON.stringify(key);
};

export const PrimitiveTypes = [
  "unit",
  "guid", //resolves to string
  "string",
  "number",
  "boolean",
  "Date",
  "base64File",
  "secret",
] as const;
export type PrimitiveType = (typeof PrimitiveTypes)[number];

export const GenericTypes = [
  "SingleSelection",
  "MultiSelection",
  "List",
  "Map",
  "Union",
  "Tuple",
  "Option",
  "Sum",
  "KeyOf",
  "Table",
] as const;
export type GenericType = (typeof GenericTypes)[number];

export type ApiConverter<T> = {
  fromAPIRawValue: BasicFun<any, T>;
  toAPIRawValue: BasicFun<[T, boolean], any>;
};
export type ApiConverters<
  T extends { [key in keyof T]: { type: any; state: any } },
> = { [key in keyof T]: ApiConverter<T[key]["type"]> } & BuiltInApiConverters;

export type UnionCase = {
  caseName: string;
  fields: Record<string, any>;
};

export type Table = {
  data: Map<string, any>;
  hasMoreValues: boolean;
  from: number;
  to: number;
};

export type BuiltInApiConverters = {
  string: ApiConverter<string>;
  number: ApiConverter<number>;
  boolean: ApiConverter<boolean>;
  base64File: ApiConverter<string>;
  secret: ApiConverter<string>;
  Date: ApiConverter<Date>;
  union: ApiConverter<UnionCase>;
  SingleSelection: ApiConverter<
    CollectionSelection<CollectionReference | EnumReference>
  >;
  MultiSelection: ApiConverter<
    OrderedMap<string, CollectionReference | EnumReference>
  >;
  List: ApiConverter<List<any>>;
  Map: ApiConverter<List<[any, any]>>;
  Tuple: ApiConverter<List<any>>;
  Sum: ApiConverter<Sum<any, any>>;
  Table: ApiConverter<Table>;
};

export type PrimitiveBuiltIn = {
  renderers: Set<keyof BuiltIns["renderers"]>;
  defaultValue: PredicateValue;
  defaultState: {
    commonFormState: CommonFormState;
    customFormState: object;
  };
};
export type GenericBuiltIn = { defaultValue: any; defaultState?: any };
export type BuiltIns = {
  primitives: Map<string, PrimitiveBuiltIn>;
  generics: Map<string, GenericBuiltIn>;
  renderers: {
    unit: Set<string>;
    boolean: Set<string>;
    number: Set<string>;
    string: Set<string>;
    base64File: Set<string>;
    secret: Set<string>;
    date: Set<string>;
    enumSingleSelection: Set<string>;
    enumMultiSelection: Set<string>;
    streamSingleSelection: Set<string>;
    streamMultiSelection: Set<string>;
    list: Set<string>;
    map: Set<string>;
    tuple: Set<string>;
    sum: Set<string>;
  };
};

export const builtInsFromFieldViews = (fieldViews: any): BuiltIns => {
  const builtins: BuiltIns = {
    primitives: Map<string, PrimitiveBuiltIn>([
      [
        "unit",
        {
          renderers: Set(["unit"]),
          defaultValue: PredicateValue.Default.unit(),
          defaultState: UnitFormState.Default(),
        },
      ] as [string, PrimitiveBuiltIn],
      [
        "string",
        {
          renderers: Set(["string"]),
          defaultValue: PredicateValue.Default.string(),
          defaultState: StringFormState.Default(),
        },
      ] as [string, PrimitiveBuiltIn],
      [
        "number",
        {
          renderers: Set(["number"]),
          defaultValue: PredicateValue.Default.number(),
          defaultState: NumberFormState.Default(),
        },
      ] as [string, PrimitiveBuiltIn],
      [
        "boolean",
        {
          renderers: Set(["boolean"]),
          defaultValue: PredicateValue.Default.boolean(),
          defaultState: BooleanFormState.Default(),
        },
      ] as [string, PrimitiveBuiltIn],
      [
        "date",
        {
          renderers: Set(["date"]),
          defaultValue: PredicateValue.Default.date(),
          defaultState: DateFormState.Default(),
        },
      ] as [string, PrimitiveBuiltIn],
      [
        "Date",
        {
          renderers: Set(["date"]),
          defaultValue: PredicateValue.Default.date(),
          defaultState: DateFormState.Default(),
        },
      ] as [string, PrimitiveBuiltIn],
      [
        "base64File",
        {
          renderers: Set(["base64File"]),
          defaultValue: PredicateValue.Default.string(),
          defaultState: Base64FileFormState.Default(),
        },
      ] as [string, PrimitiveBuiltIn],
      [
        "secret",
        {
          renderers: Set(["secret"]),
          defaultValue: PredicateValue.Default.string(),
          defaultState: SecretFormState.Default(),
        },
      ] as [string, PrimitiveBuiltIn],
    ]),
    generics: Map<string, GenericBuiltIn>([
      [
        "SingleSelection",
        {
          defaultValue: PredicateValue.Default.option(
            false,
            PredicateValue.Default.unit(),
          ),
        },
      ],
      [
        "MultiSelection",
        { defaultValue: PredicateValue.Default.record(OrderedMap()) },
      ],
      [
        "List",
        {
          defaultValue: PredicateValue.Default.tuple(List()),
          defaultState: ListFieldState().zero(),
        },
      ],
      [
        "Map",
        {
          defaultValue: PredicateValue.Default.tuple(List()),
          defaultState: MapFieldState().zero(),
        },
      ],
      [
        "Tuple",
        {
          defaultValue: (values: PredicateValue[]) =>
            PredicateValue.Default.tuple(List(values)),
          defaultState: (argStates: List<any>) =>
            TupleFormState().Default(argStates),
        },
      ],
      [
        "Sum",
        {
          defaultValue: (kind: "l" | "r", value: PredicateValue) =>
            PredicateValue.Default.sum(
              kind == "l" ? Sum.Default.left(value) : Sum.Default.right(value),
            ),
          defaultState: (left: any, right: any) =>
            SumFormState().Default({ left, right }),
        },
      ],
      [
        "Union",
        {
          defaultValue: (fields: Map<string, PredicateValue>) =>
            PredicateValue.Default.record(fields),
          // TODO : Union Renderer form
          // defaultState: (fields: Map<string, any>) =>
          //   UnionFormState().Default(fields),
        },
      ],
    ]),
    renderers: {
      unit: Set(),
      boolean: Set(),
      date: Set(),
      enumMultiSelection: Set(),
      enumSingleSelection: Set(),
      streamMultiSelection: Set(),
      streamSingleSelection: Set(),
      number: Set(),
      string: Set(),
      list: Set(),
      tuple: Set(),
      base64File: Set(),
      secret: Set(),
      map: Set(),
      sum: Set(),
    },
  };
  Object.keys(builtins.renderers).forEach((_categoryName) => {
    const categoryName = _categoryName as keyof BuiltIns["renderers"];
    if (categoryName in fieldViews) {
      Object.keys(fieldViews[categoryName]).forEach((viewName) => {
        builtins.renderers[categoryName] =
          builtins.renderers[categoryName].add(viewName);
      });
    }
  });
  return builtins;
};

export const defaultState =
  <T>(
    types: Map<TypeName, ParsedType<T>>,
    builtIns: BuiltIns,
    injectedPrimitives?: InjectedPrimitives<T>,
  ) =>
  (
    t: ParsedType<T>,
  ): {
    commonFormState: CommonFormState;
    customFormState: object;
  } => {
    if (
      t.kind == "union" ||
      (t.kind == "application" &&
        (t.value == "SingleSelection" || t.value == "MultiSelection"))
    ) {
      throw Error(
        `t.kind: ${t.kind} ${
          t.kind == "application" ? ` t.value: ${t.value}` : ""
        } not currently supported by the defaultState function`,
      );
    }

    if (t.kind == "primitive") {
      const primitive = builtIns.primitives.get(t.value as string);
      const injectedPrimitive = injectedPrimitives?.injectedPrimitives.get(
        t.value as keyof T,
      );
      if (primitive != undefined) return primitive.defaultState;
      if (injectedPrimitive != undefined)
        return {
          commonFormState: CommonFormState.Default(),
          customFormState: injectedPrimitive.defaultState,
        };
    }

    if (t.kind == "application" && t.value == "Tuple") {
      return builtIns.generics
        .get("Tuple")!
        .defaultState(
          t.args.map((_) =>
            defaultState(types, builtIns, injectedPrimitives)(_),
          ),
        );
    }

    if (t.kind == "application" && t.value == "Sum") {
      return builtIns.generics
        .get("Sum")!
        .defaultState(
          defaultState(types, builtIns, injectedPrimitives)(t.args[0]),
          defaultState(types, builtIns, injectedPrimitives)(t.args[1]),
        );
    }

    if (t.kind == "application") {
      const generic = builtIns.generics.get(t.value);
      if (generic) return generic.defaultState;
    }

    if (t.kind == "lookup")
      return defaultState(
        types,
        builtIns,
        injectedPrimitives,
      )(types.get(t.name)!);

    throw Error(
      `cannot find type ${JSON.stringify(t)} when resolving defaultValue`,
    );
  };

export const defaultValue =
  <T>(
    types: Map<TypeName, ParsedType<T>>,
    builtIns: BuiltIns,
    injectedPrimitives?: InjectedPrimitives<T>,
  ) =>
  (t: ParsedType<T>): PredicateValue => {
    if (t.kind == "primitive") {
      const primitive = builtIns.primitives.get(t.value as string);
      const injectedPrimitive = injectedPrimitives?.injectedPrimitives.get(
        t.value as keyof T,
      );
      if (primitive != undefined) return primitive.defaultValue;
      if (injectedPrimitive != undefined) return injectedPrimitive.defaultValue;
    }

    if (t.kind == "application" && t.value == "Tuple") {
      return builtIns.generics
        .get("Tuple")!
        .defaultValue(
          t.args.map((_) =>
            defaultValue(types, builtIns, injectedPrimitives)(_),
          ),
        );
    }
    if (t.kind == "application" && t.value == "Sum") {
      return builtIns.generics
        .get("Sum")!
        .defaultValue(
          defaultValue(types, builtIns, injectedPrimitives)(t.args[0]),
          defaultValue(types, builtIns, injectedPrimitives)(t.args[1]),
        );
    }
    if (t.kind == "application") {
      const generic = builtIns.generics.get(t.value);
      if (generic) return generic.defaultValue;
    }

    if (t.kind == "lookup")
      return defaultValue(
        types,
        builtIns,
        injectedPrimitives,
      )(types.get(t.name)!);

    if (t.kind == "record") {
      let res = {} as Record<string, PredicateValue>;

      t.fields.forEach((field, fieldName) => {
        res[fieldName] = defaultValue(
          types,
          builtIns,
          injectedPrimitives,
        )(field);
      });
      return PredicateValue.Default.record(OrderedMap(res));
    }

    if (t.kind == "table") {
      let res = {} as Record<string, PredicateValue>;

      const tableType = types.get(t.tableType);

      if (tableType == undefined) {
        throw Error(
          `cannot find type ${t.tableType} when resolving defaultValue`,
        );
      }

      if (tableType.kind != "record") {
        throw Error(`tableType ${t.tableType} is not a record`);
      }

      tableType.fields.forEach((column, columnName) => {
        res[columnName] = defaultValue(
          types,
          builtIns,
          injectedPrimitives,
        )(column);
      });
      return PredicateValue.Default.record(OrderedMap(res));
    }
    throw Error(
      `cannot find type ${JSON.stringify(t)} when resolving defaultValue`,
    );
  };

export const fromAPIRawValue =
  <T extends { [key in keyof T]: { type: any; state: any } }>(
    t: ParsedType<T>,
    types: Map<TypeName, ParsedType<T>>,
    builtIns: BuiltIns,
    converters: ApiConverters<T>,
    injectedPrimitives?: InjectedPrimitives<T>,
  ) =>
  (raw: any): ValueOrErrors<PredicateValue, string> => {
    if (t.kind == "primitive") {
      // unit is a special kind of primitive
      if (t.value == "unit") {
        return ValueOrErrors.Default.return(PredicateValue.Default.unit());
      }

      if (
        !PredicateValue.Operations.IsPrimitive(raw) &&
        !injectedPrimitives?.injectedPrimitives
          .keySeq()
          .contains(t.value as keyof T)
      ) {
        return ValueOrErrors.Default.throwOne(
          `primitive expected but got ${JSON.stringify(raw)}`,
        );
      }
      return ValueOrErrors.Default.return(
        converters[t.value].fromAPIRawValue(raw),
      );
    }
    if (t.kind == "union") {
      const result = converters[t.kind].fromAPIRawValue(raw);
      const caseType = t.args.get(result.caseName);
      if (caseType == undefined)
        return ValueOrErrors.Default.throwOne(
          `union case ${result.caseName} not found in type ${JSON.stringify(
            t,
          )}`,
        );

      return fromAPIRawValue(
        caseType.fields,
        types,
        builtIns,
        converters,
        injectedPrimitives,
      )(result.fields).Then((fields) => {
        return ValueOrErrors.Default.return(
          PredicateValue.Default.unionCase(
            result.caseName,
            fields as ValueRecord,
          ),
        );
      });
    }

    if (t.kind == "application") {
      if (t.value == "SingleSelection") {
        const result = converters[t.value].fromAPIRawValue(raw);
        const isSome = result.kind == "l";
        const value = isSome
          ? PredicateValue.Default.record(OrderedMap(result.value))
          : PredicateValue.Default.unit();

        return ValueOrErrors.Default.return(
          PredicateValue.Default.option(isSome, value),
        );
      }
      if (t.value == "MultiSelection") {
        const result = converters[t.value].fromAPIRawValue(raw);
        const values = result.map((_) =>
          PredicateValue.Default.record(OrderedMap(_)),
        );
        return ValueOrErrors.Default.return(
          PredicateValue.Default.record(OrderedMap(values)),
        );
      }
      if (t.value == "List") {
        const result = converters[t.value].fromAPIRawValue(raw);
        return ValueOrErrors.Operations.All(
          result.map((_) =>
            fromAPIRawValue(
              t.args[0],
              types,
              builtIns,
              converters,
              injectedPrimitives,
            )(_),
          ),
        ).Then((values) =>
          ValueOrErrors.Default.return(PredicateValue.Default.tuple(values)),
        );
      }
      if (t.value == "Map" && t.args.length == 2) {
        const result = converters[t.value].fromAPIRawValue(raw);

        return ValueOrErrors.Operations.All(
          List<ValueOrErrors<PredicateValue, string>>(
            result.map((_) =>
              fromAPIRawValue(
                t.args[0],
                types,
                builtIns,
                converters,
                injectedPrimitives,
              )(_[0]).Then((key) =>
                fromAPIRawValue(
                  t.args[1],
                  types,
                  builtIns,
                  converters,
                  injectedPrimitives,
                )(_[1]).Then((value) =>
                  ValueOrErrors.Default.return(
                    PredicateValue.Default.tuple(List([key, value])),
                  ),
                ),
              ),
            ),
          ),
        ).Then((values) =>
          ValueOrErrors.Default.return(
            PredicateValue.Default.tuple(List(values)),
          ),
        );
      }

      if (t.value == "Tuple") {
        const result = converters[t.value].fromAPIRawValue(raw);
        return ValueOrErrors.Operations.All(
          List<ValueOrErrors<PredicateValue, string>>(
            result.map((_, index) =>
              fromAPIRawValue(
                t.args[index],
                types,
                builtIns,
                converters,
                injectedPrimitives,
              )(_),
            ),
          ),
        ).Then((values) =>
          ValueOrErrors.Default.return(
            PredicateValue.Default.tuple(List(values)),
          ),
        );
      }

      if (t.value === "Sum" && t.args.length === 2) {
        const result = converters[t.value].fromAPIRawValue(raw);

        return fromAPIRawValue(
          result.kind == "l" ? t.args[0] : t.args[1],
          types,
          builtIns,
          converters,
          injectedPrimitives,
        )(result.value).Then((value) =>
          ValueOrErrors.Default.return(
            PredicateValue.Default.sum(
              Sum.Updaters.map2(replaceWith(value), replaceWith(value))(result),
            ),
          ),
        );
      }
    }

    if (t.kind == "lookup")
      return fromAPIRawValue(
        types.get(t.name)!,
        types,
        builtIns,
        converters,
        injectedPrimitives,
      )(raw);

    if (t.kind == "record") {
      if (typeof raw != "object") {
        return ValueOrErrors.Default.throwOne(
          `object expected but got ${JSON.stringify(raw)}`,
        );
      }
      let result: Map<string, PredicateValue> = Map();
      let errors: List<string> = List();
      t.fields.forEach((fieldType, fieldName) => {
        const fieldValue = raw[fieldName];
        if (fieldValue !== null && fieldValue === undefined) {
          return;
        }
        const parsedValue = fromAPIRawValue(
          fieldType,
          types,
          builtIns,
          converters,
          injectedPrimitives,
        )(fieldValue);
        if (parsedValue.kind == "errors") {
          errors = errors.concat(parsedValue.errors);
        } else {
          result = result.set(fieldName, parsedValue.value);
        }
      });
      if (errors.size > 0) {
        return ValueOrErrors.Default.throw(errors);
      }
      return ValueOrErrors.Default.return(
        PredicateValue.Default.record(result),
      );
    }

    if (t.kind == "table") {
      const converted = converters["Table"].fromAPIRawValue(raw);
      let result: Map<string, ValueRecord> = Map();
      let errors: List<string> = List();
      const tableType = types.get(t.tableType);
      if (tableType == undefined) {
        throw Error(
          `cannot find type ${t.tableType} when resolving defaultValue`,
        );
      }
      if (tableType.kind != "record") {
        throw Error(`tableType ${t.tableType} is not a record`);
      }
      converted.data.forEach((row: any, rowIndex: string) => {
        let rowResult: Map<string, PredicateValue> = Map();
        tableType.fields.forEach((fieldType, fieldName) => {
          const fieldValue = row[fieldName];
          if (fieldValue != null && fieldValue == undefined) {
            return;
          }
          const parsedValue = fromAPIRawValue(
            fieldType,
            types,
            builtIns,
            converters,
            injectedPrimitives,
          )(fieldValue);
          if (parsedValue.kind == "errors") {
            errors = errors.concat(parsedValue.errors);
          } else {
            rowResult = rowResult.set(fieldName, parsedValue.value);
          }
        });
        result = result.set(
          rowIndex.toString(),
          PredicateValue.Default.record(rowResult),
        );
      });

      if (errors.size > 0) {
        return ValueOrErrors.Default.throw(errors);
      }
      return ValueOrErrors.Default.return(
        // PredicateValue.Default.record(result),
        PredicateValue.Default.record(
          OrderedMap<string, PredicateValue>([
            ["data", PredicateValue.Default.record(result)],
            ["hasMoreValues", converted.hasMoreValues],
            ["to", converted.to],
            ["from", converted.from],
          ]),
        ),
      );
    }

    return ValueOrErrors.Default.throwOne(
      `unsupported type ${JSON.stringify(t)} for raw: `,
    );
  };

export const toAPIRawValue =
  <T extends { [key in keyof T]: { type: any; state: any } }>(
    t: ParsedType<T>,
    types: Map<TypeName, ParsedType<T>>,
    builtIns: BuiltIns,
    converters: ApiConverters<T>,
    injectedPrimitives?: InjectedPrimitives<T>,
  ) =>
  (raw: PredicateValue, formState: any): ValueOrErrors<any, string> => {
    if (t.kind == "primitive") {
      if (t.value == "unit") {
        return ValueOrErrors.Default.return(unit);
      }
      return ValueOrErrors.Operations.Return(
        converters[t.value as string | keyof T].toAPIRawValue([
          raw,
          formState?.commonFormState?.modifiedByUser ?? false,
        ]),
      );
    }

    if (t.kind == "union") {
      if (!PredicateValue.Operations.IsRecord(raw)) {
        return ValueOrErrors.Default.throwOne(
          `Option expected but got ${JSON.stringify(raw)}`,
        );
      }
      const caseName = raw.fields.get("caseName");
      if (
        caseName == undefined ||
        !PredicateValue.Operations.IsString(caseName)
      ) {
        return ValueOrErrors.Default.throwOne(
          `caseName expected but got ${JSON.stringify(raw)}`,
        );
      }
      const fields = raw.fields.get("fields");
      if (fields == undefined || !PredicateValue.Operations.IsRecord(fields)) {
        return ValueOrErrors.Default.throwOne(
          `fields expected but got ${JSON.stringify(raw)}`,
        );
      }
      const rawUnionCase = {
        caseName,
        fields: fields.fields.toJS(),
      };
      return ValueOrErrors.Operations.Return(
        converters["union"].toAPIRawValue([
          rawUnionCase,
          formState?.commonFormState?.modifiedByUser ?? false,
        ]),
      );
    }
    if (t.kind == "application") {
      if (t.value == "SingleSelection") {
        if (!PredicateValue.Operations.IsOption(raw)) {
          return ValueOrErrors.Default.throwOne(
            `Option expected but got ${JSON.stringify(raw)}`,
          );
        }

        if (raw.isSome) {
          if (!PredicateValue.Operations.IsRecord(raw.value)) {
            return ValueOrErrors.Default.throwOne(
              `Record expected but got ${JSON.stringify(raw.value)}`,
            );
          }
          const rawValue = raw.value.fields.toJS();
          if (
            !CollectionReference.Operations.IsCollectionReference(rawValue) &&
            !EnumReference.Operations.IsEnumReference(rawValue)
          ) {
            return ValueOrErrors.Default.throwOne(
              `CollectionReference or EnumReference expected but got ${rawValue}`,
            );
          }

          return ValueOrErrors.Operations.Return(
            converters[t.value].toAPIRawValue([
              Sum.Default.left(rawValue),
              formState?.commonFormState?.modifiedByUser ?? false,
            ]),
          );
        } else {
          return ValueOrErrors.Operations.Return(
            converters[t.value].toAPIRawValue([
              Sum.Default.right("no selection"),
              formState?.commonFormState?.modifiedByUser ?? false,
            ]),
          );
        }
      }

      if (t.value == "MultiSelection") {
        if (!PredicateValue.Operations.IsRecord(raw)) {
          return ValueOrErrors.Default.throwOne(
            `Record expected but got multi selection of ${JSON.stringify(raw)}`,
          );
        }

        const rawValue: Map<
          string,
          ValueOrErrors<CollectionReference | EnumReference, string>
        > = raw.fields.map((value) => {
          if (!PredicateValue.Operations.IsRecord(value)) {
            return ValueOrErrors.Default.throwOne(
              `Record expected but got ${JSON.stringify(value)}`,
            );
          }
          const fieldsObject = value.fields.toJS();

          if (
            !CollectionReference.Operations.IsCollectionReference(
              fieldsObject,
            ) &&
            !EnumReference.Operations.IsEnumReference(fieldsObject)
          ) {
            return ValueOrErrors.Default.throwOne(
              `CollectionReference or EnumReference expected but got ${JSON.stringify(
                fieldsObject,
              )}`,
            );
          }
          return ValueOrErrors.Default.return(fieldsObject);
        });

        return ValueOrErrors.Operations.All(rawValue.valueSeq().toList()).Then(
          (values) =>
            ValueOrErrors.Default.return(
              converters["MultiSelection"].toAPIRawValue([
                OrderedMap<string, EnumReference | CollectionReference>(
                  values
                    .map((v): [string, EnumReference | CollectionReference] => {
                      if (
                        CollectionReference.Operations.IsCollectionReference(v)
                      ) {
                        return [v.Id, v];
                      }
                      return [v.Value, v];
                    })
                    .toArray(),
                ),
                formState?.commonFormState?.modifiedByUser ?? false,
              ]),
            ),
        );
      }
      if (t.value == "List") {
        if (!PredicateValue.Operations.IsTuple(raw)) {
          return ValueOrErrors.Default.throwOne(
            `Tuple expected but got list of${JSON.stringify(raw)}`,
          );
        }
        return ValueOrErrors.Operations.All(
          List(
            raw.values.map((value, index) =>
              toAPIRawValue(
                t.args[0],
                types,
                builtIns,
                converters,
                injectedPrimitives,
              )(value, formState?.elementFormStates?.get(index)),
            ),
          ),
        ).Then((values) =>
          ValueOrErrors.Default.return(
            converters["List"].toAPIRawValue([
              values,
              formState?.commonFormState?.modifiedByUser ?? false,
            ]),
          ),
        );
      }
      if (t.value == "Map") {
        const keyValues = (raw as ValueTuple).values.map((keyValue, index) => {
          return toAPIRawValue(
            t.args[0],
            types,
            builtIns,
            converters,
            injectedPrimitives,
          )(
            (keyValue as ValueTuple).values.get(0)!,
            formState?.elementFormStates?.get(index)?.KeyFormState,
          )
            .Then((possiblyUndefinedKey) => {
              if (
                possiblyUndefinedKey == undefined ||
                possiblyUndefinedKey == null ||
                possiblyUndefinedKey == "" ||
                (typeof possiblyUndefinedKey == "object" &&
                  (Object.keys(possiblyUndefinedKey).length == 0 ||
                    ("IsSome" in possiblyUndefinedKey &&
                      !possiblyUndefinedKey.IsSome)))
              ) {
                return ValueOrErrors.Default.throwOne(
                  `A mapped key is undefined for type ${JSON.stringify(
                    t.args[0],
                  )}`,
                );
              } else {
                return ValueOrErrors.Default.return(possiblyUndefinedKey);
              }
            })
            .Then((key) =>
              toAPIRawValue(
                t.args[1],
                types,
                builtIns,
                converters,
                injectedPrimitives,
              )(
                (keyValue as ValueTuple).values.get(1)!,
                formState?.elementFormStates.get(index)?.ValueFormState,
              ).Then((value) =>
                ValueOrErrors.Default.return([key, value] as [any, any]),
              ),
            );
        });

        return ValueOrErrors.Operations.All(List(keyValues)).Then((values) => {
          if (
            values.map((kv) => JSON.stringify(kv[0])).toSet().size !=
            values.size
          ) {
            return ValueOrErrors.Default.throwOne(
              "Keys in the map are not unique",
            );
          }
          return ValueOrErrors.Operations.Return(
            converters["Map"].toAPIRawValue([
              values,
              formState.modifiedByUser ?? false,
            ]),
          );
        });
      }

      if (t.value === "Sum") {
        if (!PredicateValue.Operations.IsSum(raw)) {
          return ValueOrErrors.Default.throwOne(
            `Sum expected but got ${JSON.stringify(raw)}`,
          );
        }

        return toAPIRawValue(
          raw.value.kind == "l" ? t.args[0] : t.args[1],
          types,
          builtIns,
          converters,
          injectedPrimitives,
        )(
          raw.value.value,
          raw.value.kind == "l"
            ? formState.customFormState.left
            : formState.customFormState.right,
        ).Then((value) =>
          ValueOrErrors.Default.return(
            converters["Sum"].toAPIRawValue([
              raw.value.kind == "l"
                ? Sum.Default.left(value)
                : Sum.Default.right(value),
              formState?.commonFormState?.modifiedByUser ?? false,
            ]),
          ),
        );
      }

      if (t.value == "Tuple") {
        if (!PredicateValue.Operations.IsTuple(raw)) {
          return ValueOrErrors.Default.throwOne(
            `Tuple expected but got ${JSON.stringify(raw)}`,
          );
        }
        return ValueOrErrors.Operations.All(
          List(
            raw.values.map((value, index) => {
              return toAPIRawValue(
                t.args[index],
                types,
                builtIns,
                converters,
                injectedPrimitives,
              )(value, formState?.elementFormStates?.get(index));
            }),
          ),
        ).Then((values) =>
          ValueOrErrors.Default.return(
            converters["Tuple"].toAPIRawValue([
              values,
              formState?.commonFormState?.modifiedByUser ?? false,
            ]),
          ),
        );
      }
    }

    if (t.kind == "lookup")
      return toAPIRawValue(
        types.get(t.name)!,
        types,
        builtIns,
        converters,
        injectedPrimitives,
      )(raw, formState);

    if (t.kind == "record") {
      if (!PredicateValue.Operations.IsRecord(raw)) {
        return ValueOrErrors.Default.throwOne(
          `Record expected but got ${JSON.stringify(raw)}`,
        );
      }
      const res = [] as any;
      t.fields.forEach((fieldType, fieldName) => {
        // nullish coalescing operator on state used for extended type state, but this maybe should have its own kind
        const rawField = raw.fields.get(fieldName);
        if (rawField == undefined) {
          return;
        }
        res.push([
          fieldName,
          toAPIRawValue(
            fieldType,
            types,
            builtIns,
            converters,
            injectedPrimitives,
          )(
            raw.fields.get(fieldName)!,
            formState?.formFieldStates?.[fieldName] ?? formState,
          ),
        ]);
      });
      const errors: ValueOrErrors<
        List<any>,
        string
      > = ValueOrErrors.Operations.All(
        List(
          res.map(
            ([_, value]: [_: string, value: ValueOrErrors<any, string>]) =>
              value,
          ),
        ),
      );
      if (errors.kind == "errors") return errors;

      return ValueOrErrors.Operations.Return(
        res.reduce(
          (acc: any, [fieldName, value]: [fieldName: string, value: any]) => {
            acc[fieldName] = value.value;
            return acc;
          },
          {} as any,
        ),
      );
    }

    return ValueOrErrors.Operations.Return(
      defaultValue(types, builtIns, injectedPrimitives)(t),
    );
  };
