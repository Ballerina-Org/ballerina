import { Set, List, Map, isMap } from "immutable";
import {
  GenericType,
  GenericTypes,
  PrimitiveTypes,
} from "../../../../../../parser/domains/built-ins/state";
import { InjectedPrimitives } from "../../../../../../parser/domains/injectables/state";
import { ValueOrErrors } from "../../../../../../../../collections/domains/valueOrErrors/state";
import { MapRepo, Unit } from "../../../../../../../../../main";

export const DispatchisString = (_: any): _ is string => typeof _ == "string";
export const DispatchIsObject = (_: any): _ is object => typeof _ == "object";
export const DispatchIsGenericType = (_: any): _ is GenericType =>
  _ && GenericTypes.includes(_);
export const DispatchHasFun = (_: any): _ is { fun: string } =>
  DispatchIsObject(_) && "fun" in _ && DispatchisString(_.fun);
export const DispatchHasArgs = (_: any): _ is { args: Array<any> } =>
  DispatchIsObject(_) && "args" in _ && Array.isArray(_.args);
export type DispatchCaseName = string;
export type DispatchFieldName = string;
export type DispatchTypeName = string;

export type SerializedApplicationType<T> = {
  fun?: GenericType;
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

export type SerializedLookupType = string;

export type SerializedType<T> =
  | Unit
  | DispatchPrimitiveTypeName<T>
  | SerializedApplicationType<T>
  | SerializedLookupType
  | SerializedUnionType
  | SerializedRecordType
  | SerializedOptionType;

export const SerializedType = {
  isExtendedType: <T>(
    type: SerializedType<T>,
  ): type is SerializedType<T> & { extends: Array<DispatchTypeName> } =>
    typeof type == "object" &&
    "extends" in type &&
    Array.isArray(type.extends) &&
    type.extends.length == 1 &&
    DispatchisString(type.extends[0]),
  hasFields: <T>(type: SerializedType<T>): type is { fields: any } =>
    typeof type == "object" && "fields" in type,
  isPrimitive: <T>(
    _: SerializedType<T>,
    injectedPrimitives: InjectedPrimitives<T> | undefined,
  ): _ is DispatchPrimitiveTypeName<T> =>
    Boolean(
      PrimitiveTypes.some((__) => _ == __) ||
        injectedPrimitives?.injectedPrimitives.has(_ as keyof T),
    ),
  isApplication: <T>(
    _: SerializedType<T>,
  ): _ is { fun: GenericType; args: Array<SerializedType<T>> } =>
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
  isKeyOf: <T>(
    _: SerializedType<T>,
  ): _ is { fun: "KeyOf"; args: Array<string> } =>
    typeof _ == "object" &&
    "fun" in _ &&
    _.fun == "KeyOf" &&
    "args" in _ &&
    Array.isArray(_.args) &&
    _.args.length == 1 &&
    DispatchisString(_.args[0]),
};

export type DispatchPrimitiveTypeName<T> =
  | "unit"
  | "string"
  | "number"
  | "maybeBoolean"
  | "boolean"
  | "Date"
  | "base64File"
  | "secret"
  | keyof T
  | "guid";

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
  extendedTypes: Array<DispatchTypeName>;
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
  | TableType<T>;

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
      extendedTypes: Array<DispatchTypeName>,
    ): RecordType<T> => ({
      kind: "record",
      name,
      fields,
      typeName: typeName,
      extendedTypes,
    }),
    primitive: <T>(
      name: DispatchPrimitiveTypeName<T> | keyof T,
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
          : fst.kind == "lookup" && snd.kind == "lookup"
            ? fst.name == snd.name
            : fst.kind == "primitive" && snd.kind == "primitive"
              ? fst.name == snd.name
              : fst.kind == "list" && snd.kind == "list"
                ? fst.name == snd.name
                : fst.kind == "singleSelection" && snd.kind == "singleSelection"
                  ? fst.name == snd.name
                  : fst.kind == "multiSelection" && snd.kind == "multiSelection"
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

    // TODO -- possible source of error
    ParseRawKeyOf: <T>(
      fieldName: DispatchTypeName,
      rawType: SerializedType<T>,
      types: Map<DispatchTypeName, DispatchParsedType<T>>,
    ): ValueOrErrors<DispatchParsedType<T>, string> => {
      if (SerializedType.isKeyOf(rawType)) {
        const extendedType = types.get(rawType.args[0]);
        if (extendedType == undefined) {
          return ValueOrErrors.Default.throwOne(
            `Error: cannot find arg ${JSON.stringify(
              rawType.args[0],
            )} in types for key of ${fieldName}`,
          );
        }
        if (!SerializedType.isRecord(extendedType)) {
          return ValueOrErrors.Default.throwOne(
            `Error: arg for key of ${fieldName} is not a record`,
          );
        }
        const fields = extendedType.fields;
        if (!isMap(fields) || fields.size == 0) {
          return ValueOrErrors.Default.throwOne(
            `No fields found for ${rawType.args[0]} when parsing key of ${fieldName}`,
          );
        }

        const keys = fields.keySeq().toArray() as string[];
        const unionCases = keys.map(
          (key) =>
            [
              key,
              DispatchParsedType.Default.record(
                key,
                Map<string, DispatchParsedType<T>>(),
                fieldName,
                [],
              ),
            ] as [string, DispatchParsedType<T>],
        );

        return ValueOrErrors.Default.return(
          DispatchParsedType.Default.union(
            fieldName, // TODO: check if this is correct
            Map<string, DispatchParsedType<T>>(unionCases),
            fieldName,
          ),
        );
      }
      return ValueOrErrors.Default.throwOne(
        `Error: ${JSON.stringify(rawType)} is not a valid keyOf`,
      );
    },
    ParseRecord: <T>(
      typeName: DispatchTypeName,
      rawType: unknown,
      typeNames: Set<DispatchTypeName>,
      serializedTypes: Record<string, SerializedType<T>>,
      injectedPrimitives?: InjectedPrimitives<T>,
    ): ValueOrErrors<RecordType<T>, string> => {
      if (!SerializedType.isRecord(rawType)) {
        return ValueOrErrors.Default.throwOne(
          `Error: ${JSON.stringify(rawType)} is not a valid record`,
        );
      }
      return ValueOrErrors.Operations.All(
        List(
          Object.entries(rawType.fields).map(([fieldName, fieldType]) =>
            DispatchParsedType.Operations.ParseRawType(
              fieldName,
              fieldType as SerializedType<T>,
              typeNames,
              serializedTypes,
              injectedPrimitives,
            ).Then((parsedField) =>
              ValueOrErrors.Default.return([fieldName, parsedField] as const),
            ),
          ),
        ),
      )
        .Then((parsedFields) =>
          ValueOrErrors.Default.return(
            Map(
              parsedFields.map(([fieldName, parsedField]) => [
                fieldName,
                parsedField,
              ]),
            ),
          ),
        )
        .Then((parsedField) =>
          ValueOrErrors.Default.return<RecordType<T>, string>(
            DispatchParsedType.Default.record(
              typeName,
              parsedField,
              typeName,
              rawType.extends ?? [],
            ),
          ),
        );
    },
    ParseRawType: <T>(
      typeName: DispatchTypeName,
      rawType: SerializedType<T>,
      typeNames: Set<DispatchTypeName>,
      serializedTypes: Record<string, SerializedType<T>>,
      injectedPrimitives?: InjectedPrimitives<T>,
    ): ValueOrErrors<DispatchParsedType<T>, string> => {
      const result: ValueOrErrors<DispatchParsedType<T>, string> = (() => {
        if (SerializedType.isPrimitive(rawType, injectedPrimitives))
          return ValueOrErrors.Default.return(
            DispatchParsedType.Default.primitive(
              rawType == "guid" ? "string" : rawType,
              typeName,
            ),
          );
        if (SerializedType.isSingleSelection(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            `SingleSelection:Element`,
            rawType.args[0],
            typeNames,
            serializedTypes,
            injectedPrimitives,
          ).Then((parsedArgs) =>
            ValueOrErrors.Default.return(
              DispatchParsedType.Default.singleSelection(
                typeName,
                [parsedArgs],
                typeName,
              ),
            ),
          );
        if (SerializedType.isMultiSelection(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            `MultiSelection:Element`,
            rawType.args[0],
            typeNames,
            serializedTypes,
            injectedPrimitives,
          ).Then((parsedArgs) =>
            ValueOrErrors.Default.return(
              DispatchParsedType.Default.multiSelection(
                typeName,
                [parsedArgs],
                typeName,
              ),
            ),
          );
        if (SerializedType.isList(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            `List:Element`,
            rawType.args[0],
            typeNames,
            serializedTypes,
            injectedPrimitives,
          ).Then((parsedArgs) =>
            ValueOrErrors.Default.return(
              DispatchParsedType.Default.list(typeName, [parsedArgs], typeName),
            ),
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
                  injectedPrimitives,
                ),
              ),
            ),
          ).Then((parsedArgs) =>
            ValueOrErrors.Default.return(
              DispatchParsedType.Default.tuple(
                typeName,
                parsedArgs.toArray(),
                typeName,
              ),
            ),
          );
        if (SerializedType.isMap(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            "Map:Key",
            rawType.args[0],
            typeNames,
            serializedTypes,
            injectedPrimitives,
          ).Then((parsedArgs0) =>
            DispatchParsedType.Operations.ParseRawType(
              "Map:Value",
              rawType.args[1],
              typeNames,
              serializedTypes,
              injectedPrimitives,
            ).Then((parsedArgs1) =>
              ValueOrErrors.Default.return(
                DispatchParsedType.Default.map(
                  typeName,
                  [parsedArgs0, parsedArgs1],
                  typeName,
                ),
              ),
            ),
          );
        if (SerializedType.isSum(rawType))
          return DispatchParsedType.Operations.ParseRawType(
            "Sum:Left",
            rawType.args[0],
            typeNames,
            serializedTypes,
            injectedPrimitives,
          ).Then((parsedArgs0) =>
            DispatchParsedType.Operations.ParseRawType(
              "Sum:Right",
              rawType.args[1],
              typeNames,
              serializedTypes,
              injectedPrimitives,
            ).Then((parsedArgs1) =>
              ValueOrErrors.Default.return(
                DispatchParsedType.Default.sum(
                  typeName,
                  [parsedArgs0, parsedArgs1],
                  typeName,
                ),
              ),
            ),
          );
        if (SerializedType.isRecord(rawType))
          return DispatchParsedType.Operations.ParseRecord(
            typeName,
            rawType,
            typeNames,
            serializedTypes,
            injectedPrimitives,
          );
        if (SerializedType.isTable(rawType)) {
          return DispatchParsedType.Operations.ParseRawType(
            "TableType",
            rawType.args[0],
            typeNames,
            serializedTypes,
            injectedPrimitives,
          ).Then((parsedArg) =>
            ValueOrErrors.Default.return(
              DispatchParsedType.Default.table(typeName, [parsedArg], typeName),
            ),
          );
        }
        if (SerializedType.isLookup(rawType, typeNames))
          return ValueOrErrors.Default.return(
            DispatchParsedType.Default.lookup(rawType),
          );
        if (SerializedType.isUnit(rawType)) {
          return ValueOrErrors.Default.return(
            DispatchParsedType.Default.primitive("unit", typeName),
          );
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
                        injectedPrimitives,
                      ).Then((parsedType) =>
                        ValueOrErrors.Default.return([
                          unionCase.caseName,
                          parsedType,
                        ]),
                      )
                  : DispatchParsedType.Operations.ParseRawType(
                      `Union:Case ${unionCase.caseName}`,
                      unionCase.fields == undefined
                        ? { fields: {} }
                        : unionCase,
                      typeNames,
                      serializedTypes,
                      injectedPrimitives,
                    ).Then((parsedType) =>
                      ValueOrErrors.Default.return([
                        unionCase.caseName,
                        parsedType,
                      ]),
                    ),
              ),
            ),
          ).Then((parsedUnionCases) =>
            ValueOrErrors.Default.return(
              DispatchParsedType.Default.union(
                typeName,
                Map(parsedUnionCases),
                typeName,
              ),
            ),
          );
        return ValueOrErrors.Default.throwOne(
          `Unrecognised type "${typeName}" : ${JSON.stringify(rawType)}`,
        );
      })();
      return result.MapErrors((errors) =>
        errors.map((error) => `${error}\n...When parsing type "${typeName}"`),
      );
    },
    ExtendDispatchParsedTypes: <T>(
      DispatchParsedTypes: Map<DispatchTypeName, DispatchParsedType<T>>,
    ): ValueOrErrors<Map<DispatchTypeName, DispatchParsedType<T>>, string> =>
      ValueOrErrors.Operations.All(
        List<ValueOrErrors<DispatchParsedType<T>, string>>(
          DispatchParsedTypes.valueSeq()
            .toArray()
            .map((dispatchParsedType) => {
              if (dispatchParsedType.kind != "record") {
                return ValueOrErrors.Default.return(dispatchParsedType);
              }

              if (dispatchParsedType.kind == "record") {
                if (dispatchParsedType.extendedTypes.length <= 0) {
                  return ValueOrErrors.Default.return(dispatchParsedType);
                }
                const extendedType = DispatchParsedTypes.get(
                  dispatchParsedType.extendedTypes[0],
                );

                if (
                  extendedType == undefined ||
                  extendedType.kind != "record"
                ) {
                  return ValueOrErrors.Default.throwOne(
                    `Error: extended type ${JSON.stringify(
                      dispatchParsedType.extendedTypes[0],
                    )} is not a valid extended type`,
                  );
                }
                return ValueOrErrors.Default.return(
                  DispatchParsedType.Default.record(
                    dispatchParsedType.name,
                    dispatchParsedType.fields.merge(extendedType.fields),
                    dispatchParsedType.typeName,
                    dispatchParsedType.extendedTypes,
                  ),
                );
              }
              return ValueOrErrors.Default.throwOne(
                `Error: parsed type ${JSON.stringify(
                  dispatchParsedType,
                )} is not a valid parsed type`,
              );
            }),
        ),
      )
        .Then((DispatchParsedTypes) =>
          ValueOrErrors.Default.return(
            Map(
              DispatchParsedTypes.toArray().map(
                (_) => [_.typeName, _] as const,
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When extending types`),
        ),
  },
  // ValueOrErrors<[DispatchTypeName, UnionType<T>], string>

  //     if (SerializedType.isUnion(rawType)) {
  //     //   return ValueOrErrors.Operations.All(
  //     //     List<
  //     //       ValueOrErrors<[DispatchCaseName, DispatchParsedType<T>], string>
  //     //     >(
  //     //       rawType.args.map((unionCase) => {
  //     //         return DispatchParsedType.Operations.ParseRawUnionCase(
  //     //           unionCase,
  //     //           typeNames,
  //     //           injectedPrimitives,
  //     //         ).Then((parsedUnionCase) => {
  //     //           return ValueOrErrors.Default.return([
  //     //             unionCase.caseName,
  //     //             parsedUnionCase,
  //     //           ]);
  //     //         });
  //     //       }),
  //     //     ),
  //     //   ).Then((parsedUnionCases) =>
  //     //     ValueOrErrors.Default.return(
  //     //       DispatchParsedType.Default.union(
  //     //         typeName,
  //     //         Map(parsedUnionCases),
  //     //         typeName,
  //     //       ),
  //     //     ),
  //     //   );
  //     // }
  //   },
  // },
};
