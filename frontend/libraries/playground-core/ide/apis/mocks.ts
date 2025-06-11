import { apiResultStatuses } from "ballerina-core";
import { Value } from "ballerina-core";
import { ValidationResult } from "ballerina-core";
import { PromiseRepo } from "ballerina-core";

export const IDEApi = {
  validateInputString: (_: Value<string>): Promise<ValidationResult> =>
    PromiseRepo.Default.mock<ValidationResult>(
      () =>
        Math.random() > 0.9
          ? "valid"
          : {
              kind: "error",
              errors: ["validation error 1", "validation error 2"],
            },
      () => apiResultStatuses[2],
      0.8,
      0.2,
    ),
    
  getSpec: (spec: string): Promise<string> => PromiseRepo.Default.mock<string>(
      () => `{
  "name": "Alice",
  "age": 30,
  "active": true,
  "fun": "extend",
  "nested": {
    "value": 42,
    "args": [1, 2, 3],
    "fun": "args"
  },
  "tags": ["json", "monarch", "test"],
  "nullValue": null
}`,
      () => apiResultStatuses[2],
  )
};
