import {
    CollectionReference, DeserializedDispatchSpecification, DispatchLookupSources,
    Errors,
    Guid,
    Identifiable, LookupApis, SpecificationApis,
    Unit,
    ValidationResult,
    Value,
    ValueOrErrors
} from "ballerina-core";
import axios from "axios";
import {
    DispatchPassthroughFormInjectedTypes
} from "web/src/domains/dispatched-passthrough-form/injected-forms/category";
import {
    DispatchPassthroughFormCustomPresentationContext, DispatchPassthroughFormExtraContext,
    DispatchPassthroughFormFlags
} from "web/src/domains/dispatched-passthrough-form/views/concrete-renderers";

const BASE_URL = "http://localhost:5021";

export type GetLookupResponse ={
    values: any []
    hasMore: boolean
}

// export async function getLookup(specName: string, lookupName: string, source: Guid)
//     : Promise<ValueOrErrors<any, Errors<string>>> {
//    
//     const url = `${BASE_URL}/lookup/${lookupName}/${source}?skip=0&take=1`
//    
//     const result =
//         await axios.get<ValueOrErrors<GetLookupResponse, Errors<string>>>(url, {
//                 headers: {
//                     "X-Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd",
//                     "X-Spec-Id": specName
//                 },
//             });
//     return result.data;
// }

