import { FormRenderer } from "../state";

export type Renderer<T> = BaseRenderer<T> | FormRenderer<T>;

export type BaseRenderer<T> = {
  label?: string;
  tooltip?: string;
  details?: string;
};

export type LookupRenderer<T> =
  | {
      kind: "formLookup";
      name: string;
    }
  | {
      kind: "primitiveLookup";
      name: string;
    };
