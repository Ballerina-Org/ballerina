import {DispatchLookupSources, DispatchTypeName, LookupApiName, LookupApiOne, LookupApis} from "ballerina-core";

export function findByDispatchType<
    K extends keyof LookupApis,
    E extends { type: DispatchTypeName }
>(
    apis: LookupApis,
    fieldName: K,
    wanted: DispatchTypeName
): Array<{ apiName: LookupApiName; key: string; entry: E }> {
    const results: Array<{ apiName: LookupApiName; key: string; entry: E }> = [];

    apis.forEach((api, apiName) => {
        const map = api[fieldName] as Map<string, E>;
        map.forEach((entry, key) => {
            if (entry.type === wanted) {
                results.push({ apiName, key, entry });
            }
        });
    });

    return results;
}

