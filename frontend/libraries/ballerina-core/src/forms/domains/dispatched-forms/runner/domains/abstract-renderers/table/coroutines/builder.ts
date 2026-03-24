import {
  CoTypedFactory,
  TableAbstractRendererForeignMutationsExpected,
} from "../../../../../../../../../main";
import {
  TableAbstractRendererReadonlyContext,
  TableAbstractRendererState,
  Unit,
} from "../../../../../../../../../main";

export const Co = <CustomPresentationContext = Unit, ExtraContext = Unit>() =>
  CoTypedFactory<
    TableAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    >,
    TableAbstractRendererState
  >();

export const InfiniteLoaderCo = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  CoTypedFactory<
    TableAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      Pick<TableAbstractRendererForeignMutationsExpected, "onChange">,
    TableAbstractRendererState
  >();

export const PendingOperationsCo = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  CoTypedFactory<
    TableAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      Pick<TableAbstractRendererForeignMutationsExpected, "onChange"> & {
        uniqueTableIdentifier: string;
      },
    TableAbstractRendererState
  >();
