import {DispatchLookupSources, DispatchTypeName, LookupApiName, LookupApiOne, LookupApis} from "ballerina-core";

export const findByDispatchType = (
    entity: string,
    apis: LookupApis,
    wanted: DispatchTypeName
): { 
    ones: Array<{ apiName: LookupApiName; key: string; entry: { type: DispatchTypeName; methods: any } }>,
    streams: Array<{ apiName: LookupApiName; key: string; entry: { type: DispatchTypeName} }>} =>
 {
     debugger
    const onesResult: Array<{ apiName: LookupApiName; key: string; entry: any }> = [];
    const streamsResult: Array<{ apiName: LookupApiName; key: string; entry: any }> = [];
    apis.forEach(({ one, streams }, apiName) => {
        one.forEach((entry, key) => {
            if (entry.type === wanted) {
                onesResult.push({ apiName, key, entry });
            }
        });
        if (streams !== undefined) {
            streamsResult.push({ apiName, key: entity, entry: wanted });
        }

    });

    return { ones: onesResult, streams: streamsResult};
}

export const getStreamEntities = (apis:LookupApis) =>
    apis.map(({ streams }) => streams)
        .filter(streams => streams !== undefined)
        .map((name, value) => value).toArray();
