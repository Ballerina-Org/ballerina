import { Set, List, Map, isMap, OrderedMap } from "immutable";
import {} from "../../../../../../parser/domains/built-ins/state";
import { ValueOrErrors } from "../../../../../../../../collections/domains/valueOrErrors/state";
import {
  Unit,
  DispatchGenericType,
  DispatchGenericTypes,
  MapRepo,
  DispatchInjectedPrimitives,
  ParsedType,
  isString,
} from "../../../../../../../../../main";

export const DispatchisString = (_: any): _ is string => typeof _ == "string";
export const DispatchIsObject = (_: any): _ is object => typeof _ == "object";
export const DispatchIsGenericType = (_: any): _ is DispatchGenericType =>
  _ && DispatchGenericTypes.includes(_);
export const DispatchHasFun = (_: any): _ is { fun: string } =>
  DispatchIsObject(_) && "fun" in _ && DispatchisString(_.fun);
export const DispatchHasArgs = (_: any): _ is { args: Array<any> } =>
  DispatchIsObject(_) && "args" in _ && Array.isArray(_.args);
export type DispatchCaseName = string;
export type DispatchFieldName = string;
export type DispatchTypeName = string;

export type SerializedApplicationType<T> = {
  fun?: DispatchGenericType;
  args?: Array<SerializedType<T>>;
};

export type SerializedUnionType = {
  fun?: "Union";
  args?: Array<string>;
};

export type SerializedOptionType = {
  fun?: "Option";
  args?: Array<SerializedType<any>>;
};

export type SerializedRecordType = {
  extends?: Array<DispatchTypeName>;
  fields?: object;
};

export type SerializedKeyOfType<T> = {
  fun?: "KeyOf";
  args?: Array<string>;
};

export type SerializedTranslationOverrideType = {
  fun?: "TranslationOverride";
  args?: Array<string>;
};

export type ValidatedSerializedKeyOfType<T> = {
  fun: "KeyOf";
  args: [string, Array<string>?];
};

export type SerializedLookupType = string;

export type SerializedType<T> =
  | Unit
  | DispatchPrimitiveTypeName<T>
  | SerializedApplicationType<T>
  | SerializedLookupType
  | SerializedUnionType
  | SerializedRecordType
  | SerializedOptionType
  | SerializedKeyOfType<T>
  | SerializedTranslationOverrideType;

export const DispatchPrimitiveTypeNames = [
  "unit",
  "guid", //resolves to string
  "entityIdString", //resolves to string
  "entityIdUUID", //resolves to string
  "calculatedDisplayValue", //resolves to string
  "string",
  "number",
  "boolean",
  "Date",
  "base64File",
  "secret",
] as const;

const STRINGY_TYPES = [
  "guid",
  "entityIdUUID",
  "entityIdString",
  "calculatedDisplayValue",
] as const;
export type DispatchPrimitiveTypeName<T> =
  | (typeof DispatchPrimitiveTypeNames)[number]
  | keyof T;

export const SerializedType = {
  isExtendedType: <T>(
    type: SerializedType<T>,
  ): type is SerializedType<T> & { extends: Array<DispatchTypeName> } =>
    typeof type == "object" &&
    "extends" in type &&
    Array.isArray(type.extends) &&
    type.extends.length > 0 &&
    type.extends.every(DispatchisString),
  hasFields: <T>(type: SerializedType<T>): type is { fields: any } =>
    typeof type == "object" && "fields" in type,
  isPrimitive: <T>(
    _: SerializedType<T>,
    injectedPrimitives: DispatchInjectedPrimitives<T> | undefined,
  ): _ is DispatchPrimitiveTypeName<T> =>
    Boolean(
      DispatchPrimitiveTypeNames.some((__) => _ == __) ||
        injectedPrimitives?.has(_ as keyof T),
    ),
  isApplication: <T>(
    _: SerializedType<T>,
  ): _ is { fun: DispatchGenericType; args: Array<SerializedType<T>> } =>
    DispatchHasFun(_) && DispatchIsGenericType(_.fun) && DispatchHasArgs(_),
  isLookup: <T>(
    _: unknown,
    injectedPrimitives: DispatchInjectedPrimitives<T> | undefined,
  ): _ is DispatchTypeName =>
    DispatchisString(_) &&
    !injectedPrimitives?.has(_ as keyof T) &&
    !(_ in DispatchPrimitiveTypeNames),
  isList: <T>(
    _: SerializedType<T>,
  ): _ is { fun: "List"; args: Array<SerializedType<T>> } =>
    SerializedType.isApplication(_) && _.fun == "List" && _.args.length == 1,
  isMap: <T>(
    _: SerializedType<T>,
  ): _ is { fun: "Map"; args: Array<SerializedType<T>> } =>
    SerializedType.isApplication(_) && _.fun == "Map" && _.args.length == 2,
  isSum: <T>(
    _: SerializedType<T>,
  ): _ is { fun: "Sum"; args: Array<SerializedType<T>> } =>
    SerializedType.isApplication(_) && _.fun == "Sum" && _.args.length == 2,
  isSumUnitDate: <T>(_: SerializedType<T>): _ is "SumUnitDate" =>
    typeof _ == "string" && _ == "SumUnitDate",
  isSingleSelection: <T>(
    _: SerializedType<T>,
  ): _ is { fun: "SingleSelection"; args: Array<SerializedType<T>> } =>
    SerializedType.isApplication(_) &&
    _.fun == "SingleSelection" &&
    _.args.length == 1,
  isMultiSelection: <T>(
    _: SerializedType<T>,
  ): _ is { fun: "MultiSelection"; args: Array<SerializedType<T>> } =>
    SerializedType.isApplication(_) &&
    _.fun == "MultiSelection" &&
    _.args.length == 1,
  isUnion: <T>(
    _: SerializedType<T>,
  ): _ is {
    fun: "Union";
    args: Array<{
      caseName: string;
      fields: string | SerializedType<T>;
    }>;
  } =>
    DispatchHasFun(_) &&
    DispatchIsGenericType(_.fun) &&
    DispatchHasArgs(_) &&
    _.fun == "Union" &&
    _.args.length > 0 &&
    _.args.every(
      (arg) =>
        (DispatchIsObject(arg) && "caseName" in arg && !("fields" in arg)) ||
        ("fields" in arg &&
          (DispatchisString(arg.fields) || DispatchIsObject(arg.fields))),
    ),
  isTuple: <T>(
    _: SerializedType<T>,
  ): _ is { fun: "Tuple"; args: Array<SerializedType<T>> } =>
    SerializedType.isApplication(_) && _.fun == "Tuple",
  isRecord: <T>(
    _: unknown,
  ): _ is { fields: Object; extends?: Array<DispatchTypeName> } =>
    _ != null &&
    typeof _ == "object" &&
    "fields" in _ &&
    (DispatchIsObject(_.fields) || DispatchisString(_.fields)),
  isTable: <T>(
    _: SerializedType<T>,
  ): _ is { fun: "Table"; args: Array<SerializedType<T>> } =>
    SerializedType.isApplication(_) && _.fun == "Table" && _.args.length == 1,
  isOption: <T>(
    _: SerializedType<T>,
  ): _ is { fun: "Option"; args: Array<SerializedType<T>> } =>
    typeof _ == "object" &&
    "fun" in _ &&
    _.fun == "Option" &&
    "args" in _ &&
    Array.isArray(_.args) &&
    _.args.length == 1,
  isUnit: <T>(_: SerializedType<T>): _ is string => _ == "unit",
  isKeyOf: <T>(_: SerializedType<T>): _ is ValidatedSerializedKeyOfType<T> =>
    typeof _ == "object" &&
    "fun" in _ &&
    _.fun == "KeyOf" &&
    "args" in _ &&
    Array.isArray(_.args) &&
    (_.args.length == 1 || (_.args.length == 2 && Array.isArray(_.args[1]))) &&
    DispatchisString(_.args[0]),
  isOne: <T>(
    _: SerializedType<T>,
  ): _ is { fun: "One"; args: Array<SerializedType<T>> } =>
    SerializedType.isApplication(_) && _.fun == "One" && _.args.length == 1,
  isReadOnly: <T>(
    _: SerializedType<T>,
  ): _ is { fun: "ReadOnly"; args: Array<SerializedType<T>> } =>
    SerializedType.isApplication(_) &&
    _.fun == "ReadOnly" &&
    _.args.length == 1,
  isRecordFields: (_: unknown) =>
    typeof _ == "object" && _ != null && !("fun" in _) && !("args" in _),
  isTranslationOverride: (
    _: unknown,
  ): _ is { fun: "TranslationOverride"; args: Array<string> } =>
    typeof _ == "object" &&
    _ != null &&
    "fun" in _ &&
    _.fun == "TranslationOverride" &&
    "args" in _ &&
    Array.isArray(_.args) &&
    _.args.length == 2 &&
    DispatchisString(_.args[0]),
};

