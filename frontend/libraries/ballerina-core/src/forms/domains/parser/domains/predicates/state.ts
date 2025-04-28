import { Map, Set, List, OrderedMap } from "immutable";
import {
  ValueOrErrors,
  MapRepo,
  FieldName,
  ParsedType,
  Updater,
  simpleUpdater,
  replaceWith,
  Sum,
  ListRepo,
  BasicUpdater,
} from "../../../../../../main";

export type TuplePredicateExpression = {
  kind: "tuple";
  value: Expr;
  elementExpressions: FieldPredicateExpression[];
};

export type FieldPredicateExpression =
  | { kind: "unit"; value: Expr }
  | { kind: "primitive"; value: Expr }
  | { kind: "record"; value: Expr; fields: FieldPredicateExpressions }
  | { kind: "list"; value: Expr; elementExpression: FieldPredicateExpression }
  | {
      kind: "map";
      value: Expr;
      keyExpression: FieldPredicateExpression;
      valueExpression: FieldPredicateExpression;
    }
  | TuplePredicateExpression
  | {
      kind: "sum";
      value: Expr;
      leftExpression: FieldPredicateExpression;
      rightExpression: FieldPredicateExpression;
    };

const calculateVisibility = (
  expr: Expr,
  bindings: Bindings,
): ValueOrErrors<boolean, string> => {
  if (typeof expr == "boolean") {
    return ValueOrErrors.Default.return(expr);
  }
  return Expr.Operations.Evaluate(bindings)(expr).Then((result) => {
    if (typeof result == "boolean") {
      return ValueOrErrors.Default.return(result);
    }
    return ValueOrErrors.Default.throwOne(
      `Error: cannot evaluate expression ${JSON.stringify(expr)} to a boolean`,
    );
  });
};

export const FieldPredicateExpression = {
  Default: {
    unit: (value: Expr): FieldPredicateExpression => ({ kind: "unit", value }),
    primitive: (value: Expr): FieldPredicateExpression => ({
      kind: "primitive",
      value,
    }),
    record: (
      value: Expr,
      fields: FieldPredicateExpressions,
    ): FieldPredicateExpression => ({ kind: "record", value, fields }),
    list: (
      value: Expr,
      elementExpression: FieldPredicateExpression,
    ): FieldPredicateExpression => ({ kind: "list", value, elementExpression }),
    map: (
      value: Expr,
      keyExpression: FieldPredicateExpression,
      valueExpression: FieldPredicateExpression,
    ): FieldPredicateExpression => ({
      kind: "map",
      value,
      keyExpression,
      valueExpression,
    }),
    tuple: (
      value: Expr,
      elementExpressions: FieldPredicateExpression[],
    ): FieldPredicateExpression => ({
      kind: "tuple",
      value,
      elementExpressions,
    }),
    sum: (
      value: Expr,
      leftExpression?: FieldPredicateExpression,
      rightExpression?: FieldPredicateExpression,
    ): FieldPredicateExpression => ({
      kind: "sum",
      value,
      leftExpression:
        leftExpression ?? FieldPredicateExpression.Default.unit(false),
      rightExpression:
        rightExpression ?? FieldPredicateExpression.Default.unit(false),
    }),
  },
};

export type FieldPredicateExpressions = Map<
  FieldName,
  FieldPredicateExpression
>;

export type TupleFieldPredicateEvaluation = {
  kind: "tuple";
  value: boolean;
  elementValues: FormFieldPredicateEvaluation[];
};

export type ListFieldPredicateEvaluation = {
  kind: "list";
  value: boolean;
  elementValues: FormFieldPredicateEvaluation[];
};

export type MapFieldPredicateEvaluation = {
  kind: "map";
  value: boolean;
  elementValues: {
    key: FormFieldPredicateEvaluation;
    value: FormFieldPredicateEvaluation;
  }[];
};

export type SumFieldPredicateEvaluation = {
  kind: "sum";
  value: boolean;
  innerValue: FormFieldPredicateEvaluation;
};

export type FormsFieldPredicateEvaluation = {
  kind: "form";
  value: boolean;
  fields: FormFieldPredicateEvaluations;
};

export type FormFieldPredicateEvaluation =
  | { kind: "primitive"; value: boolean }
  | { kind: "unit"; value: boolean }
  | FormsFieldPredicateEvaluation
  | ListFieldPredicateEvaluation
  | MapFieldPredicateEvaluation
  | TupleFieldPredicateEvaluation
  | SumFieldPredicateEvaluation;

export const FormFieldPredicateEvaluation = {
  Default: {
    unit: (value: boolean): FormFieldPredicateEvaluation => ({
      kind: "unit",
      value,
    }),
    primitive: (value: boolean): FormFieldPredicateEvaluation => ({
      kind: "primitive",
      value,
    }),
    form: (
      value: boolean,
      fields: FormFieldPredicateEvaluations,
    ): FormFieldPredicateEvaluation => ({ kind: "form", value, fields }),
    list: (
      value: boolean,
      elementValues: FormFieldPredicateEvaluation[],
    ): FormFieldPredicateEvaluation => ({
      kind: "list",
      value,
      elementValues,
    }),
    map: (
      value: boolean,
      elementValues: {
        key: FormFieldPredicateEvaluation;
        value: FormFieldPredicateEvaluation;
      }[],
    ): FormFieldPredicateEvaluation => ({ kind: "map", value, elementValues }),
    tuple: (
      value: boolean,
      elementValues: FormFieldPredicateEvaluation[],
    ): FormFieldPredicateEvaluation => ({
      kind: "tuple",
      value,
      elementValues,
    }),
    sum: (
      value: boolean,
      innerValue: FormFieldPredicateEvaluation,
    ): FormFieldPredicateEvaluation => ({ kind: "sum", value, innerValue }),
  },
};

export type FormFieldPredicateEvaluations = Map<
  FieldName,
  FormFieldPredicateEvaluation
>;

export type EvaluationPredicateValue = {
  kind: "expression";
};

