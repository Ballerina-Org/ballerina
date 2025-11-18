import { ReadOnlyAbstractRendererView, unit } from "ballerina-core";
import React from "react";

import {
    ListElementCustomPresentationContext
} from "../../../../../../dispatched-passthrough-form/views/tailwind-renderers.tsx";
import { IdeConcreteRenderers, RendererPropsDomain } from "../common/concrete-renderers.ts";
import {CustomPresentationContexts} from "../common/custom-presentation-contexts.ts";
import {IdeFlags} from "../common/ide-flags.ts";
import {FieldExtraContext} from "../common/field-extra-context.ts";

const ReadOnly = (
  props: React.ComponentProps<
    ReadOnlyAbstractRendererView<
        CustomPresentationContexts & { listElement: ListElementCustomPresentationContext },
      IdeFlags,
      FieldExtraContext
    >
  > & {
    size?: "m" | "s";
    noErrorsView?: React.ReactNode;
  }
) => {
  return (
    <>
      {props.embeddedTemplate({
        ...RendererPropsDomain(
          props
        ).Operations.AugmentingCustomPresentationContext({
          sum: undefined,
        }),
        view: unit,
      })}
    </>
  );
};

export const ReadOnlyFieldViews = {
  readOnly: () => (props) => <ReadOnly {...props} />,
} satisfies IdeConcreteRenderers["readOnly"];
