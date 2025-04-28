import { Set, List, Map, OrderedMap, isMap } from "immutable";
import { GenericType, GenericTypes, PrimitiveTypes } from "../built-ins/state";
import { InjectedPrimitives } from "../injectables/state";
import { ValueOrErrors } from "../../../../../collections/domains/valueOrErrors/state";
import { Unit, unit } from "../../../../../../main";

export const isString = (_: any): _ is string => typeof _ == "string";
export const isObject = (_: any): _ is object => typeof _ == "object";
export const isGenericType = (_: any): _ is GenericType =>
  _ && GenericTypes.includes(_);
export const hasFun = (_: any): _ is { fun: string } =>
  isObject(_) && "fun" in _ && isString(_.fun);
export const hasArgs = (_: any): _ is { args: Array<any> } =>
  isObject(_) && "args" in _ && Array.isArray(_.args);
export type CaseName = string;
export type FieldName = string;
export type TypeName = string;
export type RawType<T> = {
  fun?: string;
  args?: Array<RawFieldType<T>> | Array<string>;
  extends?: Array<TypeName>;
  fields?: OrderedMap<FieldName, RawFieldType<T>>;
};
export const RawType = {
  isMaybeExtendedType: <T>(
    type: RawType<T>,
  ): type is RawType<T> & { extends: unknown } =>
    "extends" in type &&
    Array.isArray(type.extends) &&
    type.extends.length == 1,
  isExtendedType: <T>(
    type: RawType<T>,
  ): type is RawType<T> & { extends: Array<TypeName> } =>
    "extends" in type &&
    Array.isArray(type.extends) &&
    type.extends.length == 1 &&
    isString(type.extends[0]),
  hasFields: <T>(type: RawType<T>): type is { fields: any } => "fields" in type,
  isMaybeUnion: (_: any): _ is RawUnionType =>
    isObject(_) &&
    "fun" in _ &&
    "args" in _ &&
    _.fun == "Union" &&
    Array.isArray(_["args"]) &&
    _["args"].every(
      (__) => typeof __ == "object" && "caseName" in __ && "fields" in __,
    ),
  isKeyOf: <T>(
    _: RawFieldType<T>,
  ): _ is { fun: "KeyOf"; args: Array<string> } =>
    typeof _ == "object" &&
    "fun" in _ &&
    _.fun == "KeyOf" &&
    "args" in _ &&
    Array.isArray(_.args) &&
    _.args.length == 1 &&
    isString(_.args[0]),
};
export type RawApplicationType<T> = {
  fun?: GenericType;
  args?: Array<RawFieldType<T>>;
};

export type RawUnionCase = {
  case?: string;
  fields?: Object;
};

export type RawUnionType = {
  fun?: "Union";
  args?: Array<RawUnionCase>;
};

export type RawOptionType = {
  fun?: "Option";
  args?: Array<RawFieldType<any>>;
};

export type RawRecordType = { fields?: object };

export type RawFieldType<T> =
  | RawApplicationType<T>
  | string
  | Unit
  | PrimitiveTypeName<T>
  | RawUnionType
  | RawRecordType
  | RawUnionCase
  | RawOptionType;

