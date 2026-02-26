import {
  DispatchInjectablesTypes,
  Template,
  ValueOrErrors,
} from "../../../../../../../main";
import { Renderer } from "../../../deserializer/domains/specification/domains/forms/domains/renderer/state";
import { ListDispatcher } from "./domains/list/state";
import { MapDispatcher } from "./domains/map/state";
import { MultiSelectionDispatcher } from "./domains/multiSelection/state";
import { OneDispatcher } from "./domains/one/state";
import { ReferenceDispatcher } from "./domains/reference/state";
import { ReadOnlyDispatcher } from "./domains/readOnly/state";
import { RecordDispatcher } from "./domains/record/state";
import { LookupDispatcher } from "./domains/lookup/state";
import { SingleSelectionDispatcher } from "./domains/singleSelectionDispatcher/state";
import { SumDispatcher } from "./domains/sum/state";
import { TableDispatcher } from "./domains/table/state";
import { TupleDispatcher } from "./domains/tupleDispatcher/state";
import { UnionDispatcher } from "./domains/unionDispatcher/state";
import { PrimitiveDispatcher } from "./domains/primitive/state";
import { LookupRenderer } from "../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/lookup/state";
import { DispatcherContextWithApiSources } from "../../state";

export const Dispatcher = {
  Operations: {
    DispatchAs: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      renderer: Renderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      as: string,
      isNested: boolean,
      isInlined: boolean,
      api: string | Array<string> | undefined,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      Dispatcher.Operations.Dispatch(
        renderer,
        dispatcherContext,
        isNested,
        isInlined,
        api,
      ).MapErrors((errors) =>
        errors.map((error) => `${error}\n...When dispatching as: ${as}`),
      ),
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      renderer: Renderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      isNested: boolean,
      isInlined: boolean | undefined,
      api: string | Array<string> | undefined,
      currentLookupRenderer?: string,
    ): ValueOrErrors<Template<any, any, any, any>, string> => {
      // see the deserializer state file for a commeny explaining lookup renderers
      return renderer.kind == "primitiveRenderer"
        ? PrimitiveDispatcher.Operations.Dispatch(renderer, dispatcherContext)
        : renderer.kind == "lookupType-lookupRenderer" ||
            renderer.kind == "lookupType-inlinedRenderer"
          ? LookupDispatcher.Operations.Dispatch(
              renderer,
              dispatcherContext,
              api,
            )
          : renderer.kind == "inlinedType-lookupRenderer"
            ? LookupRenderer.Operations.ResolveRenderer(
                renderer,
                dispatcherContext.forms,
              ).Then((resolvedRenderer) =>
                Dispatcher.Operations.Dispatch(
                  resolvedRenderer,
                  dispatcherContext,
                  isNested,
                  false,
                  api,
                  renderer.lookupRenderer,
                ),
              )
            : renderer.kind == "recordRenderer"
              ? RecordDispatcher.Operations.Dispatch(
                  renderer,
                  dispatcherContext,
                  isNested,
                  isInlined ?? true,
                  currentLookupRenderer,
                )
              : renderer.kind == "listRenderer"
                ? ListDispatcher.Operations.Dispatch(
                    renderer,
                    dispatcherContext,
                    isInlined ?? true,
                  )
                : renderer.kind == "mapRenderer"
                  ? MapDispatcher.Operations.Dispatch(
                      renderer,
                      dispatcherContext,
                      isInlined ?? true,
                    )
                  : (renderer.kind == "enumRenderer" ||
                        renderer.kind == "streamRenderer") &&
                      renderer.type.kind == "singleSelection"
                    ? SingleSelectionDispatcher.Operations.Dispatch(
                        renderer,
                        dispatcherContext,
                      )
                    : (renderer.kind == "enumRenderer" ||
                          renderer.kind == "streamRenderer") &&
                        renderer.type.kind == "multiSelection"
                      ? MultiSelectionDispatcher.Operations.Dispatch(
                          renderer,
                          dispatcherContext,
                        )
                      : renderer.kind == "oneRenderer"
                        ? OneDispatcher.Operations.Dispatch(
                            renderer,
                            dispatcherContext,
                            isInlined ?? true,
                          )
                        : renderer.kind == "readOnlyRenderer"
                          ? ReadOnlyDispatcher.Operations.Dispatch(
                              renderer,
                              dispatcherContext,
                              isInlined ?? true,
                            )
                          : renderer.kind == "sumRenderer" ||
                              renderer.kind == "sumUnitDateRenderer"
                            ? SumDispatcher.Operations.Dispatch(
                                renderer,
                                dispatcherContext,
                                isInlined ?? true,
                              )
                            : renderer.kind == "tableRenderer"
                              ? TableDispatcher.Operations.Dispatch(
                                  renderer,
                                  dispatcherContext,
                                  api,
                                  isInlined ?? true,
                                  currentLookupRenderer,
                                )
                              : renderer.kind == "tupleRenderer"
                                ? TupleDispatcher.Operations.Dispatch(
                                    renderer,
                                    dispatcherContext,
                                    isInlined ?? true,
                                  )
                                : renderer.kind == "unionRenderer"
                                  ? UnionDispatcher.Operations.Dispatch(
                                      renderer,
                                      dispatcherContext,
                                      isNested,
                                      isInlined ?? true,
                                      currentLookupRenderer,
                                    )
                                  : renderer.kind == "referenceRenderer"
                                    ? ReferenceDispatcher.Operations.Dispatch(
                                        renderer,
                                        dispatcherContext,
                                        isInlined ?? true,
                                      )
                                    : ValueOrErrors.Default.throwOne(
                                        `unknown renderer ${renderer.kind}`,
                                      );
    },
  },
};