export type StringSerializedType = string;

export type UnionType<T> = {
  kind: "union";
  args: Map<DispatchCaseName, DispatchParsedType<T>>;
  asString: () => StringSerializedType;
};

export const UnionType = {
  SerializeToString: (
    serializedArgs: Map<DispatchCaseName, StringSerializedType>,
  ): StringSerializedType => {
    return `[union; cases: {${serializedArgs.map((v, k) => `${k}: ${v}`).join(", ")}}]`;
  },
};

export type RecordType<T> = {
  kind: "record";
  fields: OrderedMap<DispatchFieldName, DispatchParsedType<T>>;
  asString: () => StringSerializedType;
};

export const RecordType = {
  SerializeToString: (
    serializedFields: OrderedMap<DispatchFieldName, StringSerializedType>,
  ): StringSerializedType => {
    return `[record; fields: {${serializedFields.map((v, k) => `${k}: ${v}`).join(", ")}}]`;
  },
};

export type LookupType = {
  kind: "lookup";
  name: string;
  asString: () => StringSerializedType;
};

export const LookupType = {
  SerializeToString: (name: string): StringSerializedType => {
    return `${name}`;
  },
};

export type DispatchPrimitiveType<T> = {
  kind: "primitive";
  name: DispatchPrimitiveTypeName<T>;
  asString: () => StringSerializedType;
};

export const DispatchPrimitiveType = {
  SerializeToString: <T>(
    name: DispatchPrimitiveTypeName<T>,
  ): StringSerializedType => {
    return `[primitive; name: ${String(name)}]`;
  },
};

export type SingleSelectionType<T> = {
  kind: "singleSelection";
  args: Array<DispatchParsedType<T>>;
  asString: () => StringSerializedType;
};

export const SingleSelectionType = {
  SerializeToString: (
    serializedArgs: Array<StringSerializedType>,
  ): StringSerializedType => {
    return `[singleSelection; args: [${serializedArgs.join(", ")}]]`;
  },
};

export type MultiSelectionType<T> = {
  kind: "multiSelection";
  args: Array<DispatchParsedType<T>>;
  asString: () => StringSerializedType;
};

export const MultiSelectionType = {
  SerializeToString: (
    serializedArgs: Array<StringSerializedType>,
  ): StringSerializedType => {
    return `[multiSelection; args: [${serializedArgs.join(", ")}]]`;
  },
};

export type ListType<T> = {
  kind: "list";
  args: Array<DispatchParsedType<T>>;
  asString: () => StringSerializedType;
};

export const ListType = {
  SerializeToString: (
    serializedArgs: Array<StringSerializedType>,
  ): StringSerializedType => {
    return `[list; args: [${serializedArgs.join(", ")}]]`;
  },
};

export type TupleType<T> = {
  kind: "tuple";
  args: Array<DispatchParsedType<T>>;
  asString: () => StringSerializedType;
};

export const TupleType = {
  SerializeToString: (
    serializedArgs: Array<StringSerializedType>,
  ): StringSerializedType => {
    return `[tuple; args: [${serializedArgs.join(", ")}]]`;
  },
};

export type SumType<T> = {
  kind: "sum";
  args: Array<DispatchParsedType<T>>;
  asString: () => StringSerializedType;
};

export const SumType = {
  SerializeToString: (
    serializedArgs: Array<StringSerializedType>,
  ): StringSerializedType => {
    return `[sum; args: [${serializedArgs.join(", ")}]]`;
  },
};

export type SumNType<T> = {
  kind: "sumN";
  args: Array<DispatchParsedType<T>>;
  asString: () => StringSerializedType;
};

export const SumNType = {
  SerializeToString: (
    serializedArgs: Array<StringSerializedType>,
    arity: number,
  ): StringSerializedType => {
    return `[sumN; arity: ${arity}; args: [${serializedArgs.join(", ")}]]`;
  },
};

export type MapType<T> = {
  kind: "map";
  args: Array<DispatchParsedType<T>>;
  asString: () => StringSerializedType;
};

export const MapType = {
  SerializeToString: (
    serializedArgs: Array<StringSerializedType>,
  ): StringSerializedType => {
    return `[map; args: [${serializedArgs.join(", ")}]]`;
  },
};

export type TableType<T> = {
  kind: "table";
  arg: LookupType;
  asString: () => StringSerializedType;
};

export const TableType = {
  SerializeToString: (
    serializedArg: StringSerializedType,
  ): StringSerializedType => {
    return `[table; arg: ${serializedArg}]`;
  },
};

export type ReadOnlyType<T> = {
  kind: "readOnly";
  arg: DispatchParsedType<T>;
  asString: () => StringSerializedType;
};

export const ReadOnlyType = {
  SerializeToString: (
    serializedArg: StringSerializedType,
  ): StringSerializedType => {
    return `[readOnly; arg: ${serializedArg}]`;
  },
};

export type OneType<T> = {
  kind: "one";
  arg: LookupType;
  asString: () => StringSerializedType;
};

