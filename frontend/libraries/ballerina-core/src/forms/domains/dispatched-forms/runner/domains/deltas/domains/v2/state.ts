import {
    AggregatedFlags,
    DeltaTransferSum, DispatchDelta, DispatchDeltaCustom, DispatchDeltaTransferComparand, Maybe,
    PredicateValue, Sum,
    unit,
    Unit,
    ValueOrErrors, ValueSum,
} from "../../../../../../../../../main";
import {
  DispatchParsedType,
  TupleType,
} from "../../../../../deserializer/domains/specification/domains/types/state";
import {List} from "immutable";

export type DeltaV2 = 
    //| { kind: 'multiple', items: DispatchDeltaTransferV2 [] }
    | { kind: 'list', op: { kind: 'updateElementAt', index: number }}
    | { kind: 'list', op: { kind: 'removeElementAt', index: number }}
    | { kind: 'list', op: { kind: 'replace' }}
    | { kind: 'list', op: { kind: 'appendElement' }}
    | { kind: 'list', op: { kind: 'appendElementAt', index: number  }}
    | { kind: 'replace' }
    | { kind: 'edge', cardinality: 'one' | 'many', op: 'link' | 'unlink' | 'replace' }
    | { kind: 'field', field: string }
    | { kind: 'tuple', op: { kind: 'updateElementAt', index: number }}
    | { kind: 'tuple', op: { kind: 'removeElementAt', index: number }}
    | { kind: 'tuple', op: { kind: 'appendElementAt'}}
    | { kind: 'sum', index: number }
    | { kind: 'union', caseName: string }

// 1:1 support from the backend Value parser
export type DeltaTransferV2Body = { discriminator: string, value?: any | any [] } 

export type DispatchDeltaTransferV2 =
    { delta: DeltaV2, transfer: DeltaTransferV2Body | DispatchDeltaTransferV2 | Unit }

