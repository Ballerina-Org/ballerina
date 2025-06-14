import { Value } from "ballerina-core";
import {SpecValidationResult} from "../domains/raw-json-editor/state";

export const IDEApi = {
    async validateSpec(spec: Value<string>): Promise<SpecValidationResult> {

        const response = await fetch("https://localhost:7005/spec/validate", {
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
};