export const OneType = {
  SerializeToString: (
    serializedArg: StringSerializedType,
  ): StringSerializedType => {
    return `[one; arg: ${serializedArg}]`;
  },
};

// Filters
export type FilterContainsType<T> = {
  kind: "contains";
  contains: DispatchParsedType<T>;
  asString: () => StringSerializedType;
};

export const FilterContainsType = {
  SerializeToString: (
    serializedContains: StringSerializedType,
  ): StringSerializedType => {
    return `[filter; contains: ${serializedContains}]`;
  },
};

export type FilterEqualsToType<T> = {
  kind: "=";
  equalsTo: DispatchParsedType<T>;
  asString: () => StringSerializedType;
};

export const FilterEqualsToType = {
  SerializeToString: (
    serializedEqualsTo: StringSerializedType,
  ): StringSerializedType => {
    return `[filter; equalsTo: ${serializedEqualsTo}]`;
  },
};

export type FilterNotEqualsToType<T> = {
  kind: "!=";
  notEqualsTo: DispatchParsedType<T>;
  asString: () => StringSerializedType;
};

export const FilterNotEqualsToType = {
  SerializeToString: (
    serializedNotEqualsTo: StringSerializedType,
  ): StringSerializedType => {
    return `[filter; != ${serializedNotEqualsTo}]`;
  },
};

export type FilterGreaterThanOrEqualsToType<T> = {
  kind: ">=";
  greaterThanOrEqualsTo: DispatchParsedType<T>;
  asString: () => StringSerializedType;
};

export const FilterGreaterThanOrEqualsToType = {
  SerializeToString: (
    serializedGreaterThanOrEqualsTo: StringSerializedType,
  ): StringSerializedType => {
    return `[filter; >= ${serializedGreaterThanOrEqualsTo}]`;
  },
};

export type FilterGreaterThanType<T> = {
  kind: ">";
  greaterThan: DispatchParsedType<T>;
  asString: () => StringSerializedType;
};

export const FilterGreaterThanType = {
  SerializeToString: (
    serializedGreaterThan: StringSerializedType,
  ): StringSerializedType => {
    return `[filter; > ${serializedGreaterThan}]`;
  },
};

export type FilterIsNotNullType<T> = {
  kind: "!=null";
  asString: () => StringSerializedType;
};

export const FilterIsNotNullType = {
  SerializeToString: (): StringSerializedType => {
    return `[filter; !=null]`;
  },
};

export type FilterIsNullType<T> = {
  kind: "=null";
  asString: () => StringSerializedType;
};

export const FilterIsNullType = {
  SerializeToString: (): StringSerializedType => {
    return `[filter; =null]`;
  },
};

export type FilterSmallerThanOrEqualsToType<T> = {
  kind: "<=";
  smallerThanOrEqualsTo: DispatchParsedType<T>;
  asString: () => StringSerializedType;
};

export const FilterSmallerThanOrEqualsToType = {
  SerializeToString: (
    serializedSmallerThanOrEqualsTo: StringSerializedType,
  ): StringSerializedType => {
    return `[filter; <= ${serializedSmallerThanOrEqualsTo}]`;
  },
};

export type FilterSmallerThanType<T> = {
  kind: "<";
  smallerThan: DispatchParsedType<T>;
  asString: () => StringSerializedType;
};

export const FilterSmallerThanType = {
  SerializeToString: (
    serializedSmallerThan: StringSerializedType,
  ): StringSerializedType => {
    return `[filter; < ${serializedSmallerThan}]`;
  },
};

export type FilterStartsWithType<T> = {
  kind: "startsWith";
  startsWith: DispatchParsedType<T>;
  asString: () => StringSerializedType;
};

export const FilterStartsWithType = {
  SerializeToString: (
    serializedStartsWith: StringSerializedType,
  ): StringSerializedType => {
    return `[filter; startsWith ${serializedStartsWith}]`;
  },
};

export type FilterType<T> =
  | FilterContainsType<T>
  | FilterEqualsToType<T>
  | FilterNotEqualsToType<T>
  | FilterGreaterThanOrEqualsToType<T>
  | FilterGreaterThanType<T>
  | FilterIsNotNullType<T>
  | FilterIsNullType<T>
  | FilterSmallerThanOrEqualsToType<T>
  | FilterSmallerThanType<T>
  | FilterStartsWithType<T>;

export const FilterTypeKinds = [
  "contains",
  "=",
  "!=",
  ">=",
  ">",
  "!=null",
  "=null",
  "<=",
  "<",
  "startsWith",
] as const;

export type FilterTypeKind = (typeof FilterTypeKinds)[number];

export type DispatchParsedType<T> =
  | RecordType<T>
  | LookupType
  | DispatchPrimitiveType<T>
  | UnionType<T>
  | SingleSelectionType<T>
  | MultiSelectionType<T>
  | ListType<T>
  | TupleType<T>
  | SumType<T>
  | SumNType<T>
  | MapType<T>
  | TableType<T>
  | OneType<T>
  | ReadOnlyType<T>
  | FilterType<T>;

