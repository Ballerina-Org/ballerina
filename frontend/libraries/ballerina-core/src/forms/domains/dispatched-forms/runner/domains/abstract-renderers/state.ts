import {
  Bindings,
  DispatchOnChange,
  DispatchParsedType,
  NestedRenderer,
  PredicateValue,
  Renderer,
  simpleUpdater,
  simpleUpdaterWithChildren,
  Unit,
} from "../../../../../../../main";
import { RecordFieldRenderer } from "../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/record/domains/recordFieldRenderer/state";
import { TableCellRenderer } from "../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/table/domains/tableCellRenderer/state";

export type CommonAbstractRendererReadonlyContext<
  T extends DispatchParsedType<any>,
  V extends PredicateValue,
  C = Unit,
  ExtraContext = Unit,
> = {
  value: V;
  locked: boolean;
  disabled: boolean;
  globallyDisabled: boolean;
  readOnly: boolean;
  globallyReadOnly: boolean; // ignore writeable signals if true, set only for whole form
  bindings: Bindings;
  extraContext: ExtraContext;
  type: T;
  label?: string;
  tooltip?: string;
  details?: string;
  labelContext: string;
  customPresentationContext: C | undefined;
  remoteEntityVersionIdentifier: string;
  domNodeAncestorPath: string;
  typeAncestors: DispatchParsedType<any>[];
  lookupTypeAncestorNames: string[];
};

export type CommonAbstractRendererViewOnlyReadonlyContext = {
  domNodeId: string;
};

export type CommonAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: unknown;
};

export const CommonAbstractRendererState = {
  Default: (): CommonAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {},
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<CommonAbstractRendererState>()("commonFormState"),
      ...simpleUpdater<CommonAbstractRendererState>()("customFormState"),
      ...simpleUpdaterWithChildren<CommonAbstractRendererState>()({
        ...simpleUpdater<CommonAbstractRendererState["commonFormState"]>()(
          "modifiedByUser",
        ),
      })("commonFormState"),
    },
  },
  Operations: {
    GetLabelContext: (
      parentLabelContext: string,
      renderer:
        | NestedRenderer<any>
        | RecordFieldRenderer<any>
        | TableCellRenderer<any>
        | Renderer<any>,
      isUnionCase?: boolean,
      caseName?: string,
    ) => {
      const actualRenderer = !("renderer" in renderer)
        ? renderer
        : renderer.renderer;
      if (
        actualRenderer.kind == "lookupType-lookupRenderer" ||
        actualRenderer.kind == "inlinedType-lookupRenderer"
      ) {
        return actualRenderer.lookupRenderer;
      }
      if (isUnionCase && caseName) {
        return `${parentLabelContext}:${caseName}`;
      }
      return parentLabelContext;
    },
  },
};

export type CommonAbstractRendererForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<PredicateValue, Flags>;
};

export type DispatchCommonFormState = {
  modifiedByUser: boolean;
};

export const DispatchCommonFormState = {
  Default: (): DispatchCommonFormState => ({
    modifiedByUser: false,
  }),
  Updaters: {
    ...simpleUpdater<DispatchCommonFormState>()("modifiedByUser"),
  },
};
