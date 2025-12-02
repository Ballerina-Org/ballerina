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

/*
* 
* this is extracted the implementation of multiselection with the only difference to return item embedded in ApiValue
* 
* */

export const multi = <T extends DispatchInjectablesTypes<T>>(
    t: DispatchParsedType<T>,
    converters: DispatchApiConverters<T>
) =>
    (raw: PredicateValue, formState: any): ValueOrErrors<ApiValue, string> => {
        if (t.kind == "multiSelection") {
            if (!PredicateValue.Operations.IsRecord(raw)) {
                return ValueOrErrors.Default.throwOne(
                    `Record expected but got multi selection of ${JSON.stringify(raw)}`,
                );
            }

            const filteredRawValues = raw.fields.filter((value) => {
                if (!PredicateValue.Operations.IsRecord(value)) {
                    console.warn(
                        "Received a non-record value in a multi selection, ignoring: ",
                        JSON.stringify(value),
                    );
                    return false;
                }

                const fieldsObject = value.fields.toJS();

                if (
                    !CollectionReference.Operations.IsCollectionReference(
                        fieldsObject,
                    ) &&
                    !EnumReference.Operations.IsEnumReference(fieldsObject)
                ) {
                    console.warn(
                        "Received a non-collection or enum reference value in a multi selection, ignoring: ",
                        JSON.stringify(value),
                    );
                    return false;
                }
                return true;
            });

            const rawValue: Map<
                string,
                ValueOrErrors<CollectionReference | EnumReference, string>
            > = filteredRawValues.map((value) => {
                // should never happen due to the filter above but is a type check
                if (!PredicateValue.Operations.IsRecord(value)) {
                    return ValueOrErrors.Default.throwOne(
                        `Record expected but got ${JSON.stringify(value)}`,
                    );
                }
                const fieldsObject = value.fields.toJS();

                if (
                    !CollectionReference.Operations.IsCollectionReference(
                        fieldsObject,
                    ) &&
                    !EnumReference.Operations.IsEnumReference(fieldsObject)
                ) {
                    return ValueOrErrors.Default.throwOne(
                        `CollectionReference or EnumReference expected but got ${JSON.stringify(
                            fieldsObject,
                        )}`,
                    );
                }

                return ValueOrErrors.Default.return(fieldsObject);
            });

            const items: ValueOrErrors<ApiValue, any> = ValueOrErrors.Operations.All(rawValue.valueSeq().toList()).Then(
                (values) =>
                    ValueOrErrors.Default.return(
                        converters["MultiSelection"].toAPIRawValue([
                            OrderedMap<string, EnumReference | CollectionReference>(
                                values
                                    .map((v): [string, EnumReference | CollectionReference] => {
                                        if (
                                            CollectionReference.Operations.IsCollectionReference(v)
                                        ) {
                                            return [v.Id, v];
                                        }
                                        return [v.Value, v];
                                    })
                                    .toArray(),
                            ),
                            formState?.commonFormState?.modifiedByUser ?? false,
                        ]),
                    ).Map(values => ({ discriminator: 'Relation', mode: 'multi', items: values} satisfies ApiValue)),
            );
            return items;
        }
    }
