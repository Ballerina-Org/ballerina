import {
    CollectionReference,
    Errors,
    Identifiable,
    ParsedFormJSON,
    Unit,
    ValidationResult,
    Value,
    ValueOrErrors
} from "ballerina-core";
import axios, {AxiosRequestConfig} from "axios";
import {axiosVOE} from "./api";
import {FullSpec, V1, V2, VSpec} from "../state";



export const listSpecs = async () =>
    await axiosVOE<string[]>({
        method: "GET",
        url: "/specs",
    });

export const getSpec = async (name: string) =>
    await axiosVOE<FullSpec>({
        method: "GET",
        url: `/specs/${name}`,
    });

export const seed = async (name: string) =>
    await axiosVOE<any>({
        method: "PUT",
        url: `/specs/${name}/seed`,
    });

export const update = async (name: string, vspec: VSpec) =>
    await axiosVOE<any>({
        method: "PUT",
        data: vspec,
        url: `/specs/${name}`,
    });
//
// export async function listSpecs(): Promise<ValueOrErrors<string[], string>> {
//     const response = await axios.get<ValueOrErrors<string[], string>>(`${BASE_URL}/specs`,{
//         headers: {
//             "Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd"
//         }
//     });
//     return response.data;
// }
// export async function getSpec(name: string): Promise<ValueOrErrors<string[], string>> {
//     const response = await axios.get<ValueOrErrors<string[], string>>(`${BASE_URL}/bridge`,{
//         headers: {
//             "Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd",
//             "Spec-Id": name
//         }
//     });
//     return response.data;
// }
//
//
// export async function createSpec(spec: V2): Promise<ValueOrErrors<Unit, Errors<string>>> {
//     const result = await axios.post<ValueOrErrors<Unit, Errors<string>>>(`${BASE_URL}/spec`, spec);
//     return result.data;
// }
//
//
// export async function updateSpec(spec: V2): Promise<ValueOrErrors<Unit, Errors<string>>> {
//     const result = await axios.put<ValueOrErrors<Unit, Errors<string>>>(`${BASE_URL}/spec`, spec, {
//         headers: {
//             "X-Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd"
//         },
//
//     });
//     return result.data;
// }
//
//
// export async function updateBridge(v1: V1, spec: V2): Promise<ValueOrErrors<Unit, Errors<string>>> {
//     const result = await axios.put<ValueOrErrors<Unit, Errors<string>>>(`${BASE_URL}/bridge`, {v1: v1, v2: spec}, {
//         headers: {
//             "X-Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd",
//             "X-Spec-Id": spec.name
//         },
//     });
//     return result.data;
// }
//
// export async function reseed(specName: string, spec: any): Promise<ValueOrErrors<Unit, Errors<string>>> {
//     const result = await axios.put<ValueOrErrors<Unit, Errors<string>>>(`${BASE_URL}/spec/seeds/${specName}`, spec,{
//         headers: {
//             "X-Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd","X-Spec-Id": specName
//         },
//
//     });
//     return result.data;
// }
//
// export async function getSeed(name: string): Promise<ValueOrErrors<any, string>> {
//     const response = await axios.get<ValueOrErrors<any, string>>(`${BASE_URL}/spec/seeds/${name}`,{
//         headers: {
//             "X-Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd"
//         }
//     });
//     return response.data;
// }