export type ValueRecord = {
  kind: "record";
  fields: Map<string, PredicateValue>;
};
export const ValueRecord = {
  Default: {
    empty: (): ValueRecord => ({ kind: "record", fields: Map() }),
    fromJSON: (json: object): ValueRecord => ({
      kind: "record",
      fields: Map(json),
    }),
    fromMap: (map: Map<string, PredicateValue>): ValueRecord => ({
      kind: "record",
      fields: map,
    }),
  },
  Operations: {
    has: (record: ValueRecord, key: string): boolean => {
      return record.fields.has(key);
    },
  },
  Updaters: {
    ...simpleUpdater<ValueRecord>()("fields"),
    update: (
      key: string,
      updater: BasicUpdater<PredicateValue>,
    ): Updater<ValueRecord> => {
      return ValueRecord.Updaters.fields(MapRepo.Updaters.update(key, updater));
    },
    set: (key: string, value: PredicateValue): Updater<ValueRecord> => {
      return ValueRecord.Updaters.fields(MapRepo.Updaters.set(key, value));
    },
    remove: (key: string): BasicUpdater<ValueRecord> => {
      return ValueRecord.Updaters.fields(MapRepo.Updaters.remove(key));
    },
    clear: (): BasicUpdater<ValueRecord> => {
      return ValueRecord.Updaters.fields(replaceWith(Map()));
    },
  },
};

export type ValueCustom = {
  kind: "custom";
  value: any;
};

export type ValueUnionCase = {
  kind: "unionCase";
  caseName: string;
  fields: ValueRecord;
};
export type ValuePrimitive = number | string | boolean | Date;
export type ValueUnit = { kind: "unit" };
export type ValueTuple = { kind: "tuple"; values: List<PredicateValue> };
export type ValueOption = {
  kind: "option";
  isSome: boolean;
  value: PredicateValue;
};

export const ValueOption = {
  Default: {
    some: (value: PredicateValue): ValueOption => ({
      kind: "option",
      isSome: true,
      value,
    }),
    none: (): ValueOption => ({
      kind: "option",
      isSome: false,
      value: { kind: "unit" },
    }),
  },
  Updaters: {
    value: (_: BasicUpdater<PredicateValue>) =>
      Updater<PredicateValue>((v) => ValueOption.Default.some(_(v))),
  },
};
export type ValueVarLookup = { kind: "varLookup"; varName: string };
export type ValueSum = {
  kind: "sum";
  value: Sum<PredicateValue, PredicateValue>;
};

export const ValueSum = {
  Default: (value: Sum<PredicateValue, PredicateValue>): ValueSum => ({
    kind: "sum",
    value,
  }),
  Updaters: {
    ...simpleUpdater<ValueSum>()("value"),
  },
};

export const ValueTuple = {
  Default: (): ValueTuple => ({
    kind: "tuple",
    values: List(),
  }),
  Updaters: {
    ...simpleUpdater<ValueTuple>()("values"),
  },
};

export type PredicateValue =
  | ValuePrimitive
  | ValueUnit
  | ValueTuple
  | ValueRecord
  | ValueUnionCase
  | ValueOption
  | ValueVarLookup
  | ValueSum
  | ValueCustom;

export type ExprLambda = { kind: "lambda"; parameter: string; body: Expr };
export type ExprMatchCase = { kind: "matchCase"; operands: Expr[] };
export type ExprCase = {
  kind: "caseName";
  caseName: string;
  handler: ExprLambda;
};
export type ExprItemLookup = { kind: "itemLookup"; operands: [Expr, number] };
export type ExprFieldLookup = { kind: "fieldLookup"; operands: [Expr, string] };
export type ExprIsCase = { kind: "isCase"; operands: [Expr, string] };
export type ExprBinaryOperator = {
  kind: BinaryOperator;
  operands: [Expr, Expr];
};

export type Expr =
  | PredicateValue
  | ExprItemLookup
  | ExprFieldLookup
  | ExprIsCase
  | ExprBinaryOperator
  | ExprLambda
  | ExprMatchCase
  | ExprCase;

export const BinaryOperators = ["or", "equals"] as const;
export const BinaryOperatorsSet = Set(BinaryOperators);
export type BinaryOperator = (typeof BinaryOperators)[number];

export type Bindings = Map<string, PredicateValue>;

