import { DispatchFieldTypeConverters, DispatchFieldTypeConverters2 } from "web/src/domains/dispatched-passthrough-form/apis/field-converters.ts"
import {
  dispatchFromAPIRawValue, DispatchInjectables,
  DispatchInjectedPrimitive,
  DispatchInjectedPrimitives, injectedPrimitivesFromConcreteRenderers, MapRepo,
  Specification
} from "ballerina-core"
import { List, Map, isImmutable } from "immutable";
import { PersonConcreteRenderers } from "web/src/domains/dispatched-passthrough-form/views/concrete-renderers"

import spec from "web/public/SampleSpecs/dispatch-person-config.json";
import {
  CategoryAbstractRenderer,
  DispatchCategoryState
} from "web/src/domains/dispatched-passthrough-form/injected-forms/category.tsx";
import {PersonFormInjectedTypes} from "web/src/domains/person-from-config/injected-forms/category.tsx";
import {primitives} from "./utils";
import Data from "./seed.json"
const deserialize =(spec: any, primitives) => {
  const deserializationResult = Specification.Operations.Deserialize(
    DispatchFieldTypeConverters,
    PersonConcreteRenderers,
    primitives as DispatchInjectedPrimitives<PersonFormInjectedTypes>
  )(spec);
  return deserializationResult;
}

const problem = 
  `{
    "kind":"option",
    "isSome":true,
    "value":{
      "kind":"record",
      "fields":{
        "kind":"record",
        "fields":{
          "Value":{
            "kind":"record",
            "fields":{
              "Case":"M",
              "Value":{
                "kind":"unit"}}}}}}}`
let injectedPrimitives: DispatchInjectedPrimitives<PersonFormInjectedTypes>;
beforeAll(() => {
  const injected =
    injectedPrimitivesFromConcreteRenderers(
      PersonConcreteRenderers,
      primitives,
    )
  if  (injected.kind == "errors")
    throw new Error(injected.errors.valueSeq().toArray().join(" <*>"));
  injectedPrimitives = injected.value;
});

describe("DispatchFieldTypeConverters.fromAPIRawValue", () => {
  it("project x ", () => {
    const t = DispatchFieldTypeConverters.SingleSelection.fromAPIRawValue(JSON.parse(problem))
    expect(
      DispatchFieldTypeConverters.number.fromAPIRawValue({ kind: "int", fields: 42 })
    ).toBe(42);
  });
  it("parses a plain int value", () => {
    expect(DispatchFieldTypeConverters.number.fromAPIRawValue(42)).toBe(42);
  });

  it("parses structured int", () => {
    expect(
      DispatchFieldTypeConverters.number.fromAPIRawValue({ kind: "int", fields: 42 })
    ).toBe(42);
  });
  //
  // it("parses structured array of strings", () => {
  //   const input = {
  //     kind: "array",
  //     fields: [
  //       { kind: "string", fields: "a" },
  //       { kind: "string", fields: "b" },
  //     ],
  //   };
  //   const result = DispatchFieldTypeConverters.List.fromAPIRawValue(input);
  //   expect(immutableIs(result, List(["a", "b"]))).toBe(true);
  // });
  //
  it("parses structured record", () => {
    const input = {
      kind: "record",
      fields: [
        { Key: "x", Value: { kind: "int", fields: 1 } },
        { Key: "y", Value: { kind: "int", fields: 2 } },
      ],
    };
    const result = DispatchFieldTypeConverters.Map.fromAPIRawValue(input);
    const expected = Map({ x: 1, y: 2 });
    expect(isImmutable(result, expected)).toBe(true);
  });
  //

  it("problematic", async  () => {

    const d = deserialize(spec, injectedPrimitives);
    
    if(d.kind == "errors")
      throw new Error(d.errors.valueSeq().toArray().join(" -------- "))


    
    const specification = d.value;

    const result = specification.launchers.passthrough
      .entrySeq()
      .toArray()
      .map(([launcherName, launcher]) => {
        return MapRepo.Operations.tryFindWithError(
          launcher.form,
          specification.forms,
          () => `cannot find form "${launcher.form}" when parsing launcher "${launcherName}"`
        ).Then((parsedForm) => [launcherName, parsedForm]);
      });

    const launcherNameToFind = "person-transparent"; //person-config

    const parsedForm = result.find(([name]) => name === launcherNameToFind)?.[1];

    const p = dispatchFromAPIRawValue(
      parsedForm?.type,
      specification.types,
      DispatchFieldTypeConverters2,
      injectedPrimitives
    )(Data)
    
    expect(99).toBe(99);
  });
});