import {
    DispatchInjectablesTypes,
    DispatchInjectedPrimitives
} from "../../runner/domains/abstract-renderers/injectables/state";
import {DispatchParsedType, DispatchTypeName} from "../../deserializer/domains/specification/domains/types/state";
import {List, Map, OrderedMap} from "immutable";
import {Option, Sum} from "ballerina-core"
import {PredicateValue, ValueTuple} from "../../../parser/domains/predicates/state";
import {ValueOrErrors} from "../../../../../collections/domains/valueOrErrors/state";
import {unit} from "../../../../../fun/domains/unit/state";
import {CollectionReference, EnumReference} from "../../../collection/domains/reference/state";
import {DispatchApiConverters} from "../state";
import {ApiValue, extractSelectionItems, isRelationType} from "./adapter";
import {multi} from "./multi";
import {single} from "./single";

export const dispatchToAPIRawValue =
    <T extends DispatchInjectablesTypes<T>>(
        t: DispatchParsedType<T>,
        types: Map<DispatchTypeName, DispatchParsedType<T>>,
        converters: DispatchApiConverters<T>,
        injectedPrimitives?: DispatchInjectedPrimitives<T>,
    ) =>
        (raw: PredicateValue, formState: any): ValueOrErrors<ApiValue[], string> => {


            if (isRelationType(t)) {
                const items = extractSelectionItems(raw, t);
                
                const dispatchInner = (
                    innerType: DispatchParsedType<T>,
                    pv: PredicateValue
                ) =>
                    dispatchToAPIRawValue(
                        innerType,
                        types,
                        converters,
                        injectedPrimitives
                    )(pv, formState);//.Map(x => x.item ?? x);


                if (t.kind === "singleSelection") {
                    if (items.length === 0) {
                        return ValueOrErrors.Operations.Return([{
                            discriminator: "Relation",
                            mode: "single",
                            item: Option.Default.none(),
                        }] satisfies ApiValue[]);
                    }
                    const item = single(t, converters)(raw, formState)
                    return ValueOrErrors.Operations.Return([{
                        discriminator: "Relation",
                        mode: "single",
                        item: Option.Default.some(item),
                    }] satisfies ApiValue[]);
                }
                
                if (t.kind === "multiSelection") {
                    const values = multi(t,converters)(raw, formState)
                    return ValueOrErrors.Operations.Return([{
                        discriminator: "Relation",
                        mode: "multi",
                        items: [values],
                    }] satisfies ApiValue[]);
                }
                
                if (t.kind === "one") {
                    if (items.length === 0) {
                        return ValueOrErrors.Operations.Return([{
                            discriminator: "Relation",
                            mode: "one",
                            item: null,
                        }] satisfies ApiValue[]);
                    }

                    return dispatchInner(t.arg, items[0]).Map(item => ([{
                        discriminator: "Relation",
                        mode: "one",
                        item,
                    }] satisfies ApiValue []));
                }
            }
            const result: ValueOrErrors<any, string> = (() => {
                if (t.kind == "primitive") {
                    if (t.name == "unit") {
                        return ValueOrErrors.Default.return(unit);
                    }
                    return ValueOrErrors.Operations.Return(
                        converters[t.name as string | keyof T].toAPIRawValue([
                            raw,
                            formState?.commonFormState?.modifiedByUser ?? false,
                        ]),
                    );
                }

                if (t.kind == "union") {
                    debugger
                    if (!PredicateValue.Operations.IsUnionCase(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `Union case expected but got ${JSON.stringify(raw)}\n...when converting union to API raw value`,
                        );
                    }
                    const caseName = raw.caseName;
                    if (
                        caseName == undefined ||
                        !PredicateValue.Operations.IsString(caseName)
                    ) {
                        return ValueOrErrors.Default.throwOne(
                            `caseName expected but got ${JSON.stringify(raw)}`,
                        );
                    }
                    const caseType = t.args.get(caseName);
                    if (caseType == undefined) {
                        return ValueOrErrors.Default.throwOne(
                            `union case ${caseName} not found in type ${JSON.stringify(t)}`,
                        );
                    }

                    return dispatchToAPIRawValue(
                        caseType,
                        types,
                        converters,
                        injectedPrimitives,
                    )(raw.fields, formState).Then((value) =>
                        ValueOrErrors.Default.return(
                            converters["union"].toAPIRawValue([
                                { caseName, fields: value },
                                formState?.commonFormState?.modifiedByUser ?? false,
                            ]),
                        ),
                    );
                }
                
                if (t.kind == "list") {
                    if (!PredicateValue.Operations.IsTuple(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `Tuple expected but got list of ${JSON.stringify(raw)}`,
                        );
                    }
                    return ValueOrErrors.Operations.All(
                        List(
                            raw.values.map((value, index) =>
                                dispatchToAPIRawValue(
                                    t.args[0],
                                    types,
                                    converters,
                                    injectedPrimitives,
                                )(value, formState?.elementFormStates?.get(index)),
                            ),
                        ),
                    ).Then((values) =>
                        ValueOrErrors.Default.return(
                            converters["List"].toAPIRawValue([
                                values,
                                formState?.commonFormState?.modifiedByUser ?? false,
                            ]),
                        ),
                    );
                }
                if (t.kind == "map" && t.args.length == 2) {
                    const keyValues = (raw as ValueTuple).values.map((keyValue, index) => {
                        return dispatchToAPIRawValue(
                            t.args[0],
                            types,
                            converters,
                            injectedPrimitives,
                        )(
                            (keyValue as ValueTuple).values.get(0)!,
                            formState?.elementFormStates?.get(index)?.KeyFormState,
                        )
                            .Then((possiblyUndefinedKey) => {
                                if (
                                    possiblyUndefinedKey == undefined ||
                                    possiblyUndefinedKey == null ||
                                    possiblyUndefinedKey == "" ||
                                    (typeof possiblyUndefinedKey == "object" &&
                                        (Object.keys(possiblyUndefinedKey).length == 0 ||
                                            ("IsSome" in possiblyUndefinedKey &&
                                                !possiblyUndefinedKey.IsSome)))
                                ) {
                                    return ValueOrErrors.Default.throwOne(
                                        `A mapped key is undefined for type ${JSON.stringify(
                                            t.args[0],
                                        )}`,
                                    );
                                } else {
                                    return ValueOrErrors.Default.return(possiblyUndefinedKey);
                                }
                            })
                            .Then((key) =>
                                dispatchToAPIRawValue(
                                    t.args[1],
                                    types,
                                    converters,
                                    injectedPrimitives,
                                )(
                                    (keyValue as ValueTuple).values.get(1)!,
                                    formState?.elementFormStates?.get(index)?.ValueFormState,
                                ).Then((value) =>
                                    ValueOrErrors.Default.return([key, value] as [any, any]),
                                ),
                            );
                    });

                    return ValueOrErrors.Operations.All(List(keyValues)).Then((values) => {
                        if (
                            values.map((kv) => JSON.stringify(kv[0])).toSet().size !=
                            values.size
                        ) {
                            return ValueOrErrors.Default.throwOne(
                                "Keys in the map are not unique",
                            );
                        }
                        return ValueOrErrors.Operations.Return(
                            converters["Map"].toAPIRawValue([
                                values,
                                formState?.commonFormState?.modifiedByUser ?? false,
                            ]),
                        );
                    });
                }

                if (t.kind == "sum" && t.args.length === 2) {
                    if (!PredicateValue.Operations.IsSum(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `Sum expected but got ${JSON.stringify(raw)}`,
                        );
                    }

                    return dispatchToAPIRawValue(
                        raw.value.kind == "l" ? t.args[0] : t.args[1],
                        types,
                        converters,
                        injectedPrimitives,
                    )(
                        raw.value.value,
                        raw.value.kind == "l"
                            ? formState?.commonFormState?.left
                            : formState?.commonFormState?.right,
                    ).Then((value) =>
                        ValueOrErrors.Default.return(
                            converters["Sum"].toAPIRawValue([
                                raw.value.kind == "l"
                                    ? Sum.Default.left(value)
                                    : Sum.Default.right(value),
                                formState?.commonFormState?.modifiedByUser ?? false,
                            ]),
                        ),
                    );
                }

                if (t.kind == "sumN") {
                    if (!PredicateValue.Operations.IsSumN(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `SumN expected but got ${JSON.stringify(raw)}`,
                        );
                    }
                    return dispatchToAPIRawValue(
                        t.args[raw.caseIndex],
                        types,
                        converters,
                        injectedPrimitives,
                    )(raw.value, formState?.commonFormState?.modifiedByUser).Then((value) =>
                        ValueOrErrors.Default.return(
                            converters["SumN"].toAPIRawValue([
                                PredicateValue.Default.sumN(raw.caseIndex, raw.arity, value),
                                formState?.commonFormState?.modifiedByUser ?? false,
                            ]),
                        ),
                    );
                }

                if (t.kind == "tuple") {
                    if (!PredicateValue.Operations.IsTuple(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `Tuple expected but got ${JSON.stringify(raw)}`,
                        );
                    }
                    return ValueOrErrors.Operations.All(
                        List(
                            raw.values.map((value, index) => {
                                return dispatchToAPIRawValue(
                                    t.args[index],
                                    types,
                                    converters,
                                    injectedPrimitives,
                                )(value, formState?.itemFormStates?.get(index));
                            }),
                        ),
                    ).Then((values) =>
                        ValueOrErrors.Default.return(
                            converters["Tuple"].toAPIRawValue([
                                values,
                                formState?.commonFormState?.modifiedByUser ?? false,
                            ]),
                        ),
                    );
                }

                if (t.kind == "lookup")
                    return dispatchToAPIRawValue(
                        types.get(t.name)!,
                        types,
                        converters,
                        injectedPrimitives,
                    )(raw, formState);

                if (t.kind == "record") {
                    if (!PredicateValue.Operations.IsRecord(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `Record expected but got ${JSON.stringify(raw)}`,
                        );
                    }
                    const res = [] as any;
                    t.fields.forEach((fieldType, fieldName) => {
                        const rawField = raw.fields.get(fieldName);
                        if (rawField == undefined) {
                            return;
                        }
                        res.push([
                            fieldName,
                            dispatchToAPIRawValue(
                                fieldType,
                                types,
                                converters,
                                injectedPrimitives,
                            )(
                                raw.fields.get(fieldName)!,
                                formState?.fieldStates?.get(fieldName),
                            ),
                        ]);



                    });
                    const errors: ValueOrErrors<
                        List<any>,
                        string
                    > = ValueOrErrors.Operations.All(
                        List(
                            res.map(
                                ([_, value]: [_: string, value: ValueOrErrors<any, string>]) =>
                                    value,
                            ),
                        ),
                    );
                    if (errors.kind == "errors") return errors;

                    const v2 = {
                        discriminator: "record",
                        value: res.map(([fieldName, value]: any) => [
                            {
                                discriminator: "id",
                                value: ["", "", null, fieldName]   // or whatever ID tuple you expect
                            },
                            {
                                discriminator: "string",
                                value: value.value
                            }
                        ])
                    };


                    return ValueOrErrors.Operations.Return(v2
                    );
                }

                // if (t.kind == "one") {
                //     if (!PredicateValue.Operations.IsOption(raw)) {
                //         return ValueOrErrors.Default.throwOne(
                //             `Option expected but got ${JSON.stringify(raw)}\n...when converting one to API raw value`,
                //         );
                //     }
                //
                //     if (!raw.isSome) {
                //         return ValueOrErrors.Default.return(
                //             converters["One"].toAPIRawValue([
                //                 raw,
                //                 formState?.commonFormState?.modifiedByUser ?? false,
                //             ]),
                //         );
                //     }
                //
                //     return dispatchToAPIRawValue(
                //         t.arg,
                //         types,
                //         converters,
                //         injectedPrimitives,
                //     )(raw.value, formState?.commonFormState?.modifiedByUser ?? false).Then(
                //         (value) => {
                //             return ValueOrErrors.Default.return(
                //                 converters["One"].toAPIRawValue([
                //                     PredicateValue.Default.option(true, value),
                //                     formState?.commonFormState?.modifiedByUser ?? false,
                //                 ]),
                //             );
                //         },
                //     );
                // }

                if (t.kind == "readOnly") {
                    if (!PredicateValue.Operations.IsReadOnly(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `ReadOnly expected but got ${JSON.stringify(raw)}`,
                        );
                    }
                    return dispatchToAPIRawValue(
                        t.arg,
                        types,
                        converters,
                        injectedPrimitives,
                    )(raw.ReadOnly, formState).Then((childValue) =>
                        ValueOrErrors.Default.return(
                            converters["ReadOnly"].toAPIRawValue([
                                { ReadOnly: childValue },
                                formState?.commonFormState?.modifiedByUser ?? false,
                            ]),
                        ),
                    );
                }

                if (t.kind == "table") {
                    if (!PredicateValue.Operations.IsTable(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `Table expected but got ${JSON.stringify(raw)}`,
                        );
                    }

                    return ValueOrErrors.Default.return(
                        converters["Table"].toAPIRawValue([
                            raw,
                            formState?.commonFormState?.modifiedByUser ?? false,
                        ]),
                    );
                }

                // Filters
                if (t.kind == "contains") {
                    if (!PredicateValue.Operations.IsFilterContains(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `FilterContains expected but got ${JSON.stringify(raw)}`,
                        );
                    }
                    return dispatchToAPIRawValue(
                        t.contains,
                        types,
                        converters,
                        injectedPrimitives,
                    )(raw.contains, formState).Then((value) =>
                        ValueOrErrors.Default.return(
                            converters["Contains"].toAPIRawValue([
                                PredicateValue.Default.filterContains(value),
                                formState?.commonFormState?.modifiedByUser ?? false,
                            ]),
                        ),
                    );
                }

                if (t.kind == "=") {
                    if (!PredicateValue.Operations.IsFilterEqualsTo(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `FilterEqualsTo expected but got ${JSON.stringify(raw)}`,
                        );
                    }
                    return dispatchToAPIRawValue(
                        t.equalsTo,
                        types,
                        converters,
                        injectedPrimitives,
                    )(raw.equalsTo, formState).Then((value) =>
                        ValueOrErrors.Default.return(
                            converters["="].toAPIRawValue([
                                PredicateValue.Default.filterEqualsTo(value),
                                formState?.commonFormState?.modifiedByUser ?? false,
                            ]),
                        ),
                    );
                }

                if (t.kind == "!=") {
                    if (!PredicateValue.Operations.IsFilterNotEqualsTo(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `FilterNotEqualsTo expected but got ${JSON.stringify(raw)}`,
                        );
                    }
                    return dispatchToAPIRawValue(
                        t.notEqualsTo,
                        types,
                        converters,
                        injectedPrimitives,
                    )(raw.notEqualsTo, formState).Then((value) =>
                        ValueOrErrors.Default.return(
                            converters["!="].toAPIRawValue([
                                PredicateValue.Default.filterNotEqualsTo(value),
                                formState?.commonFormState?.modifiedByUser ?? false,
                            ]),
                        ),
                    );
                }

                if (t.kind == ">=") {
                    if (!PredicateValue.Operations.IsFilterGreaterThanOrEqualsTo(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `FilterGreaterThanOrEqualsTo expected but got ${JSON.stringify(raw)}`,
                        );
                    }
                    return dispatchToAPIRawValue(
                        t.greaterThanOrEqualsTo,
                        types,
                        converters,
                        injectedPrimitives,
                    )(raw.greaterThanOrEqualsTo, formState).Then((value) =>
                        ValueOrErrors.Default.return(
                            converters[">="].toAPIRawValue([
                                PredicateValue.Default.filterGreaterThanOrEqualsTo(value),
                                formState?.commonFormState?.modifiedByUser ?? false,
                            ]),
                        ),
                    );
                }

                if (t.kind == ">") {
                    if (!PredicateValue.Operations.IsFilterGreaterThan(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `FilterGreaterThan expected but got ${JSON.stringify(raw)}`,
                        );
                    }
                    return dispatchToAPIRawValue(
                        t.greaterThan,
                        types,
                        converters,
                        injectedPrimitives,
                    )(raw.greaterThan, formState).Then((value) =>
                        ValueOrErrors.Default.return(
                            converters[">"].toAPIRawValue([
                                PredicateValue.Default.filterGreaterThan(value),
                                formState?.commonFormState?.modifiedByUser ?? false,
                            ]),
                        ),
                    );
                }

                if (t.kind == "!=null") {
                    if (!PredicateValue.Operations.IsFilterIsNotNull(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `FilterIsNotNull expected but got ${JSON.stringify(raw)}`,
                        );
                    }
                    return ValueOrErrors.Default.return(
                        converters["!=null"].toAPIRawValue([
                            raw,
                            formState?.commonFormState?.modifiedByUser ?? false,
                        ]),
                    );
                }

                if (t.kind == "=null") {
                    if (!PredicateValue.Operations.IsFilterIsNull(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `FilterIsNull expected but got ${JSON.stringify(raw)}`,
                        );
                    }

                    return ValueOrErrors.Default.return(
                        converters["=null"].toAPIRawValue([
                            raw,
                            formState?.commonFormState?.modifiedByUser ?? false,
                        ]),
                    );
                }

                if (t.kind == "<=") {
                    if (!PredicateValue.Operations.IsFilterSmallerThanOrEqualsTo(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `FilterSmallerThanOrEqualsTo expected but got ${JSON.stringify(raw)}`,
                        );
                    }
                    return dispatchToAPIRawValue(
                        t.smallerThanOrEqualsTo,
                        types,
                        converters,
                        injectedPrimitives,
                    )(raw.smallerThanOrEqualsTo, formState).Then((value) =>
                        ValueOrErrors.Default.return(
                            converters["<="].toAPIRawValue([
                                PredicateValue.Default.filterSmallerThanOrEqualsTo(value),
                                formState?.commonFormState?.modifiedByUser ?? false,
                            ]),
                        ),
                    );
                }

                if (t.kind == "<") {
                    if (!PredicateValue.Operations.IsFilterSmallerThan(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `FilterSmallerThan expected but got ${JSON.stringify(raw)}`,
                        );
                    }
                    return dispatchToAPIRawValue(
                        t.smallerThan,
                        types,
                        converters,
                        injectedPrimitives,
                    )(raw.smallerThan, formState).Then((value) =>
                        ValueOrErrors.Default.return(
                            converters["<"].toAPIRawValue([
                                PredicateValue.Default.filterSmallerThan(value),
                                formState?.commonFormState?.modifiedByUser ?? false,
                            ]),
                        ),
                    );
                }

                if (t.kind == "startsWith") {
                    if (!PredicateValue.Operations.IsFilterStartsWith(raw)) {
                        return ValueOrErrors.Default.throwOne(
                            `FilterStartsWith expected but got ${JSON.stringify(raw)}`,
                        );
                    }
                    return dispatchToAPIRawValue(
                        t.startsWith,
                        types,
                        converters,
                        injectedPrimitives,
                    )(raw.startsWith, formState).Then((value) =>
                        ValueOrErrors.Default.return(
                            converters["StartsWith"].toAPIRawValue([
                                PredicateValue.Default.filterStartsWith(value),
                                formState?.commonFormState?.modifiedByUser ?? false,
                            ]),
                        ),
                    );
                }

                return ValueOrErrors.Default.throwOne(
                    `Unsupported type ${JSON.stringify(t)}`,
                );
            })();
            return result.MapErrors((errors) =>
                errors.map(
                    (error) =>
                        `${error}\n...When converting type ${JSON.stringify(
                            t,
                            null,
                            2,
                        )} and value ${JSON.stringify(raw, null, 2)} to API raw value`,
                ),
            );
        };