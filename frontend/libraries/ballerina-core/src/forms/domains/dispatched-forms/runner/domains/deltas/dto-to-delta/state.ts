import { List } from "immutable";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import { ValueOrErrors } from "../../../../../../../collections/domains/valueOrErrors/state";
import { Unit } from "../../../../../../../fun/domains/unit/state";
import { PredicateValue } from "../../../../../parser/domains/predicates/state";
import {
  DeltaTransfer,
  DispatchDeltaTransferOperations,
} from "../dispatch-delta-dto/state";
import { DispatchDelta, DispatchDeltaCustom } from "../dispatch-delta/state";

export const DispatchDeltaDTOToDelta =
  <DispatchDeltaTransferCustom, Flags = Unit>(
    fromApiRaw: (
      raw: any,
      type: DispatchParsedType<any>,
    ) => ValueOrErrors<PredicateValue, string>,
    parseCustomDeltaDTO: (
      fromApiRaw: (raw: any) => ValueOrErrors<PredicateValue, string>,
    ) => (
      customDeltaDTO: DispatchDeltaTransferCustom,
    ) => ValueOrErrors<DispatchDeltaCustom<Flags>, string>,
  ) =>
  (entityType: DispatchParsedType<any>) =>
  (
    deltaDTO: DeltaTransfer<DispatchDeltaTransferCustom>,
  ): ValueOrErrors<DispatchDelta<Flags>, string> => {
    const withCommon = <T extends object>(
      delta: T,
    ): T & {
      state: undefined;
      flags: Flags | undefined;
      sourceAncestorLookupTypeNames: string[];
    } => ({
      ...delta,
      state: undefined,
      flags: undefined,
      sourceAncestorLookupTypeNames: [],
    });

    const parseRaw = (
      raw: any,
      type: DispatchParsedType<any>,
    ): ValueOrErrors<PredicateValue, string> => fromApiRaw(raw, type);

    const sumArgType = (
      type: DispatchParsedType<any>,
      index: number,
      operation: string,
    ): ValueOrErrors<DispatchParsedType<any>, string> => {
      if (type.kind != "sum" || type.args.length <= index) {
        return ValueOrErrors.Default.throwOne<DispatchParsedType<any>, string>(
          `${operation} expects a sum type with args[${index}] but got ${JSON.stringify(type)}`,
        );
      }
      return ValueOrErrors.Default.return(type.args[index]);
    };

    const listElementType = (
      type: DispatchParsedType<any>,
      operation: string,
    ): ValueOrErrors<DispatchParsedType<any>, string> => {
      if (type.kind != "list" || type.args.length == 0) {
        return ValueOrErrors.Default.throwOne<DispatchParsedType<any>, string>(
          `${operation} expects a list type with one element arg but got ${JSON.stringify(type)}`,
        );
      }
      return ValueOrErrors.Default.return(type.args[0]);
    };

    const argTypeAt = (
      type: DispatchParsedType<any>,
      index: number,
      operation: string,
    ): ValueOrErrors<DispatchParsedType<any>, string> => {
      if (
        !("args" in type) ||
        !Array.isArray(type.args) ||
        type.args.length <= index
      ) {
        return ValueOrErrors.Default.throwOne<DispatchParsedType<any>, string>(
          `${operation} expects a type with args[${index}] but got ${JSON.stringify(type)}`,
        );
      }
      return ValueOrErrors.Default.return(type.args[index]);
    };

    const mapArgType = (
      type: DispatchParsedType<any>,
      index: number,
      operation: string,
    ): ValueOrErrors<DispatchParsedType<any>, string> => {
      if (type.kind != "map" || type.args.length <= index) {
        return ValueOrErrors.Default.throwOne<DispatchParsedType<any>, string>(
          `${operation} expects a map type with args[${index}] but got ${JSON.stringify(type)}`,
        );
      }
      return ValueOrErrors.Default.return(type.args[index]);
    };

    const tableRowType = (
      type: DispatchParsedType<any>,
      operation: string,
    ): ValueOrErrors<DispatchParsedType<any>, string> => {
      if (type.kind != "table") {
        return ValueOrErrors.Default.throwOne<DispatchParsedType<any>, string>(
          `${operation} expects a table type but got ${JSON.stringify(type)}`,
        );
      }
      return ValueOrErrors.Default.return(type.arg);
    };

    const oneNestedType = (
      type: DispatchParsedType<any>,
      operation: string,
    ): ValueOrErrors<DispatchParsedType<any>, string> => {
      if (type.kind != "one") {
        return ValueOrErrors.Default.throwOne<DispatchParsedType<any>, string>(
          `${operation} expects a one type but got ${JSON.stringify(type)}`,
        );
      }
      return ValueOrErrors.Default.return(type.arg);
    };

    const rec = (
      type: DispatchParsedType<any>,
      deltaDTO: DeltaTransfer<DispatchDeltaTransferCustom>,
    ): ValueOrErrors<DispatchDelta<Flags>, string> => {
      if (DispatchDeltaTransferOperations.isNumberReplace(deltaDTO)) {
        if (type.kind != "primitive" || type.name != "number") {
          return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
            `Number type expected but got ${JSON.stringify(type)}`,
          );
        }
        return parseRaw(deltaDTO.Replace, type).Then((value) => {
          if (!PredicateValue.Operations.IsNumber(value)) {
            return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
              `Number expected but got ${JSON.stringify(value)}`,
            );
          }
          return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "NumberReplace",
              replace: value,
              type,
            }),
          );
        });
      }
      if (DispatchDeltaTransferOperations.isStringReplace(deltaDTO)) {
        if (type.kind != "primitive" || type.name != "string") {
          return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
            `String type expected but got ${JSON.stringify(type)}`,
          );
        }
        return parseRaw(deltaDTO.Replace, type).Then((value) => {
          if (!PredicateValue.Operations.IsString(value)) {
            return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
              `String expected but got ${JSON.stringify(value)}`,
            );
          }
          return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "StringReplace",
              replace: value,
              type,
            }),
          );
        });
      }
      if (DispatchDeltaTransferOperations.isBoolReplace(deltaDTO)) {
        if (type.kind != "primitive" || type.name != "boolean") {
          return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
            `Boolean type expected but got ${JSON.stringify(type)}`,
          );
        }
        return parseRaw(deltaDTO.Replace, type).Then((value) => {
          if (!PredicateValue.Operations.IsBoolean(value)) {
            return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
              `Boolean expected but got ${JSON.stringify(value)}`,
            );
          }
          return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "BoolReplace",
              replace: value,
              type,
            }),
          );
        });
      }
      if (DispatchDeltaTransferOperations.isTimeReplace(deltaDTO)) {
        if (type.kind != "primitive" || type.name != "Date") {
          return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
            `Date type expected but got ${JSON.stringify(type)}`,
          );
        }
        return parseRaw(deltaDTO.Replace, type).Then((value) => {
          if (!PredicateValue.Operations.IsDate(value)) {
            return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
              `Date expected but got ${JSON.stringify(value)}`,
            );
          }
          return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "TimeReplace",
              replace: value.toISOString(),
              type,
            }),
          );
        });
      }
      if (DispatchDeltaTransferOperations.isGuidReplace(deltaDTO)) {
        if (
          type.kind != "primitive" ||
          (type.name != "guid" &&
            type.name != "entityIdString" &&
            type.name != "entityIdUUID" &&
            type.name != "calculatedDisplayValue")
        ) {
          return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
            `Guid-like type expected but got ${JSON.stringify(type)}`,
          );
        }
        return parseRaw(deltaDTO.Replace, type).Then((value) => {
          if (!PredicateValue.Operations.IsString(value)) {
            return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
              `String expected but got ${JSON.stringify(value)}`,
            );
          }
          return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "GuidReplace",
              replace: value,
              type,
            }),
          );
        });
      }
      if (DispatchDeltaTransferOperations.isUnit(deltaDTO)) {
        return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
          withCommon({
            kind: "UnitReplace",
            replace: PredicateValue.Default.unit(),
            type,
          }),
        );
      }
      if (DispatchDeltaTransferOperations.isOptionReplace(deltaDTO)) {
        return parseRaw(deltaDTO.Replace, type).Then((value) =>
          ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "OptionReplace",
              replace: value,
              type,
            }),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isOptionValue(deltaDTO)) {
        return rec(
          type,
          deltaDTO.Value as DeltaTransfer<DispatchDeltaTransferCustom>,
        ).Then((value) =>
          ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "OptionValue",
              value,
            }),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isSumReplace(deltaDTO)) {
        return parseRaw(deltaDTO.Replace, type).Then((value) =>
          ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "SumReplace",
              replace: value,
              type,
            }),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isSumLeft(deltaDTO)) {
        return sumArgType(type, 0, "SumLeft").Then((leftType) =>
          rec(
            leftType,
            deltaDTO.Left as DeltaTransfer<DispatchDeltaTransferCustom>,
          ).Then((value) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "SumLeft",
                value,
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isSumRight(deltaDTO)) {
        return sumArgType(type, 1, "SumRight").Then((rightType) =>
          rec(
            rightType,
            deltaDTO.Right as DeltaTransfer<DispatchDeltaTransferCustom>,
          ).Then((value) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "SumRight",
                value,
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isArrayAdd(deltaDTO)) {
        return listElementType(type, "ArrayAdd").Then((elementType) =>
          parseRaw(deltaDTO.Add, elementType).Then((value) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "ArrayAdd",
                value,
                type: elementType,
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isArrayReplace(deltaDTO)) {
        return parseRaw(deltaDTO.Replace, type).Then((value) =>
          ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "ArrayReplace",
              replace: value,
              type,
            }),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isArrayValue(deltaDTO)) {
        return listElementType(type, "ArrayValue").Then((elementType) =>
          rec(elementType, deltaDTO.Value.Item2).Then((value) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "ArrayValue",
                value: [deltaDTO.Value.Item1, value],
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isArrayValueAll(deltaDTO)) {
        return listElementType(type, "ArrayValueAll").Then((elementType) =>
          rec(elementType, deltaDTO.ValueAll).Then((nestedDelta) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "ArrayValueAll",
                nestedDelta,
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isArrayAddAt(deltaDTO)) {
        return listElementType(type, "ArrayAddAt").Then((elementType) =>
          parseRaw(deltaDTO.AddAt.Item2, elementType).Then((value) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "ArrayAddAt",
                value: [deltaDTO.AddAt.Item1, value],
                elementState: undefined,
                elementType,
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isArrayRemoveAt(deltaDTO)) {
        return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
          withCommon({
            kind: "ArrayRemoveAt",
            index: deltaDTO.RemoveAt,
          }),
        );
      }
      if (DispatchDeltaTransferOperations.isArrayRemoveAll(deltaDTO)) {
        return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
          withCommon({
            kind: "ArrayRemoveAll",
          }),
        );
      }
      if (DispatchDeltaTransferOperations.isArrayMoveFromTo(deltaDTO)) {
        return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
          withCommon({
            kind: "ArrayMoveFromTo",
            from: deltaDTO.MoveFromTo.Item1,
            to: deltaDTO.MoveFromTo.Item2,
          }),
        );
      }
      if (DispatchDeltaTransferOperations.isArrayDuplicateAt(deltaDTO)) {
        return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
          withCommon({
            kind: "ArrayDuplicateAt",
            index: deltaDTO.DuplicateAt,
          }),
        );
      }
      if (DispatchDeltaTransferOperations.isSetReplace(deltaDTO)) {
        return parseRaw(deltaDTO.Replace, type).Then((value) =>
          ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "SetReplace",
              replace: value,
              type,
            }),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isSetValue(deltaDTO)) {
        return argTypeAt(type, 0, "SetValue").Then((elementType) =>
          parseRaw(deltaDTO.Value.Item1, elementType).Then((parsedKey) =>
            rec(elementType, deltaDTO.Value.Item2).Then((value) =>
              ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
                withCommon({
                  kind: "SetValue",
                  value: [parsedKey, value],
                  type: elementType,
                }),
              ),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isSetAdd(deltaDTO)) {
        return argTypeAt(type, 0, "SetAdd").Then((elementType) =>
          parseRaw(deltaDTO.Add, elementType).Then((value) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "SetAdd",
                value,
                type: elementType,
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isSetRemove(deltaDTO)) {
        return argTypeAt(type, 0, "SetRemove").Then((elementType) =>
          parseRaw(deltaDTO.Remove, elementType).Then((value) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "SetRemove",
                value,
                type: elementType,
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isMapReplace(deltaDTO)) {
        return parseRaw(deltaDTO.Replace, type).Then((value) =>
          ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "MapReplace",
              replace: value,
              type,
            }),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isMapValue(deltaDTO)) {
        return mapArgType(type, 1, "MapValue").Then((valueType) =>
          rec(
            valueType,
            deltaDTO.Value.Item2 as DeltaTransfer<DispatchDeltaTransferCustom>,
          ).Then((value) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "MapValue",
                value: [deltaDTO.Value.Item1, value],
                ballerinaValue: {
                  key: PredicateValue.Default.unit(),
                  value: PredicateValue.Default.unit(),
                },
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isMapKey(deltaDTO)) {
        return mapArgType(type, 0, "MapKey").Then((keyType) =>
          rec(
            keyType,
            deltaDTO.Key.Item2 as DeltaTransfer<DispatchDeltaTransferCustom>,
          ).Then((value) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "MapKey",
                value: [deltaDTO.Key.Item1, value],
                ballerinaValue: {
                  oldKey: PredicateValue.Default.unit(),
                  newKey: PredicateValue.Default.unit(),
                },
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isMapAdd(deltaDTO)) {
        return mapArgType(type, 0, "MapAdd").Then((keyType) =>
          mapArgType(type, 1, "MapAdd").Then((valueType) =>
            parseRaw(deltaDTO.Add.Item1, keyType).Then((key) =>
              parseRaw(deltaDTO.Add.Item2, valueType).Then((value) =>
                ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
                  withCommon({
                    kind: "MapAdd",
                    keyValue: [key, value],
                    keyState: undefined,
                    keyType,
                    valueState: undefined,
                    valueType,
                  }),
                ),
              ),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isMapRemove(deltaDTO)) {
        return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
          withCommon({
            kind: "MapRemove",
            index: deltaDTO.Remove,
            ballerinaValue: {
              key: PredicateValue.Default.unit(),
            },
          }),
        );
      }
      if (DispatchDeltaTransferOperations.isRecordReplace(deltaDTO, type)) {
        return parseRaw(deltaDTO.Replace, type).Then((value) =>
          ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "RecordReplace",
              replace: value,
              type,
            }),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isRecordField(deltaDTO, type)) {
        if (type.kind != "record") {
          return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
            `Record field expected but got ${JSON.stringify(type)}`,
          );
        }
        const fieldName = deltaDTO.Discriminator;
        const fieldType = type.fields.get(fieldName);
        if (fieldType == undefined) {
          return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
            `Field ${fieldName} not found in record type ${JSON.stringify(type)}`,
          );
        }
        return rec(fieldType, deltaDTO[fieldName]).Then((value) =>
          ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "RecordField",
              field: [fieldName, value],
              recordType: type,
            }),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isUnionReplace(deltaDTO)) {
        return parseRaw(deltaDTO.Replace, type).Then((value) =>
          ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "UnionReplace",
              replace: value,
              type,
            }),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isUnionCase(deltaDTO, type)) {
        if (type.kind != "union") {
          return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
            `Union case expected but got ${JSON.stringify(type)}`,
          );
        }
        const caseName = deltaDTO.Discriminator;
        const caseType = type.args.get(caseName);
        if (caseType == undefined) {
          return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
            `Case ${caseName} not found in union type ${JSON.stringify(type)}`,
          );
        }
        return rec(caseType, deltaDTO[caseName]).Then((value) =>
          ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "UnionCase",
              caseName: [caseName, value],
            }),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isTupleReplace(deltaDTO)) {
        return parseRaw(deltaDTO.Replace, type).Then((value) =>
          ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "TupleReplace",
              replace: value,
              type,
            }),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isTupleValue(deltaDTO, type)) {
        const itemKey = Object.keys(deltaDTO).find((k) => /^Item\d+$/.test(k));
        if (type.kind != "tuple") {
          return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
            `Tuple expected but got ${JSON.stringify(type)}`,
          );
        }
        if (!itemKey) {
          return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
            `Tuple item key not found in tuple delta ${JSON.stringify(deltaDTO)}`,
          );
        }
        const parsedIndex = Number(itemKey.replace("Item", "")) - 1;
        if (
          Number.isNaN(parsedIndex) ||
          parsedIndex < 0 ||
          parsedIndex >= type.args.length
        ) {
          return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
            `Invalid tuple item index ${parsedIndex} for ${JSON.stringify(type)}`,
          );
        }
        return rec(
          type.args[parsedIndex],
          (deltaDTO as Record<string, unknown>)[
            itemKey
          ] as DeltaTransfer<DispatchDeltaTransferCustom>,
        ).Then((value) =>
          ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
            withCommon({
              kind: "TupleCase",
              item: [parsedIndex, value],
              tupleType: type,
            }),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isTableValue(deltaDTO)) {
        return tableRowType(type, "TableValue").Then((rowType) =>
          rec(
            rowType,
            deltaDTO.Value.Item2 as DeltaTransfer<DispatchDeltaTransferCustom>,
          ).Then((nestedDelta) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "TableValue",
                id: deltaDTO.Value.Item1,
                nestedDelta,
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isTableValueAll(deltaDTO)) {
        return tableRowType(type, "TableValueAll").Then((rowType) =>
          rec(
            rowType,
            deltaDTO.ValueAll as DeltaTransfer<DispatchDeltaTransferCustom>,
          ).Then((nestedDelta) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "TableValueAll",
                nestedDelta,
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isTableAdd(deltaDTO)) {
        return tableRowType(type, "TableAdd").Then((rowType) =>
          parseRaw(deltaDTO.Add, rowType).Then((value) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "TableAdd",
                value,
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isTableAddBatch(deltaDTO)) {
        return tableRowType(type, "TableAddBatch").Then((rowType) =>
          ValueOrErrors.Operations.All(
            List(deltaDTO.AddBatch.map((rawRow) => parseRaw(rawRow, rowType))),
          ).Then((values) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "TableAddBatch",
                values,
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isTableAddEmpty(deltaDTO)) {
        return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
          "TableAddEmpty is not supported as an incoming delta because a row Id is required",
        );
      }
      if (DispatchDeltaTransferOperations.isTableRemoveAt(deltaDTO)) {
        return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
          withCommon({
            kind: "TableRemove",
            id: deltaDTO.RemoveAt,
          }),
        );
      }
      if (DispatchDeltaTransferOperations.isTableRemoveAll(deltaDTO)) {
        return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
          withCommon({
            kind: "TableRemoveAll",
          }),
        );
      }
      if (DispatchDeltaTransferOperations.isTableDuplicateAt(deltaDTO)) {
        return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
          withCommon({
            kind: "TableDuplicate",
            id: deltaDTO.DuplicateAt,
          }),
        );
      }
      if (DispatchDeltaTransferOperations.isTableActionOnAll(deltaDTO)) {
        return ValueOrErrors.Default.throwOne<DispatchDelta<Flags>, string>(
          "TableActionOnAll is not supported as an incoming delta",
        );
      }
      if (DispatchDeltaTransferOperations.isTableMoveFromTo(deltaDTO)) {
        return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
          withCommon({
            kind: "TableMoveTo",
            id: deltaDTO.MoveFromTo[0],
            to: deltaDTO.MoveFromTo[1],
          }),
        );
      }
      if (DispatchDeltaTransferOperations.isOneValue(deltaDTO)) {
        return oneNestedType(type, "OneValue").Then((nestedType) =>
          rec(
            nestedType,
            deltaDTO.Value as DeltaTransfer<DispatchDeltaTransferCustom>,
          ).Then((nestedDelta) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "OneValue",
                nestedDelta,
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isOneReplace(deltaDTO)) {
        return oneNestedType(type, "OneReplace").Then((nestedType) =>
          parseRaw(deltaDTO.Replace, nestedType).Then((replace) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "OneReplace",
                replace,
                type,
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isOneCreateValue(deltaDTO)) {
        return oneNestedType(type, "OneCreateValue").Then((nestedType) =>
          parseRaw(deltaDTO.CreateValue, nestedType).Then((value) =>
            ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
              withCommon({
                kind: "OneCreateValue",
                value,
                type,
              }),
            ),
          ),
        );
      }
      if (DispatchDeltaTransferOperations.isOneDeleteValue(deltaDTO)) {
        return ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
          withCommon({
            kind: "OneDeleteValue",
          }),
        );
      }
      return parseCustomDeltaDTO((raw) => fromApiRaw(raw, type))(
        deltaDTO as DispatchDeltaTransferCustom,
      ).Then((parsedCustomDelta) =>
        ValueOrErrors.Default.return<DispatchDelta<Flags>, string>(
          withCommon({
            kind: "CustomDelta",
            value: parsedCustomDelta,
          }),
        ),
      );
    };
    return rec(entityType, deltaDTO);
  };