export const PredicateValue = {
  Default: {
    string: () => "",
    number: () => 0,
    boolean: () => false,
    date: () => new Date(),
    unit: (): PredicateValue => ({ kind: "unit" }),
    tuple: (values: List<PredicateValue>): ValueTuple => ({
      kind: "tuple",
      values,
    }),
    record: (fields: OrderedMap<string, PredicateValue>): ValueRecord => ({
      kind: "record",
      fields,
    }),
    unionCase: (caseName: string, fields: ValueRecord): ValueUnionCase => ({
      kind: "unionCase",
      caseName,
      fields,
    }),
    option: (isSome: boolean, value: PredicateValue): ValueOption => ({
      kind: "option",
      isSome,
      value,
    }),
    varLookup: (varName: string): PredicateValue => ({
      kind: "varLookup",
      varName,
    }),
    sum: (value: Sum<PredicateValue, PredicateValue>): ValueSum => ({
      kind: "sum",
      value,
    }),
  },
  Operations: {
    IsPrimitive: (
      value: PredicateValue | Expr,
    ): value is boolean | number | string | Date => {
      return (
        PredicateValue.Operations.IsBoolean(value) ||
        PredicateValue.Operations.IsNumber(value) ||
        PredicateValue.Operations.IsString(value) ||
        PredicateValue.Operations.IsDate(value)
      );
    },
    IsBoolean: (value: PredicateValue | Expr): value is boolean => {
      return typeof value == "boolean";
    },
    IsNumber: (value: PredicateValue | Expr): value is number => {
      return typeof value == "number";
    },
    IsString: (value: PredicateValue | Expr): value is string => {
      return typeof value == "string";
    },
    IsDate: (value: PredicateValue | Expr): value is Date => {
      return (
        typeof value == "object" &&
        Object.prototype.toString.call(value) === "[object Date]" &&
        value instanceof Date &&
        !isNaN(value.getTime())
      );
    },
    IsUnit: (value: PredicateValue | Expr): value is ValueUnit => {
      return (
        typeof value == "object" &&
        !PredicateValue.Operations.IsDate(value) &&
        value.kind == "unit"
      );
    },
    IsUnionCase: (value: PredicateValue | Expr): value is ValueUnionCase => {
      return (
        typeof value == "object" &&
        !PredicateValue.Operations.IsDate(value) &&
        value.kind == "unionCase"
      );
    },
    IsRecord: (value: PredicateValue | Expr): value is ValueRecord => {
      return (
        typeof value == "object" &&
        !PredicateValue.Operations.IsDate(value) &&
        value.kind == "record"
      );
    },
    IsTuple: (value: PredicateValue | Expr): value is ValueTuple => {
      return (
        typeof value == "object" &&
        !PredicateValue.Operations.IsDate(value) &&
        value.kind == "tuple"
      );
    },
    IsOption: (value: PredicateValue | Expr): value is ValueOption => {
      return (
        typeof value == "object" &&
        !PredicateValue.Operations.IsDate(value) &&
        value.kind == "option"
      );
    },
    IsSum: (value: PredicateValue | Expr): value is ValueSum => {
      return (
        typeof value == "object" &&
        !PredicateValue.Operations.IsDate(value) &&
        value.kind == "sum"
      );
    },
    IsVarLookup: (value: PredicateValue | Expr): value is ValueVarLookup => {
      return (
        typeof value == "object" &&
        !PredicateValue.Operations.IsDate(value) &&
        value.kind == "varLookup"
      );
    },
    IsCustom: (value: PredicateValue | Expr): value is ValueCustom => {
      return (
        typeof value == "object" &&
        !PredicateValue.Operations.IsDate(value) &&
        value.kind == "custom"
      );
    },
    ParseAsDate: (json: any): ValueOrErrors<PredicateValue, string> => {
      if (PredicateValue.Operations.IsDate(json))
        return ValueOrErrors.Default.return(json);
      return ValueOrErrors.Default.throwOne(`date has invalid value property`);
    },
    ParseAsVarLookup: (json: any): ValueOrErrors<PredicateValue, string> => {
      if (json.kind == "varLookup" && typeof json.varName == "string")
        return ValueOrErrors.Default.return(
          PredicateValue.Default.varLookup(json.varName),
        );
      return ValueOrErrors.Default.throwOne(
        `varLookup has invalid varName property`,
      );
    },
    ParseAsUnionCase: (json: any): ValueOrErrors<PredicateValue, string> => {
      if (typeof json.caseName == "string")
        return ValueOrErrors.Default.return(
          PredicateValue.Default.unionCase(json.caseName, json.value),
        );

      return ValueOrErrors.Default.throwOne(
        `union case has invalid caseName property`,
      );
    },
    ParseAsRecord: (
      json: any,
      types: Map<string, ParsedType<any>>,
    ): ValueOrErrors<PredicateValue, string> => {
      if ("fields" in json && typeof json.fields == "object") {
        return ValueOrErrors.Operations.All(
          List(
            Object.entries(json.fields).map(([fieldName, fieldValue]) =>
              PredicateValue.Operations.parse(
                fieldValue,
                { kind: "expression" },
                types,
              ).Then((value) =>
                ValueOrErrors.Default.return([fieldName, value] as [
                  string,
                  PredicateValue,
                ]),
              ),
            ),
          ),
        ).Then((entries) =>
          ValueOrErrors.Default.return(
            PredicateValue.Default.record(OrderedMap(entries)),
          ),
        );
      }
      return ValueOrErrors.Default.throwOne(`record has no field property`);
    },
    ParseAsTuple: (
      json: any,
      types: Map<string, ParsedType<any>>,
    ): ValueOrErrors<PredicateValue, string> => {
      if (json.values != undefined && Array.isArray(json.values)) {
        return ValueOrErrors.Operations.All(
          List<ValueOrErrors<PredicateValue, string>>(
            json.values.map((elementValue: any) =>
              PredicateValue.Operations.parse(
                elementValue,
                { kind: "expression" },
                types,
              ),
            ),
          ),
        ).Then((values) =>
          ValueOrErrors.Default.return(PredicateValue.Default.tuple(values)),
        );
      }
      return ValueOrErrors.Default.throwOne(
        `Error: tuple has no values property`,
      );
    },
    parse: <T>(
      json: any,
      type: ParsedType<T> | EvaluationPredicateValue,
      types: Map<string, ParsedType<T>>,
    ): ValueOrErrors<PredicateValue, string> => {
      const result: ValueOrErrors<PredicateValue, string> = (() => {
        if (
          type.kind == "expression" &&
          (typeof json == "boolean" ||
            typeof json == "number" ||
            typeof json == "string")
        )
          return ValueOrErrors.Default.return(json);
        if (type.kind == "expression" && json.kind == undefined) {
          return ValueOrErrors.Default.throwOne(
            `evaluation statement has no kind value`,
          );
        }
        if (type.kind == "expression" && json.kind == "guid") {
          return ValueOrErrors.Default.return(json);
        }
        if (type.kind == "expression" && json.kind == "date") {
          return PredicateValue.Operations.ParseAsDate(json);
        }
        if (type.kind == "expression" && json.kind == "unit") {
          return ValueOrErrors.Default.return(PredicateValue.Default.unit());
        }
        if (type.kind == "expression" && json.kind == "varLookup") {
          return PredicateValue.Operations.ParseAsVarLookup(json);
        }
        if (
          type.kind == "expression" &&
          json.kind == "record" &&
          "caseName" in json
        ) {
          return PredicateValue.Operations.ParseAsUnionCase(json);
        }
        if (
          type.kind == "expression" &&
          json.kind == "record" &&
          "fields" in json
        ) {
          return PredicateValue.Operations.ParseAsRecord(json, types);
        }
        if (
          type.kind == "expression" &&
          json.kind == "tuple" &&
          "values" in json
        ) {
          return PredicateValue.Operations.ParseAsTuple(json, types);
        }
        if (type.kind == "primitive" && type.value == "Date") {
          if (PredicateValue.Operations.IsDate(json)) {
            return ValueOrErrors.Default.return(json);
          }
          return ValueOrErrors.Default.throwOne(
            `failed to parse date ${JSON.stringify(json)}`,
          );
        }
        if (type.kind == "primitive" && type.value == "maybeBoolean") {
          return json == undefined
            ? ValueOrErrors.Default.return(false)
            : ValueOrErrors.Default.return(json);
        }
        if (type.kind == "primitive") {
          return ValueOrErrors.Default.return(json);
        }
        if (type.kind == "lookup") {
          const subType = types.get(type.name);
          if (subType == undefined) {
            return ValueOrErrors.Default.throwOne(
              `cannot find field ${type.name} in types`,
            );
          }
          return PredicateValue.Operations.parse(json, subType, types);
        }
        if (type.kind == "union") {
          const unionCase = type.args.get(json);
          if (unionCase == undefined) {
            return ValueOrErrors.Default.throwOne(
              `Error: cannot find union case ${JSON.stringify(json)} in types`,
            );
          }
          return PredicateValue.Operations.ParseAsUnionCase({
            kind: "unionCase",
            caseName: json,
            value: { kind: "form", value: Map() },
          });
        }
        if (type.kind == "application" && type.value == "List") {
          return ValueOrErrors.Operations.All(
            List<ValueOrErrors<PredicateValue, string>>(
              json.map((elementValue: any) =>
                PredicateValue.Operations.parse(
                  elementValue,
                  type.args[0],
                  types,
                ),
              ),
            ),
          ).Then((values) =>
            ValueOrErrors.Default.return(PredicateValue.Default.tuple(values)),
          );
        }
        if (type.kind == "application" && type.value == "Map") {
          return ValueOrErrors.Operations.All(
            List<ValueOrErrors<PredicateValue, string>>(
              json.map((keyValue: any) =>
                PredicateValue.Operations.parse(
                  keyValue.key,
                  type.args[0],
                  types,
                ).Then((key) =>
                  PredicateValue.Operations.parse(
                    keyValue.value,
                    type?.args[1],
                    types,
                  ).Then((value) =>
                    ValueOrErrors.Default.return(
                      PredicateValue.Default.tuple(List([key, value])),
                    ),
                  ),
                ),
              ),
            ),
          ).Then((values) =>
            ValueOrErrors.Default.return(PredicateValue.Default.tuple(values)),
          );
        }
        if (type.kind == "application" && type.value == "Sum") {
          return PredicateValue.Operations.parse(
            json.left,
            type?.args[0],
            types,
          ).Then((left) =>
            PredicateValue.Operations.parse(
              json.right,
              type?.args[1],
              types,
            ).Then((right) =>
              ValueOrErrors.Default.return(
                PredicateValue.Default.tuple(List([left, right])),
              ),
            ),
          );
        }
        if (type.kind == "application" && type.value == "SingleSelection") {
          ValueOrErrors.Default.return(
            PredicateValue.Default.option(json["IsSome"], json["Value"]),
          );
        }
        if (type.kind == "application" && type.value == "MultiSelection") {
          return ValueOrErrors.Operations.All(
            List<ValueOrErrors<PredicateValue, string>>(
              json.map((elementValue: any) =>
                PredicateValue.Operations.parse(
                  elementValue,
                  type.args[0],
                  types,
                ),
              ),
            ),
          ).Then((values) =>
            ValueOrErrors.Default.return(PredicateValue.Default.tuple(values)),
          );
        }
        if (type.kind == "record") {
          return ValueOrErrors.Operations.All(
            List<ValueOrErrors<[string, PredicateValue], string>>(
              Object.entries(json).map(([fieldName, fieldValue]) => {
                const subType = type.fields.get(fieldName);
                if (subType == undefined) {
                  return ValueOrErrors.Default.throwOne(
                    `cannot find field ${fieldName} in type ${JSON.stringify(
                      type,
                    )}`,
                  );
                }
                return PredicateValue.Operations.parse(
                  fieldValue,
                  subType,
                  types,
                ).Then((value) =>
                  ValueOrErrors.Default.return([fieldName, value]),
                );
              }),
            ),
          ).Then((entries: List<[string, PredicateValue]>) =>
            ValueOrErrors.Default.return(
              PredicateValue.Default.record(
                OrderedMap(entries.map((_) => [_[0], _[1]])),
              ),
            ),
          );
        }
        return ValueOrErrors.Default.throwOne(
          `unsupported type ${JSON.stringify(type)}`,
        );
      })();
      return result.MapErrors((errors) =>
        errors.map(
          (error) =>
            `${error}\n...When parsing expression ${JSON.stringify(
              json,
              null,
              2,
            )}`,
        ),
      );
    },
    recordToTuple: (r: ValueRecord): ValueTuple => {
      const valuesSortedByName = r.fields
        .toSeq()
        .map((v, k) => [k, v])
        .sortBy(([k, v]) => k)
        .map(([k, v]) => v)
        .valueSeq()
        .toArray();
      return PredicateValue.Default.tuple(List(valuesSortedByName));
    },
    Equals:
      (vars: Bindings) =>
      (
        v1: PredicateValue,
        v2: PredicateValue,
      ): ValueOrErrors<boolean, string> =>
        typeof v1 == "boolean" ||
        typeof v1 == "number" ||
        typeof v1 == "string" ||
        typeof v2 == "boolean" ||
        typeof v2 == "number" ||
        typeof v2 == "string"
          ? typeof v1 == typeof v2
            ? ValueOrErrors.Default.return(v1 == v2)
            : ValueOrErrors.Default.throwOne(
                `cannot compare expressions of different types ${JSON.stringify(
                  v1,
                )} and ${JSON.stringify(v2)}.`,
              )
          : PredicateValue.Operations.IsDate(v1) &&
              PredicateValue.Operations.IsDate(v2)
            ? v1.getTime() == v2.getTime()
              ? ValueOrErrors.Default.return(true)
              : ValueOrErrors.Default.return(false)
            : PredicateValue.Operations.IsUnionCase(v1) &&
                PredicateValue.Operations.IsUnionCase(v2)
              ? v1.caseName == v2.caseName
                ? PredicateValue.Operations.Equals(vars)(v1.fields, v2.fields)
                : ValueOrErrors.Default.return(false)
              : PredicateValue.Operations.IsTuple(v1) &&
                  PredicateValue.Operations.IsTuple(v2)
                ? v1.values.size != v2.values.size
                  ? ValueOrErrors.Default.return(false)
                  : v1.values.size == 0
                    ? ValueOrErrors.Default.return(true)
                    : PredicateValue.Operations.Equals(vars)(
                        v1.values.get(0)!,
                        v2.values.get(0)!,
                      ).Then((firstEqual) =>
                        firstEqual
                          ? PredicateValue.Operations.Equals(vars)(
                              PredicateValue.Default.tuple(v1.values.slice(1)),
                              PredicateValue.Default.tuple(v2.values.slice(1)),
                            )
                          : ValueOrErrors.Default.return(false),
                      )
                : PredicateValue.Operations.IsRecord(v1) &&
                    PredicateValue.Operations.IsRecord(v2)
                  ? PredicateValue.Operations.Equals(vars)(
                      PredicateValue.Operations.recordToTuple(v1),
                      PredicateValue.Operations.recordToTuple(v2),
                    )
                  : PredicateValue.Operations.IsUnit(v1) &&
                      PredicateValue.Operations.IsUnit(v2)
                    ? ValueOrErrors.Default.return(true)
                    : PredicateValue.Operations.IsUnit(v1) !=
                        PredicateValue.Operations.IsUnit(v2)
                      ? ValueOrErrors.Default.throwOne(
                          `cannot compare expressions of different types ${JSON.stringify(
                            v1,
                          )} and ${JSON.stringify(v2)}.`,
                        )
                      : ValueOrErrors.Default.throwOne(
                          `structural equality is not implemented yet between ${JSON.stringify(
                            v1,
                          )} and ${JSON.stringify(v2)}.`,
                        ),
  },
};