export const DispatchDeltaTransferV2 = {
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
                            DispatchDeltaTransferV2,
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
                        DispatchDeltaTransferV2,
                        DispatchDeltaTransferComparand,
                        AggregatedFlags<Flags>,
                    ],
                    string
                > => {
                    const result: ValueOrErrors<
                        [
                            DispatchDeltaTransferV2,
                            DispatchDeltaTransferComparand,
                            AggregatedFlags<Flags>,
                        ],
                        string
                    > = (() => {
                        if (delta.kind == "NumberReplace") {
                            return toRawObject(delta.replace, delta.type, delta.state).Then(
                                (value) =>{
                                    debugger
                                    return ValueOrErrors.Default.return<
                                        [
                                            DispatchDeltaTransferV2,
                                            DispatchDeltaTransferComparand,
                                            AggregatedFlags<Flags>,
                                        ],
                                        string
                                    >([
                                        {
                                            delta: { kind: 'replace' },
                                            transfer: { discriminator: "float32", value: String(delta.replace) }
                                        },
                                        "[NumberReplace]",
                                        delta.flags ? [[delta.flags, "[NumberReplace]"]] : [],
                                    ])},
                            );
                        }
                  
                        if (delta.kind == "StringReplace") {
                            debugger
                            return toRawObject(delta.replace, delta.type, delta.state).Then(
                                (value) =>
                                    ValueOrErrors.Default.return<
                                        [
                                            DispatchDeltaTransferV2,
                                            DispatchDeltaTransferComparand,
                                            AggregatedFlags<Flags>,
                                        ],
                                        string
                                    >([
                                        {
                                            delta: { kind: 'replace' },
                                            transfer: { discriminator: "string", value: delta.replace }
                                        },
                                        "[StringReplace]",
                                        delta.flags ? [[delta.flags, "[StringReplace]"]] : [],
                                    ]),
                            );
                        }
                        if (delta.kind == "BoolReplace") {
                            return toRawObject(delta.replace, delta.type, delta.state).Then(
                                (value) =>{
                                    
           
                                    return ValueOrErrors.Default.return<
                                        [
                                            DispatchDeltaTransferV2,
                                            DispatchDeltaTransferComparand,
                                            AggregatedFlags<Flags>,
                                        ],
                                        string
                                    >([
                                        {
                                            delta: { kind: 'replace' },
                                            transfer: { discriminator: "boolean", value: String(delta.replace) }
                                        },
                                        "[BoolReplace]",
                                        delta.flags ? [[delta.flags, "[BoolReplace]"]] : [],
                                    ])},
                            );
                        }
                        if (delta.kind == "TimeReplace") {
                            return toRawObject(delta.replace, delta.type, delta.state).Then(
                                (value) =>
                                    ValueOrErrors.Default.return<
                                        [
                                            DispatchDeltaTransferV2,
                                            DispatchDeltaTransferComparand,
                                            AggregatedFlags<Flags>,
                                        ],
                                        string
                                    >([
                                        {
                                            delta: { kind: 'replace' },
                                            transfer: { discriminator: "timespan", value: value }
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
                                            DispatchDeltaTransferV2,
                                            DispatchDeltaTransferComparand,
                                            AggregatedFlags<Flags>,
                                        ],
                                        string
                                    >([
                                        {
                                            delta: { kind: 'replace' },
                                            transfer: { discriminator: "guid", value: value }
                                        },
                                        "[GuidReplace]",
                                        delta.flags ? [[delta.flags, "[GuidReplace]"]] : [],
                                    ]),
                            );
                        }
                        if (delta.kind == "UnitReplace") {
                            return ValueOrErrors.Default.return<
                                [
                                    DispatchDeltaTransferV2,
                                    DispatchDeltaTransferComparand,
                                    AggregatedFlags<Flags>,
                                ],
                                string
                            >([
                                {
                                    delta: { kind: 'replace' },
                                    transfer: { discriminator: "unit" }
                                },
                                "UnitReplace",
                                delta.flags ? [[delta.flags, "UnitReplace"]] : [],
                            ]);
                        }
                        if (delta.kind == "OptionReplace") {
                            return toRawObject(delta.replace, delta.type, delta.state).Then(
                                (value) =>{
                                    const op = value.IsSome ? 'replace':'unlink';
                                    const id: Maybe<string> = value.Value.Value != null || value.Value.DisplayValue != null ? value.Value.Id : undefined

                                    if(!id) return ValueOrErrors.Default.throw(List(["Can't determine Id for OptionReplace delta transfer"]))
                                    
                                    const deltaTransfer: DispatchDeltaTransferV2 =  {
                                        delta: { kind: 'edge', cardinality: 'one', op: op },
                                        transfer: {
                                            discriminator: "guid",
                                            value: id!
                                        }
                                    }
                                  
                                    return ValueOrErrors.Default.return<
                                        [
                                            DispatchDeltaTransferV2,
                                            DispatchDeltaTransferComparand,
                                            AggregatedFlags<Flags>,
                                        ],
                                        string
                                    >([ 
                                        deltaTransfer,
                                        "[OptionReplace]",
                                        delta.flags ? [[delta.flags, "[OptionReplace]"]] : [],
                                    ])},
                            );
                        }
                        // if (delta.kind == "OptionValue") {
                        //     return DispatchDeltaTransfer.Default.FromDelta(
                        //         toRawObject,
                        //         parseCustomDelta,
                        //     )(delta.value).Then((value) =>
                        //         ValueOrErrors.Default.return<
                        //             [
                        //                 DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                 DispatchDeltaTransferComparand,
                        //                 AggregatedFlags<Flags>,
                        //             ],
                        //             string
                        //         >([
                        //             {
                        //                 Discriminator: "OptionValue",
                        //                 Value: value[0],
                        //             },
                        //             `[OptionValue]${value[1]}`,
                        //             delta.flags
                        //                 ? [[delta.flags, `[OptionValue]${value[1]}`], ...value[2]]
                        //                 : value[2],
                        //         ]),
                        //     );
                        // }
                        if (delta.kind == "SumReplace") {
                            const sum = delta.replace as ValueSum;

                            return toRawObject(delta.replace, delta.type, delta.state).Then(
                                (value) =>
                                    ValueOrErrors.Default.return<
                                        [
                                            DispatchDeltaTransferV2,
                                            DispatchDeltaTransferComparand,
                                            AggregatedFlags<Flags>,
                                        ],
                                        string
                                    >([
                                        {
                                            delta: { kind: 'sum', index: sum.value.kind == "r" ? 2 : 1 },
                                            transfer:
                                                sum.value.kind == "r"
                                                    ? { discriminator: "datetime", value: sum.value.value}
                                                    : { discriminator: "unit"},
                                            
                                        },
                                        "[SumReplace]",
                                        delta.flags ? [[delta.flags, "[SumReplace]"]] : [],
                                    ]),
                            );
                        }
                        if (delta.kind == "SumLeft") {
                            return DispatchDeltaTransferV2.Default.FromDelta(
                                toRawObject,
                                parseCustomDelta,
                            )(delta.value).Then((value) =>
                                ValueOrErrors.Default.return<
                                    [
                                        DispatchDeltaTransferV2,
                                        DispatchDeltaTransferComparand,
                                        AggregatedFlags<Flags>,
                                    ],
                                    string
                                >([
                                    {
                                        delta: { kind: 'sum', index: 1 },
                                        transfer: value[0]
                                    },
                                    `[SumLeft]${value[1]}`,
                                    delta.flags
                                        ? [[delta.flags, `[SumLeft]${value[1]}`], ...value[2]]
                                        : value[2],
                                ]),
                            );
                        }
                        if (delta.kind == "SumRight") {
                            return DispatchDeltaTransferV2.Default.FromDelta(
                                toRawObject,
                                parseCustomDelta,
                            )(delta.value).Then((value) =>
                                ValueOrErrors.Default.return<
                                    [
                                        DispatchDeltaTransferV2,
                                        DispatchDeltaTransferComparand,
                                        AggregatedFlags<Flags>,
                                    ],
                                    string
                                >([
                                    {
                                        delta: { kind: 'sum', index: 2 },
                                        transfer: value[0]
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
                                            DispatchDeltaTransferV2,
                                            DispatchDeltaTransferComparand,
                                            AggregatedFlags<Flags>,
                                        ],
                                        string
                                    >([
                                        {
                                            delta: { kind: "list", op: {kind: 'replace' }},
                                            transfer:

                                                {
                                                    "discriminator": "list",
                                                    "value": value
                                                }
                                        },

                                        "[ArrayReplace]",
                                        delta.flags ? [[delta.flags, "[ArrayReplace]"]] : [],
                                    ]),
                            );
                        }
                        if (delta.kind == "ArrayValue") {
                            return DispatchDeltaTransferV2.Default.FromDelta(
                                toRawObject,
                                parseCustomDelta,
                            )(delta.value[1]).Then((value) => {
                                    debugger
                                    return ValueOrErrors.Default.return<
                                        [
                                            DispatchDeltaTransferV2,
                                            DispatchDeltaTransferComparand,
                                            AggregatedFlags<Flags>,
                                        ],
                                        string
                                    >([
                                        {
                                            delta: { kind: "list", op: {kind: 'updateElementAt', index: delta.value[0]}},
                                            transfer: value[0]
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
                                    ])
                                },
                            );
                        }
                        if (delta.kind == "ArrayValueAll") {
                            return DispatchDeltaTransferV2.Default.FromDelta(
                                toRawObject,
                                parseCustomDelta,
                            )(delta.nestedDelta).Then((value) =>
                                ValueOrErrors.Default.return<
                                    [
                                        DispatchDeltaTransferV2,
                                        DispatchDeltaTransferComparand,
                                        AggregatedFlags<Flags>,
                                    ],
                                    string
                                >([
                                    {
                                        delta: { kind: "list", op: { kind: 'replace' }},
                                        transfer: {                                     
                                            "discriminator": "list",
                                            "value": value[0]
                                        }
                                    },
                                    `[ArrayValueAll]${value[1]}`,
                                    delta.flags
                                        ? [[delta.flags, `[ArrayValueAll]${value[1]}`], ...value[2]]
                                        : value[2],
                                ]),
                            );
                        }
                        if (delta.kind == "ArrayAdd") {
                            debugger
                            return toRawObject(delta.value, delta.type, delta.state).Then(
                                (value) => {
                                    debugger
                                    return ValueOrErrors.Default.return<
                                        [
                                            DispatchDeltaTransferV2,
                                            DispatchDeltaTransferComparand,
                                            AggregatedFlags<Flags>,
                                        ],
                                        string
                                    >([
                                        {
                                            delta: { kind: "list", op: { kind: 'appendElement' }},
                                            transfer: value
                                        },
                                        "[ArrayAdd]",
                                        delta.flags ? [[delta.flags, "[ArrayAdd]"]] : [],
                                    ])},
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
                                        DispatchDeltaTransferV2,
                                        DispatchDeltaTransferComparand,
                                        AggregatedFlags<Flags>,
                                    ],
                                    string
                                >([
                                    {
                                        delta: { kind: "list", op: { kind: 'appendElementAt', index: element }},
                                        transfer: {
                                            "discriminator": "list",
                                            "value": delta.value[0]
                                        }
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
                                    DispatchDeltaTransferV2,
                                    DispatchDeltaTransferComparand,
                                    AggregatedFlags<Flags>,
                                ],
                                string
                            >([
                                {
                                    delta: { kind: "list", op: { kind: 'removeElementAt', index: delta.index }},
                                    transfer: {}
                                },
                                `[ArrayRemoveAt]`,
                                delta.flags ? [[delta.flags, "[ArrayRemoveAt]"]] : [],
                            ]);
                        }
                        if (delta.kind == "ArrayRemoveAll") {
                            return ValueOrErrors.Default.return<
                                [
                                    DispatchDeltaTransferV2,
                                    DispatchDeltaTransferComparand,
                                    AggregatedFlags<Flags>,
                                ],
                                string
                            >([
                                {
                                    delta: {kind: "replace"},
                                    transfer:

                                        {
                                            "discriminator": "list",
                                            "value": []
                                        }
                                },
                                `[ArrayRemoveAll]`,
                                delta.flags ? [[delta.flags, "[ArrayRemoveAll]"]] : [],
                            ]);
                        }
                        // if (delta.kind == "ArrayMoveFromTo") {
                        //     return ValueOrErrors.Default.return<
                        //         [
                        //             DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //             DispatchDeltaTransferComparand,
                        //             AggregatedFlags<Flags>,
                        //         ],
                        //         string
                        //     >([
                        //         {
                        //             Discriminator: "ArrayMoveFromTo",
                        //             MoveFromTo: { Item1: delta.from, Item2: delta.to },
                        //         },
                        //         `[ArrayMoveFromTo]`,
                        //         delta.flags ? [[delta.flags, "[ArrayMoveFromTo]"]] : [],
                        //     ]);
                        // }
                        // if (delta.kind == "ArrayDuplicateAt") {
                        //     return ValueOrErrors.Default.return<
                        //         [
                        //             DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //             DispatchDeltaTransferComparand,
                        //             AggregatedFlags<Flags>,
                        //         ],
                        //         string
                        //     >([
                        //         {
                        //             Discriminator: "ArrayDuplicateAt",
                        //             Discriminator: "ArrayDuplicateAt",
                        //             DuplicateAt: delta.index,
                        //         },
                        //         `[ArrayDuplicateAt]`,
                        //         delta.flags ? [[delta.flags, "[ArrayDuplicateAt]"]] : [],
                        //     ]);
                        // }
                        if (delta.kind == "SetReplace") {
                            return toRawObject(delta.replace, delta.type, delta.state).Then(
                                (value) =>{
                                    debugger
                                    return ValueOrErrors.Default.return<
                                        [
                                            DispatchDeltaTransferV2,
                                            DispatchDeltaTransferComparand,
                                            AggregatedFlags<Flags>,
                                        ],
                                        string
                                    >([
                                        {
                                            delta: { kind: "edge", cardinality: "many", op: 'replace'},
                                            transfer: {
                                                "discriminator": "list",
                                                "value": value.map((item: any) =>
                                                    ({
                                                        discriminator: "guid",
                                                        value: item.Id!
                                                    }))
                                            }
                                        },
                                        "[SetReplace]",
                                        delta.flags ? [[delta.flags, "[SetReplace]"]] : [],
                                    ])},
                            );
                        }
                        // if (delta.kind == "SetValue") {
                        //     return DispatchDeltaTransfer.Default.FromDelta(
                        //         toRawObject,
                        //         parseCustomDelta,
                        //     )(delta.value[1]).Then((value) =>
                        //         ValueOrErrors.Default.return<
                        //             [
                        //                 DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                 DispatchDeltaTransferComparand,
                        //                 AggregatedFlags<Flags>,
                        //             ],
                        //             string
                        //         >([
                        //             {
                        //                 Discriminator: "SetValue",
                        //                 Value: { Item1: delta.value[0], Item2: value[0] },
                        //             },
                        //             `[SetValue][${delta.value[0]}]${value[1]}`,
                        //             delta.flags
                        //                 ? [
                        //                     [delta.flags, `[SetValue][${delta.value[0]}]${value[1]}`],
                        //                     ...value[2],
                        //                 ]
                        //                 : value[2],
                        //         ]),
                        //     );
                        // }
                        // if (delta.kind == "SetAdd") {
                        //     return toRawObject(delta.value, delta.type, delta.state).Then(
                        //         (value) =>
                        //             ValueOrErrors.Default.return<
                        //                 [
                        //                     DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                     DispatchDeltaTransferComparand,
                        //                     AggregatedFlags<Flags>,
                        //                 ],
                        //                 string
                        //             >([
                        //                 {
                        //                     Discriminator: "SetAdd",
                        //                     Add: value,
                        //                 },
                        //                 `[SetAdd]`,
                        //                 delta.flags ? [[delta.flags, "[SetAdd]"]] : [],
                        //             ]),
                        //     );
                        // }
                        // if (delta.kind == "SetRemove") {
                        //     return toRawObject(delta.value, delta.type, delta.state).Then(
                        //         (value) =>
                        //             ValueOrErrors.Default.return<
                        //                 [
                        //                     DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                     DispatchDeltaTransferComparand,
                        //                     AggregatedFlags<Flags>,
                        //                 ],
                        //                 string
                        //             >([
                        //                 {
                        //                     Discriminator: "SetRemove",
                        //                     Remove: value,
                        //                 },
                        //                 `[SetRemove]`,
                        //                 delta.flags ? [[delta.flags, "[SetRemove]"]] : [],
                        //             ]),
                        //     );
                        // }
                        // if (delta.kind == "MapReplace") {
                        //     return toRawObject(delta.replace, delta.type, delta.state).Then(
                        //         (value) =>
                        //             ValueOrErrors.Default.return<
                        //                 [
                        //                     DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                     DispatchDeltaTransferComparand,
                        //                     AggregatedFlags<Flags>,
                        //                 ],
                        //                 string
                        //             >([
                        //                 {
                        //                     Discriminator: "MapReplace",
                        //                     Replace: value,
                        //                 },
                        //                 "[MapReplace]",
                        //                 delta.flags ? [[delta.flags, "[MapReplace]"]] : [],
                        //             ]),
                        //     );
                        // }
                        if (delta.kind == "MapKey") {
                            debugger
                            return DispatchDeltaTransferV2.Default.FromDelta(
                                toRawObject,
                                parseCustomDelta,
                            )(delta.value[1]).Then((value) => {
                                    debugger
                                    return ValueOrErrors.Default.return<
                                        [
                                            DispatchDeltaTransferV2,
                                            DispatchDeltaTransferComparand,
                                            AggregatedFlags<Flags>,
                                        ],
                                        string
                                    >([
                                        {
                                            delta: { kind: 'tuple', op: { kind: 'updateElementAt', index: delta.value[0]}},
                                            transfer: {
                                                delta: {kind: 'field', field: 'Key'},
                                                transfer: value[0]
                                            }

                                            // Key: { Item1: delta.value[0], Item2: value[0] },
                                        },
                                        `[MapKey][${delta.value[0]}]${value[1]}`,
                                        delta.flags
                                            ? [
                                                [delta.flags, `[MapKey][${delta.value[0]}]${value[1]}`],
                                                ...value[2],
                                            ]
                                            : value[2],
                                    ])
                                },
                            );
                        }
                        if (delta.kind == "MapValue") {
                            return DispatchDeltaTransferV2.Default.FromDelta(
                                toRawObject,
                                parseCustomDelta,
                            )(delta.value[1]).Then((value) =>
                                ValueOrErrors.Default.return<
                                    [
                                        DispatchDeltaTransferV2,
                                        DispatchDeltaTransferComparand,
                                        AggregatedFlags<Flags>,
                                    ],
                                    string
                                >([
                                    {
                                        // Discriminator: "MapValue",
                                        // Value: { Item1: delta.value[0], Item2: value[0] },
                                        delta: { kind: 'tuple', op: { kind: 'updateElementAt', index: delta.value[0]}},
                                        transfer: {
                                            delta: {kind: 'field', field: 'Value'},
                                            transfer: value[0]
                                        }
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
                                ).Then((value) => {
                                    debugger
                                    return ValueOrErrors.Default.return<
                                        [
                                            DispatchDeltaTransferV2,
                                            DispatchDeltaTransferComparand,
                                            AggregatedFlags<Flags>,
                                        ],
                                        string
                                    >([ 
                                        {
                                            delta: { kind: 'tuple', op: { kind: 'appendElementAt' }},
                                            transfer: {
                                                discriminator: "record",
                                                value: [
                                                    [
                                                        {"discriminator":"id","value":["","",null,"Key"]}, 
                                                        {"discriminator":"string","value":key}
                                                    ],
                                                    [
                                                        {"discriminator":"id","value":["","",null,"Value"]},
                                                        {"discriminator":"string","value":value}
                                                    ]
                                                ]
                                            }
                                        },
                                        // {
                                        //     Discriminator: "MapAdd",
                                        //     Add: { Item1: key, Item2: value },
                                        // },
                                        `[MapAdd]`,
                                        delta.flags ? [[delta.flags, "[MapAdd]"]] : [],
                                    ])},
                                ),
                            );
                        }
                        if (delta.kind == "MapRemove") {
                            return ValueOrErrors.Default.return<
                                [
                                    DispatchDeltaTransferV2,
                                    DispatchDeltaTransferComparand,
                                    AggregatedFlags<Flags>,
                                ],
                                string
                            >([
                                {
                                    delta: { kind: 'tuple', op: { kind: 'removeElementAt', index: delta.index }},
                                    transfer: {}
                                },
                                // {
                                //     Discriminator: "MapRemove",
                                //     Remove: delta.index,
                                // },
                                `[MapRemove]`,
                                delta.flags ? [[delta.flags, "[MapRemove]"]] : [],
                            ]);
                        }
                        // if (delta.kind == "RecordReplace") {
                        //     if (delta.type.kind != "lookup") {
                        //         return ValueOrErrors.Default.throwOne<
                        //             [
                        //                 DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                 DispatchDeltaTransferComparand,
                        //                 AggregatedFlags<Flags>,
                        //             ],
                        //             string
                        //         >(
                        //             `Error: cannot process non look up record delta ${delta}, not currently supported.`,
                        //         );
                        //     }
                        //     const lookupName = delta.type.name;
                        //     return toRawObject(delta.replace, delta.type, delta.state).Then(
                        //         (value) =>
                        //             ValueOrErrors.Default.return<
                        //                 [
                        //                     DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                     DispatchDeltaTransferComparand,
                        //                     AggregatedFlags<Flags>,
                        //                 ],
                        //                 string
                        //             >([
                        //                 {
                        //                     Discriminator: "Replace",
                        //                     Replace: value,
                        //                 },
                        //                 `[${lookupName}Replace]`,
                        //                 delta.flags ? [[delta.flags, `[${lookupName}Replace]`]] : [],
                        //             ]),
                        //     );
                        // }
                        if (delta.kind == "RecordField") {
                            return DispatchDeltaTransferV2.Default.FromDelta(
                                toRawObject,
                                parseCustomDelta,
                            )(delta.field[1]).Then((value) =>
                            {
                                debugger
                                return ValueOrErrors.Default.return<
                                    [
                                        DispatchDeltaTransferV2,
                                        DispatchDeltaTransferComparand,
                                        AggregatedFlags<Flags>,
                                    ],
                                    string
                                >([
                                    {
                                        delta: { kind: 'field', field: delta.field[0] },
                                        transfer: value[0]
                                    },
                                    // {
                                    //     Discriminator: delta.field[0],
                                    //     [delta.field[0]]: value[0],
                                    // } as {
                                    //     Discriminator: string;
                                    // } & {
                                    //     [
                                    //         field: string
                                    //         ]: DispatchDeltaTransferV2;
                                    // },
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
                                ])},
                            );
                        }
                        // if (delta.kind == "UnionReplace") {
                        //     return toRawObject(delta.replace, delta.type, delta.state).Then(
                        //         (value) => {
                        //             debugger
                        //             return ValueOrErrors.Default.return<
                        //                 [
                        //                     DispatchDeltaTransferV2,
                        //                     DispatchDeltaTransferComparand,
                        //                     AggregatedFlags<Flags>,
                        //                 ],
                        //                 string
                        //             >([
                        //                 {
                        //                     delta: { kind: 'replace' },
                        //                     transfer: { discriminator: "union-case",
                        //                     value: [{discriminator:"string",value:value}]}//this is wrong
                        //                 },
                        //                 "[UnionReplace]",
                        //                 delta.flags ? [[delta.flags, "[UnionReplace]"]] : [],
                        //             ])},
                        //     );
                        // }
                        // if (delta.kind == "UnionCase") {
                        //     return DispatchDeltaTransfer.Default.FromDelta(
                        //         toRawObject,
                        //         parseCustomDelta,
                        //     )(delta.caseName[1]).Then((value) =>
                        //         ValueOrErrors.Default.return<
                        //             [
                        //                 DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                 DispatchDeltaTransferComparand,
                        //                 AggregatedFlags<Flags>,
                        //             ],
                        //             string
                        //         >([
                        //             {
                        //                 Discriminator: `Case${delta.caseName[0]}`,
                        //                 [`Case${delta.caseName[0]}`]: value[0],
                        //             } as {
                        //                 Discriminator: string;
                        //             } & {
                        //                 [
                        //                     caseName: string
                        //                     ]: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
                        //             },
                        //             `[UnionCase][${delta.caseName[0]}]`,
                        //             delta.flags
                        //                 ? [
                        //                     [delta.flags, `[UnionCase][${delta.caseName[0]}]`],
                        //                     ...value[2],
                        //                 ]
                        //                 : value[2],
                        //         ]),
                        //     );
                        // }
                        // if (delta.kind == "TupleReplace") {
                        //     return toRawObject(delta.replace, delta.type, delta.state).Then(
                        //         (value) =>
                        //             ValueOrErrors.Default.return<
                        //                 [
                        //                     DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                     DispatchDeltaTransferComparand,
                        //                     AggregatedFlags<Flags>,
                        //                 ],
                        //                 string
                        //             >([
                        //                 {
                        //                     Discriminator: "TupleReplace",
                        //                     Replace: value,
                        //                 },
                        //                 "[TupleReplace]",
                        //                 delta.flags ? [[delta.flags, "[TupleReplace]"]] : [],
                        //             ]),
                        //     );
                        // }
                        // if (delta.kind == "TupleCase") {
                        //     return DispatchDeltaTransfer.Default.FromDelta(
                        //         toRawObject,
                        //         parseCustomDelta,
                        //     )(delta.item[1]).Then((value) =>
                        //         ValueOrErrors.Default.return<
                        //             [
                        //                 DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                 DispatchDeltaTransferComparand,
                        //                 AggregatedFlags<Flags>,
                        //             ],
                        //             string
                        //         >([
                        //             {
                        //                 Discriminator: `Tuple${
                        //                     (delta.tupleType as TupleType<any>).args.length
                        //                 }Item${delta.item[0] + 1}`,
                        //                 [`Item${delta.item[0] + 1}`]: value[0],
                        //             } as {
                        //                 Discriminator: string;
                        //             } & {
                        //                 [
                        //                     item: string
                        //                     ]: DispatchDeltaTransfer<DispatchDeltaTransferCustom>;
                        //             },
                        //             `[TupleCase][${delta.item[0] + 1}]${value[1]}`,
                        //             delta.flags
                        //                 ? [
                        //                     [
                        //                         delta.flags,
                        //                         `[TupleCase][${delta.item[0] + 1}]${value[1]}`,
                        //                     ],
                        //                     ...value[2],
                        //                 ]
                        //                 : value[2],
                        //         ]),
                        //     );
                        // }
                        // if (delta.kind == "TableValue") {
                        //     return DispatchDeltaTransfer.Default.FromDelta(
                        //         toRawObject,
                        //         parseCustomDelta,
                        //     )(delta.nestedDelta).Then((value) =>
                        //         ValueOrErrors.Default.return<
                        //             [
                        //                 DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                 DispatchDeltaTransferComparand,
                        //                 AggregatedFlags<Flags>,
                        //             ],
                        //             string
                        //         >([
                        //             {
                        //                 Discriminator: "TableValue",
                        //                 Value: { Item1: delta.id, Item2: value[0] },
                        //             },
                        //             `[TableValue][${delta.id}]${value[1]}`,
                        //             delta.flags
                        //                 ? [
                        //                     [delta.flags, `[TableValue][${delta.id}]${value[1]}`],
                        //                     ...value[2],
                        //                 ]
                        //                 : value[2],
                        //         ]),
                        //     );
                        // }
                        // if (delta.kind == "TableValueAll") {
                        //     return DispatchDeltaTransfer.Default.FromDelta(
                        //         toRawObject,
                        //         parseCustomDelta,
                        //     )(delta.nestedDelta).Then((value) =>
                        //         ValueOrErrors.Default.return<
                        //             [
                        //                 DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                 DispatchDeltaTransferComparand,
                        //                 AggregatedFlags<Flags>,
                        //             ],
                        //             string
                        //         >([
                        //             {
                        //                 Discriminator: "TableValueAll",
                        //                 ValueAll: value[0],
                        //             },
                        //             `[TableValueAll]${value[1]}`,
                        //             delta.flags
                        //                 ? [[delta.flags, `[TableValueAll]${value[1]}`], ...value[2]]
                        //                 : value[2],
                        //         ]),
                        //     );
                        // }
                        // if (delta.kind == "TableAddEmpty") {
                        //     return ValueOrErrors.Default.return<
                        //         [
                        //             DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //             DispatchDeltaTransferComparand,
                        //             AggregatedFlags<Flags>,
                        //         ],
                        //         string
                        //     >([
                        //         {
                        //             Discriminator: "TableAddEmpty",
                        //         },
                        //         `[TableAddEmpty]`,
                        //         delta.flags ? [[delta.flags, "[TableAddEmpty]"]] : [],
                        //     ]);
                        // }
                        // if (delta.kind == "TableRemove") {
                        //     return ValueOrErrors.Default.return<
                        //         [
                        //             DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //             DispatchDeltaTransferComparand,
                        //             AggregatedFlags<Flags>,
                        //         ],
                        //         string
                        //     >([
                        //         {
                        //             Discriminator: "TableRemoveAt",
                        //             RemoveAt: delta.id,
                        //         },
                        //         `[TableRemoveAt][${delta.id}]`,
                        //         delta.flags
                        //             ? [[delta.flags, `[TableRemoveAt][${delta.id}]`]]
                        //             : [],
                        //     ]);
                        // }
                        // if (delta.kind == "TableRemoveAll") {
                        //     return ValueOrErrors.Default.return<
                        //         [
                        //             DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //             DispatchDeltaTransferComparand,
                        //             AggregatedFlags<Flags>,
                        //         ],
                        //         string
                        //     >([
                        //         {
                        //             Discriminator: "TableRemoveAll",
                        //             RemoveAll: unit,
                        //         },
                        //         `[TableRemoveAll]`,
                        //         delta.flags ? [[delta.flags, "[TableRemoveAll]"]] : [],
                        //     ]);
                        // }
                        // if (delta.kind == "TableDuplicate") {
                        //     return ValueOrErrors.Default.return<
                        //         [
                        //             DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //             DispatchDeltaTransferComparand,
                        //             AggregatedFlags<Flags>,
                        //         ],
                        //         string
                        //     >([
                        //         {
                        //             Discriminator: "TableDuplicateAt",
                        //             DuplicateAt: delta.id,
                        //         },
                        //         `[TableDuplicateAt][${delta.id}]`,
                        //         delta.flags
                        //             ? [[delta.flags, `[TableDuplicateAt][${delta.id}]`]]
                        //             : [],
                        //     ]);
                        // }
                        // if (delta.kind == "TableMoveTo") {
                        //     return ValueOrErrors.Default.return<
                        //         [
                        //             DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //             DispatchDeltaTransferComparand,
                        //             AggregatedFlags<Flags>,
                        //         ],
                        //         string
                        //     >([
                        //         {
                        //             Discriminator: "TableMoveFromTo",
                        //             MoveFromTo: {
                        //                 Item1: delta.id,
                        //                 Item2: delta.to,
                        //             },
                        //         },
                        //         `[TableMoveFromTo][${delta.id}][${delta.to}]`,
                        //         delta.flags
                        //             ? [[delta.flags, `[TableMoveFromTo][${delta.id}][${delta.to}]`]]
                        //             : [],
                        //     ]);
                        // }
                        // if (delta.kind == "OneValue") {
                        //     return DispatchDeltaTransfer.Default.FromDelta(
                        //         toRawObject,
                        //         parseCustomDelta,
                        //     )(delta.nestedDelta).Then((value) =>
                        //         ValueOrErrors.Default.return<
                        //             [
                        //                 DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                 DispatchDeltaTransferComparand,
                        //                 AggregatedFlags<Flags>,
                        //             ],
                        //             string
                        //         >([
                        //             {
                        //                 Discriminator: "OneValue",
                        //                 Value: value[0],
                        //             },
                        //             `[OneValue]${value[1]}`,
                        //             delta.flags
                        //                 ? [[delta.flags, `[OneValue]${value[1]}`], ...value[2]]
                        //                 : value[2],
                        //         ]),
                        //     );
                        // }
                        // if (delta.kind == "OneReplace") {
                        //     if (delta.type.kind != "one") {
                        //         return ValueOrErrors.Default.throwOne<
                        //             [
                        //                 DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                 DispatchDeltaTransferComparand,
                        //                 AggregatedFlags<Flags>,
                        //             ],
                        //             string
                        //         >(
                        //             `Error: one expected but received ${JSON.stringify(
                        //                 delta.type,
                        //             )} in OneReplace.`,
                        //         );
                        //     }
                        //     return toRawObject(delta.replace, delta.type.arg, unit).Then(
                        //         (value) => {
                        //             return ValueOrErrors.Default.return<
                        //                 [
                        //                     DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                     DispatchDeltaTransferComparand,
                        //                     AggregatedFlags<Flags>,
                        //                 ],
                        //                 string
                        //             >([
                        //                 {
                        //                     Discriminator: "OneReplace",
                        //                     Replace: value,
                        //                 },
                        //                 `[OneReplace]`,
                        //                 delta.flags ? [[delta.flags, `[OneReplace]`]] : [],
                        //             ]);
                        //         },
                        //     );
                        // }
                        // if (delta.kind == "OneCreateValue") {
                        //     if (delta.type.kind != "one") {
                        //         return ValueOrErrors.Default.throwOne<
                        //             [
                        //                 DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                 DispatchDeltaTransferComparand,
                        //                 AggregatedFlags<Flags>,
                        //             ],
                        //             string
                        //         >(
                        //             `Error: one expected but received ${JSON.stringify(
                        //                 delta.type,
                        //             )} in OneCreateValue.`,
                        //         );
                        //     }
                        //
                        //     return toRawObject(delta.value, delta.type.arg, unit).Then(
                        //         (value) =>
                        //             ValueOrErrors.Default.return<
                        //                 [
                        //                     DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                     DispatchDeltaTransferComparand,
                        //                     AggregatedFlags<Flags>,
                        //                 ],
                        //                 string
                        //             >([
                        //                 {
                        //                     Discriminator: "OneCreateValue",
                        //                     CreateValue: value,
                        //                 },
                        //                 `[OneCreateValue]`,
                        //                 delta.flags ? [[delta.flags, `[OneCreateValue]`]] : [],
                        //             ]),
                        //     );
                        // }
                        // TODO -- suspicious
                        // if (delta.kind == "OneDeleteValue") {
                        //     return ValueOrErrors.Default.return<
                        //         [
                        //             DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //             DispatchDeltaTransferComparand,
                        //             AggregatedFlags<Flags>,
                        //         ],
                        //         string
                        //     >([
                        //         {
                        //             Discriminator: "OneDeleteValue",
                        //             DeleteValue: unit,
                        //         },
                        //         `[OneDeleteValue]`,
                        //         delta.flags ? [[delta.flags, `[OneDeleteValue]`]] : [],
                        //     ]);
                        // }
                        // if (delta.kind == "CustomDelta") {
                        //     return parseCustomDelta(
                        //         toRawObject,
                        //         DispatchDeltaTransfer.Default.FromDelta(
                        //             toRawObject,
                        //             parseCustomDelta,
                        //         ),
                        //     )(delta).Then((value) =>
                        //         ValueOrErrors.Default.return<
                        //             [
                        //                 DispatchDeltaTransfer<DispatchDeltaTransferCustom>,
                        //                 DispatchDeltaTransferComparand,
                        //                 AggregatedFlags<Flags>,
                        //             ],
                        //             string
                        //         >([
                        //             {
                        //                 ...value[0],
                        //             },
                        //             value[1],
                        //             delta.flags ? [[delta.flags, value[1]], ...value[2]] : value[2],
                        //         ]),
                        //     );
                        // }
                        return ValueOrErrors.Default.throwOne<
                            [
                                DispatchDeltaTransferV2,
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
