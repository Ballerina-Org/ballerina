export type TopLevelKey = "types" | "forms" | "apis" | "launchers" | "typesV2" | "schema" | "config";

export type JsonPrimitive = string | number | boolean | null;
export type JsonValue = JsonPrimitive | JsonValue[] | { [k: string]: JsonValue };
export type JsonSection = Record<string, JsonValue>;
export type KnownSections = Partial<Record<TopLevelKey, JsonSection>>;