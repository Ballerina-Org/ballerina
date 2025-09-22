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
import {getLookups} from "../seeds";
import {LocalStorage_SpecName} from "../../domains/storage/local";

const lookupSources: DispatchLookupSources = (typeName: string) =>{

    return ValueOrErrors.Default.return({
            one: (apiName: string) =>
                ValueOrErrors.Default.return(
                    {
                        get:async (id: Guid) => {
                            const fieldName = apiName.replace(/Api$/, "");
                            
                            const check =
                                await getLookups(LocalStorage_SpecName.get()!,fieldName, id, 0, 1).then(valueOrErrors => {
                              
                                    if (valueOrErrors.kind == "value" && valueOrErrors.value.values.length == 0) {
                                    return ValueOrErrors.Default.throwOne("Lookup sources has no matched entity");
                                }
                                return valueOrErrors.kind == "value" ? (valueOrErrors.value.values as any)[0] : {}
                            });
                            
                            return check;
                        },
                        getManyUnlinked:
                            (fromApiRaw: BasicFun<any, ValueOrErrors<PredicateValue, string>>) =>
                                (id: Guid) =>
                                    (streamParams: Map<string, string>) =>
                                        ([streamPosition]: [ValueStreamPosition]) => {
               
                                            const fieldName = apiName.replace(/Api$/, "");
                                            const call = 
                                                getLookups(LocalStorage_SpecName.get()!,fieldName, id, streamPosition.chunkIndex || 0, streamPosition.chunkSize || 2)
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

