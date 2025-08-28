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

import { AddressApi } from "../../../person/domains/address/apis/mocks";
import { v4 } from "uuid";
import { PersonApi } from "../../../person/apis/mocks";

const permissions = ["Create", "Read", "Update", "Delete"];
const colors = ["Red", "Green", "Blue"];
const genders = ["M", "F", "X"];
const interests = ["Soccer", "Hockey", "BoardGames", "HegelianPhilosophy"];




const streamApis: DispatchInfiniteStreamSources = (streamName: string) =>
    streamName == "departments"
        ? ValueOrErrors.Default.return(PersonApi.getDepartments())
        : streamName == "cities"
            ? ValueOrErrors.Default.return(AddressApi.getCities())
            : ValueOrErrors.Default.throwOne(`Cannot find stream API ${streamName}`);



export const UnmockingApisStreams = {
    streamApis,

};
//
