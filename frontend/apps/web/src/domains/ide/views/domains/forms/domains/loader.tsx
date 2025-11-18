
import {
    EnumFieldViewsMultiSelection,
    EnumFieldViewsSingleSelection,
    MapFieldViews,

    ReadOnlyFieldViews,

    StreamMultiSelectionFieldViews,
    StreamSingleSelectionFieldViews,
    SumFieldViews,
    SumUnitDateFieldViews,
    UnionFieldViews,
    UnitFieldViews,
    StringFieldViews,
    RecordFieldViews,
    ListFieldViews
} from "./ui-kit"
import {
    DispatchPassthroughFormConcreteRenderers,
    DispatchPassthroughFormCustomPresentationContext,
    DispatchPassthroughFormFlags,

    DispatchPassthroughFormExtraContext,
} from "../../../../../dispatched-passthrough-form/views/tailwind-renderers";
import React from "react";
import {BooleanFieldViews} from "./ui-kit/boolean.tsx";
import { NumberFieldViews } from "./ui-kit/number.tsx";
import {TupleFieldViews} from "./ui-kit/tuple.tsx";
import {IdeConcreteRenderers} from "./common/concrete-renderers.ts";

export const IdeRenderers: IdeConcreteRenderers = {
    boolean: BooleanFieldViews, 
    number: NumberFieldViews,
    string: StringFieldViews, 
    date: DispatchPassthroughFormConcreteRenderers.date,
    enumSingleSelection: EnumFieldViewsSingleSelection,
    enumMultiSelection: EnumFieldViewsMultiSelection,
    streamSingleSelection: StreamSingleSelectionFieldViews,
    streamMultiSelection: StreamMultiSelectionFieldViews,
    list: ListFieldViews,
    map: MapFieldViews,
    tuple: TupleFieldViews,
    sum: SumFieldViews,
    sumUnitDate: SumUnitDateFieldViews,
    unit: UnitFieldViews,
    injectedCategory: {
        defaultCategory: () => (props) => {
            return (
                <>
                    {props.context.customPresentationContext?.listElement
                        ?.isLastListElement && <p>Last</p>}
                    {props.context.label && <h3>{props.context.label}</h3>}
                    {props.context.tooltip && <p>{props.context.tooltip}</p>}
                    {props.context.details && (
                        <p>
                            <em>{props.context.details}</em>
                        </p>
                    )}
                    <button
                        style={
                            props.context.value.value.kind == "child"
                                ? { borderColor: "red" }
                                : {}
                        }
                        onClick={(_) =>
                            props.foreignMutations.setNewValue(
                                {
                                    kind: "custom",
                                    value: {
                                        kind: "child",
                                        extraSpecial: false,
                                    },
                                },
                                undefined,
                            )
                        }
                    >
                        child
                    </button>
                    <button
                        style={
                            props.context.value.value.kind == "adult"
                                ? { borderColor: "red" }
                                : {}
                        }
                        onClick={(_) =>
                            props.foreignMutations.setNewValue(
                                {
                                    kind: "custom",
                                    value: {
                                        kind: "adult",
                                        extraSpecial: false,
                                    },
                                },
                                undefined,
                            )
                        }
                    >
                        adult
                    </button>
                    <button
                        style={
                            props.context.value.value.kind == "senior"
                                ? { borderColor: "red" }
                                : {}
                        }
                        onClick={(_) =>
                            props.foreignMutations.setNewValue(
                                {
                                    kind: "custom",
                                    value: {
                                        kind: "senior",
                                        extraSpecial: false,
                                    },
                                },
                                undefined,
                            )
                        }
                    >
                        senior
                    </button>
                </>
            );
        },
    },
    table: DispatchPassthroughFormConcreteRenderers.table,
    record: RecordFieldViews, 
    one: DispatchPassthroughFormConcreteRenderers.one,
    base64File: {},
    secret: {},
    union: UnionFieldViews,
    readOnly: ReadOnlyFieldViews,
};