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
    ValueFilter, SearchableInfiniteStreamState, OrderedMapRepo, Errors,
} from "ballerina-core";
import { Range, Map, List } from "immutable";

import { AddressApi } from "../../../person/domains/address/apis/mocks";
import { v4 } from "uuid";
import { PersonApi } from "../../../person/apis/mocks";
import {getStreams} from "../seeds";
import {City} from "../../../person/domains/address/state";

const permissions = ["Create", "Read", "Update", "Delete"];
const colors = ["Red", "Green", "Blue"];
const genders = ["M", "F", "X"];
const interests = ["Soccer", "Hockey", "BoardGames", "HegelianPhilosophy"];


const Api = {
    process:
        (res:Promise<ValueOrErrors<any, Errors<string>>>): SearchableInfiniteStreamState["customFormState"]["getChunk"] =>

            
   (_searchText) =>
                (_streamPosition) =>
                    res.then ((res) =>
                    {
                        console.log("****************************")
                        return res.kind == "errors" ? ({data: OrderedMapRepo.Default.fromIdentifiables([]), hasMoreValues: false}) : ({
                        data: OrderedMapRepo.Default.fromIdentifiables(res.value.value),
                        hasMoreValues: true,
                    })}),
};
const streamApis: DispatchInfiniteStreamSources = (streamName: string) =>
    ValueOrErrors.Default.return(Api.process(getStreams("sample",streamName, 0, 11)));




export const UnmockingApisStreams = {
    streamApis,

};
//
