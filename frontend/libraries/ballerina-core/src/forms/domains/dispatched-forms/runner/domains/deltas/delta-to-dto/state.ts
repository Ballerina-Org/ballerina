import { ValueOrErrors } from "../../../../../../../collections/domains/valueOrErrors/state";
import { Unit, unit } from "../../../../../../../fun/domains/unit/state";
import { PredicateValue } from "../../../../../parser/domains/predicates/state";
import {
  DispatchParsedType,
  TupleType,
} from "../../../../deserializer/domains/specification/domains/types/state";
import { DispatchDelta, DispatchDeltaCustom } from "../dispatch-delta/state";
import {
  DeltaTransfer,
  DispatchDeltaTransferComparand,
} from "../dispatch-delta-dto/state";

export type BaseFlags = { kind: "localOnly" };

export type AggregatedFlags<Flags> = Array<
  [Flags, DispatchDeltaTransferComparand]
>;

export const DispatchDeltaFromDTO =
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
          DeltaTransfer<DispatchDeltaTransferCustom>,
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
      DeltaTransfer<DispatchDeltaTransferCustom>,
      DispatchDeltaTransferComparand,
      AggregatedFlags<Flags>,
    ],
    string
  > => {
    const result: ValueOrErrors<
      [
        DeltaTransfer<DispatchDeltaTransferCustom>,
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
                DeltaTransfer<DispatchDeltaTransferCustom>,
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
                DeltaTransfer<DispatchDeltaTransferCustom>,
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
                DeltaTransfer<DispatchDeltaTransferCustom>,
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
                DeltaTransfer<DispatchDeltaTransferCustom>,
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
                DeltaTransfer<DispatchDeltaTransferCustom>,
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
        return ValueOrErrors.Default.return<
          [
            DeltaTransfer<DispatchDeltaTransferCustom>,
            DispatchDeltaTransferComparand,
            AggregatedFlags<Flags>,
          ],
          string
        >([
          {},
          "UnitReplace",
          delta.flags ? [[delta.flags, "UnitReplace"]] : [],
        ]);
      }
      if (delta.kind == "OptionReplace") {
        return toRawObject(delta.replace, delta.type, delta.state).Then(
          (value) =>
            ValueOrErrors.Default.return<
              [
                DeltaTransfer<DispatchDeltaTransferCustom>,
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
        return DispatchDeltaFromDTO(
          toRawObject,
          parseCustomDelta,
        )(delta.value).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
                DeltaTransfer<DispatchDeltaTransferCustom>,
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
        return DispatchDeltaFromDTO(
          toRawObject,
          parseCustomDelta,
        )(delta.value).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
        return DispatchDeltaFromDTO(
          toRawObject,
          parseCustomDelta,
        )(delta.value).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
            delta.flags ? [[delta.flags, "[SumRight]"], ...value[2]] : value[2],
          ]),
        );
      }
      if (delta.kind == "ArrayReplace") {
        return toRawObject(delta.replace, delta.type, delta.state).Then(
          (value) =>
            ValueOrErrors.Default.return<
              [
                DeltaTransfer<DispatchDeltaTransferCustom>,
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
        return DispatchDeltaFromDTO(
          toRawObject,
          parseCustomDelta,
        )(delta.value[1]).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
                  [delta.flags, `[ArrayValue][${delta.value[0]}]${value[1]}`],
                  ...value[2],
                ]
              : value[2],
          ]),
        );
      }
      if (delta.kind == "ArrayValueAll") {
        return DispatchDeltaFromDTO(
          toRawObject,
          parseCustomDelta,
        )(delta.nestedDelta).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
        return toRawObject(delta.value, delta.type, delta.state).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
            DeltaTransfer<DispatchDeltaTransferCustom>,
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
            DeltaTransfer<DispatchDeltaTransferCustom>,
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
            DeltaTransfer<DispatchDeltaTransferCustom>,
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
            DeltaTransfer<DispatchDeltaTransferCustom>,
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
                DeltaTransfer<DispatchDeltaTransferCustom>,
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
        return DispatchDeltaFromDTO(
          toRawObject,
          parseCustomDelta,
        )(delta.value[1]).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
        return toRawObject(delta.value, delta.type, delta.state).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
        return toRawObject(delta.value, delta.type, delta.state).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
                DeltaTransfer<DispatchDeltaTransferCustom>,
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
        return DispatchDeltaFromDTO(
          toRawObject,
          parseCustomDelta,
        )(delta.value[1]).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
        return DispatchDeltaFromDTO(
          toRawObject,
          parseCustomDelta,
        )(delta.value[1]).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
                DeltaTransfer<DispatchDeltaTransferCustom>,
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
            DeltaTransfer<DispatchDeltaTransferCustom>,
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
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
                DeltaTransfer<DispatchDeltaTransferCustom>,
                DispatchDeltaTransferComparand,
                AggregatedFlags<Flags>,
              ],
              string
            >([
              {
                Discriminator: "Replace",
                Replace: value,
              },
              `[${lookupName}Replace]`,
              delta.flags ? [[delta.flags, `[${lookupName}Replace]`]] : [],
            ]),
        );
      }
      if (delta.kind == "RecordField") {
        return DispatchDeltaFromDTO(
          toRawObject,
          parseCustomDelta,
        )(delta.field[1]).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
              [field: string]: DeltaTransfer<DispatchDeltaTransferCustom>;
            },
            `[RecordField][${delta.field[0]}]${value[1]}`,
            delta.flags
              ? [
                  [delta.flags, `[RecordField][${delta.field[0]}]${value[1]}`],
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
                DeltaTransfer<DispatchDeltaTransferCustom>,
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
        return DispatchDeltaFromDTO(
          toRawObject,
          parseCustomDelta,
        )(delta.caseName[1]).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
              DispatchDeltaTransferComparand,
              AggregatedFlags<Flags>,
            ],
            string
          >([
            {
              Discriminator: `Case${delta.caseName[0]}`,
              [`Case${delta.caseName[0]}`]: value[0],
            } as {
              Discriminator: string;
            } & {
              [caseName: string]: DeltaTransfer<DispatchDeltaTransferCustom>;
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
                DeltaTransfer<DispatchDeltaTransferCustom>,
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
        return DispatchDeltaFromDTO(
          toRawObject,
          parseCustomDelta,
        )(delta.item[1]).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
              [item: string]: DeltaTransfer<DispatchDeltaTransferCustom>;
            },
            `[TupleCase][${delta.item[0] + 1}]${value[1]}`,
            delta.flags
              ? [
                  [delta.flags, `[TupleCase][${delta.item[0] + 1}]${value[1]}`],
                  ...value[2],
                ]
              : value[2],
          ]),
        );
      }
      if (delta.kind == "TableValue") {
        return DispatchDeltaFromDTO(
          toRawObject,
          parseCustomDelta,
        )(delta.nestedDelta).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
      if (delta.kind == "TableValueAll") {
        return DispatchDeltaFromDTO(
          toRawObject,
          parseCustomDelta,
        )(delta.nestedDelta).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
              DispatchDeltaTransferComparand,
              AggregatedFlags<Flags>,
            ],
            string
          >([
            {
              Discriminator: "TableValueAll",
              ValueAll: value[0],
            },
            `[TableValueAll]${value[1]}`,
            delta.flags
              ? [[delta.flags, `[TableValueAll]${value[1]}`], ...value[2]]
              : value[2],
          ]),
        );
      }
      if (delta.kind == "TableAddEmpty") {
        return ValueOrErrors.Default.return<
          [
            DeltaTransfer<DispatchDeltaTransferCustom>,
            DispatchDeltaTransferComparand,
            AggregatedFlags<Flags>,
          ],
          string
        >([
          {
            Discriminator: "TableAddEmpty",
          },
          `[TableAddEmpty][${delta.uniqueTableIdentifier}][${delta.newIndex}]`,
          delta.flags
            ? [
                [
                  delta.flags,
                  `[TableAddEmpty][${delta.uniqueTableIdentifier}][${delta.newIndex}]`,
                ],
              ]
            : [],
        ]);
      }
      if (delta.kind == "TableRemove") {
        return ValueOrErrors.Default.return<
          [
            DeltaTransfer<DispatchDeltaTransferCustom>,
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
          delta.flags ? [[delta.flags, `[TableRemoveAt][${delta.id}]`]] : [],
        ]);
      }
      if (delta.kind == "TableRemoveAll") {
        return ValueOrErrors.Default.return<
          [
            DeltaTransfer<DispatchDeltaTransferCustom>,
            DispatchDeltaTransferComparand,
            AggregatedFlags<Flags>,
          ],
          string
        >([
          {
            Discriminator: "TableRemoveAll",
            RemoveAll: unit,
          },
          `[TableRemoveAll]`,
          delta.flags ? [[delta.flags, "[TableRemoveAll]"]] : [],
        ]);
      }
      if (delta.kind == "TableDuplicate") {
        return ValueOrErrors.Default.return<
          [
            DeltaTransfer<DispatchDeltaTransferCustom>,
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
          delta.flags ? [[delta.flags, `[TableDuplicateAt][${delta.id}]`]] : [],
        ]);
      }
      if (delta.kind == "TableActionOnAll") {
        return ValueOrErrors.Default.return<
          [
            DeltaTransfer<DispatchDeltaTransferCustom>,
            DispatchDeltaTransferComparand,
            AggregatedFlags<Flags>,
          ],
          string
        >([
          {
            Discriminator: "TableActionOnAll",
            ActionOnAll: delta.filtersAndSorting,
          },
          `[TableActionOnAll]`,
          delta.flags ? [[delta.flags, `[TableActionOnAll]`]] : [],
        ]);
      }
      if (delta.kind == "TableMoveTo") {
        return ValueOrErrors.Default.return<
          [
            DeltaTransfer<DispatchDeltaTransferCustom>,
            DispatchDeltaTransferComparand,
            AggregatedFlags<Flags>,
          ],
          string
        >([
          {
            Discriminator: "TableMoveFromTo",
            MoveFromTo: {
              Item1: delta.id,
              Item2: delta.to,
            },
          },
          `[TableMoveFromTo][${delta.id}][${delta.to}]`,
          delta.flags
            ? [[delta.flags, `[TableMoveFromTo][${delta.id}][${delta.to}]`]]
            : [],
        ]);
      }
      if (delta.kind == "TableAddBatchEmpty") {
        return ValueOrErrors.Default.return<
          [
            DeltaTransfer<DispatchDeltaTransferCustom>,
            DispatchDeltaTransferComparand,
            AggregatedFlags<Flags>,
          ],
          string
        >([
          {
            Discriminator: "TableAddBatchEmpty",
            AddBatchEmpty: delta.count,
          },
          `[TableAddBatchEmpty][${delta.uniqueTableIdentifier}]`,
          delta.flags
            ? [
                [
                  delta.flags,
                  `[TableAddBatchEmpty][${delta.uniqueTableIdentifier}]`,
                ],
              ]
            : [],
        ]);
      }
      if (delta.kind == "TableRemoveBatch") {
        return ValueOrErrors.Default.return<
          [
            DeltaTransfer<DispatchDeltaTransferCustom>,
            DispatchDeltaTransferComparand,
            AggregatedFlags<Flags>,
          ],
          string
        >([
          {
            Discriminator: "TableRemoveBatch",
            RemoveBatch: delta.ids,
          },
          `[TableRemoveBatch][${delta.uniqueTableIdentifier}]`,
          delta.flags
            ? [
                [
                  delta.flags,
                  `[TableRemoveBatch][${delta.uniqueTableIdentifier}]`,
                ],
              ]
            : [],
        ]);
      }
      if (delta.kind == "OneValue") {
        return DispatchDeltaFromDTO(
          toRawObject,
          parseCustomDelta,
        )(delta.nestedDelta).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
          (value) => {
            return ValueOrErrors.Default.return<
              [
                DeltaTransfer<DispatchDeltaTransferCustom>,
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
              delta.flags ? [[delta.flags, `[OneReplace]`]] : [],
            ]);
          },
        );
      }
      if (delta.kind == "OneCreateValue") {
        if (delta.type.kind != "one") {
          return ValueOrErrors.Default.throwOne<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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

        return toRawObject(delta.value, delta.type.arg, unit).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
            delta.flags ? [[delta.flags, `[OneCreateValue]`]] : [],
          ]),
        );
      }
      // TODO -- suspicious
      if (delta.kind == "OneDeleteValue") {
        return ValueOrErrors.Default.return<
          [
            DeltaTransfer<DispatchDeltaTransferCustom>,
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
          DispatchDeltaFromDTO(toRawObject, parseCustomDelta),
        )(delta).Then((value) =>
          ValueOrErrors.Default.return<
            [
              DeltaTransfer<DispatchDeltaTransferCustom>,
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
          DeltaTransfer<DispatchDeltaTransferCustom>,
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
  };