export const Expr = {
  Default: {
    itemLookup: (e: Expr, i: number): Expr => ({
      kind: "itemLookup",
      operands: [e, i],
    }),
    fieldLookup: (e: Expr, f: string): Expr => ({
      kind: "fieldLookup",
      operands: [e, f],
    }),
    isCase: (e: Expr, c: string): Expr => ({
      kind: "isCase",
      operands: [e, c],
    }),
    binaryOperator: (op: BinaryOperator, e1: Expr, e2: Expr): Expr => ({
      kind: op,
      operands: [e1, e2],
    }),
    matchCase: (operands: Expr[]): Expr => ({
      kind: "matchCase",
      operands,
    }),
    lambda: (parameter: string, body: Expr): Expr => ({
      kind: "lambda",
      parameter,
      body,
    }),
    case: (caseName: string, handler: ExprLambda): Expr => ({
      kind: "caseName",
      caseName,
      handler,
    }),
  },
  Operations: {
    IsItemLookup: (e: Expr): e is ExprItemLookup => {
      return (
        typeof e == "object" &&
        !PredicateValue.Operations.IsDate(e) &&
        e.kind == "itemLookup"
      );
    },
    IsFieldLookup: (e: Expr): e is ExprFieldLookup => {
      return (
        typeof e == "object" &&
        !PredicateValue.Operations.IsDate(e) &&
        e.kind == "fieldLookup"
      );
    },
    IsIsCase: (e: Expr): e is ExprIsCase => {
      return (
        typeof e == "object" &&
        !PredicateValue.Operations.IsDate(e) &&
        e.kind == "isCase"
      );
    },
    IsBinaryOperator: (e: Expr): e is ExprBinaryOperator => {
      return (
        typeof e == "object" &&
        !PredicateValue.Operations.IsDate(e) &&
        BinaryOperatorsSet.has(e.kind as BinaryOperator)
      );
    },
    IsCase: (e: Expr): e is ExprCase => {
      return (
        typeof e == "object" &&
        !PredicateValue.Operations.IsDate(e) &&
        e.kind == "caseName"
      );
    },
    IsCaseArray: (e: Expr[]): e is ExprCase[] => {
      return e.every((e) => Expr.Operations.IsCase(e));
    },
    IsMatchCase: (e: Expr): e is ExprMatchCase => {
      return (
        typeof e == "object" &&
        !PredicateValue.Operations.IsDate(e) &&
        e.kind == "matchCase"
      );
    },
    IsLambda: (e: Expr): e is ExprLambda => {
      return (
        typeof e == "object" &&
        !PredicateValue.Operations.IsDate(e) &&
        e.kind == "lambda"
      );
    },
    parseAsVisibilityExpression: (json: any): ValueOrErrors<Expr, string> =>
      Expr.Operations.parse(json).MapErrors((errors) =>
        errors.map(
          (error) => `${error}\n...When parsing visibility expression`,
        ),
      ),
    parseAsDisabledExpression: (json: any): ValueOrErrors<Expr, string> =>
      Expr.Operations.parse(json).MapErrors((errors) =>
        errors.map((error) => `${error}\n...When parsing disabled expression`),
      ),
    parse: (json: any): ValueOrErrors<Expr, string> => {
      const asValue = PredicateValue.Operations.parse(
        json,
        { kind: "expression" },
        Map<string, ParsedType<unknown>>(),
      );
      if (asValue.kind == "value") return asValue;

      if (Expr.Operations.IsItemLookup(json)) {
        const [first, second]: Array<any> = json["operands"];
        return Expr.Operations.parse(first).Then((first) =>
          ValueOrErrors.Default.return(
            // Tuples are 1-indexed
            Expr.Default.itemLookup(first, second - 1),
          ),
        );
      }
      if (Expr.Operations.IsFieldLookup(json)) {
        const [first, second]: Array<any> = json["operands"];
        return Expr.Operations.parse(first).Then((first) =>
          ValueOrErrors.Default.return(Expr.Default.fieldLookup(first, second)),
        );
      }
      if (Expr.Operations.IsIsCase(json)) {
        const [first, second]: Array<any> = json["operands"];
        return Expr.Operations.parse(first).Then((first) =>
          ValueOrErrors.Default.return(Expr.Default.isCase(first, second)),
        );
      }

      if (Expr.Operations.IsBinaryOperator(json)) {
        const [first, second]: Array<any> = json["operands"];
        if (BinaryOperatorsSet.contains(json["kind"] as BinaryOperator)) {
          return Expr.Operations.parse(first).Then((first) =>
            Expr.Operations.parse(second).Then((second) =>
              ValueOrErrors.Default.return(
                Expr.Default.binaryOperator(json["kind"], first, second),
              ),
            ),
          );
        }
      }

      if (Expr.Operations.IsMatchCase(json)) {
        return ValueOrErrors.Operations.All(
          List<ValueOrErrors<Expr, string>>(
            json["operands"].map((operand) => Expr.Operations.parse(operand)),
          ),
        ).Then((operands) =>
          ValueOrErrors.Default.return(
            Expr.Default.matchCase(operands.toArray()),
          ),
        );
      }
      if (Expr.Operations.IsLambda(json)) {
        return Expr.Operations.parse(json["body"]).Then((body) =>
          ValueOrErrors.Default.return(
            Expr.Default.lambda(json["parameter"], body),
          ),
        );
      }
      if (Expr.Operations.IsCase(json)) {
        return Expr.Operations.parse(json["handler"]).Then((handler) =>
          Expr.Operations.IsLambda(handler)
            ? ValueOrErrors.Default.return(
                Expr.Default.case(json["caseName"], handler),
              )
            : ValueOrErrors.Default.throwOne(
                `expected lambda expression, got ${JSON.stringify(handler)}`,
              ),
        );
      }
      return ValueOrErrors.Default.throwOne(
        `cannot parse ${JSON.stringify(json)} to Expr.`,
      );
    },
    EvaluateAsTuple:
      (vars: Bindings) =>
      (e: Expr): ValueOrErrors<ValueTuple, string> =>
        !PredicateValue.Operations.IsTuple(e)
          ? ValueOrErrors.Default.throwOne(
              `expected tuple expression, got ${JSON.stringify(e)}`,
            )
          : ValueOrErrors.Default.return(e),
    EvaluateAsRecord:
      (vars: Bindings) =>
      (e: Expr): ValueOrErrors<ValueRecord, string> =>
        !PredicateValue.Operations.IsRecord(e)
          ? ValueOrErrors.Default.throwOne(
              `expected record expression, got ${JSON.stringify(e)}`,
            )
          : ValueOrErrors.Default.return(e),
    EvaluateAsUnionCase:
      (vars: Bindings) =>
      (e: Expr): ValueOrErrors<ValueUnionCase, string> =>
        !PredicateValue.Operations.IsUnionCase(e)
          ? ValueOrErrors.Default.throwOne(
              `expected union case expression, got ${JSON.stringify(e)}`,
            )
          : ValueOrErrors.Default.return(e),
    EvaluateAsBoolean:
      (vars: Bindings) =>
      (e: Expr): ValueOrErrors<boolean, string> =>
        !PredicateValue.Operations.IsBoolean(e)
          ? ValueOrErrors.Default.throwOne(
              `expected boolean expression, got ${JSON.stringify(e)}`,
            )
          : ValueOrErrors.Default.return(e),
    MatchCase:
      (vars: Bindings) =>
      (
        e: ValueUnionCase,
        cases: ExprCase[],
      ): ValueOrErrors<PredicateValue, string> => {
        const matchedCase = cases.find((c) => c.caseName == e.caseName);
        if (matchedCase == undefined) {
          return ValueOrErrors.Default.throwOne(
            `cannot find match case ${JSON.stringify(
              e.caseName,
            )} in ${JSON.stringify(cases)}`,
          );
        }
        const updatedBindings = vars.set(
          matchedCase.handler.parameter,
          e.fields,
        );
        return Expr.Operations.Evaluate(updatedBindings)(matchedCase.handler);
      },
    ComputePredicateEvaluation:
      (vars: Bindings) =>
      (expr: Expr): ValueOrErrors<boolean, string> => {
        if (typeof expr == "boolean") {
          return ValueOrErrors.Default.return(expr);
        }
        return Expr.Operations.Evaluate(vars)(expr).Then((result) => {
          if (typeof result == "boolean") {
            return ValueOrErrors.Default.return(result);
          }
          return ValueOrErrors.Default.throwOne(
            `Error: cannot evaluate expression ${JSON.stringify(
              expr,
            )} to a boolean`,
          );
        });
      },
    EvaluateAs:
      (as: string) =>
      (vars: Bindings) =>
      (e: Expr): ValueOrErrors<PredicateValue, string> => {
        return Expr.Operations.Evaluate(vars)(e).MapErrors((errors) =>
          errors.map(
            (error) =>
              `${error}\n...When evaluating expression ${JSON.stringify(
                e,
              )} as ${as}`,
          ),
        );
      },
    Evaluate:
      (vars: Bindings) =>
      (e: Expr): ValueOrErrors<PredicateValue, string> => {
        const result: ValueOrErrors<PredicateValue, string> = (() => {
          return PredicateValue.Operations.IsBoolean(e) ||
            PredicateValue.Operations.IsNumber(e) ||
            PredicateValue.Operations.IsString(e) ||
            PredicateValue.Operations.IsUnit(e) ||
            PredicateValue.Operations.IsRecord(e) ||
            PredicateValue.Operations.IsTuple(e) ||
            PredicateValue.Operations.IsUnionCase(e) ||
            PredicateValue.Operations.IsUnit(e)
            ? ValueOrErrors.Default.return(e)
            : PredicateValue.Operations.IsVarLookup(e)
              ? MapRepo.Operations.tryFindWithError(
                  e.varName,
                  vars,
                  () =>
                    `Error: cannot find variable ${JSON.stringify(e.varName)}`,
                )
              : Expr.Operations.IsItemLookup(e)
                ? Expr.Operations.Evaluate(vars)(e.operands[0]).Then(
                    (maybeTuple: PredicateValue) =>
                      Expr.Operations.EvaluateAsTuple(vars)(maybeTuple).Then(
                        (tuple: ValueTuple) =>
                          ListRepo.Operations.tryFindWithError(
                            e.operands[1],
                            tuple.values,
                            () =>
                              `Error: cannot find element of index ${
                                e.operands[1]
                              } in tuple ${JSON.stringify(tuple)}`,
                          ),
                      ),
                  )
                : Expr.Operations.IsFieldLookup(e)
                  ? Expr.Operations.Evaluate(vars)(e.operands[0]).Then(
                      (maybeRecord: PredicateValue) =>
                        Expr.Operations.EvaluateAsRecord(vars)(
                          maybeRecord,
                        ).Then((record: ValueRecord) =>
                          MapRepo.Operations.tryFindWithError(
                            e.operands[1],
                            record.fields,
                            () =>
                              `Error: cannot find field ${
                                e.operands[1]
                              } in record ${JSON.stringify(record)}`,
                          ),
                        ),
                    )
                  : Expr.Operations.IsIsCase(e)
                    ? Expr.Operations.Evaluate(vars)(e.operands[0]).Then(
                        (maybeUnionCase: PredicateValue) =>
                          Expr.Operations.EvaluateAsUnionCase(vars)(
                            maybeUnionCase,
                          ).Then((unionCase: ValueUnionCase) =>
                            ValueOrErrors.Default.return(
                              unionCase.caseName == e.operands[1],
                            ),
                          ),
                      )
                    : Expr.Operations.IsMatchCase(e)
                      ? Expr.Operations.Evaluate(vars)(e.operands[0]).Then(
                          (maybeUnionCase: PredicateValue) =>
                            Expr.Operations.EvaluateAsUnionCase(vars)(
                              maybeUnionCase,
                            ).Then((unionCase: ValueUnionCase) => {
                              const cases = e.operands.slice(1);
                              if (!Expr.Operations.IsCaseArray(cases)) {
                                return ValueOrErrors.Default.throwOne(
                                  `Error: expected cases, got ${JSON.stringify(cases)}`,
                                );
                              }
                              return Expr.Operations.MatchCase(vars)(
                                unionCase,
                                cases,
                              );
                            }),
                        )
                      : Expr.Operations.IsLambda(e)
                        ? Expr.Operations.Evaluate(vars)(e.body)
                        : Expr.Operations.IsBinaryOperator(e) &&
                            e.kind == "equals"
                          ? Expr.Operations.Evaluate(vars)(e.operands[0]).Then(
                              (v1) =>
                                Expr.Operations.Evaluate(vars)(
                                  e.operands[1],
                                ).Then((v2) =>
                                  PredicateValue.Operations.Equals(vars)(
                                    v1,
                                    v2,
                                  ).Then((eq) =>
                                    ValueOrErrors.Default.return(eq),
                                  ),
                                ),
                            )
                          : Expr.Operations.IsBinaryOperator(e) &&
                              e.kind == "or"
                            ? Expr.Operations.Evaluate(vars)(
                                e.operands[0],
                              ).Then((v1) =>
                                Expr.Operations.Evaluate(vars)(
                                  e.operands[1],
                                ).Then((v2) =>
                                  Expr.Operations.EvaluateAsBoolean(vars)(
                                    v1,
                                  ).Then((v1) =>
                                    Expr.Operations.EvaluateAsBoolean(vars)(
                                      v2,
                                    ).Then((v2) =>
                                      ValueOrErrors.Default.return(v1 || v2),
                                    ),
                                  ),
                                ),
                              )
                            : ValueOrErrors.Default.throwOne(
                                `Error: unsupported expression ${JSON.stringify(e)}`,
                              );
        })();
        return result.MapErrors((errors) =>
          errors.map(
            (error) =>
              `${error}\n...When evaluating expression ${JSON.stringify(
                e,
                null,
                2,
              )}`,
          ),
        );
      },
  },
};

