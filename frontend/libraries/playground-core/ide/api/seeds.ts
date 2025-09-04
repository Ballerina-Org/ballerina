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

export async function updateEntity(specName: string, entityName: string, entityId: string, change: any)
    : Promise<ValueOrErrors<Unit, Errors<string>>> {

    const delta  = (value: any) => ({
        "kind": "replace",
        "replace": value})

    const result =
        await axios.put<ValueOrErrors<Unit, Errors<string>>>(
            `${BASE_URL}/entity/${entityName}/${entityId}`, delta(change), {
                headers: {
                    "X-Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd",
                    "X-Spec-Id": specName
                },
            });
    return result.data;
}

export async function getSeedEntity(specName: string, entityName: string)
    : Promise<ValueOrErrors<any, Errors<string>>> {


    
    const result =
        await axios.get<ValueOrErrors<any, Errors<string>>>(
            `${BASE_URL}/entity/many/${entityName}?skip=0&take=1`, {
                headers: {
                    "X-Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd",
                    "X-Spec-Id": specName
                },
            });
    return result.data;
}

export async function getLookup(specName: string, entityName: string, id: Guid, skip: number, take: number)
    : Promise<ValueOrErrors<any, Errors<string>>> {

    const result =
        await axios.get<ValueOrErrors<any, Errors<string>>>(
            `${BASE_URL}/lookup/${entityName}/${id}/unlinked?skip=${skip}&take=${take}`, {
                headers: {
                    "X-Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd",
                    "X-Spec-Id": specName
                },
            });
    return result.data;
}
export async function getStreams(specName: string, entityName: string, skip: number, take: number)
    : Promise<ValueOrErrors<any, Errors<string>>> {

    const result =
        await axios.get<ValueOrErrors<{ value: CollectionReference[]}, Errors<string>>>(
            `${BASE_URL}/entity/many/${entityName}?skip=${skip}&take=${take}`, {
                headers: {
                    "X-Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd",
                    "X-Spec-Id": specName
                },
            });
    return result.data;
}
export async function getSeedEntityId(specName: string, entityName: string, id: Guid)
    : Promise<ValueOrErrors<any, Errors<string>>> {

    const result =
        await axios.get<ValueOrErrors<any, Errors<string>>>(
            `${BASE_URL}/entity/${entityName}/${id}?skip=0&take=1`, {
                headers: {
                    "X-Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd",
                    "X-Spec-Id": specName
                },
            });
    return result.data;
}
