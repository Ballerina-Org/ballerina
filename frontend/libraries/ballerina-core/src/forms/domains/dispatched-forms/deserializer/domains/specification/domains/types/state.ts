import { Set, List, Map, isMap, OrderedMap } from "immutable";
import {} from "../../../../../../parser/domains/built-ins/state";
import { ValueOrErrors } from "../../../../../../../../collections/domains/valueOrErrors/state";
import {
  Unit,
  DispatchGenericType,
  DispatchGenericTypes,
  MapRepo,
  DispatchInjectedPrimitives,
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

export type ValidatedSerializedKeyOfType<T> = {
  fun: "KeyOf";
  args: Array<string>;
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
  | SerializedKeyOfType<T>;

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
  isLookup: (_: unknown, forms: Set<DispatchTypeName>): _ is DispatchTypeName =>
    DispatchisString(_) && forms.has(_),
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
    _.args.length == 1 &&
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
  | MapType<T>
  | TableType<T>
  | OneType<T>
  | ReadOnlyType<T>;

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
      typeNames: Set<DispatchTypeName>,
      serializedTypes: Record<string, SerializedType<T>>,
      alreadyParsedTypes: Map<
        string,
        ValueOrErrors<DispatchParsedType<T>, string>
      >,
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
            typeNames,
            serializedTypes,
            alreadyParsedTypes,
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
                    parsingResult[0].fields
                      .keySeq()
                      .toArray()
                      .map((key) => [
                        key,
                        DispatchParsedType.Default.record(
                          Map<string, DispatchParsedType<T>>(),
                        ),
                      ]),
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
        default:
          throw new Error(`Unknown type: ${JSON.stringify(type)}`);
      }
    },
    ParseRecord: <T>(
      typeName: DispatchTypeName,
      rawType: unknown,
      typeNames: Set<DispatchTypeName>,
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
      // will already been parsed if it was extended by another type which was already parsed
      alreadyParsedTypes.has(typeName)
        ? alreadyParsedTypes
            .get(typeName)!
            .Then((parsedType) =>
              parsedType.kind != "record"
                ? ValueOrErrors.Default.throwOne(
                    `Error: ${JSON.stringify(parsedType)} is not a record type`,
                  )
                : ValueOrErrors.Default.return([
                    parsedType,
                    alreadyParsedTypes,
                  ]),
            )
        : !SerializedType.isRecord(rawType)
          ? ValueOrErrors.Default.throwOne(
              `Error: ${JSON.stringify(rawType)} is not a valid record`,
            )
          : (SerializedType.isExtendedType(rawType)
              ? ValueOrErrors.Operations.All(
                  List<
                    ValueOrErrors<[DispatchTypeName, RecordType<T>], string>
                  >(
                    rawType.extends.map((extendedTypeName) =>
                      alreadyParsedTypes.has(extendedTypeName)
                        ? alreadyParsedTypes
                            .get(extendedTypeName)!
                            .Then((extendedType) =>
                              extendedType.kind != "record"
                                ? ValueOrErrors.Default.throwOne<
                                    [DispatchTypeName, RecordType<T>],
                                    string
                                  >(
                                    `Error: ${JSON.stringify(
                                      extendedType,
                                    )} is not a record type`,
                                  )
                                : ValueOrErrors.Default.return<
                                    [DispatchTypeName, RecordType<T>],
                                    string
                                  >([extendedTypeName, extendedType]),
                            )
                        : serializedTypes[extendedTypeName] == undefined
                          ? ValueOrErrors.Default.throwOne<
                              [DispatchTypeName, RecordType<T>],
                              string
                            >(
                              `Error: cannot find extended type ${extendedTypeName} in types`,
                            )
                          : DispatchParsedType.Operations.ParseRawType(
                              extendedTypeName,
                              serializedTypes[extendedTypeName],
                              typeNames,
                              serializedTypes,
                              alreadyParsedTypes,
                              injectedPrimitives,
                            ).Then((parsedType) =>
                              parsedType[0].kind == "record"
                                ? ValueOrErrors.Default.return<
                                    [DispatchTypeName, RecordType<T>],
                                    string
                                  >([extendedTypeName, parsedType[0]])
                                : ValueOrErrors.Default.throwOne<
                                    [DispatchTypeName, RecordType<T>],
                                    string
                                  >(
                                    `Error: ${JSON.stringify(
                                      parsedType[0],
                                    )} is not a record type`,
                                  ),
                            ),
                    ),
                  ),
                )
                  .MapErrors((errors) =>
                    errors.map(
                      (error) => `${error}\n...When parsing extended types`,
                    ),
                  )
                  .Then((parsedExtendedRecordTypes) =>
                    ValueOrErrors.Default.return<
                      Map<DispatchTypeName, RecordType<T>>,
                      string
                    >(
                      parsedExtendedRecordTypes.reduce(
                        (acc, type) => acc.set(type[0], type[1]),
                        Map<DispatchTypeName, RecordType<T>>(),
                      ),
                    ),
                  )
              : ValueOrErrors.Default.return<
                  Map<DispatchTypeName, RecordType<T>>,
                  string
                >(Map<DispatchTypeName, RecordType<T>>())
            ).Then((parsedExtendedRecordTypesMap) =>
              ValueOrErrors.Operations.All(
                List(
                  Object.entries(rawType.fields).map(([fieldName, fieldType]) =>
                    DispatchParsedType.Operations.ParseRawType(
                      fieldName,
                      fieldType as SerializedType<T>,
                      typeNames,
                      serializedTypes,
                      alreadyParsedTypes,
                      injectedPrimitives,
                    ).Then((parsedField) =>
                      ValueOrErrors.Default.return<
                        readonly [
                          string,
                          [
                            DispatchParsedType<T>,
                            Map<
                              string,
                              ValueOrErrors<DispatchParsedType<T>, string>
                            >,
                          ],
                        ],
                        string
                      >([fieldName, parsedField] as const),
                    ),
                  ),
                ),
              )
                .Then((parsedFields) =>
                  ValueOrErrors.Default.return<
                    Map<string, DispatchParsedType<T>>,
                    string
                  >(
                    Map(
                      parsedFields.map(([fieldName, parsedField]) => [
                        fieldName,
                        parsedField[0],
                      ]),
                    ),
                  ),
                )
                .Then((parsedFieldsMap) =>
                  ValueOrErrors.Default.return<RecordType<T>, string>(
                    DispatchParsedType.Default.record(
                      parsedFieldsMap.merge(
                        parsedExtendedRecordTypesMap.reduce(
                          (acc, type) => acc.merge(type.fields),
                          Map<DispatchTypeName, DispatchParsedType<T>>(),
                        ),
                      ),
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
                    >([
                      parsedRecord,
                      parsedExtendedRecordTypesMap.reduce(
                        (acc, type, name) =>
                          acc.set(name, ValueOrErrors.Default.return(type)),
                        alreadyParsedTypes,
                      ),
                    ]),
                  ),
                ),
            ),
    ParseRawType: <T>(
      typeName: DispatchTypeName,
      rawType: SerializedType<T>,
      typeNames: Set<DispatchTypeName>,
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
      const result: ValueOrErrors<
        [
          DispatchParsedType<T>,
          Map<DispatchTypeName, ValueOrErrors<DispatchParsedType<T>, string>>,
        ],
        string
      > = (() => {
        const stringyTypes = [
          "guid",
          "entityIdUUID",
          "entityIdString",
          "calculatedDisplayValue",
        ];
        if (SerializedType.isPrimitive(rawType, injectedPrimitives))
          return ValueOrErrors.Default.return([
            DispatchParsedType.Default.primitive(
              typeof rawType === "string" && stringyTypes.includes(rawType)
                ? "string"
                : rawType,
            ),
            alreadyParsedTypes,
          ]);
        if (SerializedType.isSingleSelection(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            `SingleSelection:Element`,
            rawType.args[0],
            typeNames,
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then((parsedArgs) =>
            ValueOrErrors.Default.return([
              DispatchParsedType.Default.singleSelection([parsedArgs[0]]),
              alreadyParsedTypes,
            ]),
          );
        if (SerializedType.isMultiSelection(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            `MultiSelection:Element`,
            rawType.args[0],
            typeNames,
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then((parsedArgs) =>
            ValueOrErrors.Default.return([
              DispatchParsedType.Default.multiSelection([parsedArgs[0]]),
              alreadyParsedTypes,
            ]),
          );
        if (SerializedType.isList(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            `List:Element`,
            rawType.args[0],
            typeNames,
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then((parsedArgs) =>
            ValueOrErrors.Default.return([
              DispatchParsedType.Default.list([parsedArgs[0]]),
              alreadyParsedTypes,
            ]),
          );
        if (SerializedType.isTuple(rawType))
          return ValueOrErrors.Operations.All(
            List(
              rawType.args.map((arg, index) =>
                DispatchParsedType.Operations.ParseRawType(
                  `Tuple:Item ${index + 1}`,
                  arg,
                  typeNames,
                  serializedTypes,
                  alreadyParsedTypes,
                  injectedPrimitives,
                ),
              ),
            ),
          ).Then((parsedArgs) =>
            ValueOrErrors.Default.return([
              DispatchParsedType.Default.tuple(
                parsedArgs.map(([parsedArg]) => parsedArg).toArray(),
              ),
              alreadyParsedTypes,
            ]),
          );
        if (SerializedType.isMap(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            "Map:Key",
            rawType.args[0],
            typeNames,
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then((parsedArgs0) =>
            DispatchParsedType.Operations.ParseRawType(
              "Map:Value",
              rawType.args[1],
              typeNames,
              serializedTypes,
              alreadyParsedTypes,
              injectedPrimitives,
            ).Then((parsedArgs1) =>
              ValueOrErrors.Default.return([
                DispatchParsedType.Default.map([
                  parsedArgs0[0],
                  parsedArgs1[0],
                ]),
                alreadyParsedTypes,
              ]),
            ),
          );
        if (SerializedType.isSum(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            "Sum:Left",
            rawType.args[0],
            typeNames,
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then((parsedArgs0) =>
            DispatchParsedType.Operations.ParseRawType(
              "Sum:Right",
              rawType.args[1],
              typeNames,
              serializedTypes,
              alreadyParsedTypes,
              injectedPrimitives,
            ).Then((parsedArgs1) =>
              ValueOrErrors.Default.return([
                DispatchParsedType.Default.sum([
                  parsedArgs0[0],
                  parsedArgs1[0],
                ]),
                alreadyParsedTypes,
              ]),
            ),
          );
        if (SerializedType.isRecord(rawType))
          return DispatchParsedType.Operations.ParseRecord(
            typeName,
            rawType,
            typeNames,
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          );
        if (SerializedType.isTable(rawType)) {
          return DispatchParsedType.Operations.ParseRawType(
            "TableArg",
            rawType.args[0],
            typeNames,
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then((parsedArg) =>
            parsedArg[0].kind != "lookup"
              ? ValueOrErrors.Default.throwOne(
                  `Error: ${JSON.stringify(parsedArg[0])} is not a lookup type`,
                )
              : ValueOrErrors.Default.return([
                  DispatchParsedType.Default.table(parsedArg[0]),
                  alreadyParsedTypes,
                ]),
          );
        }
        if (SerializedType.isLookup(rawType, typeNames))
          return ValueOrErrors.Default.return([
            DispatchParsedType.Default.lookup(rawType),
            alreadyParsedTypes,
          ]);
        if (SerializedType.isUnit(rawType)) {
          return ValueOrErrors.Default.return([
            DispatchParsedType.Default.primitive("unit"),
            alreadyParsedTypes,
          ]);
        }
        if (SerializedType.isUnion(rawType))
          // for now we assume all union cases are lookup types
          return ValueOrErrors.Operations.All(
            List<ValueOrErrors<[string, DispatchParsedType<T>], string>>(
              rawType.args.map((unionCase) =>
                DispatchParsedType.Operations.ParseRawType(
                  `Union:Case ${unionCase.caseName}`,
                  unionCase.fields == undefined
                    ? { fields: {} }
                    : // we allow the record fields to be defined directly in the spec instead of
                      // inside a fields key
                      SerializedType.isRecordFields(unionCase.fields)
                      ? { fields: unionCase.fields }
                      : unionCase.fields,
                  typeNames,
                  serializedTypes,
                  alreadyParsedTypes,
                  injectedPrimitives,
                ).Then((parsedType) =>
                  ValueOrErrors.Default.return([
                    unionCase.caseName,
                    parsedType[0],
                  ]),
                ),
              ),
            ),
          ).Then((parsedUnionCases) =>
            ValueOrErrors.Default.return([
              DispatchParsedType.Default.union(Map(parsedUnionCases)),
              alreadyParsedTypes,
            ]),
          );
        if (SerializedType.isOne(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            "One:Element",
            rawType.args[0],
            typeNames,
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then((parsedArg) =>
            parsedArg[0].kind != "lookup"
              ? ValueOrErrors.Default.throwOne(
                  `one content type ${JSON.stringify(parsedArg[0])} is not a lookup type`,
                )
              : ValueOrErrors.Default.return([
                  DispatchParsedType.Default.one(parsedArg[0]),
                  alreadyParsedTypes,
                ]),
          );
        if (SerializedType.isReadOnly(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            "ReadOnly:Element",
            rawType.args[0],
            typeNames,
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then(([parsedArg, _]) =>
            ValueOrErrors.Default.return([
              DispatchParsedType.Default.readOnly(parsedArg),
              alreadyParsedTypes,
            ]),
          );
        if (SerializedType.isKeyOf(rawType))
          return DispatchParsedType.Operations.ParseRawKeyOf(
            rawType,
            typeNames,
            serializedTypes,
            alreadyParsedTypes,
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
