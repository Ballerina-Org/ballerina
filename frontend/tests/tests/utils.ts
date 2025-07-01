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


export const primitives =[
  DispatchInjectedPrimitive.Default(
    "injectedCategory",
    CategoryAbstractRenderer,
    {
      kind: "custom",
      value: {
        kind: "adult",
        extraSpecial: false,
      },
    },
    DispatchCategoryState.Default(),
  )]

