import {
  PredicateValue,
  unit,
  Unit,
  ValueOrErrors,
} from "../../../../../../../main";
import {
  DispatchParsedType,
  TupleType,
} from "../../../deserializer/domains/specification/domains/types/state";

export type DispatchDelta<T = Unit> =
  | DispatchDeltaPrimitive<T>
  | DispatchDeltaOption<T>
  | DispatchDeltaSum<T>
  | DispatchDeltaList<T>
  | DispatchDeltaSet<T>
  | DispatchDeltaMap<T>
  | DispatchDeltaRecord<T>
  | DispatchDeltaUnion<T>
  | DispatchDeltaTuple<T>
  | DispatchDeltaCustom<T>
  | DispatchDeltaUnit<T>
  | DispatchDeltaTable<T>
  | DispatchDeltaOne<T>;
export type DispatchDeltaPrimitive<T = Unit> =
  | {
      kind: "NumberReplace";
      replace: number;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "StringReplace";
      replace: string;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "BoolReplace";
      replace: boolean;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "TimeReplace";
      replace: string;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "GuidReplace";
      replace: string;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    };

export type DispatchDeltaUnit<T = Unit> = {
  kind: "UnitReplace";
  replace: PredicateValue;
  state: any;
  type: DispatchParsedType<any>;
  flags: T | undefined;
};
export type DispatchDeltaOption<T = Unit> =
  | {
      kind: "OptionReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | { kind: "OptionValue"; value: DispatchDelta<T>; flags: T | undefined };
export type DispatchDeltaSum<T = Unit> =
  | {
      kind: "SumReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | { kind: "SumLeft"; value: DispatchDelta<T>; flags: T | undefined }
  | { kind: "SumRight"; value: DispatchDelta<T>; flags: T | undefined };
export type DispatchDeltaList<T = Unit> =
  | {
      kind: "ArrayReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "ArrayValue";
      value: [number, DispatchDelta<T>];
      flags: T | undefined;
    }
  | {
      kind: "ArrayAdd";
      value: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "ArrayAddAt";
      value: [number, PredicateValue];
      elementState: any;
      elementType: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | { kind: "ArrayRemoveAt"; index: number; flags: T | undefined }
  | { kind: "ArrayMoveFromTo"; from: number; to: number; flags: T | undefined }
  | { kind: "ArrayDuplicateAt"; index: number; flags: T | undefined };
export type DispatchDeltaSet<T = Unit> =
  | {
      kind: "SetReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "SetValue";
      value: [PredicateValue, DispatchDelta<T>];
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "SetAdd";
      value: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "SetRemove";
      value: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    };
export type DispatchDeltaMap<T = Unit> =
  | {
      kind: "MapReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "MapKey";
      value: [number, DispatchDelta<T>];
      flags: T | undefined;
    }
  | {
      kind: "MapValue";
      value: [number, DispatchDelta<T>];
      flags: T | undefined;
    }
  | {
      kind: "MapAdd";
      keyValue: [PredicateValue, PredicateValue];
      keyState: any;
      keyType: DispatchParsedType<any>;
      valueState: any;
      valueType: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | { kind: "MapRemove"; index: number; flags: T | undefined };
export type DispatchDeltaRecord<T = Unit> =
  | {
      kind: "RecordReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "RecordField";
      field: [string, DispatchDelta<T>];
      recordType: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "RecordAdd";
      field: [string, PredicateValue];
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    };
export type DispatchDeltaUnion<T = Unit> =
  | {
      kind: "UnionReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "UnionCase";
      caseName: [string, DispatchDelta<T>];
      flags: T | undefined;
    };
export type DispatchDeltaTuple<T = Unit> =
  | {
      kind: "TupleReplace";
      replace: PredicateValue;
      state: any;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "TupleCase";
      item: [number, DispatchDelta<T>];
      tupleType: DispatchParsedType<any>;
      flags: T | undefined;
    };
export type DispatchDeltaTable<T = Unit> =
  | {
      kind: "TableValue";
      id: string;
      nestedDelta: DispatchDelta<T>;
      flags: T | undefined;
    }
  | {
      kind: "TableAddEmpty";
      flags: T | undefined;
    }
  | {
      kind: "TableRemove";
      id: string;
      flags: T | undefined;
    }
  | {
      kind: "TableMoveTo";
      id: string;
      to: string;
      flags: T | undefined;
    }
  | {
      kind: "TableDuplicate";
      id: string;
      flags: T | undefined;
    };
export type DispatchDeltaOne<T = Unit> =
  | {
      kind: "OneReplace";
      replace: PredicateValue;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "OneValue";
      nestedDelta: DispatchDelta<T>;
      flags: T | undefined;
    }
  | {
      kind: "OneCreateValue";
      value: PredicateValue;
      type: DispatchParsedType<any>;
      flags: T | undefined;
    }
  | {
      kind: "OneDeleteValue";
      flags: T | undefined;
    };

export type DispatchDeltaCustom<T = Unit> = {
  kind: "CustomDelta";
  value: {
    kind: string;
    [key: string]: any;
  };
  flags: T | undefined;
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
  | { Discriminator: string; Replace: any }
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
export type DispatchDeltaTransferTable<DispatchDeltaTransferCustom> =
  | {
      Discriminator: "TableValue";
      Value: {
        Item1: string;
        Item2: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
      };
    }
  | { Discriminator: "TableAddEmpty" }
  | { Discriminator: "TableRemoveAt"; RemoveAt: string }
  | { Discriminator: "TableDuplicateAt"; DuplicateAt: string }
  | {
      Discriminator: "TableMoveFromTo";
      MoveFromTo: [string, string];
    };
export type DispatchDeltaTransferOne<DispatchDeltaTransferCustom> =
  | {
      Discriminator: "OneValue";
      Value: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
    }
  | { Discriminator: "OneReplace"; Replace: any }
  | { Discriminator: "OneCreateValue"; CreateValue: any }
  | {
      Discriminator: "OneDeleteValue";
      DeleteValue: Unit;
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
  | DispatchDeltaTransferOne<DispatchDeltaTransferCustom>
  | DispatchDeltaTransferCustom;

export type DispatchDeltaTransferComparand = string;

export type AggregatedFlags<Flags> = Array<
  [Flags, DispatchDeltaTransferComparand]
>;

export const DispatchDeltaTransfer = {
  Default: {
    FromDelta:
      <DispatchDeltaTransferCustom, Flags = Unit>(
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
            delta: DispatchDelta<Flags>,
          ) => ValueOrErrors<
            [
              DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
              DispatchDeltaTransferComparand,
              AggregatedFlags<Flags>,
            ],
            string
          >,
        ) => (
          deltaCustom: DispatchDeltaCustom<Flags>,
        ) => ValueOrErrors<
          [
            DispatchDeltaTransferCustom,
            DispatchDeltaTransferComparand,
            AggregatedFlags<Flags>,
          ],
          string
        >,
      ) =>
      (
        delta: DispatchDelta<Flags>,
      ): ValueOrErrors<
        [
          DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
          DispatchDeltaTransferComparand,
          AggregatedFlags<Flags>,
        ],
        string
      > => {
        const result: ValueOrErrors<
          [
            DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
            DispatchDeltaTransferComparand,
            AggregatedFlags<Flags>,
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "NumberReplace",
                    Replace: value,
                  },
                  "[NumberReplace]",
                  delta.flags ? [[delta.flags, "[NumberReplace]"]] : [],
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "StringReplace",
                    Replace: value,
                  },
                  "[StringReplace]",
                  delta.flags ? [[delta.flags, "[StringReplace]"]] : [],
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "BoolReplace",
                    Replace: value,
                  },
                  "[BoolReplace]",
                  delta.flags ? [[delta.flags, "[BoolReplace]"]] : [],
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "TimeReplace",
                    Replace: value,
                  },
                  "[TimeReplace]",
                  delta.flags ? [[delta.flags, "[TimeReplace]"]] : [],
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "GuidReplace",
                    Replace: value,
                  },
                  "[GuidReplace]",
                  delta.flags ? [[delta.flags, "[GuidReplace]"]] : [],
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "UnitReplace",
                    Replace: value,
                  },
                  "UnitReplace",
                  delta.flags ? [[delta.flags, "UnitReplace"]] : [],
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "OptionReplace",
                    Replace: value,
                  },
                  "[OptionReplace]",
                  delta.flags ? [[delta.flags, "[OptionReplace]"]] : [],
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
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: "OptionValue",
                  Value: value[0],
                },
                `[OptionValue]${value[1]}`,
                delta.flags
                  ? [[delta.flags, `[OptionValue]${value[1]}`], ...value[2]]
                  : value[2],
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "SumReplace",
                    Replace: value,
                  },
                  "[SumReplace]",
                  delta.flags ? [[delta.flags, "[SumReplace]"]] : [],
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
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: "SumLeft",
                  Left: value[0],
                },
                `[SumLeft]${value[1]}`,
                delta.flags
                  ? [[delta.flags, `[SumLeft]${value[1]}`], ...value[2]]
                  : value[2],
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
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: "SumRight",
                  Right: value[0],
                },
                `[SumRight]${value[1]}`,
                delta.flags
                  ? [[delta.flags, "[SumRight]"], ...value[2]]
                  : value[2],
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "ArrayReplace",
                    Replace: value,
                  },
                  "[ArrayReplace]",
                  delta.flags ? [[delta.flags, "[ArrayReplace]"]] : [],
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
                  AggregatedFlags<Flags>,
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
                delta.flags
                  ? [
                      [
                        delta.flags,
                        `[ArrayValue][${delta.value[0]}]${value[1]}`,
                      ],
                      ...value[2],
                    ]
                  : value[2],
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "ArrayAdd",
                    Add: value,
                  },
                  "[ArrayAdd]",
                  delta.flags ? [[delta.flags, "[ArrayAdd]"]] : [],
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
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: "ArrayAddAt",
                  AddAt: { Item1: delta.value[0], Item2: element },
                },
                `[ArrayAddAt][${delta.value[0]}]`,
                delta.flags
                  ? [[delta.flags, `[ArrayAddAt][${delta.value[0]}]`]]
                  : [],
              ]),
            );
          }
          if (delta.kind == "ArrayRemoveAt") {
            return ValueOrErrors.Default.return<
              [
                DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                DispatchDeltaTransferComparand,
                AggregatedFlags<Flags>,
              ],
              string
            >([
              {
                Discriminator: "ArrayRemoveAt",
                RemoveAt: delta.index,
              },
              `[ArrayRemoveAt]`,
              delta.flags ? [[delta.flags, "[ArrayRemoveAt]"]] : [],
            ]);
          }
          if (delta.kind == "ArrayMoveFromTo") {
            return ValueOrErrors.Default.return<
              [
                DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                DispatchDeltaTransferComparand,
                AggregatedFlags<Flags>,
              ],
              string
            >([
              {
                Discriminator: "ArrayMoveFromTo",
                MoveFromTo: { Item1: delta.from, Item2: delta.to },
              },
              `[ArrayMoveFromTo]`,
              delta.flags ? [[delta.flags, "[ArrayMoveFromTo]"]] : [],
            ]);
          }
          if (delta.kind == "ArrayDuplicateAt") {
            return ValueOrErrors.Default.return<
              [
                DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                DispatchDeltaTransferComparand,
                AggregatedFlags<Flags>,
              ],
              string
            >([
              {
                Discriminator: "ArrayDuplicateAt",
                DuplicateAt: delta.index,
              },
              `[ArrayDuplicateAt]`,
              delta.flags ? [[delta.flags, "[ArrayDuplicateAt]"]] : [],
            ]);
          }
          if (delta.kind == "SetReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "SetReplace",
                    Replace: value,
                  },
                  "[SetReplace]",
                  delta.flags ? [[delta.flags, "[SetReplace]"]] : [],
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
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: "SetValue",
                  Value: { Item1: delta.value[0], Item2: value[0] },
                },
                `[SetValue][${delta.value[0]}]${value[1]}`,
                delta.flags
                  ? [
                      [delta.flags, `[SetValue][${delta.value[0]}]${value[1]}`],
                      ...value[2],
                    ]
                  : value[2],
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "SetAdd",
                    Add: value,
                  },
                  `[SetAdd]`,
                  delta.flags ? [[delta.flags, "[SetAdd]"]] : [],
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "SetRemove",
                    Remove: value,
                  },
                  `[SetRemove]`,
                  delta.flags ? [[delta.flags, "[SetRemove]"]] : [],
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "MapReplace",
                    Replace: value,
                  },
                  "[MapReplace]",
                  delta.flags ? [[delta.flags, "[MapReplace]"]] : [],
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
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: "MapKey",
                  Key: { Item1: delta.value[0], Item2: value[0] },
                },
                `[MapKey][${delta.value[0]}]${value[1]}`,
                delta.flags
                  ? [
                      [delta.flags, `[MapKey][${delta.value[0]}]${value[1]}`],
                      ...value[2],
                    ]
                  : value[2],
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
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: "MapValue",
                  Value: { Item1: delta.value[0], Item2: value[0] },
                },
                `[MapValue][${delta.value[0]}]${value[1]}`,
                delta.flags
                  ? [
                      [delta.flags, `[MapValue][${delta.value[0]}]${value[1]}`],
                      ...value[2],
                    ]
                  : value[2],
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "MapAdd",
                    Add: { Item1: key, Item2: value },
                  },
                  `[MapAdd]`,
                  delta.flags ? [[delta.flags, "[MapAdd]"]] : [],
                ]),
              ),
            );
          }
          if (delta.kind == "MapRemove") {
            return ValueOrErrors.Default.return<
              [
                DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                DispatchDeltaTransferComparand,
                AggregatedFlags<Flags>,
              ],
              string
            >([
              {
                Discriminator: "MapRemove",
                Remove: delta.index,
              },
              `[MapRemove]`,
              delta.flags ? [[delta.flags, "[MapRemove]"]] : [],
            ]);
          }
          if (delta.kind == "RecordReplace") {
            if (delta.type.kind != "lookup") {
              return ValueOrErrors.Default.throwOne<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                  AggregatedFlags<Flags>,
                ],
                string
              >(
                `Error: cannot process non look up record delta ${delta}, not currently supported.`,
              );
            }
            const lookupName = delta.type.name;
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: lookupName,
                    [lookupName]: value,
                  },
                  `[${lookupName}Replace]`,
                  delta.flags ? [[delta.flags, `[${lookupName}Replace]`]] : [],
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
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: delta.field[0],
                  [delta.field[0]]: value[0],
                } as {
                  Discriminator: string;
                } & {
                  [
                    field: string
                  ]: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
                },
                `[RecordField][${delta.field[0]}]${value[1]}`,
                delta.flags
                  ? [
                      [
                        delta.flags,
                        `[RecordField][${delta.field[0]}]${value[1]}`,
                      ],
                      ...value[2],
                    ]
                  : value[2],
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "UnionReplace",
                    Replace: value,
                  },
                  "[UnionReplace]",
                  delta.flags ? [[delta.flags, "[UnionReplace]"]] : [],
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
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: "UnionCase",
                  [delta.caseName[0]]: value[0],
                } as {
                  Discriminator: "UnionCase";
                } & {
                  [
                    caseName: string
                  ]: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
                },
                `[UnionCase][${delta.caseName[0]}]`,
                delta.flags
                  ? [
                      [delta.flags, `[UnionCase][${delta.caseName[0]}]`],
                      ...value[2],
                    ]
                  : value[2],
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
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "TupleReplace",
                    Replace: value,
                  },
                  "[TupleReplace]",
                  delta.flags ? [[delta.flags, "[TupleReplace]"]] : [],
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
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: `Tuple${
                    (delta.tupleType as TupleType<any>).args.length
                  }Item${delta.item[0] + 1}`,
                  [`Item${delta.item[0] + 1}`]: value[0],
                } as {
                  Discriminator: string;
                } & {
                  [
                    item: string
                  ]: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
                },
                `[TupleCase][${delta.item[0] + 1}]${value[1]}`,
                delta.flags
                  ? [
                      [
                        delta.flags,
                        `[TupleCase][${delta.item[0] + 1}]${value[1]}`,
                      ],
                      ...value[2],
                    ]
                  : value[2],
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
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: "TableValue",
                  Value: { Item1: delta.id, Item2: value[0] },
                },
                `[TableValue][${delta.id}]${value[1]}`,
                delta.flags
                  ? [
                      [delta.flags, `[TableValue][${delta.id}]${value[1]}`],
                      ...value[2],
                    ]
                  : value[2],
              ]),
            );
          }
          if (delta.kind == "TableAddEmpty") {
            return ValueOrErrors.Default.return<
              [
                DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                DispatchDeltaTransferComparand,
                AggregatedFlags<Flags>,
              ],
              string
            >([
              {
                Discriminator: "TableAddEmpty",
              },
              `[TableAddEmpty]`,
              delta.flags ? [[delta.flags, "[TableAddEmpty]"]] : [],
            ]);
          }
          if (delta.kind == "TableRemove") {
            return ValueOrErrors.Default.return<
              [
                DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                DispatchDeltaTransferComparand,
                AggregatedFlags<Flags>,
              ],
              string
            >([
              {
                Discriminator: "TableRemoveAt",
                RemoveAt: delta.id,
              },
              `[TableRemoveAt][${delta.id}]`,
              delta.flags
                ? [[delta.flags, `[TableRemoveAt][${delta.id}]`]]
                : [],
            ]);
          }
          if (delta.kind == "TableDuplicate") {
            return ValueOrErrors.Default.return<
              [
                DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                DispatchDeltaTransferComparand,
                AggregatedFlags<Flags>,
              ],
              string
            >([
              {
                Discriminator: "TableDuplicateAt",
                DuplicateAt: delta.id,
              },
              `[TableDuplicateAt][${delta.id}]`,
              delta.flags
                ? [[delta.flags, `[TableDuplicateAt][${delta.id}]`]]
                : [],
            ]);
          }
          if (delta.kind == "TableMoveTo") {
            return ValueOrErrors.Default.return<
              [
                DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                DispatchDeltaTransferComparand,
                AggregatedFlags<Flags>,
              ],
              string
            >([
              {
                Discriminator: "TableMoveFromTo",
                MoveFromTo: [delta.id, delta.to],
              },
              `[TableMoveFromTo][${delta.id}][${delta.to}]`,
              delta.flags
                ? [[delta.flags, `[TableMoveFromTo][${delta.id}][${delta.to}]`]]
                : [],
            ]);
          }
          if (delta.kind == "OneValue") {
            return DispatchDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.nestedDelta).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: "OneValue",
                  Value: value[0],
                },
                `[OneValue]${value[1]}`,
                delta.flags
                  ? [[delta.flags, `[OneValue]${value[1]}`], ...value[2]]
                  : value[2],
              ]),
            );
          }
          if (delta.kind == "OneReplace") {
            if (delta.type.kind != "one") {
              return ValueOrErrors.Default.throwOne<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                  AggregatedFlags<Flags>,
                ],
                string
              >(
                `Error: one expected but received ${JSON.stringify(
                  delta.type,
                )} in OneReplace.`,
              );
            }
            return toRawObject(delta.replace, delta.type.arg, unit).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "OneReplace",
                    Replace: value,
                  },
                  `[OneReplace]`,
                  delta.flags
                    ? [[delta.flags, `[OneReplace]`], ...value[2]]
                    : value[2],
                ]),
            );
          }
          if (delta.kind == "OneCreateValue") {
            if (delta.type.kind != "one") {
              return ValueOrErrors.Default.throwOne<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                  AggregatedFlags<Flags>,
                ],
                string
              >(
                `Error: one expected but received ${JSON.stringify(
                  delta.type,
                )} in OneCreateValue.`,
              );
            }

            return toRawObject(delta.value, delta.type.arg, unit).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: "OneCreateValue",
                    CreateValue: value,
                  },
                  `[OneCreateValue]`,
                  delta.flags
                    ? [[delta.flags, `[OneCreateValue]`], ...value[2]]
                    : value[2],
                ]),
            );
          }
          // TODO -- suspicious
          if (delta.kind == "OneDeleteValue") {
            return ValueOrErrors.Default.return<
              [
                DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                DispatchDeltaTransferComparand,
                AggregatedFlags<Flags>,
              ],
              string
            >([
              {
                Discriminator: "OneDeleteValue",
                DeleteValue: unit,
              },
              `[OneDeleteValue]`,
              delta.flags ? [[delta.flags, `[OneDeleteValue]`]] : [],
            ]);
          }
          if (delta.kind == "CustomDelta") {
            return parseCustomDelta(
              toRawObject,
              DispatchDeltaTransfer.Default.FromDelta(
                toRawObject,
                parseCustomDelta,
              ),
            )(delta).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  ...value[0],
                },
                value[1],
                delta.flags ? [[delta.flags, value[1]], ...value[2]] : value[2],
              ]),
            );
          }
          return ValueOrErrors.Default.throwOne<
            [
              DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
              DispatchDeltaTransferComparand,
              AggregatedFlags<Flags>,
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
