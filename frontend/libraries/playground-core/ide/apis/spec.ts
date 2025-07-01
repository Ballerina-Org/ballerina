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

export const IDEApi = {
    async validateSpec(spec: Value<string>): Promise<SpecValidationResult> {

        const response = 
          await fetch(
            `${url}/spec/validate`, 
            post(JSON.stringify({ specBody: spec.value })));

        if (!response.ok) {
            throw new Error(`spec/validate HTTP error (status: ${response.status})`);
        }
        
        return await response.json(); 
    },
    async save(name: string, spec: string): Promise<true> {

        const response = 
          await fetch(
            `${url}/spec/save`, post(JSON.stringify({ specBody: spec, name })));

        if (!response.ok) {
            throw new Error(`spec/validate HTTP error (status: ${response.status})`);
        }

        return await response.json();
    },
    async seed(spec: Value<string>): Promise<ValidationResultWithPayload<string>> {
        const response = 
          await fetch(
            `${url}/entity/seed`, post(JSON.stringify({ specBody: spec.value })));
        
        if (!response.ok) {
            throw new Error(`spec/validate HTTP error (status: ${response.status})`);
        }

        const result =  await response.json();
        return Object.assign("valid", { payload: result }) as "valid" & { payload: typeof result };
    },
    async entity(name: string,spec: string): Promise<ValidationResultWithPayload<{ spec: string, launchers: string[],  example: string}>>{
    
        const response = 
          await fetch(
            `${url}/entity/entityname?specName=${spec}&entity=${name}`, get);

        if (!response.ok) {
            throw new Error(`spec/validate HTTP error (status: ${response.status})`);
        }
   
        const result =  await response.json();
        return Object.assign("valid", { payload: result }) as "valid" & { payload: typeof result };
    },
    async names(): Promise<ValidationResultWithPayload<string[]>> {
        const response = await fetch(`${url}/spec`, get);

        if (!response.ok) {
            throw new Error(`spec/validate HTTP error (status: ${response.status})`);
        }

        const result =  await response.json();
        return Object.assign("valid", { payload: result }) as "valid" & { payload: typeof result };
    },
    async load(name: string): Promise<ValidationResultWithPayload<{ spec: string, example: string, launchers: string[]}>> {
      
        const response = await fetch(`${url}/entity/name?name=${name}`, get);

        if (!response.ok) {
            throw new Error(`spec/validate HTTP error (status: ${response.status})`);
        }

        const result =  await response.json();

        return Object.assign("valid", { payload: result }) as "valid" & { payload: typeof result };
    },
    async searchCities(searchText: string): Promise<ValidationResultWithPayload<CollectionReference[]>> {
        const response = 
          await fetch(`${url}/search/getMany?specName=Spec Name&page=0&size=10&name=CityRef`, get);

        if (!response.ok) {
            throw new Error(`spec/validate HTTP error (status: ${response.status})`);
        }

        const result =  await response.json();
        return  Object.assign("valid", { payload: result }) as "valid" & { payload: typeof result };
    },
    async entity_names(specName: string): Promise<ValidationResultWithPayload<string[]>> {

        const response = await fetch(`${url}/entity/api_names?specName=${specName}`, get);

        if (!response.ok) {
            throw new Error(`spec/validate HTTP error (status: ${response.status})`);
        }

        const result =  await response.json();
        return Object.assign("valid", { payload: result }) as "valid" & { payload: typeof result };
    },
    async launcher_names(specName: string, entityName: string): Promise<ValidationResultWithPayload<string[]>> {
        const url2 = `${url}/entity/api_l?specName=${specName}&entityName=${entityName}`; 
        const response = await fetch(url2, get);
        if (!response.ok) {
            throw new Error(`spec/validate HTTP error (status: ${response.status})`);
        }

        const result =  await response.json();
        return Object.assign("valid", { payload: result }) as "valid" & { payload: typeof result };
    },
};