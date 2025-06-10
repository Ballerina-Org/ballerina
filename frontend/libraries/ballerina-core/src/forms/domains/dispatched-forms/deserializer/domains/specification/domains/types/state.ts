import { Set, List, Map, isMap } from "immutable";
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
};

export type UnionType<T> = {
  kind: "union";
  name: DispatchTypeName;
  args: Map<DispatchCaseName, DispatchParsedType<T>>;
  typeName: DispatchTypeName;
};

export type RecordType<T> = {
  kind: "record";
  name: DispatchTypeName;
  fields: Map<DispatchFieldName, DispatchParsedType<T>>;
  typeName: DispatchTypeName;
};

export type LookupType = {
  kind: "lookup";
  name: string;
  typeName: DispatchTypeName;
};

export type DispatchPrimitiveType<T> = {
  kind: "primitive";
  name: DispatchPrimitiveTypeName<T>;
  typeName: DispatchTypeName;
};

export type SingleSelectionType<T> = {
  kind: "singleSelection";
  name: DispatchTypeName;
  args: Array<DispatchParsedType<T>>;
  typeName: DispatchTypeName;
};

export type MultiSelectionType<T> = {
  kind: "multiSelection";
  name: DispatchTypeName;
  args: Array<DispatchParsedType<T>>;
  typeName: DispatchTypeName;
};

export type ListType<T> = {
  kind: "list";
  name: DispatchTypeName;
  args: Array<DispatchParsedType<T>>;
  typeName: DispatchTypeName;
};

export type TupleType<T> = {
  kind: "tuple";
  name: DispatchTypeName;
  args: Array<DispatchParsedType<T>>;
  typeName: DispatchTypeName;
};

export type SumType<T> = {
  kind: "sum";
  name: DispatchTypeName;
  args: Array<DispatchParsedType<T>>;
  typeName: DispatchTypeName;
};

export type MapType<T> = {
  kind: "map";
  name: DispatchTypeName;
  args: Array<DispatchParsedType<T>>;
  typeName: DispatchTypeName;
};

export type TableType<T> = {
  kind: "table";
  name: DispatchTypeName;
  args: Array<DispatchParsedType<T>>;
  typeName: DispatchTypeName;
};

