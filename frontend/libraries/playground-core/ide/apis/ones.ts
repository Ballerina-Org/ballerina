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

export const IdeOnesApi = {

    async getMany(specName: string, apiName: string): Promise<ValidationResultWithPayload<any[]>>{
      // const response =
      //   await fetch(
      //     `${url}/table/${apiName}/many?specName=${specName}&page=${0}&size=${10}`, get);    
        const response = 
          await fetch(
            `${url}/one/${apiName}/many?specName=${specName}&page=${0}&size=${3}`, get);

        if (!response.ok) {
            throw new Error(`get tables api HTTP error (status: ${response.status})" ${response.statusText}`);
        }
   
        const result: { kind: string, elements: string []} =  await response.json();
        const validated = Object.assign("valid", { payload: result.elements }) as "valid" & { payload: any[] }

        return validated;
    },
  async get(specName: string, apiName: string): Promise<ValidationResultWithPayload<any>>{

    const response =
      await fetch(
        `${url}/table/${apiName}?specName=${specName}`, get);

    if (!response.ok) {
      throw new Error(`get tables api HTTP error (status: ${response.status})" ${response.statusText}`);
    }

    const result: { kind: string, elements: string []} =  await response.json();
    const validated = Object.assign("valid", { payload: result.elements }) as "valid" & { payload: any }

    return validated;
  },
};