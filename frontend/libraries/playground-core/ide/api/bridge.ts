import { Unit, ValueOrErrors} from "ballerina-core";
import axios from "axios";

const BASE_URL = "http://localhost:5021";

export async function getSpecs(name: string): Promise<ValueOrErrors<string[], string>> {
    const response =
        await axios.get<ValueOrErrors<string[], string>>(`${BASE_URL}/bridge`,{
            headers: {
                "X-Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd",
                "X-Spec-Id": name
            }
        });
    return response.data;
}

export async function validate(specName: string, launcher: string): Promise<ValueOrErrors<Unit, string>> {
    const response =
        await axios.post<ValueOrErrors<Unit, string>>(`${BASE_URL}/bridge-validate/launcher/${launcher}`,{},{
            headers: {
                "X-Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd",
                "X-Spec-Id": specName
            }
        });
    return response.data
}

// export async function validateV1(specName: string, v1: SpecSource): Promise<ValueOrErrors<string[], string>> {
//     const response =
//         await axios.post<ValueOrErrors<string[], string>>(`${BASE_URL}/bridge-validate/v1body`,v1.specBody.value,{
//             headers: {
//                 "X-Tenant-Id": "c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd",
//                  "X-Spec-Id": specName
//             }
//         });
//     return response.data
// }