export type OneType<T> = {
  kind: "one";
  name: DispatchTypeName;
  args: DispatchParsedType<T>;
  typeName: DispatchTypeName;
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
  | OneType<T>;

export const DispatchParsedType = {
  Default: {
    table: <T>(
      name: DispatchTypeName,
      args: Array<DispatchParsedType<T>>,
      typeName: DispatchTypeName,
    ): TableType<T> => ({
      kind: "table",
      name,
      args,
      typeName,
    }),
    record: <T>(
      name: DispatchTypeName,
      fields: Map<DispatchFieldName, DispatchParsedType<T>>,
      typeName: DispatchTypeName,
    ): RecordType<T> => ({
      kind: "record",
      name,
      fields,
      typeName: typeName,
    }),
    primitive: <T>(
      name: DispatchPrimitiveTypeName<T>,
      typeName: DispatchTypeName,
    ): DispatchParsedType<T> => ({
      kind: "primitive",
      name,
      typeName: typeName,
    }),
    singleSelection: <T>(
      name: DispatchTypeName,
      args: Array<DispatchParsedType<T>>,
      typeName: DispatchTypeName,
    ): DispatchParsedType<T> => ({
      kind: "singleSelection",
      name,
      args,
      typeName: typeName,
    }),
    multiSelection: <T>(
      name: DispatchTypeName,
      args: Array<DispatchParsedType<T>>,
      typeName: DispatchTypeName,
    ): DispatchParsedType<T> => ({
      kind: "multiSelection",
      name,
      args,
      typeName: typeName,
    }),
    list: <T>(
      name: DispatchTypeName,
      args: Array<DispatchParsedType<T>>,
      typeName: DispatchTypeName,
    ): DispatchParsedType<T> => ({
      kind: "list",
      name,
      args,
      typeName: typeName,
    }),
    tuple: <T>(
      name: DispatchTypeName,
      args: Array<DispatchParsedType<T>>,
      typeName: DispatchTypeName,
    ): DispatchParsedType<T> => ({
      kind: "tuple",
      name,
      args,
      typeName: typeName,
    }),
    sum: <T>(
      name: DispatchTypeName,
      args: Array<DispatchParsedType<T>>,
      typeName: DispatchTypeName,
    ): DispatchParsedType<T> => ({
      kind: "sum",
      name,
      args,
      typeName: typeName,
    }),
    map: <T>(
      name: DispatchTypeName,
      args: Array<DispatchParsedType<T>>,
      typeName: DispatchTypeName,
    ): DispatchParsedType<T> => ({
      kind: "map",
      name,
      args,
      typeName: typeName,
    }),
    union: <T>(
      name: DispatchTypeName,
      args: Map<DispatchCaseName, DispatchParsedType<T>>,
      typeName: DispatchTypeName,
    ): DispatchParsedType<T> => ({
      kind: "union",
      name,
      args,
      typeName,
    }),
    lookup: <T>(name: string): LookupType => ({
      kind: "lookup",
      name,
      typeName: name,
    }),
    one: <T>(
      name: DispatchTypeName,
      args: DispatchParsedType<T>,
      typeName: DispatchTypeName,
    ): OneType<T> => ({
      kind: "one",
      name,
      args,
      typeName,
    }),
  },
  Operations: {
    Equals: <T>(
      fst: DispatchParsedType<T>,
      snd: DispatchParsedType<T>,
    ): boolean =>
      fst.kind == "record" && snd.kind == "record"
        ? fst.name == snd.name
        : fst.kind == "table" && snd.kind == "table"
          ? fst.name == snd.name
          : fst.kind == "one" && snd.kind == "one"
            ? fst.name == snd.name
            : fst.kind == "lookup" && snd.kind == "lookup"
              ? fst.name == snd.name
              : fst.kind == "primitive" && snd.kind == "primitive"
                ? fst.name == snd.name
                : fst.kind == "list" && snd.kind == "list"
                  ? fst.name == snd.name
                  : fst.kind == "singleSelection" &&
                      snd.kind == "singleSelection"
                    ? fst.name == snd.name
                    : fst.kind == "multiSelection" &&
                        snd.kind == "multiSelection"
                      ? fst.name == snd.name
                      : fst.kind == "map" && snd.kind == "map"
                        ? fst.name == snd.name
                        : fst.kind == "sum" && snd.kind == "sum"
                          ? fst.name == snd.name
                          : fst.kind == "tuple" && snd.kind == "tuple"
                            ? fst.name == snd.name &&
                              fst.args.length == snd.args.length &&
                              fst.args.every((v, i) =>
                                DispatchParsedType.Operations.Equals(
                                  v,
                                  snd.args[i],
                                ),
                              )
                            : fst.kind == "union" && snd.kind == "union"
                              ? fst.args.size == snd.args.size &&
                                fst.args.every(
                                  (v, i) => v.name == snd.args.get(i)!.name,
                                )
                              : false,
    ParseRawKeyOf: <T>(
      typeName: DispatchTypeName,
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
                  typeName,
                  Map(
                    parsingResult[0].fields
                      .keySeq()
                      .toArray()
                      .map((key) => [
                        key,
                        DispatchParsedType.Default.record(
                          key,
                          Map<string, DispatchParsedType<T>>(),
                          typeName,
                        ),
                      ]),
                  ),
                  typeName,
                ),
                parsingResult[1],
              ]),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing keyOf type`),
        ),
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
                      typeName,
                      parsedFieldsMap.merge(
                        parsedExtendedRecordTypesMap.reduce(
                          (acc, type) => acc.merge(type.fields),
                          Map<DispatchTypeName, DispatchParsedType<T>>(),
                        ),
                      ),
                      typeName,
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
        if (SerializedType.isPrimitive(rawType, injectedPrimitives))
          return ValueOrErrors.Default.return([
            DispatchParsedType.Default.primitive(
              rawType == "guid" ? "string" : rawType,
              typeName,
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
              DispatchParsedType.Default.singleSelection(
                typeName,
                [parsedArgs[0]],
                typeName,
              ),
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
              DispatchParsedType.Default.multiSelection(
                typeName,
                [parsedArgs[0]],
                typeName,
              ),
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
              DispatchParsedType.Default.list(
                typeName,
                [parsedArgs[0]],
                typeName,
              ),
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
                typeName,
                parsedArgs.map(([parsedArg]) => parsedArg).toArray(),
                typeName,
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
                DispatchParsedType.Default.map(
                  typeName,
                  [parsedArgs0[0], parsedArgs1[0]],
                  typeName,
                ),
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
                DispatchParsedType.Default.sum(
                  typeName,
                  [parsedArgs0[0], parsedArgs1[0]],
                  typeName,
                ),
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
            "TableType",
            rawType.args[0],
            typeNames,
            serializedTypes,
            alreadyParsedTypes,
            injectedPrimitives,
          ).Then((parsedArg) =>
            ValueOrErrors.Default.return([
              DispatchParsedType.Default.table(
                typeName,
                [parsedArg[0]],
                typeName,
              ),
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
            DispatchParsedType.Default.primitive("unit", typeName),
            alreadyParsedTypes,
          ]);
        }
        if (SerializedType.isUnion(rawType))
          // for now we assume all union cases are lookup types
          return ValueOrErrors.Operations.All(
            List<ValueOrErrors<[string, DispatchParsedType<T>], string>>(
              rawType.args.map((unionCase) =>
                typeof unionCase.fields == "string" // lookup case
                  ? serializedTypes[unionCase.fields] == undefined // probably dont need this
                    ? ValueOrErrors.Default.throwOne(
                        `Cannot find union case type: ${JSON.stringify(
                          unionCase.fields,
                        )} in types`,
                      )
                    : DispatchParsedType.Operations.ParseRawType(
                        `Union:Case ${unionCase.caseName}`,
                        unionCase.fields,
                        typeNames,
                        serializedTypes,
                        alreadyParsedTypes,
                        injectedPrimitives,
                      ).Then((parsedType) =>
                        ValueOrErrors.Default.return([
                          unionCase.caseName,
                          parsedType[0],
                        ]),
                      )
                  : DispatchParsedType.Operations.ParseRawType(
                      `Union:Case ${unionCase.caseName}`,
                      unionCase.fields == undefined
                        ? { fields: {} }
                        : unionCase,
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
              DispatchParsedType.Default.union(
                typeName,
                Map(parsedUnionCases),
                typeName,
              ),
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
            ValueOrErrors.Default.return([
              DispatchParsedType.Default.one(typeName, parsedArg[0], typeName),
              alreadyParsedTypes,
            ]),
          );
        if (SerializedType.isKeyOf(rawType))
          return DispatchParsedType.Operations.ParseRawKeyOf(
            typeName,
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