export const evaluatePredicates = <T>(
  context: {
    global: PredicateValue;
    visibilityPredicateExpressions: FieldPredicateExpressions;
    disabledPredicatedExpressions: FieldPredicateExpressions;
  },
  root: PredicateValue,
): ValueOrErrors<
  {
    visiblityPredicateEvaluations: FormFieldPredicateEvaluation;
    disabledPredicateEvaluations: FormFieldPredicateEvaluation;
  },
  string
> => {
  const bindings: Bindings = Map<string, PredicateValue>()
    .set("global", context.global)
    .set("root", root)
    .set("local", root);
  const traverse = (
    bindings: Bindings,
    predicate: FieldPredicateExpression,
    raw: PredicateValue,
  ): ValueOrErrors<FormFieldPredicateEvaluation, string> => {
    if (predicate.kind == "primitive") {
      return calculateVisibility(predicate.value, bindings).Then((result) => {
        return ValueOrErrors.Default.return({
          kind: "primitive",
          value: result,
        });
      });
    }
    if (predicate.kind == "record") {
      if (typeof raw != "object" || !("kind" in raw) || raw.kind != "record") {
        return ValueOrErrors.Default.throwOne(
          `parsing expected record in raw, got ${JSON.stringify(raw)}`,
        );
      }
      return calculateVisibility(predicate.value, bindings).Then((result) =>
        ValueOrErrors.Operations.All(
          List<ValueOrErrors<[string, FormFieldPredicateEvaluation], string>>(
            predicate.fields
              .entrySeq()
              .map<
                ValueOrErrors<[string, FormFieldPredicateEvaluation], string>
              >(([fieldName, fieldPredicate]) => {
                const fieldRaw = raw.fields.get(fieldName);

                if (fieldRaw == undefined) {
                  return ValueOrErrors.Default.return([
                    fieldName,
                    { kind: "primitive", value: false },
                  ]);
                }

                if (fieldPredicate.kind == "record") {
                  const localBindings = bindings.get("local")! as ValueRecord;
                  const fieldLocal = localBindings.fields.get(fieldName);
                  if (fieldLocal == undefined) {
                    return ValueOrErrors.Default.throwOne(
                      `Error: parsing cannot find field ${fieldName} in local ${JSON.stringify(
                        localBindings,
                      )}`,
                    );
                  }
                  const fieldBindings = bindings.set("local", fieldLocal);
                  return traverse(fieldBindings, fieldPredicate, fieldRaw).Then(
                    (evaluation) =>
                      ValueOrErrors.Default.return([fieldName, evaluation]),
                  );
                }

                return traverse(bindings, fieldPredicate, fieldRaw).Then(
                  (evaluation) =>
                    ValueOrErrors.Default.return([fieldName, evaluation]),
                );
              }),
          ),
        ).Then((evaluations) => {
          return ValueOrErrors.Default.return({
            kind: "form",
            value: result,
            fields: Map(evaluations.map((_) => [_[0], _[1]])),
          });
        }),
      );
    }
    if (predicate.kind == "list") {
      return calculateVisibility(predicate.value, bindings).Then((result) => {
        if (PredicateValue.Operations.IsTuple(raw)) {
          return ValueOrErrors.Operations.All(
            List<ValueOrErrors<FormFieldPredicateEvaluation, string>>(
              raw.values.map((value, index) => {
                const elementLocal = raw.values.get(index);
                if (elementLocal == undefined) {
                  return ValueOrErrors.Default.throwOne(
                    `Error: cannot find element of index ${index} in local ${JSON.stringify(
                      raw,
                    )}`,
                  );
                }
                const elementBindings = bindings.set("local", elementLocal);
                return traverse(
                  elementBindings,
                  predicate.elementExpression,
                  value,
                );
              }),
            ),
          ).Then((elementResults) => {
            return ValueOrErrors.Default.return({
              kind: "list",
              value: result,
              elementValues: elementResults.toArray(),
            });
          });
        }
        return ValueOrErrors.Default.throwOne(
          `Error: parsing expected tuple, got ${JSON.stringify(raw)}`,
        );
      });
    }
    if (predicate.kind == "map") {
      return calculateVisibility(predicate.value, bindings).Then((result) => {
        if (typeof raw == "object" && "kind" in raw && raw.kind == "tuple") {
          return ValueOrErrors.Operations.All(
            List<
              ValueOrErrors<
                {
                  key: FormFieldPredicateEvaluation;
                  value: FormFieldPredicateEvaluation;
                },
                string
              >
            >(
              raw.values.map((kv) => {
                if (PredicateValue.Operations.IsTuple(kv)) {
                  const keyLocal = kv.values.get(0)!;
                  const valueLocal = kv.values.get(1)!;
                  if (keyLocal == undefined || valueLocal == undefined) {
                    return ValueOrErrors.Default.throwOne(
                      `Error: cannot find key or value of ${JSON.stringify(
                        kv,
                      )} in local ${JSON.stringify(raw)}`,
                    );
                  }
                  const keyBindings = bindings.set("local", keyLocal);
                  const valueBindings = bindings.set("local", valueLocal);
                  return traverse(
                    keyBindings,
                    predicate.keyExpression,
                    keyLocal,
                  ).Then((keyResult) => {
                    return traverse(
                      valueBindings,
                      predicate.valueExpression,
                      valueLocal,
                    ).Then((valueResult) => {
                      return ValueOrErrors.Default.return({
                        key: keyResult,
                        value: valueResult,
                      });
                    });
                  });
                }
                return ValueOrErrors.Default.throwOne(
                  `Error: parsing expected tuple of key and value, got ${JSON.stringify(
                    kv,
                  )}`,
                );
              }),
            ),
          ).Then((keyValues) => {
            return ValueOrErrors.Default.return({
              kind: "map",
              value: result,
              elementValues: keyValues.toArray(),
            });
          });
        }
        return ValueOrErrors.Default.throwOne(
          `Error: parsing expected tuple of key value pairs, got ${JSON.stringify(
            raw,
          )}`,
        );
      });
    }
    if (predicate.kind == "tuple") {
      return calculateVisibility(predicate.value, bindings).Then((result) => {
        if (PredicateValue.Operations.IsTuple(raw)) {
          return ValueOrErrors.Operations.All(
            List<ValueOrErrors<FormFieldPredicateEvaluation, string>>(
              raw.values.map((value, index) => {
                const elementLocal = raw.values.get(index);
                if (elementLocal == undefined) {
                  return ValueOrErrors.Default.throwOne(
                    `Error: cannot find element of index ${index} in local ${JSON.stringify(
                      raw,
                    )}`,
                  );
                }
                const elementBindings = bindings.set("local", elementLocal);
                return traverse(
                  elementBindings,
                  predicate.elementExpressions[index],
                  value,
                );
              }),
            ),
          ).Then((elementResults) => {
            return ValueOrErrors.Default.return({
              kind: "tuple",
              value: result,
              elementValues: elementResults.toArray(),
            });
          });
        }
        return ValueOrErrors.Default.throwOne(
          `Error: parsing expected tuple, got ${JSON.stringify(raw)}`,
        );
      });
    }
    if (predicate.kind == "sum") {
      return calculateVisibility(predicate.value, bindings).Then((result) => {
        if (PredicateValue.Operations.IsSum(raw)) {
          const local = raw.value.value;
          const innerBindings = bindings.set("local", local);
          return traverse(
            innerBindings,
            raw.value.kind === "l"
              ? predicate.leftExpression
              : predicate.rightExpression,
            local,
          ).Then((innerRes) => {
            return ValueOrErrors.Default.return(
              FormFieldPredicateEvaluation.Default.sum(result, innerRes),
            );
          });
        }

        return ValueOrErrors.Default.throwOne(
          `Error: parsing expected sum, got ${JSON.stringify(raw)}`,
        );
      });
    }
    if (predicate.kind == "unit") {
      return calculateVisibility(predicate.value, bindings).Then((result) => {
        return ValueOrErrors.Default.return({
          kind: "unit",
          value: result,
        });
      });
    }
    return ValueOrErrors.Default.throwOne(
      `Error: parsing unsupported predicate kind ${JSON.stringify(raw)}`,
    );
  };

  const res = traverse(
    bindings,
    {
      kind: "record",
      value: true,
      fields: context.visibilityPredicateExpressions,
    },
    root,
  ).Then((visibilities) => {
    return traverse(
      bindings,
      {
        kind: "record",
        value: true,
        fields: context.disabledPredicatedExpressions,
      },
      root,
    ).Then((disabledFields) => {
      return ValueOrErrors.Default.return({
        visiblityPredicateEvaluations: visibilities,
        disabledPredicateEvaluations: disabledFields,
      });
    });
  });

  if (res.kind == "errors") {
    console.error("error evaluating predicates", res);
  }
  return res;
};
