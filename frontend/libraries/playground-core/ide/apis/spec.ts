import {Unit, Value } from "ballerina-core";
import { SpecValidationResult } from "../domains/spec-editor/state";

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
    async entity(spec: Value<string>): Promise<string> {

        const response = await fetch(`${url}/entity/play`, {
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