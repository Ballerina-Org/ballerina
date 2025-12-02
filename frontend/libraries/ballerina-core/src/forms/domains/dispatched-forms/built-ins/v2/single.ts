import {
    DispatchInjectablesTypes
} from "../../runner/domains/abstract-renderers/injectables/state";
import {DispatchParsedType} from "../../deserializer/domains/specification/domains/types/state";
import {Sum, Option} from "ballerina-core"
import {PredicateValue} from "../../../parser/domains/predicates/state";
import {ValueOrErrors} from "../../../../../collections/domains/valueOrErrors/state";
import {CollectionReference, EnumReference} from "../../../collection/domains/reference/state";
import {DispatchApiConverters} from "../state";
import {ApiValue} from "./adapter";

/*
* 
* this is extracted implementation of singleSelection with the only difference to return item embedded in ApiValue
* 
* */

export const single = <T extends DispatchInjectablesTypes<T>>(
    t: DispatchParsedType<T>,
    converters: DispatchApiConverters<T>
) =>
    (raw: PredicateValue, formState: any): ValueOrErrors<ApiValue, string> => {
        if (t.kind == "singleSelection") {
            if (!PredicateValue.Operations.IsOption(raw)) {
                return ValueOrErrors.Default.throwOne(
                    `Option expected but got ${JSON.stringify(raw)}`,
                );
            }

            if (raw.isSome) {
                if (!PredicateValue.Operations.IsRecord(raw.value)) {
                    return ValueOrErrors.Default.throwOne(
                        `Record expected but got ${JSON.stringify(raw.value)}`,
                    );
                }
                const rawValue = raw.value.fields.toJS();
                if (
                    !CollectionReference.Operations.IsCollectionReference(rawValue) &&
                    !EnumReference.Operations.IsEnumReference(rawValue)
                ) {
                    return ValueOrErrors.Default.throwOne(
                        `CollectionReference or EnumReference expected but got ${rawValue}`,
                    );
                }

                return ValueOrErrors.Operations.Return(
                    converters["SingleSelection"].toAPIRawValue([
                        Sum.Default.left(rawValue),
                        formState?.commonFormState?.modifiedByUser ?? false,
                    ]),
                );
            } else {
                return ValueOrErrors.Operations.Return(
                    converters["SingleSelection"].toAPIRawValue([
                        Sum.Default.right("no selection"),
                        formState?.commonFormState?.modifiedByUser ?? false,
                    ]).Map((value:any) => ({ discriminator: "Relation", mode: 'single', item: Option.Default.some(value)} satisfies ApiValue)),
                );
            }
        }
    }