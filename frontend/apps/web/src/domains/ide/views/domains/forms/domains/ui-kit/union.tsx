import { unit } from "ballerina-core";
import {IdeConcreteRenderers, RendererPropsDomain} from "../common/concrete-renderers";


export const UnionFieldViews = {
  readonlyUnion: () => (props) => {
    return (
      <>
        {props.embeddedCaseTemplate(props.context.value.caseName)(undefined)({
          ...RendererPropsDomain(
            props
          ).Operations.AugmentingCustomPresentationContext({
            sum: undefined,
          }),
          view: unit,
        })}
      </>
    );
  },
  union: () => (props) => {
    return (
      <>
        {props.embeddedCaseTemplate(props.context.value.caseName)(undefined)({
          ...RendererPropsDomain(
            props
          ).Operations.AugmentingCustomPresentationContext({
            sum: undefined,
          }),
          view: unit,
        })}
      </>
    );
  },
  businessPartnerPreviewDetails: () => (props) => {
    return (
      <>
        {props.embeddedCaseTemplate(props.context.value.caseName)(undefined)({
          ...RendererPropsDomain(
            props
          ).Operations.AugmentingCustomPresentationContext({
            sum: undefined,
          }),
          view: unit,
        })}
      </>
    );
  },
  buttonIcon: () => (props) => {
    return (
      <>
        {props.embeddedCaseTemplate(props.context.value.caseName)(undefined)({
          ...RendererPropsDomain(
            props
          ).Operations.AugmentingCustomPresentationContext({
            sum: undefined,
          }),
          view: unit,
        })}
      </>
    );
  },
} satisfies IdeConcreteRenderers["union"];
