import { Map, Set } from "immutable";
import {
  Bindings,
  DispatchOnChange,
  DispatchParsedType,
  FormLayout,
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
  legacy_domNodeAncestorPath: string;
  predictionAncestorPath: string;
  layoutAncestorPath: string;
  typeAncestors: DispatchParsedType<any>[];
  lookupTypeAncestorNames: string[];
  preprocessedSpecContext?: PreprocessedSpecContext;
  usePreprocessor: boolean;
};

export type CommonAbstractRendererViewOnlyReadonlyContext = {
  domNodeId: string;
  legacy_domNodeId: string;
};

export type PreprocessedSpecContext = {
  formLayouts: Map<string, FormLayout>;
  tableLayouts: Map<string, string[]>;
  disabledFields: Set<string>;
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
