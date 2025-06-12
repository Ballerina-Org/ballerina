import { Value } from "ballerina-core";

export const IDEApi = {
    async validateSpec(specName: Value<string>): Promise<boolean> {

        const response = await fetch("https://localhost:7005/spec/validate", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json"
            },
            body: JSON.stringify({ specBody: specName.value })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        return await response.json().then(x => x.isValid);
    },
};