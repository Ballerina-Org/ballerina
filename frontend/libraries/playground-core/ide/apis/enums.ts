import {CollectionReference, Identifiable, Unit, ValidationResult, Value} from "ballerina-core";
import {SpecValidationResult, ValidationResultWithPayload} from "../domains/spec-editor/state";

//TODO: move to axios

const url = "https://localhost:7005"

const get = {
    method: "GET",
    headers: {
        "Content-Type": "application/json",
        "Accept": "application/json"
    }
} 

const post = (body: string) => ({
  method: "POST",
  headers: {
    "Content-Type": "application/json",
      "Accept": "application/json"
    },
    body: body
  });

export const IdeEnumsApi = {

    async get(specName: string,apiName: string): Promise<ValidationResultWithPayload<string[]>>{
    
        const response = 
          await fetch(
            `${url}/enum?specName=${specName}&enumApiName=${apiName}`, get);

        if (!response.ok) {
            throw new Error(`get enums api HTTP error (status: ${response.status})" ${response.statusText}`);
        }
   
        const result: { kind: string, elements: string []} =  await response.json();
        const validated = Object.assign("valid", { payload: result.elements }) as "valid" & { payload: string[] }

        return validated;
    },
};