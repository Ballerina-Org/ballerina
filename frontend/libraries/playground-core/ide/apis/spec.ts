import {Unit, ValidationResult, Value} from "ballerina-core";
import {SpecValidationResult, ValidationResultWithPayload} from "../domains/spec-editor/state";

const url = "https://localhost:7005"

export const IDEApi = {
    async validateSpec(spec: Value<string>): Promise<SpecValidationResult> {

        const response = await fetch(`${url}/spec/validate`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json"
            },
            body: JSON.stringify({ specBody: spec.value })
        });

        if (!response.ok) {
            throw new Error(`spec/validate HTTP error (status: ${response.status})`);
        }
        
        return await response.json(); 
    },
    async lock(spec: Value<string>): Promise<Unit> {

        const response = await fetch(`${url}/spec/lock`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json"
            },
            body: JSON.stringify({ specBody: spec.value })
        });

        if (!response.ok) {
            throw new Error(`spec/validate HTTP error (status: ${response.status})`);
        }

        return await response.json();
    },
    async save(name: string, spec: string): Promise<true> {

        const response = await fetch(`${url}/spec/save`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json"
            },
            body: JSON.stringify({ specBody: spec, name })
        });

        if (!response.ok) {
            throw new Error(`spec/validate HTTP error (status: ${response.status})`);
        }

        const res =  await response.json();

        return res;
    },
    async seed(spec: Value<string>): Promise<ValidationResultWithPayload<string>> {

        const response = await fetch(`${url}/entity/seed`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json"
            },
            body: JSON.stringify({ specBody: spec.value })
        });

        if (!response.ok) {
            throw new Error(`spec/validate HTTP error (status: ${response.status})`);
        }

        const result =  await response.json();
        
        const t = Object.assign("valid", { payload: result }) as "valid" & { payload: typeof result };
        return t
    },
};