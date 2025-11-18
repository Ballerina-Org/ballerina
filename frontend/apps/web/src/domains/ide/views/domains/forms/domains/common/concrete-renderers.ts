import {ConcreteRenderers} from "ballerina-core";
import {
    DispatchPassthroughFormInjectedTypes
} from "../../../../../../dispatched-passthrough-form/injected-forms/category.tsx";
import {
    ListElementCustomPresentationContext
} from "../../../../../../dispatched-passthrough-form/views/tailwind-renderers.tsx";
import {FieldExtraContext} from "./field-extra-context.ts";
import {CustomPresentationContexts} from "./custom-presentation-contexts.ts";
import {IdeFlags} from "./ide-flags.ts";

export type IdeConcreteRenderers = ConcreteRenderers<
    DispatchPassthroughFormInjectedTypes,
    IdeFlags,
    CustomPresentationContexts & { listElement: ListElementCustomPresentationContext },
    FieldExtraContext
>;
export const RendererPropsDomain = <
    CustomCtx extends CustomPresentationContexts,
    Props extends {
        context: {
            customPresentationContext: CustomCtx | undefined;
        };
    },
>(
    props: Props
) => ({
    Operations: {
        AugmentingCustomPresentationContext: (customCtx: Partial<CustomCtx>) => ({
            ...props,
            context: {
                ...props.context,
                customPresentationContext: {
                    ...props.context.customPresentationContext,
                    ...customCtx,
                },
            },
        }),
    },
});
