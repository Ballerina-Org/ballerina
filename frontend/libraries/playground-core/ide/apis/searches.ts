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

export const IdeSearchesApi = {

    async get(specName: string, apiName: string, page: number, size: number): Promise<ValidationResultWithPayload<CollectionReference[]>>{
    
        const response = 
          await fetch( //search/ref?specName=Persona&apiName=cities
            `${url}/search/${apiName}/many?specName=${specName}&page=${page}&size=${size}`, get);

        if (!response.ok) {
            throw new Error(`get enums api HTTP error (status: ${response.status})" ${response.statusText}`);
        }
   
        const result: CollectionReference [] =  await response.json();
        debugger
        const validated = Object.assign("valid", { payload: result }) as "valid" & { payload: CollectionReference[] }

        return validated;
    },
};