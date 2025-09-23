import {
    ValueOrErrors,
    DispatchEnumOptionsSources,
} from "ballerina-core";

import {getSeeds} from "../seeds";
import {LocalStorage_SpecName} from "../../domains/storage/local";

const enumApis: DispatchEnumOptionsSources = (enumName: string) => {
    debugger
    const specName = LocalStorage_SpecName.get()!

    const call = getSeeds(specName, enumName, 0, 25);
    return ValueOrErrors.Default.return(
        () => 
            call.then(
                res => {
                    return res.kind == "errors" ? []:
                    res.value.map((_:any) => ({Value: _.value[0].Discriminator}))})
    )
}
export const UnmockingApisEnums = {
    enumApis,
};

