import {
  DispatchSpecificationDeserializationResult, ErrorRendererProps, IdWrapperProps, Option,
} from "ballerina-core";

import {PersonFormInjectedTypes as EntityFormInjectedTypes} from "../../../person-from-config/injected-forms/category.tsx";

export const ShowFormsParsingErrors = (
  parsedFormsConfig: DispatchSpecificationDeserializationResult<EntityFormInjectedTypes>,
) => (
  <div style={{ display: "flex", border: "red" }}>
    {parsedFormsConfig.kind == "errors" &&
      JSON.stringify(parsedFormsConfig.errors)}
  </div>
);

export const IdWrapper = ({ domNodeId, children }: IdWrapperProps) => {

  return (
    <div id={domNodeId}>{children}</div>
  )
} ;

export const ErrorRenderer = ({ message }: ErrorRendererProps) => (
  <div
    style={{
      display: "flex",
      border: "2px dashed red",
      maxWidth: "200px",
      maxHeight: "50px",
      overflowY: "scroll",
      padding: "10px",
    }}
  >
  <pre
    style={{
      whiteSpace: "pre-wrap",
      maxWidth: "200px",
      lineBreak: "anywhere",
    }}
  >{`Error: ${message}`}</pre>
  </div>
);

export const GetLoadedValue = <T,>(spec: any) => {
  const syncKind = spec?.sync?.kind;
  const valueKind = spec?.sync?.value?.kind;
  const isLoaded = syncKind === "loaded" && valueKind === "value";
  const value = isLoaded ? (spec.sync.value.value as T) : undefined;

  return {
    then(fn: (val: T) => void) {
      if (isLoaded) fn(value!);
      return this;
    },
    otherwise(kind: string | string[], fn: () => void) {
      const matches =
        Array.isArray(kind) ? kind.includes(syncKind) : kind === syncKind;
      if (!isLoaded && matches) fn();
      return this;
    },
  };
};