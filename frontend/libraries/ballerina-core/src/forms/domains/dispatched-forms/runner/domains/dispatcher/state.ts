import {
  DispatcherContext,
  DispatchInjectablesTypes,
  DispatchParsedType,
  LookupType,
  MapRepo,
  StringSerializedType,
  Template,
  ValueOrErrors,
} from "../../../../../../../main";
import { Renderer } from "../../../deserializer/domains/specification/domains/forms/domains/renderer/state";
import { ListDispatcher } from "./domains/list/state";
import { MapDispatcher } from "./domains/map/state";
import { MultiSelectionDispatcher } from "./domains/multiSelection/state";
import { OneDispatcher } from "./domains/one/state";
import { PrimitiveDispatcher } from "./domains/primitive/state";
import { RecordDispatcher } from "./domains/record/state";
import { LookupDispatcher } from "./domains/lookup/state";
import { SingleSelectionDispatcher } from "./domains/singleSelectionDispatcher/state";
import { SumDispatcher } from "./domains/sum/state";
import { TableDispatcher } from "./domains/table/state";
import { TupleDispatcher } from "./domains/tupleDispatcher/state";
import { UnionDispatcher } from "./domains/unionDispatcher/state";

export const Dispatcher = {
  Operations: {
    DispatchAs: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      type: DispatchParsedType<T>,
      renderer: Renderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts
      >,
      as: string,
      isNested: boolean,
      formName?: string,
      launcherName?: string,
      api?: string | string[],
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
      Dispatcher.Operations.Dispatch(
        renderer,
        dispatcherContext,
        isNested,
        formName,
        launcherName,
        api,
      ).MapErrors((errors) =>
        errors.map((error) => `${error}\n...When dispatching as: ${as}`),
      ),
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      renderer: Renderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts
      >,
      isNested: boolean,
      formName?: string,
      launcherName?: string,
      api?: string | string[],
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
      renderer.kind == "lookupRenderer"
        ? LookupDispatcher.Operations.Dispatch(
            renderer,
            dispatcherContext,
          ).Then((template) =>
            ValueOrErrors.Default.return([
              template[0].mapContext((_: any) => ({
                ..._,
                type: renderer.type,
              })),
              template[1],
            ]),
          )
        : renderer.kind == "primitiveRenderer"
          ? PrimitiveDispatcher.Operations.Dispatch(
              renderer,
              dispatcherContext,
            ).Then((template) =>
              ValueOrErrors.Default.return([
                template[0].mapContext((_: any) => ({
                  ..._,
                  type: renderer.type,
                })),
                template[1],
              ]),
            )
          : renderer.kind == "recordRenderer"
            ? RecordDispatcher.Operations.Dispatch(
                renderer,
                dispatcherContext,
                isNested,
                formName,
                launcherName,
              ).Then((template) =>
                ValueOrErrors.Default.return([
                  template[0].mapContext((_: any) => ({
                    ..._,
                    type: renderer.type,
                  })),
                  template[1],
                ]),
              )
            : renderer.kind == "listRenderer"
              ? ListDispatcher.Operations.Dispatch(
                  renderer,
                  dispatcherContext,
                ).Then((template) =>
                  ValueOrErrors.Default.return([
                    template[0].mapContext((_: any) => ({
                      ..._,
                      type: renderer.type,
                    })),
                    template[1],
                  ]),
                )
              : renderer.kind == "mapRenderer"
                ? MapDispatcher.Operations.Dispatch(
                    renderer,
                    dispatcherContext,
                  ).Then((template) =>
                    ValueOrErrors.Default.return([
                      template[0].mapContext((_: any) => ({
                        ..._,
                        type: renderer.type,
                      })),
                      template[1],
                    ]),
                  )
                : (renderer.kind == "enumRenderer" ||
                      renderer.kind == "streamRenderer") &&
                    renderer.type.kind == "singleSelection"
                  ? SingleSelectionDispatcher.Operations.Dispatch(
                      renderer,
                      dispatcherContext,
                    ).Then((template) =>
                      ValueOrErrors.Default.return([
                        template[0].mapContext((_: any) => ({
                          ..._,
                          type: renderer.type,
                        })),
                        template[1],
                      ]),
                    )
                  : (renderer.kind == "enumRenderer" ||
                        renderer.kind == "streamRenderer") &&
                      renderer.type.kind == "multiSelection"
                    ? MultiSelectionDispatcher.Operations.Dispatch(
                        renderer,
                        dispatcherContext,
                      ).Then((template) =>
                        ValueOrErrors.Default.return([
                          template[0].mapContext((_: any) => ({
                            ..._,
                            type: renderer.type,
                          })),
                          template[1],
                        ]),
                      )
                    : renderer.kind == "oneRenderer"
                      ? OneDispatcher.Operations.Dispatch(
                          renderer,
                          dispatcherContext,
                        ).Then((template) =>
                          ValueOrErrors.Default.return([
                            template[0].mapContext((_: any) => ({
                              ..._,
                              type: renderer.type,
                            })),
                            template[1],
                          ]),
                        )
                      : renderer.kind == "sumRenderer" ||
                          renderer.kind == "sumUnitDateRenderer"
                        ? SumDispatcher.Operations.Dispatch(
                            renderer,
                            dispatcherContext,
                          ).Then((template) =>
                            ValueOrErrors.Default.return([
                              template[0].mapContext((_: any) => ({
                                ..._,
                                type: renderer.type,
                              })),
                              template[1],
                            ]),
                          )
                        : renderer.kind == "tableRenderer"
                          ? TableDispatcher.Operations.Dispatch(
                              renderer,
                              dispatcherContext,
                              api,
                              isNested,
                              formName,
                              launcherName,
                            ).Then((template) =>
                              ValueOrErrors.Default.return([
                                template[0].mapContext((_: any) => ({
                                  ..._,
                                  type: renderer.type,
                                })),
                                template[1],
                              ]),
                            )
                          : renderer.kind == "tupleRenderer"
                            ? TupleDispatcher.Operations.Dispatch(
                                renderer,
                                dispatcherContext,
                              ).Then((template) =>
                                ValueOrErrors.Default.return([
                                  template[0].mapContext((_: any) => ({
                                    ..._,
                                    type: renderer.type,
                                  })),
                                  template[1],
                                ]),
                              )
                            : renderer.kind == "unionRenderer"
                              ? UnionDispatcher.Operations.Dispatch(
                                  renderer,
                                  dispatcherContext,
                                  isNested,
                                ).Then((template) =>
                                  ValueOrErrors.Default.return([
                                    template[0].mapContext((_: any) => ({
                                      ..._,
                                      type: renderer.type,
                                    })),
                                    template[1],
                                  ]),
                                )
                              : ValueOrErrors.Default.throwOne(
                                  `unknown renderer ${renderer.kind}`,
                                ),
  },
};