export const RawFieldType = {
  isMaybePrimitive: (_: any) => isString(_),
  isPrimitive: <T>(
    _: RawFieldType<T>,
    injectedPrimitives: InjectedPrimitives<T> | undefined,
  ): _ is PrimitiveTypeName<T> =>
    Boolean(
      PrimitiveTypes.some((__) => _ == __) ||
        injectedPrimitives?.injectedPrimitives.has(_ as keyof T),
    ),
  isMaybeApplication: (_: any): _ is Object => isObject(_),
  isApplication: <T>(
    _: RawFieldType<T>,
  ): _ is { fun: GenericType; args: Array<RawFieldType<T>> } =>
    hasFun(_) && isGenericType(_.fun) && hasArgs(_),
  isMaybeLookup: (_: any) => isString(_),
  isLookup: <T>(_: RawFieldType<T>): _ is TypeName => isString(_),
  isList: <T>(
    _: RawFieldType<T>,
  ): _ is { fun: "List"; args: Array<RawFieldType<T>> } =>
    RawFieldType.isApplication(_) && _.fun == "List" && _.args.length == 1,
  isMap: <T>(
    _: RawFieldType<T>,
  ): _ is { fun: "Map"; args: Array<RawFieldType<T>> } =>
    RawFieldType.isApplication(_) && _.fun == "Map" && _.args.length == 2,
  isSum: <T>(
    _: RawFieldType<T>,
  ): _ is { fun: "Sum"; args: Array<RawFieldType<T>> } =>
    RawFieldType.isApplication(_) && _.fun == "Sum" && _.args.length == 2,
  isSingleSelection: <T>(
    _: RawFieldType<T>,
  ): _ is { fun: "SingleSelection"; args: Array<RawFieldType<T>> } =>
    RawFieldType.isApplication(_) &&
    _.fun == "SingleSelection" &&
    _.args.length == 1,
  isMultiSelection: <T>(
    _: RawFieldType<T>,
  ): _ is { fun: "MultiSelection"; args: Array<RawFieldType<T>> } =>
    RawFieldType.isApplication(_) &&
    _.fun == "MultiSelection" &&
    _.args.length == 1,
  isUnionCase: <T>(
    _: RawFieldType<T>,
  ): _ is { caseName: string; fields?: object } =>
    typeof _ == "object" && "caseName" in _,
  isUnion: <T>(
    _: RawFieldType<T>,
  ): _ is {
    fun: "Union";
    args: Array<{ caseName: string; fields?: object | string }>;
  } =>
    hasFun(_) &&
    isGenericType(_.fun) &&
    hasArgs(_) &&
    _.fun == "Union" &&
    _.args.length > 0 &&
    _.args.every((__) => typeof __ == "object" && "caseName" in __),
  isTuple: <T>(
    _: RawFieldType<T>,
  ): _ is { fun: "Tuple"; args: Array<RawFieldType<T>> } =>
    RawFieldType.isApplication(_) && _.fun == "Tuple",
  isRecord: <T>(_: RawFieldType<T>): _ is { fields: Object } =>
    typeof _ == "object" && "fields" in _ && isObject(_.fields),
  isOption: <T>(
    _: RawFieldType<T>,
  ): _ is { fun: "Option"; args: Array<RawFieldType<T>> } =>
    typeof _ == "object" &&
    "fun" in _ &&
    _.fun == "Option" &&
    "args" in _ &&
    Array.isArray(_.args) &&
    _.args.length == 1,
  isUnit: <T>(_: RawFieldType<T>): _ is string => _ == "unit",
  isTable: <T>(
    _: RawFieldType<T>,
  ): _ is { fun: "Table"; args: Array<string> } =>
    RawFieldType.isApplication(_) && _.fun == "Table" && _.args.length == 1,
};

export type PrimitiveTypeName<T> =
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
export type RecordFields<T> = Map<FieldName, ParsedType<T>>;
export type ParsedUnionCase<T> = {
  kind: "unionCase";
  name: CaseName;
  fields: ParsedType<T>;
};

export type ParsedApplicationType<T> = {
  kind: "application";
  value: GenericType;
  args: Array<ParsedType<T>>;
};

export type ParsedOptionType<T> = {
  kind: "option";
  value: ParsedType<T>;
};

export type ParsedRecordType<T> = {
  kind: "record";
  value: TypeName;
  fields: RecordFields<T>;
};

export type ParsedPrimitiveType<T> = {
  kind: "primitive";
  value: PrimitiveTypeName<T> | keyof T;
};

export type ParsedTableType = {
  kind: "table";
  value: TypeName;
  tableType: TypeName;
};

export type ParsedType<T> =
  | ParsedOptionType<T>
  | ParsedRecordType<T>
  | ParsedPrimitiveType<T>
  | { kind: "lookup"; name: TypeName }
  | ParsedApplicationType<T>
  | ParsedTableType
  | { kind: "union"; args: Map<CaseName, ParsedUnionCase<T>> };

