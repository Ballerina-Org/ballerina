import {
  BaseAttentionBoxV3,
  BaseButtonV3,
  BaseDatabaseValueButton,
  BaseDatabaseValueIcon,
  BaseIconButtonV3,
  BaseInputV3,
  BaseNoContent,
} from "@blp-private-npm/ui";
import { v4 as uuidv4 } from "uuid";
import { IdeConcreteRenderers } from "../common/concrete-renderers";
import {translateForCustomDataDrivenTranslations} from "../common/translate.ts";
import {mapTypeToFlags} from "../common/map-type-to-flag.ts";


export const UnitFieldViews = {
  unit: () => (_props) => {
    return <></>;
  },
  unitEmptyString: () => (props) => {
    return (
      <BaseInputV3
        variant={"outlined"}
        shape={"square"}
        kind="readOnly"
        fullWidth
        copyable={false}
        value={""}
        onClick={() => {}}
        disabled={props.context.disabled}
        customAiApplied={false}
      />
    );
  },
  noTable: () => (props) => {
    const ddTranslations = translateForCustomDataDrivenTranslations(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );
    return (
      <>
        <BaseNoContent
          title={ddTranslations("ohNo")}
          text={ddTranslations("home.myTasks.thisBoxFeelsEmpty")}
          style={{ minWidth: 500, left: "-50%" }}
        />
      </>
    );
  },
  fieldNotConfigured: () => (props) => {
    const ddTranslations = translateForCustomDataDrivenTranslations(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );
    return (
      <BaseAttentionBoxV3
        text={ddTranslations("fieldNotConfigured")}
        size="s"
        variant="info"
        fullWidth
      />
    );
  },
  fillFromDBButton: () => (props) => {
    const ddTranslations = translateForCustomDataDrivenTranslations(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );

    const flags = mapTypeToFlags({
      ancestors: props.context.lookupTypeAncestorNames,
    });

    if (props.context.readOnly) {
      return <></>;
    }

    return (
      <BaseDatabaseValueButton
        message={ddTranslations("field.metadata.replaceAllWithDbValue")}
        onClick={() => props.foreignMutations.set(flags)}
      />
    );
  },
  fillFromDBDisabledButton: () => (props) => {
    const ddTranslations = translateForCustomDataDrivenTranslations(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );
    return (
      <BaseDatabaseValueIcon
        message={ddTranslations("field.metadata.replaceAllWithDbValueDisabled")}
      />
    );
  },
  missingGoodsStatusUpdateButton: () => (props) => {
    const ddTranslations = translateForCustomDataDrivenTranslations(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );

    return (
      <>
        <BaseIconButtonV3
          onClick={() =>
            props.foreignMutations.set({
              kind: "localAndRemote",
              customLocks: [props.context.domNodeId],
              lockedCards: [],
            })
          }
          disabled={props.context.readOnly || props.context.disabled}
          tooltip={{
            message: ddTranslations(
              "field.metadata.manuallyRefreshLateDeliveries"
            ),
          }}
          icon={"reload"}
          loading={props.context.extraContext.customLocks.has(
            props.context.domNodeId
          )}
          variant="text"
          colorVariant="secondary"
        />
      </>
    );
  },
  reloadMissingGoodsStatusDisabledButton: () => (_props) => {
    return <></>;
  },
  actionButton: () => (props) => {
    const ddTranslations = translateForCustomDataDrivenTranslations(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );
    const ddTranslationsWithCtx = translateForCustomDataDrivenTranslations(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );

    const flags = mapTypeToFlags({
      ancestors: props.context.lookupTypeAncestorNames,
    });

    return (
      <>
        <BaseButtonV3
          id={uuidv4()}
          onClick={() => props.foreignMutations.set(flags)}
          variant="secondary"
          colorVariant="secondary"
        >
          {props.context.label
            ? ddTranslationsWithCtx(props.context.label)
            : ddTranslations("runAction")}
        </BaseButtonV3>
      </>
    );
  },
  actionButtonDisabled: () => (props) => {
    const ddTranslations = translateForCustomDataDrivenTranslations(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );
    const ddTranslationsWithCtx = translateForCustomDataDrivenTranslations(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );

    return (
      <>
        <BaseButtonV3
          id={uuidv4()}
          onClick={() => {}}
          disabled={true}
          variant="secondary"
          colorVariant="secondary"
        >
          {props.context.label
            ? ddTranslationsWithCtx(props.context.label)
            : ddTranslations("runAction")}
        </BaseButtonV3>
      </>
    );
  },
} satisfies IdeConcreteRenderers["unit"];
