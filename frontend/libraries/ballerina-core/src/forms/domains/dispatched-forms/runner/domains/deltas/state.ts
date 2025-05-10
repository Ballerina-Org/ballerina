import { PredicateValue, ValueOrErrors } from "../../../../../../../main";
import {
  DispatchParsedType,
  RecordType,
  TupleType,
} from "../../../deserializer/domains/specification/domains/types/state";

export type DispatchDelta =
  | DispatchDeltaPrimitive
  | DispatchDeltaOption
  | DispatchDeltaSum
  | DispatchDeltaList
  | DispatchDeltaSet
  | DispatchDeltaMap
  | DispatchDeltaRecord
  | DispatchDeltaUnion
  | DispatchDeltaTuple
  | DispatchDeltaCustom
  | DispatchDeltaUnit
  | DispatchDeltaTable;
export type DispatchDeltaPrimitive =
  | {
      kind: "NumberReplace";
      replace: number;
      state: any;
      type: DispatchParsedType<any>;
    }
  | {
      kind: "StringReplace";
      replace: string;
      state: any;
      type: DispatchParsedType<any>;
    }
  | {
      kind: "BoolReplace";
      replace: boolean;
      state: any;
      type: DispatchParsedType<any>;
    }
  | {
      kind: "TimeReplace";
      replace: string;
      state: any;
      type: DispatchParsedType<any>;
    }
  | {
      kind: "GuidReplace";
      replace: string;
      state: any;
      type: DispatchParsedType<any>;
    };

export type DispatchDeltaUnit = {
  kind: "UnitReplace";
  replace: PredicateValue;
  state: any;
  type: DispatchParsedType<any>;
};
export type DispatchDeltaOption =
  | {
      kind: "OptionReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
    }
  | { kind: "OptionValue"; value: DispatchDelta };
export type DispatchDeltaSum =
  | {
      kind: "SumReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
    }
  | { kind: "SumLeft"; value: DispatchDelta }
  | { kind: "SumRight"; value: DispatchDelta };
export type DispatchDeltaList =
  | {
      kind: "ArrayReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
    }
  | { kind: "ArrayValue"; value: [number, DispatchDelta] }
  | {
      kind: "ArrayAdd";
      value: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
    }
  | {
      kind: "ArrayAddAt";
      value: [number, PredicateValue];
      elementState: any;
      elementType: DispatchParsedType<any>;
    }
  | { kind: "ArrayRemoveAt"; index: number }
  | { kind: "ArrayMoveFromTo"; from: number; to: number }
  | { kind: "ArrayDuplicateAt"; index: number };
export type DispatchDeltaSet =
  | {
      kind: "SetReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
    }
  | {
      kind: "SetValue";
      value: [PredicateValue, DispatchDelta];
      state: any;
      type: DispatchParsedType<any>;
    }
  | {
      kind: "SetAdd";
      value: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
    }
  | {
      kind: "SetRemove";
      value: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
    };
export type DispatchDeltaMap =
  | {
      kind: "MapReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
    }
  | {
      kind: "MapKey";
      value: [number, DispatchDelta];
    }
  | {
      kind: "MapValue";
      value: [number, DispatchDelta];
    }
  | {
      kind: "MapAdd";
      keyValue: [PredicateValue, PredicateValue];
      keyState: any;
      keyType: DispatchParsedType<any>;
      valueState: any;
      valueType: DispatchParsedType<any>;
    }
  | { kind: "MapRemove"; index: number };
export type DispatchDeltaRecord =
  | {
      kind: "RecordReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
    }
  | {
      kind: "RecordField";
      field: [string, DispatchDelta];
      recordType: DispatchParsedType<any>;
    }
  | {
      kind: "RecordAdd";
      field: [string, PredicateValue];
      state: any;
      type: DispatchParsedType<any>;
    };
export type DispatchDeltaUnion =
  | {
      kind: "UnionReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
    }
  | { kind: "UnionCase"; caseName: [string, DispatchDelta] };
