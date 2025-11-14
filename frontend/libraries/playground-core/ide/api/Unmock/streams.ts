import {
    DispatchInfiniteStreamSources, ValueOrErrors,
    SearchableInfiniteStreamState, OrderedMapRepo, SearchableInfiniteStreamAbstractRendererState, PromiseRepo,
    CollectionReference
} from "ballerina-core";
import {getSeeds} from "../seeds";
import {OrderedMap, Range} from "immutable";
import {LocalStorage_SpecName} from "../../domains/storage/local";
import {Department} from "../../../person/state";
import {v4} from "uuid";
import {faker} from "@faker-js/faker";

const inventory = {
    getData:
        (streamName: string): SearchableInfiniteStreamState["customFormState"]["getChunk"] =>
            (searchText) =>
                ([streamPosition]) =>{
                 
                    return getSeeds(LocalStorage_SpecName.get()!, streamName, streamPosition.chunkIndex, streamPosition.chunkSize)
                        .then((res) => {
                         
                                return res.kind == "errors" ? ({
                                        data: OrderedMapRepo.Default.fromIdentifiables([]),
                                        hasMoreValues: false
                                    }) :
                                    ({
                                        data:
                                            OrderedMapRepo.Default.fromIdentifiables(
                                                res.value
                                                    .map(x => ({ Id: x.id, DisplayValue: x.value.DisplayValue }))
                                                    //.map(x => x.value)

                                                    .filter((x: CollectionReference) => {
                                                    
                                                        return x.DisplayValue.toLowerCase().includes(searchText)
                                                    })),
                                        hasMoreValues: true //res.value.length === size
                                    })
                            }
                        )},
}

const streamApis: DispatchInfiniteStreamSources = (streamName: string) => {
    return ValueOrErrors.Default.return(inventory.getData(streamName));
}

export const UnmockingApisStreams = {
    streamApis,
};
