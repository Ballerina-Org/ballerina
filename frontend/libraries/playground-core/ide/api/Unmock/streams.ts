import {
    DispatchInfiniteStreamSources, ValueOrErrors,
    SearchableInfiniteStreamState, OrderedMapRepo, SearchableInfiniteStreamAbstractRendererState
} from "ballerina-core";
import {getStreams} from "../seeds";
import {OrderedMap} from "immutable";

const streamApis: DispatchInfiniteStreamSources = (streamName: string) => {
    const call  = (searchText: string, index: number, size: number): Promise<{data: OrderedMap<any, any>, hasMoreValues: boolean}> =>
        getStreams("sample", streamName, index, size).then ((res) =>
        {
            console.log(`${JSON.stringify(res)}`)
            return res.kind == "errors" ? ({data: OrderedMapRepo.Default.fromIdentifiables([]), hasMoreValues: false}) : ({
                data: OrderedMapRepo.Default.fromIdentifiables( 
                    res.value.map((x: any) => x.value).filter((x:any) => !searchText.trim() ? x:searchText.toLowerCase().includes(x.toLowerCase()))),
                hasMoreValues: true,
            })
        });
    const r:  ValueOrErrors<
        SearchableInfiniteStreamAbstractRendererState["customFormState"]["getChunk"],
        string
    > = ValueOrErrors.Default.return( 
        (searchText) =>
            ([streamPosition]) => 
                call(searchText, streamPosition.chunkIndex, streamPosition.chunkSize));
    return r

}

export const UnmockingApisStreams = {
    streamApis,
};
