import {
  CommonAbstractRendererForeignMutationsExpected,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  CommonAbstractRendererViewOnlyReadonlyContext,
  DispatchParsedType,
  PredicateValue,
  ReadOnlyType,
  ValueReadOnly,
} from "../../../../../../../../main";
import { Unit } from "../../../../../../../fun/domains/unit/state";
import { Template } from "../../../../../../../template/state";
import { View } from "../../../../../../../template/state";
import { simpleUpdater } from "../../../../../../../fun/domains/updater/domains/simpleUpdater/state";

export type ReadOnlyAbstractRendererReadonlyContext<
  CustomPresentationContext,
  ExtraContext,
> = CommonAbstractRendererReadonlyContext<
  ReadOnlyType<any>,
  ValueReadOnly,
  CustomPresentationContext,
  ExtraContext
>;

export type ReadOnlyAbstractRendererState = CommonAbstractRendererState & {
  childFormState: CommonAbstractRendererState;
};

export const ReadOnlyAbstractRendererState = {
  Default: {
    zero: () => ({
      ...CommonAbstractRendererState.Default(),
      childFormState: CommonAbstractRendererState.Default(),
    }),
    childFormState: (
      childFormState: CommonAbstractRendererState,
    ): ReadOnlyAbstractRendererState => ({
      ...CommonAbstractRendererState.Default(),
      childFormState,
    }),
  },
  Updaters: {
    Core: {
      ...simpleUpdater<ReadOnlyAbstractRendererState>()("commonFormState"),
      ...simpleUpdater<ReadOnlyAbstractRendererState>()("childFormState"),
    },
    Template: {},
  },
};

export type ReadOnlyAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
> = View<
  ReadOnlyAbstractRendererReadonlyContext<
    CustomPresentationContext,
    ExtraContext
  > &
    ReadOnlyAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  ReadOnlyAbstractRendererState,
  CommonAbstractRendererForeignMutationsExpected<Flags>,
  {
    embeddedTemplate: Template<
      ReadOnlyAbstractRendererReadonlyContext<
        CustomPresentationContext,
        ExtraContext
      > &
        ReadOnlyAbstractRendererState,
      ReadOnlyAbstractRendererState,
      CommonAbstractRendererForeignMutationsExpected<Flags>
    >;
  }
>;
