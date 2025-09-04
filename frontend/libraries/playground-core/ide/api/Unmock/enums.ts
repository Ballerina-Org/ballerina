import { faker } from "@faker-js/faker";
import {
    PromiseRepo,
    EntityApis,
    unit,
    Guid,
    DispatchInfiniteStreamSources,
    ValueOrErrors,
    DispatchEnumOptionsSources,
    DispatchTableApiSource,
    BasicFun,
    PredicateValue,
    ValueStreamPosition,
    DispatchTableApiSources,
    DispatchOneSource,
    DispatchLookupSources,
    TableAbstractRendererState,
    DispatchTableFiltersAndSorting,
    SumNType,
    DispatchParsedType,
    Value,
    ValueFilter,
} from "ballerina-core";
import { Range, Map, List } from "immutable";
import {all} from "axios";


const permissions = ["Create", "Read", "Update", "Delete"];
const colors = ["Red", "Green", "Blue"];
const genders = ["M", "F", "X", "P"];
const interests = ["Soccer", "Hockey", "BoardGames", "HegelianPhilosophy"];

const allergy = ["Soy","Peanuts","Eggs","CowMilk"];

const enumApis: DispatchEnumOptionsSources = (enumName: string) => {

    return enumName == "genders"
        ? ValueOrErrors.Default.return(() => {
          
                return PromiseRepo.Default.mock(
                    () => genders.map((_) => ({Value: _})),
                    undefined,
                    1,
                    0,
                )
            },
        )
        : (enumName == "allergy"
            ? ValueOrErrors.Default.return(() => {
            
                    return PromiseRepo.Default.mock(
                        () => allergy.map((_) => ({Value: _})),
                        undefined,
                        1,
                        0,
                    )
                },
            )
            : ValueOrErrors.Default.throwOne(
                `Cannot find enum API ${enumName}`,
            ));
}

export const UnmockingApisEnums = {

    enumApis,

};
//
