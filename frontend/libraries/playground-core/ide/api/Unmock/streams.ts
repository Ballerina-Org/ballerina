import {
    DispatchInfiniteStreamSources, ValueOrErrors,
    SearchableInfiniteStreamState, OrderedMapRepo, SearchableInfiniteStreamAbstractRendererState
} from "ballerina-core";
import {getSeeds} from "../seeds";
import {OrderedMap} from "immutable";
import {LocalStorage_SpecName} from "../../domains/storage/local";

const streamApis: DispatchInfiniteStreamSources = (streamName: string) => {
    const call  = 
        (searchText: string, index: number, size: number)
        : Promise<{data: OrderedMap<any, any>, hasMoreValues: boolean}> => {
            debugger

            return getSeeds(LocalStorage_SpecName.get()!, streamName, index, size)
                .then((res) =>

                    res.kind == "errors" ? ({
                        data: OrderedMapRepo.Default.fromIdentifiables([]),
                        hasMoreValues: false
                    }) : ({
                        data: OrderedMapRepo.Default.fromIdentifiables(
                            res.value
                                .map((x: any) => x.value)
                                .filter((x: any) =>
                                    !searchText.trim() ?
                                        x
                                        :
                                        searchText.toLowerCase().includes(x.toLowerCase()))),
                        hasMoreValues: true,
                    })
                );
        }
    
    return ValueOrErrors.Default.return( 
        (searchText) =>
            ([streamPosition]) => 
                call(searchText, streamPosition.chunkIndex, streamPosition.chunkSize));
}

export const UnmockingApisStreams = {
    streamApis,
};