export const ParsedType = {
  Default: {
    unionCase: <T>(
      name: CaseName,
      fields: ParsedType<T>,
    ): ParsedUnionCase<T> => ({ kind: "unionCase", name, fields }),
    option: <T>(value: ParsedType<T>): ParsedType<T> => ({
      kind: "option",
      value,
    }),
    record: <T>(value: TypeName, fields: RecordFields<T>): ParsedType<T> => ({
      kind: "record",
      value,
      fields,
    }),
    primitive: <T>(name: PrimitiveTypeName<T> | keyof T): ParsedType<T> => ({
      kind: "primitive",
      value: name,
    }),
    application: <T>(
      value: GenericType,
      args: Array<ParsedType<T>>,
    ): ParsedType<T> => ({ kind: "application", value, args }),
    union: <T>(args: Map<CaseName, ParsedUnionCase<T>>): ParsedType<T> => ({
      kind: "union",
      args,
    }),
    lookup: <T>(name: string): ParsedType<T> => ({ kind: "lookup", name }),
    table: (value: TypeName, tableType: TypeName): ParsedType<any> => ({
      kind: "table",
      value,
      tableType,
    }),
  },
  Operations: {
    Equals: <T>(fst: ParsedType<T>, snd: ParsedType<T>): boolean =>
      fst.kind == "record" && snd.kind == "record"
        ? fst.value == snd.value
        : fst.kind == "lookup" && snd.kind == "lookup"
          ? fst.name == snd.name
          : fst.kind == "primitive" && snd.kind == "primitive"
            ? fst.value == snd.value
            : fst.kind == "application" && snd.kind == "application"
              ? fst.value == snd.value &&
                fst.args.length == snd.args.length &&
                fst.args.every((v, i) =>
                  ParsedType.Operations.Equals(v, snd.args[i]),
                )
              : fst.kind == "option" && snd.kind == "option"
                ? fst.value.kind == "option" &&
                  snd.value.kind == "option" &&
                  ParsedType.Operations.Equals(fst.value.value, snd.value.value)
                : fst.kind == "union" && snd.kind == "union"
                  ? fst.args.size == snd.args.size &&
                    fst.args.every((v, i) => v.name == snd.args.get(i)!.name)
                  : false,
    ParseRawKeyOf: <T>(
      fieldName: TypeName,
      rawFieldType: RawFieldType<T>,
      types: Map<TypeName, ParsedType<T>>,
    ): ValueOrErrors<ParsedType<T>, string> => {
      if (RawType.isKeyOf(rawFieldType)) {
        const extendedType = types.get(rawFieldType.args[0]);
        if (extendedType == undefined) {
          return ValueOrErrors.Default.throwOne(
            `Error: cannot find arg ${JSON.stringify(
              rawFieldType.args[0],
            )} in types for key of ${fieldName}`,
          );
        }
        if (!RawFieldType.isRecord(extendedType)) {
          return ValueOrErrors.Default.throwOne(
            `Error: arg for key of ${fieldName} is not a record`,
          );
        }
        const fields = extendedType.fields;
        if (!isMap(fields) || fields.size == 0) {
          return ValueOrErrors.Default.throwOne(
            `No fields found for ${rawFieldType.args[0]} when parsing key of ${fieldName}`,
          );
        }

        const keys = fields.keySeq().toArray();
        return ValueOrErrors.Default.return(
          ParsedType.Default.union(
            Map(
              keys.map((key) => [
                key,
                ParsedType.Default.unionCase(
                  key,
                  ParsedType.Default.record(key, Map()),
                ),
              ]),
            ),
          ),
        );
      }
      return ValueOrErrors.Default.throwOne(
        `Error: ${JSON.stringify(rawFieldType)} is not a valid key of`,
      );
    },
    ParseRawFieldType: <T>(
      fieldName: TypeName,
      rawFieldType: RawFieldType<T>,
      types: Set<TypeName>,
      injectedPrimitives?: InjectedPrimitives<T>,
    ): ValueOrErrors<ParsedType<T>, string> => {
      if (RawFieldType.isPrimitive(rawFieldType, injectedPrimitives))
        return ValueOrErrors.Default.return(
          ParsedType.Default.primitive(
            rawFieldType == "guid" ? "string" : rawFieldType,
          ),
        );
      if (RawFieldType.isSingleSelection(rawFieldType))
        return ParsedType.Operations.ParseRawFieldType(
          fieldName,
          rawFieldType.args[0],
          types,
          injectedPrimitives,
        ).Then((parsedArgs) =>
          ValueOrErrors.Default.return(
            ParsedType.Default.application("SingleSelection", [parsedArgs]),
          ),
        );
      if (RawFieldType.isMultiSelection(rawFieldType))
        return ParsedType.Operations.ParseRawFieldType(
          fieldName,
          rawFieldType.args[0],
          types,
          injectedPrimitives,
        ).Then((parsedArgs) =>
          ValueOrErrors.Default.return(
            ParsedType.Default.application("MultiSelection", [parsedArgs]),
          ),
        );
      if (RawFieldType.isList(rawFieldType))
        return ParsedType.Operations.ParseRawFieldType(
          fieldName,
          rawFieldType.args[0],
          types,
          injectedPrimitives,
        ).Then((parsedArgs) =>
          ValueOrErrors.Default.return(
            ParsedType.Default.application("List", [parsedArgs]),
          ),
        );
      if (RawFieldType.isTuple(rawFieldType))
        return ValueOrErrors.Operations.All(
          List(
            rawFieldType.args.map((arg) =>
              ParsedType.Operations.ParseRawFieldType(
                fieldName,
                arg,
                types,
                injectedPrimitives,
              ),
            ),
          ),
        ).Then((parsedArgs) =>
          ValueOrErrors.Default.return(
            ParsedType.Default.application("Tuple", parsedArgs.toArray()),
          ),
        );

      if (RawFieldType.isOption(rawFieldType))
        return ParsedType.Operations.ParseRawFieldType(
          fieldName,
          rawFieldType.args[0],
          types,
          injectedPrimitives,
        ).Then((parsedArg) =>
          ValueOrErrors.Default.return(ParsedType.Default.option(parsedArg)),
        );
      if (RawFieldType.isMap(rawFieldType))
        return ParsedType.Operations.ParseRawFieldType(
          fieldName,
          rawFieldType.args[0],
          types,
          injectedPrimitives,
        ).Then((parsedArgs0) =>
          ParsedType.Operations.ParseRawFieldType(
            fieldName,
            rawFieldType.args[1],
            types,
            injectedPrimitives,
          ).Then((parsedArgs1) =>
            ValueOrErrors.Default.return(
              ParsedType.Default.application("Map", [parsedArgs0, parsedArgs1]),
            ),
          ),
        );
      if (RawFieldType.isSum(rawFieldType))
        return ParsedType.Operations.ParseRawFieldType(
          fieldName,
          rawFieldType.args[0],
          types,
          injectedPrimitives,
        ).Then((parsedArgs0) =>
          ParsedType.Operations.ParseRawFieldType(
            fieldName,
            rawFieldType.args[1],
            types,
            injectedPrimitives,
          ).Then((parsedArgs1) =>
            ValueOrErrors.Default.return(
              ParsedType.Default.application("Sum", [parsedArgs0, parsedArgs1]),
            ),
          ),
        );
      if (RawFieldType.isRecord(rawFieldType)) {
        return ValueOrErrors.Operations.All(
          List(
            Object.entries(rawFieldType.fields).map(([fieldName, fieldType]) =>
              ParsedType.Operations.ParseRawFieldType(
                fieldName,
                fieldType as RawFieldType<T>,
                types,
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
            ValueOrErrors.Default.return<ParsedType<T>, string>(
              ParsedType.Default.record(fieldName, parsedField),
            ),
          );
      }
      if (RawFieldType.isUnion(rawFieldType)) {
        return ValueOrErrors.Operations.All(
          List<ValueOrErrors<ParsedUnionCase<T>, string>>(
            rawFieldType.args.map((unionCase) => {
              if (!RawFieldType.isUnionCase(unionCase)) {
                return ValueOrErrors.Default.throwOne(
                  `Error: arg ${JSON.stringify(
                    unionCase,
                  )} is not a valid union case`,
                );
              }
              return ParsedType.Operations.ParseRawFieldType(
                unionCase.caseName,
                typeof unionCase.fields == "string"
                  ? unionCase.fields
                  : { fields: unionCase.fields ?? {} },
                types,
                injectedPrimitives,
              ).Then((parsedFields) =>
                ValueOrErrors.Default.return(
                  ParsedType.Default.unionCase(
                    unionCase.caseName,
                    parsedFields,
                  ),
                ),
              );
            }),
          ),
        ).Then((parsedUnionCases) =>
          ValueOrErrors.Default.return(
            ParsedType.Default.union(
              Map(parsedUnionCases.toArray().map((_) => [_.name, _] as const)),
            ),
          ),
        );
      }
      if (RawFieldType.isTable(rawFieldType)) {
        return ValueOrErrors.Default.return(
          ParsedType.Default.table(fieldName, rawFieldType.args[0]),
        );
      }
      if (RawFieldType.isLookup(rawFieldType))
        return ValueOrErrors.Default.return(
          ParsedType.Default.lookup(rawFieldType),
        );
      if (RawFieldType.isUnit(rawFieldType)) {
        return ValueOrErrors.Default.return(
          ParsedType.Default.primitive("unit"),
        );
      }
      return ValueOrErrors.Default.throw(
        List([
          `Invalid type ${JSON.stringify(
            rawFieldType,
          )} for field ${JSON.stringify(fieldName)}`,
        ]),
      );
    },
  },
};
