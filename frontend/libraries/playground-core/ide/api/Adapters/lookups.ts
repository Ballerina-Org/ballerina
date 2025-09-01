import {DispatchLookupSources, DispatchTypeName, LookupApiName, LookupApiOne, LookupApis} from "ballerina-core";

export function findByDispatchType(
    apis: LookupApis,
    wanted: DispatchTypeName
): Array<{ apiName: LookupApiName; key: string; entry: { type: DispatchTypeName; methods: any } }> {
    const results: Array<{ apiName: LookupApiName; key: string; entry: any }> = [];

    apis.forEach(({ one }, apiName) => {
        one.forEach((entry, key) => {
            if (entry.type === wanted) {
                results.push({ apiName, key, entry });
            }
        });
    });

    return results;
}

    // if(lookupSources){
    //     const one = lookupSources(entityName)
    //     if(one.kind == "value" && one.value.one){
    //         const call = one.value.one(entityName + "Api");
    //         if(call.kind == "value"){
    //             const source = call.value.getManyUnlinked()
    //         }
    //     }
    // }