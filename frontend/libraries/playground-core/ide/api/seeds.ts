import {
    CollectionReference,
    Errors,
    Guid,
    Identifiable,
    Unit,
    ValidationResult,
    Value,
    ValueOrErrors
} from "ballerina-core";
import axios from "axios";

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

export async function getSeedEntityUnlinked(specName: string, entityName: string, id: Guid)
    : Promise<ValueOrErrors<any, Errors<string>>> {

    const result =
        await axios.get<ValueOrErrors<any, Errors<string>>>(
            `${BASE_URL}/entity/unlinked/${entityName}/${id}?skip=0&take=2`, {
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
            `${BASE_URL}/entity/${entityName}?skip=0&take=1`, {
                headers: {
                    "X-Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd",
                    "X-Spec-Id": specName
                },
            });
    return result.data;
}
