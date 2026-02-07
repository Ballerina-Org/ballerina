import {
  PredicateValue,
  unit,
  Unit,
  ValueOrErrors,
} from "../../../../../../../../../main";
import {
  DispatchParsedType,
  TupleType,
} from "../../../../../deserializer/domains/specification/domains/types/state";
import type {
  AggregatedFlags,
  DispatchDelta,
  DispatchDeltaCustom,
  DispatchDeltaTransfer as DispatchDeltaTransferType,
  DispatchDeltaTransferComparand,
} from "../../state";

// Did not implement support for all deltas reasoning:
// - NumberReplace - we restrict to numbers in extensions
// - Set deltas - only used in multiselects, not currently supported
// - Option deltas - only used in enums and streams, not currently supported

export const BallerinaDeltaTransfer = {
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
              DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
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
          DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
          DispatchDeltaTransferComparand,
          AggregatedFlags<Flags>,
        ],
        string
      > => {
        const result: ValueOrErrors<
          [
            DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
            DispatchDeltaTransferComparand,
            AggregatedFlags<Flags>,
          ],
          string
        > = (() => {
          // done
          if (delta.kind == "StringReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: 2,
                    Replace: value,
                  },
                  "[StringReplace]",
                  delta.flags ? [[delta.flags, "[StringReplace]"]] : [],
                ]),
            );
          }
          // done
          if (delta.kind == "BoolReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: 2,
                    Replace: value,
                  },
                  "[BoolReplace]",
                  delta.flags ? [[delta.flags, "[BoolReplace]"]] : [],
                ]),
            );
          }
          // TODO - check this, is it just date?
          if (delta.kind == "TimeReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: 2,
                    Replace: value,
                  },
                  "[TimeReplace]",
                  delta.flags ? [[delta.flags, "[TimeReplace]"]] : [],
                ]),
            );
          }
          // done
          if (delta.kind == "GuidReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: 2,
                    Replace: value,
                  },
                  "[GuidReplace]",
                  delta.flags ? [[delta.flags, "[GuidReplace]"]] : [],
                ]),
            );
          }
          // done
          if (delta.kind == "UnitReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: 2,
                    Replace: {
                      Kind: 9,
                      Primitive: {
                        Kind: 12,
                      },
                    },
                  },
                  "[UnitReplace]",
                  delta.flags ? [[delta.flags, "[UnitReplace]"]] : [],
                ]),
            );
          }
          // done
          if (delta.kind == "SumReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: 2,
                    Replace: value,
                  },
                  "[SumReplace]",
                  delta.flags ? [[delta.flags, "[SumReplace]"]] : [],
                ]),
            );
          }
          // done
          if (delta.kind == "SumLeft") {
            return BallerinaDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.value).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: 6,
                  Sum: {
                    CaseIndex: 1,
                    Delta: value[0],
                  },
                },
                `[SumLeft]${value[1]}`,
                delta.flags
                  ? [[delta.flags, `[SumLeft]${value[1]}`], ...value[2]]
                  : value[2],
              ]),
            );
          }
          // done
          if (delta.kind == "SumRight") {
            return BallerinaDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.value).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: 6,
                  Sum: {
                    CaseIndex: 2,
                    Delta: value[0],
                  },
                },
                `[SumRight]${value[1]}`,
                delta.flags
                  ? [[delta.flags, "[SumRight]"], ...value[2]]
                  : value[2],
              ]),
            );
          }
          // done
          if (delta.kind == "RecordReplace") {
            if (delta.type.kind != "lookup") {
              return ValueOrErrors.Default.throwOne<
                [
                  DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
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
                    DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: 2,
                    Replace: value,
                  },
                  `[${lookupName}Replace]`,
                  delta.flags ? [[delta.flags, `[${lookupName}Replace]`]] : [],
                ]),
            );
          }
          // done
          if (delta.kind == "RecordField") {
            return BallerinaDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.field[1]).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: 3,
                  Record: {
                    Field: delta.field[0],
                    Delta: value[0],
                  },
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
          // done
          if (delta.kind == "UnionReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: 2,
                    Replace: value,
                  },
                  "[UnionReplace]",
                  delta.flags ? [[delta.flags, "[UnionReplace]"]] : [],
                ]),
            );
          }
          // done
          if (delta.kind == "UnionCase") {
            return BallerinaDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.caseName[1]).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: 4,
                  Union: {
                    Case: delta.caseName[0],
                    Delta: value[0],
                  },
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
          // done
          if (delta.kind == "TupleReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                    DispatchDeltaTransferComparand,
                    AggregatedFlags<Flags>,
                  ],
                  string
                >([
                  {
                    Discriminator: 2,
                    Replace: value,
                  },
                  "[TupleReplace]",
                  delta.flags ? [[delta.flags, "[TupleReplace]"]] : [],
                ]),
            );
          }
          // done
          if (delta.kind == "TupleCase") {
            return BallerinaDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.item[1]).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: 5,
                  Tuple: {
                    Position: delta.item[0] + 1,
                    Delta: value[0],
                  },
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
          // done
          if (delta.kind == "CustomDelta") {
            return parseCustomDelta(
              toRawObject,
              BallerinaDeltaTransfer.Default.FromDelta(
                toRawObject,
                parseCustomDelta,
              ),
            )(delta).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
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
          // extensions - pending
          if (delta.kind == "ArrayReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
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
            return BallerinaDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.value[1]).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
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
          if (delta.kind == "ArrayValueAll") {
            return BallerinaDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.nestedDelta).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                  DispatchDeltaTransferComparand,
                  AggregatedFlags<Flags>,
                ],
                string
              >([
                {
                  Discriminator: "ArrayValueAll",
                  ValueAll: value[0],
                },
                `[ArrayValueAll]${value[1]}`,
                delta.flags
                  ? [[delta.flags, `[ArrayValueAll]${value[1]}`], ...value[2]]
                  : value[2],
              ]),
            );
          }
          if (delta.kind == "ArrayAdd") {
            return toRawObject(delta.value, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
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
                  DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
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
                DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
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
          if (delta.kind == "ArrayRemoveAll") {
            return ValueOrErrors.Default.return<
              [
                DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
                DispatchDeltaTransferComparand,
                AggregatedFlags<Flags>,
              ],
              string
            >([
              {
                Discriminator: "ArrayRemoveAll",
                RemoveAll: unit,
              },
              `[ArrayRemoveAll]`,
              delta.flags ? [[delta.flags, "[ArrayRemoveAll]"]] : [],
            ]);
          }
          if (delta.kind == "ArrayMoveFromTo") {
            return ValueOrErrors.Default.return<
              [
                DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
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
                DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
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
          if (delta.kind == "MapReplace") {
            return toRawObject(delta.replace, delta.type, delta.state).Then(
              (value) =>
                ValueOrErrors.Default.return<
                  [
                    DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
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
            return BallerinaDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.value[1]).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
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
            return BallerinaDeltaTransfer.Default.FromDelta(
              toRawObject,
              parseCustomDelta,
            )(delta.value[1]).Then((value) =>
              ValueOrErrors.Default.return<
                [
                  DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
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
                    DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
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
                DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
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
          return ValueOrErrors.Default.throwOne<
            [
              DispatchDeltaTransferType<DispatchDeltaTransferCustom>,
              DispatchDeltaTransferComparand,
              AggregatedFlags<Flags>,
            ],
            string
          >(
            `Error: cannot process delta ${delta.kind}, not currently supported.`,
          );
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
