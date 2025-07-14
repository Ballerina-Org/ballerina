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

export const IdeTablesApi = {

    async getMany(specName: string, page: number, size: number, apiName: string): Promise<ValidationResultWithPayload<any[]>>{
    
        const response = 
          await fetch(
            `${url}/table/${apiName}/many?specName=${specName}&page=${page}&size=${size}`, get);

        if (!response.ok) {
            throw new Error(`get tables api HTTP error (status: ${response.status})" ${response.statusText}`);
        }
   
        const result: { kind: string, elements: string []} =  await response.json();
        return Object.assign("valid", { payload: result.elements }) as "valid" & { payload: any[] }
    },
    async get(specName: string, apiName: string): Promise<ValidationResultWithPayload<any>>{
  
      const response =
        await fetch(
          `${url}/table?specName=${specName}&apiName=${apiName}`, get);
  
      if (!response.ok) {
        throw new Error(`get tables api HTTP error (status: ${response.status})" ${response.statusText}`);
      }
  
      const result: { kind: string, elements: string []} =  await response.json();
      return Object.assign("valid", { payload: result.elements }) as "valid" & { payload: any }
    },
};