export type DispatchDeltaTuple =
  | {
      kind: "TupleReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
    }
  | {
      kind: "TupleCase";
      item: [number, DispatchDelta];
      tupleType: DispatchParsedType<any>;
    };
export type DispatchDeltaTable = {
  kind: "TableValue";
  id: string;
  nestedDelta: DispatchDelta;
  tableType: DispatchParsedType<any>;
};

export type DispatchDeltaCustom = {
  kind: "CustomDelta";
  value: {
    kind: string;
    [key: string]: any;
  };
};

export type DispatchTransferTuple2<a, b> = { Item1: a; Item2: b };
export type DispatchDeltaTransferPrimitive =
  | { Discriminator: "NumberReplace"; Replace: number }
  | { Discriminator: "StringReplace"; Replace: string }
  | { Discriminator: "BoolReplace"; Replace: boolean }
  | { Discriminator: "TimeReplace"; Replace: number }
  | { Discriminator: "GuidReplace"; Replace: string }
  | { Discriminator: "Int32Replace"; Replace: bigint }
  | { Discriminator: "Float32Replace"; Replace: number };
export type DispatchDeltaTransferUnit = {
  Discriminator: "UnitReplace";
  Replace: any;
};
export type DispatchDeltaTransferOption<DispatchDeltaTransferCustom> =
  | { Discriminator: "OptionReplace"; Replace: any }
  | {
      Discriminator: "OptionValue";
      Value: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
    };
export type DispatchDeltaTransferSum<DispatchDeltaTransferCustom> =
  | { Discriminator: "SumReplace"; Replace: any }
  | {
      Discriminator: "SumLeft";
      Left: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
    }
  | {
      Discriminator: "SumRight";
      Right: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
    };
export type DispatchDeltaTransferList<DispatchDeltaTransferCustom> =
  | { Discriminator: "ArrayAdd"; Add: any }
  | { Discriminator: "ArrayReplace"; Replace: any }
  | {
      Discriminator: "ArrayValue";
      Value: DispatchTransferTuple2<
        number,
        DispatchDeltaTransfer<DispatchDeltaTransferCustom>
      >;
    }
  | { Discriminator: "ArrayAddAt"; AddAt: DispatchTransferTuple2<number, any> }
  | { Discriminator: "ArrayRemoveAt"; RemoveAt: number }
  | {
      Discriminator: "ArrayMoveFromTo";
      MoveFromTo: DispatchTransferTuple2<number, number>;
    }
  | { Discriminator: "ArrayDuplicateAt"; DuplicateAt: number };
export type DispatchDeltaTransferSet<DispatchDeltaTransferCustom> =
  | { Discriminator: "SetReplace"; Replace: any }
  | {
      Discriminator: "SetValue";
      Value: DispatchTransferTuple2<
        any,
        DispatchDeltaTransfer<DispatchDeltaTransferCustom>
      >;
    }
  | { Discriminator: "SetAdd"; Add: any }
  | { Discriminator: "SetRemove"; Remove: any };
export type DispatchDeltaTransferMap<DispatchDeltaTransferCustom> =
  | { Discriminator: "MapReplace"; Replace: any }
  | {
      Discriminator: "MapValue";
      Value: DispatchTransferTuple2<
        number,
        DispatchDeltaTransfer<DispatchDeltaTransferCustom>
      >;
    }
  | {
      Discriminator: "MapKey";
      Key: DispatchTransferTuple2<
        number,
        DispatchDeltaTransfer<DispatchDeltaTransferCustom>
      >;
    }
  | { Discriminator: "MapAdd"; Add: DispatchTransferTuple2<any, any> }
  | { Discriminator: "MapRemove"; Remove: number };
export type DispatchDeltaTransferRecord<DispatchDeltaTransferCustom> =
  | { Discriminator: "RecordReplace"; Replace: any }
  | ({ Discriminator: "RecordField" } & {
      [field: string]: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
    });
