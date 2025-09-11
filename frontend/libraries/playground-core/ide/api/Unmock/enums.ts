import {
    ValueOrErrors,
    DispatchEnumOptionsSources,
} from "ballerina-core";

import {getEnums} from "../seeds";

const enumApis: DispatchEnumOptionsSources = (enumName: string) => {
    const call = getEnums("sample", enumName, 0, 11);
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

