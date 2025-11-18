import {
  BaseConfigurationList,
  BaseFormList,
  BaseLabelV3,
  BaseMapElement,
} from "@blp-private-npm/ui";
import { unit } from "ballerina-core";
import { Fragment } from "react";
import { v4 as uuidv4 } from "uuid";
import {IdeConcreteRenderers, RendererPropsDomain} from "../common/concrete-renderers.ts";
import {
    translateForCustomDataDrivenTranslations,
    translateForDataDrivenTranslationsWithContext
} from "../common/translate.ts";


export const MapFieldViews = {
  map: () => (props) => {
    const ddTranslations = translateForCustomDataDrivenTranslations(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );
    const isReadonly = props.context.readOnly;

    return (
      <>
        <BaseConfigurationList
          sections={props.context.value.values
            .map((_, elementIndex) => {
              return {
                id: elementIndex.toString(),
                label: "",
                content: (
                  <BaseMapElement
                    left={props.embeddedKeyTemplate(elementIndex)(undefined)({
                      ...RendererPropsDomain(
                        props
                      ).Operations.AugmentingCustomPresentationContext({
                        sum: undefined,
                      }),
                      view: unit,
                    })}
                    right={props.embeddedValueTemplate(elementIndex)(undefined)(
                      {
                        ...RendererPropsDomain(
                          props
                        ).Operations.AugmentingCustomPresentationContext({
                          sum: undefined,
                        }),
                        view: unit,
                      }
                    )}
                  />
                ),
              };
            })
            .toArray()}
          onAddSection={
            isReadonly
              ? undefined
              : () => props.foreignMutations.add!(undefined)
          }
          onDeleteSection={
            isReadonly
              ? undefined
              : (elementIndex) =>
                  props.foreignMutations.remove!(
                    parseInt(elementIndex),
                    undefined
                  )
          }
          addSectionButtonName={ddTranslations("addNewElement")}
        />
      </>
    );
  },
  mapWithLabel: () => (props) => {
    const ddTranslations = translateForCustomDataDrivenTranslations(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );
    const ddTranslationsWithCtx = translateForDataDrivenTranslationsWithContext(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );

    const isReadonly = props.context.readOnly;

    return (
      <BaseLabelV3
        dense
        label={ddTranslationsWithCtx(
          props.context.label,
          props.context.labelContext,
          props.context.domNodeId
        )}
      >
        <BaseConfigurationList
          sections={props.context.value.values
            .map((_, elementIndex) => {
              return {
                id: elementIndex.toString(),
                label: ddTranslationsWithCtx(
                  props.context.label,
                  props.context.labelContext,
                  props.context.domNodeId
                ),

                content: (
                  <BaseMapElement
                    left={props.embeddedKeyTemplate(elementIndex)(undefined)({
                      ...RendererPropsDomain(
                        props
                      ).Operations.AugmentingCustomPresentationContext({
                        sum: undefined,
                      }),
                      view: unit,
                    })}
                    right={props.embeddedValueTemplate(elementIndex)(undefined)(
                      {
                        ...RendererPropsDomain(
                          props
                        ).Operations.AugmentingCustomPresentationContext({
                          sum: undefined,
                        }),
                        view: unit,
                      }
                    )}
                  />
                ),
              };
            })
            .toArray()}
          onAddSection={
            isReadonly
              ? undefined
              : () => props.foreignMutations.add!(undefined)
          }
          onDeleteSection={
            isReadonly
              ? undefined
              : (elementIndex) =>
                  props.foreignMutations.remove!(
                    parseInt(elementIndex),
                    undefined
                  )
          }
          addSectionButtonName={ddTranslations("addNewElement")}
        />
      </BaseLabelV3>
    );
  },
  nestedMap: () => (props) => {
    const ddTranslations = translateForCustomDataDrivenTranslations(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );

    const isReadonly = props.context.readOnly;

    return (
      <>
        <BaseFormList
          id={uuidv4()}
          elements={props.context.value.values
            .map((_, elementIndex) => {
              return {
                id: elementIndex.toString(),
                content: (
                  <BaseMapElement
                    left={props.embeddedKeyTemplate(elementIndex)(undefined)({
                      ...RendererPropsDomain(
                        props
                      ).Operations.AugmentingCustomPresentationContext({
                        sum: undefined,
                      }),
                      view: unit,
                    })}
                    right={props.embeddedValueTemplate(elementIndex)(undefined)(
                      {
                        ...RendererPropsDomain(
                          props
                        ).Operations.AugmentingCustomPresentationContext({
                          sum: undefined,
                        }),
                        view: unit,
                      }
                    )}
                  />
                ),
                onDelete: isReadonly
                  ? undefined
                  : () =>
                      props.foreignMutations.remove!(elementIndex, undefined),
              };
            })
            .toArray()}
          onAddElement={
            isReadonly
              ? undefined
              : () => props.foreignMutations.add!(undefined)
          }
          addElementButtonName={ddTranslations("addNewField")}
        />
      </>
    );
  },
  keyValue: () => (props) => {
    return (
      <>
        {!props.context.value.values.isEmpty() ? (
          props.context.value.values.map((_, elementIndex) => (
            <Fragment key={elementIndex}>
              {props.embeddedValueTemplate(elementIndex)(undefined)({
                ...RendererPropsDomain(
                  props
                ).Operations.AugmentingCustomPresentationContext({
                  sum: undefined,
                }),
                view: unit,
              })}
            </Fragment>
          ))
        ) : (
          <></>
        )}
      </>
    );
  },
} satisfies IdeConcreteRenderers["map"];
