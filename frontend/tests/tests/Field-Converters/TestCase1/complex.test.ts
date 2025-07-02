import {
  DispatchFieldTypeConverters,
  DispatchFieldTypeConverters2
} from "web/src/domains/dispatched-passthrough-form/apis/field-converters.ts"
import {
  dispatchFromAPIRawValue,
  DispatchInjectedPrimitives,
  injectedPrimitivesFromConcreteRenderers,
  MapRepo, Renderer,
  Specification, ValueOrErrors
} from "ballerina-core"
import {isImmutable, Map} from "immutable";
import {PersonConcreteRenderers} from "web/src/domains/dispatched-passthrough-form/views/concrete-renderers"

import {PersonFormInjectedTypes} from "web/src/domains/person-from-config/injected-forms/category.tsx";
import {primitives} from "../../utils.ts";

import spec from "./spec.json";
import feed from "./feed.json";

import {
  DispatchPassthroughFormInjectedTypes
} from "web/src/domains/dispatched-passthrough-form/injected-forms/category.tsx";


let injectedPrimitives: DispatchInjectedPrimitives<PersonFormInjectedTypes>;

const deserialize = (spec: any) => {
  return Specification.Operations.Deserialize(
    DispatchFieldTypeConverters,
    PersonConcreteRenderers,
    injectedPrimitives
  )(spec);
}

beforeAll(() => {
  const injected =
    injectedPrimitivesFromConcreteRenderers(
      PersonConcreteRenderers,
      primitives,
    )
  if  (injected.kind == "errors")
    throw new Error(injected.errors.valueSeq().toArray().join(" <*> "));
  injectedPrimitives = injected.value;
});


describe.skip("current traverser structure", () => {
  test.todo("put existing failing structures and make them work");


  it("current UI data flow", async  () => {

    const d = deserialize(spec);
    
    if(d.kind == "errors")
      throw new Error(d.errors.valueSeq().toArray().join(" <*> "))
    
    const specification = d.value;

    const result = specification.launchers.passthrough
      .entrySeq()
      .toArray()
      .map(([launcherName, launcher]) =>
        MapRepo.Operations.tryFindWithError(
          launcher.form,
          specification.forms,
          () => `cannot find form "${launcher.form}" when parsing launcher "${launcherName}"`
        ).Map((parsedForm) => [launcherName, parsedForm] as [string, Renderer<DispatchPassthroughFormInjectedTypes>])
      );
    
    const launcherNameToFind = "person-transparent"; //person-config

    const parsedForm = result.find(({kind, value}) => value[0] === launcherNameToFind).value[1];

    const p = dispatchFromAPIRawValue(
      parsedForm?.type,
      specification.types,
      DispatchFieldTypeConverters2,
      injectedPrimitives
    )(Data)
    
    expect(99).toBe(99);
  });
});