export type DispatchDeltaTransferUnion<DispatchDeltaTransferCustom> =
  | { Discriminator: "UnionReplace"; Replace: any }
  | ({ Discriminator: "UnionCase" } & {
      [caseName: string]: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
    });
export type DispatchDeltaTransferTuple<DispatchDeltaTransferCustom> =
  | { Discriminator: "TupleReplace"; Replace: any }
  | ({ Discriminator: string } & {
      [item: string]: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
    });
export type DispatchDeltaTransferTable<DispatchDeltaTransferCustom> = {
  Discriminator: "TableValue";
  Value: {
    Item1: string;
    Item2: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
  };
};

export type DispatchDeltaTransfer<DispatchDeltaTransferCustom> =
  | DispatchDeltaTransferPrimitive
  | DispatchDeltaTransferUnit
  | DispatchDeltaTransferOption<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferSum<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferList<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferSet<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferMap<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferRecord<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferUnion<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferTuple<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferTable<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferCustom;

export type DispatchDeltaTransferComparand = string;

export const DispatchDeltaTransfer = {
  Default: {
    FromDelta:
      <DispatchDeltaTransferCustom>(
        toRawObject: (
          value: PredicateValue,
          type: DispatchParsedType<any>,
          state: any,
        ) => ValueOrErrors<any, string>,
        parseCustomDelta: (
          toRawObject: (
            value: PredicateValue,
            type: DispatchParsedType<any>,
            state: any,
          ) => ValueOrErrors<any, string>,
          fromDelta: (
            delta: DispatchDelta,
          ) => ValueOrErrors<
            [
              DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
              DispatchDeltaTransferComparand,
            ],
            string
          >,
        ) => (
          deltaCustom: DispatchDeltaCustom,
        ) => ValueOrErrors<
          [DispatchDeltaTransferCustom, DispatchDeltaTransferComparand],
          string
        >,
      ) =>
      (
        delta: DispatchDelta,
      ): ValueOrErrors<
        [
          DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
          DispatchDeltaTransferComparand,
        ],
        string
      > => {
        const result: ValueOrErrors<
          [
            DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
            DispatchDeltaTransferComparand,
          ],
          string
        > = (() => {
          if (delta.kind == "NumberReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "NumberReplace",
                    Replace: value,
                  },
                  "[NumberReplace]",
                ]),
            );
          }
          if (delta.kind == "StringReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "StringReplace",
                    Replace: value,
                  },
                  "[StringReplace]",
                ]),
            );
          }
          if (delta.kind == "BoolReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "BoolReplace",
                    Replace: value,
                  },
                  "[BoolReplace]",
                ]),
            );
          }
          if (delta.kind == "TimeReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "TimeReplace",
                    Replace: value,
                  },
                  "[TimeReplace]",
                ]),
            );
          }
          if (delta.kind == "GuidReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "GuidReplace",
                    Replace: value,
                  },
                  "[GuidReplace]",
                ]),
            );
          }
          if (delta.kind == "UnitReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "UnitReplace",
                    Replace: value,
                  },
                  "UnitReplace",
                ]),
            );
          }
          if (delta.kind == "OptionReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "OptionReplace",
                    Replace: value,
                  },
                  "[OptionReplace]",
                ]),
            );
          }
          if (delta.kind == "OptionValue") {
            return DispatchDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.value).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                ],
                string
              >([
                {
                  Discriminator: "OptionValue",
                  Value: value[0],
                },
                `[OptionValue]${value[1]}`,
              ]),
            );
          }
          if (delta.kind == "SumReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "SumReplace",
                    Replace: value,
                  },
                  "[SumReplace]",
                ]),
            );
          }
          if (delta.kind == "SumLeft") {
            return DispatchDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.value).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                ],
                string
              >([
                {
                  Discriminator: "SumLeft",
                  Left: value[0],
                },
                `[SumLeft]${value[1]}`,
              ]),
            );
          }
          if (delta.kind == "SumRight") {
            return DispatchDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.value).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                ],
                string
              >([
                {
                  Discriminator: "SumRight",
                  Right: value[0],
                },
                `[SumRight]${value[1]}`,
              ]),
            );
          }
          if (delta.kind == "ArrayReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "ArrayReplace",
                    Replace: value,
                  },
                  "[ArrayReplace]",
                ]),
            );
          }
          if (delta.kind == "ArrayValue") {
            return DispatchDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.value[1]).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                ],
                string
              >([
                {
                  Discriminator: "ArrayValue",
                  Value: {
                    Item1: delta.value[0],
                    Item2: value[0],
                  },
                },
                `[ArrayValue][${delta.value[0]}]${value[1]}`,
              ]),
            );
          }
          if (delta.kind == "ArrayAdd") {
            return toRawObject(delta.value, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "ArrayAdd",
                    Add: value,
                  },
                  "[ArrayAdd]",
                ]),
            );
          }
          if (delta.kind == "ArrayAddAt") {
            return toRawObject(
              delta.value[1],
              delta.elementType,
              delta.elementState,
            ).Then((element) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                ],
                string
              >([
                {
                  Discriminator: "ArrayAddAt",
                  AddAt: { Item1: delta.value[0], Item2: element },
                },
                `[ArrayAddAt][${delta.value[0]}]`,
              ]),
            );
          }
          if (delta.kind == "ArrayRemoveAt") {
            return ValueOrErrors.Default.return<
              [
                DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                DispatchDeltaTransferComparand,
              ],
              string
            >([
              {
                Discriminator: "ArrayRemoveAt",
                RemoveAt: delta.index,
              },
              `[ArrayRemoveAt]`,
            ]);
          }
          if (delta.kind == "ArrayMoveFromTo") {
            return ValueOrErrors.Default.return<
              [
                DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                DispatchDeltaTransferComparand,
              ],
              string
            >([
              {
                Discriminator: "ArrayMoveFromTo",
                MoveFromTo: { Item1: delta.from, Item2: delta.to },
              },
              `[ArrayMoveFromTo]`,
            ]);
          }
          if (delta.kind == "ArrayDuplicateAt") {
            return ValueOrErrors.Default.return<
              [
                DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                DispatchDeltaTransferComparand,
              ],
              string
            >([
              {
                Discriminator: "ArrayDuplicateAt",
                DuplicateAt: delta.index,
              },
              `[ArrayDuplicateAt]`,
            ]);
          }
          if (delta.kind == "SetReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "SetReplace",
                    Replace: value,
                  },
                  "[SetReplace]",
                ]),
            );
          }
          if (delta.kind == "SetValue") {
            return DispatchDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.value[1]).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                ],
                string
              >([
                {
                  Discriminator: "SetValue",
                  Value: { Item1: delta.value[0], Item2: value[0] },
                },
                `[SetValue][${delta.value[0]}]${value[1]}`,
              ]),
            );
          }
          if (delta.kind == "SetAdd") {
            return toRawObject(delta.value, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "SetAdd",
                    Add: value,
                  },
                  `[SetAdd]`,
                ]),
            );
          }
          if (delta.kind == "SetRemove") {
            return toRawObject(delta.value, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "SetRemove",
                    Remove: value,
                  },
                  `[SetRemove]`,
                ]),
            );
          }
          if (delta.kind == "MapReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "MapReplace",
                    Replace: value,
                  },
                  "[MapReplace]",
                ]),
            );
          }
          if (delta.kind == "MapKey") {
            return DispatchDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.value[1]).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                ],
                string
              >([
                {
                  Discriminator: "MapKey",
                  Key: { Item1: delta.value[0], Item2: value[0] },
                },
                `[MapKey][${delta.value[0]}]${value[1]}`,
              ]),
            );
          }
          if (delta.kind == "MapValue") {
            return DispatchDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.value[1]).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                ],
                string
              >([
                {
                  Discriminator: "MapValue",
                  Value: { Item1: delta.value[0], Item2: value[0] },
                },
                `[MapValue][${delta.value[0]}]${value[1]}`,
              ]),
            );
          }
          if (delta.kind == "MapAdd") {
            return toRawObject(
              delta.keyValue[0],
              delta.keyType,
              delta.keyState,
            ).Then((key) =>
              toRawObject(
                delta.keyValue[1],
                delta.valueType,
                delta.valueState,
              ).Then((value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "MapAdd",
                    Add: { Item1: key, Item2: value },
                  },
                  `[MapAdd]`,
                ]),
              ),
            );
          }
          if (delta.kind == "MapRemove") {
            return ValueOrErrors.Default.return<
              [
                DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                DispatchDeltaTransferComparand,
              ],
              string
            >([
              {
                Discriminator: "MapRemove",
                Remove: delta.index,
              },
              `[MapRemove]`,
            ]);
          }
          if (delta.kind == "RecordReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "RecordReplace",
                    Replace: value,
                  },
                  "[RecordReplace]",
                ]),
            );
          }
          if (delta.kind == "RecordField") {
            return DispatchDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.field[1]).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                ],
                string
              >([
                {
                  Discriminator: `${
                    (delta.recordType as RecordType<any>).name
                  }${delta.field[0]}`,
                  [delta.field[0]]: value[0],
                } as { Discriminator: string } & {
                  [
                    field: string
                  ]: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
                },
                `[RecordField][${delta.field[0]}]${value[1]}`,
              ]),
            );
          }
          if (delta.kind == "UnionReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "UnionReplace",
                    Replace: value,
                  },
                  "[UnionReplace]",
                ]),
            );
          }
          if (delta.kind == "UnionCase") {
            return DispatchDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.caseName[1]).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                ],
                string
              >([
                {
                  Discriminator: "UnionCase",
                  [delta.caseName[0]]: value,
                } as { Discriminator: "UnionCase" } & {
                  [
                    caseName: string
                  ]: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
                },
                `[UnionCase][${delta.caseName[0]}]`,
              ]),
            );
          }
          if (delta.kind == "TupleReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                  ],
                  string
                >([
                  {
                    Discriminator: "TupleReplace",
                    Replace: value,
                  },
                  "[TupleReplace]",
                ]),
            );
          }
          if (delta.kind == "TupleCase") {
            return DispatchDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.item[1]).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                ],
                string
              >([
                {
                  Discriminator: `Tuple${
                    (delta.tupleType as TupleType<any>).args.length
                  }Item${delta.item[0] + 1}`,
                  [`Item${delta.item[0] + 1}`]: value[0],
                } as { Discriminator: string } & {
                  [
                    item: string
                  ]: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
                },
                `[TupleCase][${delta.item[0] + 1}]${value[1]}`,
              ]),
            );
          }
          if (delta.kind == "TableValue") {
            return DispatchDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.nestedDelta).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                ],
                string
              >([
                {
                  Discriminator: "TableValue",
                  Value: { Item1: delta.id, Item2: value[0] },
                },
                `[TableValue][${delta.id}]${value[1]}`,
              ]),
            );
          }
          if (delta.kind == "CustomDelta") {
            return parseCustomDelta(
              toRawObject,
              DispatchDeltaTransfer.Default.FromDelta(
                toRawObject,
                parseCustomDelta,
              ),
            )(delta);
          }
          return ValueOrErrors.Default.throwOne<
            [
              DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
              DispatchDeltaTransferComparand,
            ],
            string
          >(`Error: cannot process delta ${delta}, not currently supported.`);
        })();
        return result.MapErrors((errors) =>
          errors.map(
            (error) =>
              `${error}\n...When dispatching delta: ${JSON.stringify(
                delta,
                null,
                2,
              )}`,
          ),
        );
      },
  },
};