export const DispatchParsedType = {
  Default: {
    table: <T>(arg: LookupType): TableType<T> => ({
      kind: "table",
      arg,
      asString: () => TableType.SerializeToString(arg.asString()),
    }),
    record: <T>(
      fields: Map<DispatchFieldName, DispatchParsedType<T>>,
    ): RecordType<T> => ({
      kind: "record",
      fields,
      asString: () =>
        RecordType.SerializeToString(fields.map((v) => v.asString())),
    }),
    primitive: <T>(
      name: DispatchPrimitiveTypeName<T>,
    ): DispatchParsedType<T> => ({
      kind: "primitive",
      name,
      asString: () => DispatchPrimitiveType.SerializeToString(name),
    }),
    singleSelection: <T>(
      args: Array<DispatchParsedType<T>>,
    ): DispatchParsedType<T> => ({
      kind: "singleSelection",
      args,
      asString: () =>
        SingleSelectionType.SerializeToString(args.map((v) => v.asString())),
    }),
    multiSelection: <T>(
      args: Array<DispatchParsedType<T>>,
    ): DispatchParsedType<T> => ({
      kind: "multiSelection",
      args,
      asString: () =>
        MultiSelectionType.SerializeToString(args.map((v) => v.asString())),
    }),
    list: <T>(args: Array<DispatchParsedType<T>>): DispatchParsedType<T> => ({
      kind: "list",
      args,
      asString: () => ListType.SerializeToString(args.map((v) => v.asString())),
    }),
    tuple: <T>(args: Array<DispatchParsedType<T>>): DispatchParsedType<T> => ({
      kind: "tuple",
      args,
      asString: () =>
        TupleType.SerializeToString(args.map((v) => v.asString())),
    }),
    sum: <T>(args: Array<DispatchParsedType<T>>): DispatchParsedType<T> => ({
      kind: "sum",
      args,
      asString: () => SumType.SerializeToString(args.map((v) => v.asString())),
    }),
    sumN: <T>(args: Array<DispatchParsedType<T>>): SumNType<T> => ({
      kind: "sumN",
      args,
      asString: () =>
        SumNType.SerializeToString(
          args.map((v) => v.asString()),
          args.length,
        ),
    }),
    map: <T>(args: Array<DispatchParsedType<T>>): DispatchParsedType<T> => ({
      kind: "map",
      args,
      asString: () => MapType.SerializeToString(args.map((v) => v.asString())),
    }),
    union: <T>(
      args: Map<DispatchCaseName, DispatchParsedType<T>>,
    ): DispatchParsedType<T> => ({
      kind: "union",
      args,
      asString: () =>
        UnionType.SerializeToString(args.map((v) => v.asString())),
    }),
    readOnly: <T>(arg: DispatchParsedType<T>): ReadOnlyType<T> => ({
      kind: "readOnly",
      arg,
      asString: () => ReadOnlyType.SerializeToString(arg.asString()),
    }),
    lookup: <T>(name: string): LookupType => ({
      kind: "lookup",
      name,
      asString: () => LookupType.SerializeToString(name),
    }),
    one: <T>(arg: LookupType): OneType<T> => ({
      kind: "one",
      arg,
      asString: () => OneType.SerializeToString(arg.asString()),
    }),
    filterContains: <T>(
      contains: DispatchParsedType<T>,
    ): FilterContainsType<T> => ({
      kind: "contains",
      contains,
      asString: () => FilterContainsType.SerializeToString(contains.asString()),
    }),
    filterEqualsTo: <T>(
      equalsTo: DispatchParsedType<T>,
    ): FilterEqualsToType<T> => ({
      kind: "=",
      equalsTo,
      asString: () => FilterEqualsToType.SerializeToString(equalsTo.asString()),
    }),
    filterNotEqualsTo: <T>(
      notEqualsTo: DispatchParsedType<T>,
    ): FilterNotEqualsToType<T> => ({
      kind: "!=",
      notEqualsTo,
      asString: () =>
        FilterNotEqualsToType.SerializeToString(notEqualsTo.asString()),
    }),
    filterGreaterThanOrEqualsTo: <T>(
      greaterThanOrEqualsTo: DispatchParsedType<T>,
    ): FilterGreaterThanOrEqualsToType<T> => ({
      kind: ">=",
      greaterThanOrEqualsTo,
      asString: () =>
        FilterGreaterThanOrEqualsToType.SerializeToString(
          greaterThanOrEqualsTo.asString(),
        ),
    }),
    filterGreaterThan: <T>(
      greaterThan: DispatchParsedType<T>,
    ): FilterGreaterThanType<T> => ({
      kind: ">",
      greaterThan,
      asString: () =>
        FilterGreaterThanType.SerializeToString(greaterThan.asString()),
    }),
    filterIsNotNull: <T>(): FilterIsNotNullType<T> => ({
      kind: "!=null",
      asString: () => FilterIsNotNullType.SerializeToString(),
    }),
    filterIsNull: <T>(): FilterIsNullType<T> => ({
      kind: "=null",
      asString: () => FilterIsNullType.SerializeToString(),
    }),
    filterSmallerThanOrEqualsTo: <T>(
      smallerThanOrEqualsTo: DispatchParsedType<T>,
    ): FilterSmallerThanOrEqualsToType<T> => ({
      kind: "<=",
      smallerThanOrEqualsTo,
      asString: () =>
        FilterSmallerThanOrEqualsToType.SerializeToString(
          smallerThanOrEqualsTo.asString(),
        ),
    }),
    filterSmallerThan: <T>(
      smallerThan: DispatchParsedType<T>,
    ): FilterSmallerThanType<T> => ({
      kind: "<",
      smallerThan,
      asString: () =>
        FilterSmallerThanType.SerializeToString(smallerThan.asString()),
    }),
    filterStartsWith: <T>(
      startsWith: DispatchParsedType<T>,
    ): FilterStartsWithType<T> => ({
      kind: "startsWith",
      startsWith,
      asString: () =>
        FilterStartsWithType.SerializeToString(startsWith.asString()),
    }),
  },
  Operations: {
    // We don't use this at the moment, if we need it, then we can fix
    // Equals: <T>(
    //   fst: DispatchParsedType<T>,
    //   snd: DispatchParsedType<T>,
    // ): boolean =>
    //   fst.kind == "record" && snd.kind == "record"
    //     ? fst.name == snd.name
    //     : fst.kind == "table" && snd.kind == "table"
    //       ? fst.name == snd.name
    //       : fst.kind == "one" && snd.kind == "one"
    //         ? fst.name == snd.name
    //         : fst.kind == "lookup" && snd.kind == "lookup"
    //           ? fst.name == snd.name
    //           : fst.kind == "primitive" && snd.kind == "primitive"
    //             ? fst.name == snd.name
    //             : fst.kind == "list" && snd.kind == "list"
    //               ? fst.name == snd.name
    //               : fst.kind == "singleSelection" &&
    //                   snd.kind == "singleSelection"
    //                 ? fst.name == snd.name
    //                 : fst.kind == "multiSelection" &&
    //                     snd.kind == "multiSelection"
    //                   ? fst.name == snd.name
    //                   : fst.kind == "map" && snd.kind == "map"
    //                     ? fst.name == snd.name
    //                     : fst.kind == "sum" && snd.kind == "sum"
    //                       ? fst.name == snd.name
    //                       : fst.kind == "tuple" && snd.kind == "tuple"
    //                         ? fst.name == snd.name &&
    //                           fst.args.length == snd.args.length &&
    //                           fst.args.every((v, i) =>
    //                             DispatchParsedType.Operations.Equals(
    //                               v,
    //                               snd.args[i],
    //                             ),
    //                           )
    //                         : fst.kind == "union" && snd.kind == "union"
    //                           ? fst.args.size == snd.args.size &&
    //                             fst.args.every(
    //                               (v, i) => v.name == snd.args.get(i)!.name,
    //                             )
    //                           : false,
    ParseRawKeyOf: <T>(
      rawType: ValidatedSerializedKeyOfType<T>,
      serializedTypes: Record<string, SerializedType<T>>,
      alreadyParsedTypes: Map<
        string,
        ValueOrErrors<DispatchParsedType<T>, string>
      >,
      injectedPrimitives?: DispatchInjectedPrimitives<T>,
    ): ValueOrErrors<
      [
        DispatchParsedType<T>,
        Map<DispatchTypeName, ValueOrErrors<DispatchParsedType<T>, string>>,
      ],
      string
    > =>
      (alreadyParsedTypes.has(rawType.args[0])
        ? alreadyParsedTypes
            .get(rawType.args[0])!
            .Then((parsedType) =>
              ValueOrErrors.Default.return<
                [
                  DispatchParsedType<T>,
                  Map<
                    DispatchTypeName,
                    ValueOrErrors<DispatchParsedType<T>, string>
                  >,
                ],
                string
              >([parsedType, alreadyParsedTypes]),
            )
        : DispatchParsedType.Operations.ParseRawType(
            rawType.args[0],
            serializedTypes[rawType.args[0]],
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          )
      )
        .Then((parsingResult) =>
          parsingResult[0].kind != "record"
            ? ValueOrErrors.Default.throwOne<
                [
                  DispatchParsedType<T>,
                  Map<
                    DispatchTypeName,
                    ValueOrErrors<DispatchParsedType<T>, string>
                  >,
                ],
                string
              >(
                `Error: ${JSON.stringify(
                  parsingResult[0],
                )} is not a record type`,
              )
            : ValueOrErrors.Default.return<
                [
                  DispatchParsedType<T>,
                  Map<
                    DispatchTypeName,
                    ValueOrErrors<DispatchParsedType<T>, string>
                  >,
                ],
                string
              >([
                DispatchParsedType.Default.union(
                  Map(
                    (() => {
                      const excludedKeys: Record<string, true> | null = rawType
                        .args[1]
                        ? rawType.args[1].reduce(
                            (acc, key) => {
                              acc[key] = true;
                              return acc;
                            },
                            {} as Record<string, true>,
                          )
                        : null;
                      return parsingResult[0].fields
                        .keySeq()
                        .filter(
                          (key) =>
                            excludedKeys == null || !(key in excludedKeys),
                        )
                        .toArray()
                        .map((key) => [
                          key,
                          DispatchParsedType.Default.record(
                            Map<string, DispatchParsedType<T>>(),
                          ),
                        ]);
                    })(),
                  ),
                ),
                parsingResult[1],
              ]),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing keyOf type`),
        ),
    SerializeToString: <T>(
      type: DispatchParsedType<T>,
    ): StringSerializedType => {
      switch (type.kind) {
        case "primitive":
          return DispatchPrimitiveType.SerializeToString(type.name);
        case "record":
          return RecordType.SerializeToString(
            type.fields.map((v) =>
              DispatchParsedType.Operations.SerializeToString(v),
            ),
          );
        case "table":
          return TableType.SerializeToString(
            DispatchParsedType.Operations.SerializeToString(type.arg),
          );
        case "one":
          return OneType.SerializeToString(
            DispatchParsedType.Operations.SerializeToString(type.arg),
          );
        case "singleSelection":
          return SingleSelectionType.SerializeToString(
            type.args.map((v) =>
              DispatchParsedType.Operations.SerializeToString(v),
            ),
          );
        case "multiSelection":
          return MultiSelectionType.SerializeToString(
            type.args.map((v) =>
              DispatchParsedType.Operations.SerializeToString(v),
            ),
          );
        case "list":
          return ListType.SerializeToString(
            type.args.map((v) =>
              DispatchParsedType.Operations.SerializeToString(v),
            ),
          );
        case "tuple":
          return TupleType.SerializeToString(
            type.args.map((v) =>
              DispatchParsedType.Operations.SerializeToString(v),
            ),
          );
        case "sum":
          return SumType.SerializeToString(
            type.args.map((v) =>
              DispatchParsedType.Operations.SerializeToString(v),
            ),
          );
        case "sumN":
          return SumNType.SerializeToString(
            type.args.map((v) =>
              DispatchParsedType.Operations.SerializeToString(v),
            ),
            type.args.length,
          );
        case "map":
          return MapType.SerializeToString(
            type.args.map((v) =>
              DispatchParsedType.Operations.SerializeToString(v),
            ),
          );
        case "union":
          return UnionType.SerializeToString(
            type.args.map((v) =>
              DispatchParsedType.Operations.SerializeToString(v),
            ),
          );
        case "lookup":
          return LookupType.SerializeToString(type.name);
        case "contains":
          return FilterContainsType.SerializeToString(type.contains.asString());
        case "=":
          return FilterEqualsToType.SerializeToString(type.equalsTo.asString());
        case "!=":
          return FilterNotEqualsToType.SerializeToString(
            type.notEqualsTo.asString(),
          );
        case ">=":
          return FilterGreaterThanOrEqualsToType.SerializeToString(
            type.greaterThanOrEqualsTo.asString(),
          );
        case ">":
          return FilterGreaterThanType.SerializeToString(
            type.greaterThan.asString(),
          );
        case "!=null":
          return FilterIsNotNullType.SerializeToString();
        case "=null":
          return FilterIsNullType.SerializeToString();
        case "<=":
          return FilterSmallerThanOrEqualsToType.SerializeToString(
            type.smallerThanOrEqualsTo.asString(),
          );
        case "<":
          return FilterSmallerThanType.SerializeToString(
            type.smallerThan.asString(),
          );
        case "startsWith":
          return FilterStartsWithType.SerializeToString(
            type.startsWith.asString(),
          );
        default:
          throw new Error(`Unknown type: ${JSON.stringify(type)}`);
      }
    },
    GetExtendedRecordTypes: <T>(
      extendedTypeName: DispatchTypeName,
      alreadyParsedTypes: Map<
        DispatchTypeName,
        ValueOrErrors<DispatchParsedType<T>, string>
      >,
    ): ValueOrErrors<[string, RecordType<T>], string> => {
      return MapRepo.Operations.tryFindWithError(
        extendedTypeName,
        alreadyParsedTypes,
        () => `cannot find extended type ${extendedTypeName} in types`,
      ).Then((extendedType) =>
        extendedType.Then((extendedType) =>
          extendedType.kind != "record"
            ? ValueOrErrors.Default.throwOne<
                [DispatchTypeName, RecordType<T>],
                string
              >(`Error: ${JSON.stringify(extendedType)} is not a record type`)
            : ValueOrErrors.Default.return<
                [DispatchTypeName, RecordType<T>],
                string
              >([extendedTypeName, extendedType]),
        ),
      );
    },
    ParseRecord: <T>(
      rawType: unknown,
      serializedTypes: Record<string, SerializedType<T>>,
      alreadyParsedTypes: Map<
        DispatchTypeName,
        ValueOrErrors<DispatchParsedType<T>, string>
      >,
      injectedPrimitives?: DispatchInjectedPrimitives<T>,
    ): ValueOrErrors<
      [
        RecordType<T>,
        Map<DispatchTypeName, ValueOrErrors<DispatchParsedType<T>, string>>,
      ],
      string
    > =>
      !SerializedType.isRecord(rawType)
        ? ValueOrErrors.Default.throwOne(
            `Error: ${JSON.stringify(rawType)} is not a valid record`,
          )
        : (SerializedType.isExtendedType(rawType)
            ? rawType.extends
                .reduce<
                  ValueOrErrors<
                    [
                      Map<DispatchTypeName, RecordType<T>>,
                      Map<
                        DispatchTypeName,
                        ValueOrErrors<DispatchParsedType<T>, string>
                      >,
                    ],
                    string
                  >
                >(
                  (acc, extendedTypeName) =>
                    acc.Then(([resultMap, accumulatedAlreadyParsedTypes]) =>
                      accumulatedAlreadyParsedTypes.has(extendedTypeName)
                        ? DispatchParsedType.Operations.GetExtendedRecordTypes(
                            extendedTypeName,
                            accumulatedAlreadyParsedTypes,
                          ).Then(([extendedTypeName, recordType]) =>
                            ValueOrErrors.Default.return<
                              [
                                Map<DispatchTypeName, RecordType<T>>,
                                Map<
                                  DispatchTypeName,
                                  ValueOrErrors<DispatchParsedType<T>, string>
                                >,
                              ],
                              string
                            >([
                              resultMap.set(extendedTypeName, recordType),
                              accumulatedAlreadyParsedTypes,
                            ]),
                          )
                        : serializedTypes[extendedTypeName] == undefined
                          ? ValueOrErrors.Default.throwOne<
                              [
                                Map<DispatchTypeName, RecordType<T>>,
                                Map<
                                  DispatchTypeName,
                                  ValueOrErrors<DispatchParsedType<T>, string>
                                >,
                              ],
                              string
                            >(
                              `Error: cannot find extended type ${extendedTypeName} in types`,
                            )
                          : DispatchParsedType.Operations.ParseRawType(
                              extendedTypeName,
                              serializedTypes[extendedTypeName],
                              serializedTypes,
                              accumulatedAlreadyParsedTypes,
                              injectedPrimitives,
                            ).Then((parsedType) =>
                              parsedType[0].kind == "record"
                                ? ValueOrErrors.Default.return<
                                    [
                                      Map<DispatchTypeName, RecordType<T>>,
                                      Map<
                                        DispatchTypeName,
                                        ValueOrErrors<
                                          DispatchParsedType<T>,
                                          string
                                        >
                                      >,
                                    ],
                                    string
                                  >([
                                    resultMap.set(
                                      extendedTypeName,
                                      parsedType[0],
                                    ),
                                    parsedType[1].set(
                                      extendedTypeName,
                                      ValueOrErrors.Default.return(
                                        parsedType[0],
                                      ),
                                    ),
                                  ])
                                : ValueOrErrors.Default.throwOne<
                                    [
                                      Map<DispatchTypeName, RecordType<T>>,
                                      Map<
                                        DispatchTypeName,
                                        ValueOrErrors<
                                          DispatchParsedType<T>,
                                          string
                                        >
                                      >,
                                    ],
                                    string
                                  >(
                                    `Error: ${JSON.stringify(
                                      parsedType[0],
                                    )} is not a record type`,
                                  ),
                            ),
                    ),
                  ValueOrErrors.Default.return<
                    [
                      Map<DispatchTypeName, RecordType<T>>,
                      Map<
                        DispatchTypeName,
                        ValueOrErrors<DispatchParsedType<T>, string>
                      >,
                    ],
                    string
                  >([
                    Map<DispatchTypeName, RecordType<T>>(),
                    alreadyParsedTypes,
                  ]),
                )
                .MapErrors((errors) =>
                  errors.map(
                    (error) => `${error}\n...When parsing extended types`,
                  ),
                )
                .Then(
                  ([
                    parsedExtendedRecordTypesMap,
                    accumulatedAlreadyParsedTypes,
                  ]) =>
                    ValueOrErrors.Default.return<
                      [
                        Map<DispatchTypeName, RecordType<T>>,
                        Map<
                          DispatchTypeName,
                          ValueOrErrors<DispatchParsedType<T>, string>
                        >,
                      ],
                      string
                    >([
                      parsedExtendedRecordTypesMap,
                      accumulatedAlreadyParsedTypes,
                    ]),
                )
            : ValueOrErrors.Default.return<
                [
                  Map<DispatchTypeName, RecordType<T>>,
                  Map<
                    DispatchTypeName,
                    ValueOrErrors<DispatchParsedType<T>, string>
                  >,
                ],
                string
              >([Map<DispatchTypeName, RecordType<T>>(), alreadyParsedTypes])
          ).Then(
            ([parsedExtendedRecordTypesMap, accumulatedAlreadyParsedTypes]) =>
              Object.keys(rawType.fields)
                .reduce<
                  ValueOrErrors<
                    [
                      Map<string, DispatchParsedType<T>>,
                      Map<
                        DispatchTypeName,
                        ValueOrErrors<DispatchParsedType<T>, string>
                      >,
                    ],
                    string
                  >
                >(
                  (acc, fieldName) =>
                    acc.Then(
                      ([
                        parsedFieldsMap,
                        accumulatedAlreadyParsedTypesForFields,
                      ]) =>
                        DispatchParsedType.Operations.ParseRawType(
                          `Record field type: ${fieldName}`,
                          (rawType.fields as Record<string, unknown>)[
                            fieldName
                          ] as SerializedType<T>,
                          serializedTypes,
                          accumulatedAlreadyParsedTypesForFields,
                          injectedPrimitives,
                        ).Then((parsedField) =>
                          ValueOrErrors.Default.return<
                            [
                              Map<string, DispatchParsedType<T>>,
                              Map<
                                DispatchTypeName,
                                ValueOrErrors<DispatchParsedType<T>, string>
                              >,
                            ],
                            string
                          >([
                            parsedFieldsMap.set(fieldName, parsedField[0]),
                            parsedField[1],
                          ]),
                        ),
                    ),
                  ValueOrErrors.Default.return<
                    [
                      Map<string, DispatchParsedType<T>>,
                      Map<
                        DispatchTypeName,
                        ValueOrErrors<DispatchParsedType<T>, string>
                      >,
                    ],
                    string
                  >([
                    Map<string, DispatchParsedType<T>>(),
                    accumulatedAlreadyParsedTypes,
                  ]),
                )
                .Then(
                  ([
                    parsedFieldsMap,
                    accumulatedAlreadyParsedTypesForFields,
                  ]) => {
                    const { mergedFields, updatedAccumulatedTypes } =
                      parsedExtendedRecordTypesMap.reduce(
                        (
                          acc,
                          type,
                          name,
                        ): {
                          mergedFields: Map<
                            DispatchTypeName,
                            DispatchParsedType<T>
                          >;
                          updatedAccumulatedTypes: Map<
                            DispatchTypeName,
                            ValueOrErrors<DispatchParsedType<T>, string>
                          >;
                        } => ({
                          mergedFields: acc.mergedFields.merge(type.fields),
                          updatedAccumulatedTypes:
                            acc.updatedAccumulatedTypes.set(
                              name,
                              ValueOrErrors.Default.return(type),
                            ),
                        }),
                        {
                          mergedFields: Map<
                            DispatchTypeName,
                            DispatchParsedType<T>
                          >(),
                          updatedAccumulatedTypes:
                            accumulatedAlreadyParsedTypesForFields,
                        },
                      );
                    return ValueOrErrors.Default.return<RecordType<T>, string>(
                      DispatchParsedType.Default.record(
                        parsedFieldsMap.merge(mergedFields),
                      ),
                    ).Then((parsedRecord) =>
                      ValueOrErrors.Default.return<
                        [
                          RecordType<T>,
                          Map<
                            DispatchTypeName,
                            ValueOrErrors<DispatchParsedType<T>, string>
                          >,
                        ],
                        string
                      >([parsedRecord, updatedAccumulatedTypes]),
                    );
                  },
                ),
          ),
    ParseRawFilterType: <T>(
      rawFilterType: unknown,
      arg: DispatchParsedType<T>,
    ): ValueOrErrors<FilterType<T>, string> => {
      if (typeof rawFilterType != "string") {
        return ValueOrErrors.Default.throwOne<FilterType<T>, string>(
          `rawType is not a string`,
        );
      }

      switch (rawFilterType) {
        case "contains":
          return ValueOrErrors.Default.return(
            DispatchParsedType.Default.filterContains(arg),
          );
        case "equalsTo":
          return ValueOrErrors.Default.return(
            DispatchParsedType.Default.filterEqualsTo(arg),
          );
        case "startswith":
          return ValueOrErrors.Default.return(
            DispatchParsedType.Default.filterStartsWith(arg),
          );
        case "!=":
          return ValueOrErrors.Default.return(
            DispatchParsedType.Default.filterNotEqualsTo(arg),
          );
        case "=":
          return ValueOrErrors.Default.return(
            DispatchParsedType.Default.filterEqualsTo(arg),
          );
        case ">=":
          return ValueOrErrors.Default.return(
            DispatchParsedType.Default.filterGreaterThanOrEqualsTo(arg),
          );
        case ">":
          return ValueOrErrors.Default.return(
            DispatchParsedType.Default.filterGreaterThan(arg),
          );
        case "<=":
          return ValueOrErrors.Default.return(
            DispatchParsedType.Default.filterSmallerThanOrEqualsTo(arg),
          );
        case "<":
          return ValueOrErrors.Default.return(
            DispatchParsedType.Default.filterSmallerThan(arg),
          );
        case "=null":
          return ValueOrErrors.Default.return(
            DispatchParsedType.Default.filterIsNull(),
          );
        case "!=null":
          return ValueOrErrors.Default.return(
            DispatchParsedType.Default.filterIsNotNull(),
          );
        default:
          return ValueOrErrors.Default.throwOne<FilterType<T>, string>(
            `Unknown filter type: ${rawFilterType}`,
          );
      }
    },
    ParseRawType: <T>(
      typeName: DispatchTypeName,
      rawType: SerializedType<T>,
      serializedTypes: Record<string, SerializedType<T>>,
      alreadyParsedTypes: Map<
        DispatchTypeName,
        ValueOrErrors<DispatchParsedType<T>, string>
      >,
      injectedPrimitives?: DispatchInjectedPrimitives<T>,
    ): ValueOrErrors<
      [
        DispatchParsedType<T>,
        Map<DispatchTypeName, ValueOrErrors<DispatchParsedType<T>, string>>,
      ],
      string
    > => {
      if (
        alreadyParsedTypes.has(typeName) &&
        !SerializedType.isLookup(rawType, injectedPrimitives)
      )
        return alreadyParsedTypes
          .get(typeName)!
          .Then((parsedType) =>
            ValueOrErrors.Default.return([parsedType, alreadyParsedTypes]),
          );
      const result: ValueOrErrors<
        [
          DispatchParsedType<T>,
          Map<DispatchTypeName, ValueOrErrors<DispatchParsedType<T>, string>>,
        ],
        string
      > = (() => {
        if (SerializedType.isPrimitive(rawType, injectedPrimitives))
          return ValueOrErrors.Default.return([
            DispatchParsedType.Default.primitive(
              typeof rawType === "string" &&
                STRINGY_TYPES.includes(
                  rawType as (typeof STRINGY_TYPES)[number],
                )
                ? "string"
                : rawType,
            ),
            alreadyParsedTypes,
          ]);
        if (SerializedType.isSingleSelection(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            `SingleSelection:Element`,
            rawType.args[0],
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then(([parsedArg, newAlreadyParsedTypes]) =>
            ValueOrErrors.Default.return([
              DispatchParsedType.Default.singleSelection([parsedArg]),
              newAlreadyParsedTypes,
            ]),
          );
        if (SerializedType.isMultiSelection(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            `MultiSelection:Element`,
            rawType.args[0],
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then(([parsedArg, newAlreadyParsedTypes]) =>
            ValueOrErrors.Default.return([
              DispatchParsedType.Default.multiSelection([parsedArg]),
              newAlreadyParsedTypes,
            ]),
          );
        if (SerializedType.isList(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            `List:Element`,
            rawType.args[0],
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then(([parsedArg, newAlreadyParsedTypes]) =>
            ValueOrErrors.Default.return([
              DispatchParsedType.Default.list([parsedArg]),
              newAlreadyParsedTypes,
            ]),
          );
        if (SerializedType.isTuple(rawType))
          return rawType.args
            .reduce<
              ValueOrErrors<
                [
                  List<DispatchParsedType<T>>,
                  Map<
                    DispatchTypeName,
                    ValueOrErrors<DispatchParsedType<T>, string>
                  >,
                ],
                string
              >
            >((acc, arg, index) => acc.Then(([parsedArgsList, accumulatedAlreadyParsedTypes]) => DispatchParsedType.Operations.ParseRawType(`Tuple:Item ${index + 1}`, arg, serializedTypes, accumulatedAlreadyParsedTypes, injectedPrimitives).Then((parsedArg) => ValueOrErrors.Default.return<[List<DispatchParsedType<T>>, Map<DispatchTypeName, ValueOrErrors<DispatchParsedType<T>, string>>], string>([parsedArgsList.push(parsedArg[0]), parsedArg[1]]))), ValueOrErrors.Default.return<[List<DispatchParsedType<T>>, Map<DispatchTypeName, ValueOrErrors<DispatchParsedType<T>, string>>], string>([List<DispatchParsedType<T>>(), alreadyParsedTypes]))
            .Then(([parsedArgsList, accumulatedAlreadyParsedTypes]) =>
              ValueOrErrors.Default.return([
                DispatchParsedType.Default.tuple(parsedArgsList.toArray()),
                accumulatedAlreadyParsedTypes,
              ]),
            );
        if (SerializedType.isMap(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            "Map:Key",
            rawType.args[0],
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then(([parsedArg0, newAlreadyParsedTypes]) =>
            DispatchParsedType.Operations.ParseRawType(
              "Map:Value",
              rawType.args[1],
              serializedTypes,
              newAlreadyParsedTypes,
              injectedPrimitives,
            ).Then(([parsedArg1, newAlreadyParsedTypes2]) =>
              ValueOrErrors.Default.return([
                DispatchParsedType.Default.map([parsedArg0, parsedArg1]),
                newAlreadyParsedTypes2,
              ]),
            ),
          );
        if (SerializedType.isSum(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            "Sum:Left",
            rawType.args[0],
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then(([parsedArg0, newAlreadyParsedTypes]) =>
            DispatchParsedType.Operations.ParseRawType(
              "Sum:Right",
              rawType.args[1],
              serializedTypes,
              newAlreadyParsedTypes,
              injectedPrimitives,
            ).Then(([parsedArg1, newAlreadyParsedTypes2]) =>
              ValueOrErrors.Default.return([
                DispatchParsedType.Default.sum([parsedArg0, parsedArg1]),
                newAlreadyParsedTypes2,
              ]),
            ),
          );
        if (SerializedType.isRecord(rawType))
          return DispatchParsedType.Operations.ParseRecord(
            rawType,
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          );
        if (SerializedType.isTable(rawType)) {
          return DispatchParsedType.Operations.ParseRawType(
            "TableArg",
            rawType.args[0],
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then(([parsedArg, newAlreadyParsedTypes]) =>
            parsedArg.kind != "lookup"
              ? ValueOrErrors.Default.throwOne(
                  `Error: ${JSON.stringify(parsedArg)} is not a lookup type`,
                )
              : ValueOrErrors.Default.return([
                  DispatchParsedType.Default.table(parsedArg),
                  newAlreadyParsedTypes,
                ]),
          );
        }
        if (SerializedType.isLookup(rawType, injectedPrimitives)) {
          const resolvedType = serializedTypes[rawType];
          if (!resolvedType) {
            return ValueOrErrors.Default.throwOne(
              `Error: ${JSON.stringify(rawType)} is not a valid lookup type (not found in serializedTypes)`,
            );
          }
          if (!isString(rawType)) {
            return ValueOrErrors.Default.throwOne(
              `Error: ${JSON.stringify(rawType)} is not a valid lookup type (not a string)`,
            );
          }
          if (alreadyParsedTypes.has(rawType)) {
            return ValueOrErrors.Default.return([
              DispatchParsedType.Default.lookup(rawType),
              alreadyParsedTypes,
            ]);
          }

          return DispatchParsedType.Operations.ParseRawType(
            rawType,
            resolvedType,
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then(([parsedType, newAlreadyParsedTypes]) =>
            ValueOrErrors.Default.return([
              DispatchParsedType.Default.lookup(rawType),
              newAlreadyParsedTypes.set(
                rawType,
                ValueOrErrors.Default.return(parsedType),
              ),
            ]),
          );
        }

        if (SerializedType.isUnit(rawType)) {
          return ValueOrErrors.Default.return([
            DispatchParsedType.Default.primitive("unit"),
            alreadyParsedTypes,
          ]);
        }
        if (SerializedType.isUnion(rawType))
          return rawType.args
            .reduce<
              ValueOrErrors<
                [
                  Map<string, DispatchParsedType<T>>,
                  Map<
                    DispatchTypeName,
                    ValueOrErrors<DispatchParsedType<T>, string>
                  >,
                ],
                string
              >
            >(
              (acc, unionCase) =>
                acc.Then(
                  ([parsedUnionCasesMap, accumulatedAlreadyParsedTypes]) =>
                    DispatchParsedType.Operations.ParseRawType(
                      isString(unionCase.fields)
                        ? unionCase.fields
                        : `Union:Case ${unionCase.caseName}`,
                      unionCase.fields == undefined
                        ? { fields: {} }
                        : // we allow the record fields to be defined directly in the spec instead of
                          // inside a fields key
                          SerializedType.isRecordFields(unionCase.fields)
                          ? { fields: unionCase.fields }
                          : unionCase.fields,
                      serializedTypes,
                      accumulatedAlreadyParsedTypes,
                      injectedPrimitives,
                    ).Then(([parsedType, newAlreadyParsedTypes]) =>
                      ValueOrErrors.Default.return<
                        [
                          Map<string, DispatchParsedType<T>>,
                          Map<
                            DispatchTypeName,
                            ValueOrErrors<DispatchParsedType<T>, string>
                          >,
                        ],
                        string
                      >([
                        parsedUnionCasesMap.set(unionCase.caseName, parsedType),
                        newAlreadyParsedTypes,
                      ]),
                    ),
                ),
              ValueOrErrors.Default.return<
                [
                  Map<string, DispatchParsedType<T>>,
                  Map<
                    DispatchTypeName,
                    ValueOrErrors<DispatchParsedType<T>, string>
                  >,
                ],
                string
              >([Map<string, DispatchParsedType<T>>(), alreadyParsedTypes]),
            )
            .Then(([parsedUnionCasesMap, accumulatedAlreadyParsedTypes]) =>
              ValueOrErrors.Default.return([
                DispatchParsedType.Default.union(parsedUnionCasesMap),
                accumulatedAlreadyParsedTypes,
              ]),
            );
        if (SerializedType.isOne(rawType)) {
          return DispatchParsedType.Operations.ParseRawType(
            rawType.args[0] as string,
            rawType.args[0],
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then(([parsedArg, newAlreadyParsedTypes]) =>
            parsedArg.kind != "lookup"
              ? ValueOrErrors.Default.throwOne(
                  `one content type ${JSON.stringify(parsedArg)} is not a lookup type`,
                )
              : ValueOrErrors.Default.return([
                  DispatchParsedType.Default.one(parsedArg),
                  newAlreadyParsedTypes,
                ]),
          );
        }
        if (SerializedType.isReadOnly(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            "ReadOnly:Element",
            rawType.args[0],
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then(([parsedArg, newAlreadyParsedTypes]) =>
            ValueOrErrors.Default.return([
              DispatchParsedType.Default.readOnly(parsedArg),
              newAlreadyParsedTypes,
            ]),
          );
        if (SerializedType.isKeyOf(rawType))
          return DispatchParsedType.Operations.ParseRawKeyOf(
            rawType,
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          );
        if (SerializedType.isTranslationOverride(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            "TranslationOverride:Language",
            rawType.args[0],
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then(([parsedArg0, newAlreadyParsedTypes]) =>
            ValueOrErrors.Default.return([
              DispatchParsedType.Default.map([
                DispatchParsedType.Default.singleSelection([parsedArg0]),
                DispatchParsedType.Default.primitive("string"),
              ]),
              newAlreadyParsedTypes,
            ]),
          );
        return ValueOrErrors.Default.throwOne(
          `Unrecognised type "${typeName}" : ${JSON.stringify(rawType)}`,
        );
      })();
      return result.MapErrors((errors) =>
        errors.map((error) => `${error}\n...When parsing type "${typeName}"`),
      );
    },
    ResolveLookupType: <T>(
      typeName: string,
      types: Map<DispatchTypeName, DispatchParsedType<T>>,
    ): ValueOrErrors<DispatchParsedType<T>, string> =>
      MapRepo.Operations.tryFindWithError(
        typeName,
        types,
        () => `cannot find lookup type ${typeName} in types`,
      ),
    AsResolvedType: <T>(
      type: DispatchParsedType<T>,
      types: Map<DispatchTypeName, DispatchParsedType<T>>,
    ): ValueOrErrors<DispatchParsedType<T>, string> =>
      type.kind == "lookup"
        ? DispatchParsedType.Operations.ResolveLookupType(type.name, types)
        : ValueOrErrors.Default.return(type),
  },
};
