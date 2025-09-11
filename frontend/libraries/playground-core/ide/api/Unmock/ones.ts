import {
    BasicFun,
    DispatchLookupSources,
    Guid,
    PredicateValue,
    TableAbstractRendererState,
    ValueOrErrors,
    ValueStreamPosition,
} from "ballerina-core";
import {Map} from "immutable";
import {getLookup} from "../seeds";

const lookupSources: DispatchLookupSources = (typeName: string) =>{

    return ValueOrErrors.Default.return({
            one: (apiName: string) =>
                ValueOrErrors.Default.return(
                    {
                        get: (id: Guid) => {
                            const fieldName = apiName.replace(/Api$/, "");
                            return getLookup("sample",fieldName, id, 0, 1).then(valueOrErrors => {
                                return valueOrErrors.kind == "value" ? valueOrErrors.value.values[0] : undefined
                            });
                        },
                        getManyUnlinked:
                            (fromApiRaw: BasicFun<any, ValueOrErrors<PredicateValue, string>>) =>
                                (id: Guid) =>
                                    (streamParams: Map<string, string>) =>
                                        ([streamPosition]: [ValueStreamPosition]) => {
                                            const fieldName = apiName.replace(/Api$/, "");
                                            const call = 
                                                getLookup("sample",fieldName, id, streamPosition.chunkIndex || 0, streamPosition.chunkSize || 2)
                                                    .then( valueOrErrors => ({
                                                        Values: valueOrErrors.kind == "value" ? valueOrErrors.value : [],
                                                        HasMore: false,//TODO
                                                        From: 1,
                                                        To: 5,
                                                    }))
                                            return call
 
                                            .then((res) => (

                                                {
                                                    hasMoreValues: res.HasMore,
                                                    to: res.To,
                                                    from: res.From,
                                                    data: TableAbstractRendererState.Operations.tableValuesToValueRecord(
                                                        res.Values,
                                                        fromApiRaw,
                                                    ),
                                                }
                                            ));
                                        },
                    }
                )
        })};
export const UnmockingApisLookups = {
    lookupSources,
};

