import {
  DispatcherContext,
  DispatchParsedType,
  DispatchPrimitiveType,
  MapRepo,
  Template,
  ValueOrErrors,
} from "../../../../../../../main";
import { Renderer } from "../../../deserializer/domains/specification/domains/forms/domains/renderer/state";
import { ListDispatcher } from "./domains/list/state";
import { LookupDispatcher } from "./domains/lookup/state";
import { MapDispatcher } from "./domains/map/state";
import { MultiSelectionDispatcher } from "./domains/multiSelection/state";
import { OneDispatcher } from "./domains/one/state";
import { PrimitiveDispatcher } from "./domains/primitive/state";
import { RecordDispatcher } from "./domains/record/state";
import { SingleSelectionDispatcher } from "./domains/singleSelectionDispatcher/state";
import { SumDispatcher } from "./domains/sum/state";
import { TableDispatcher } from "./domains/table/state";
import { TupleDispatcher } from "./domains/tupleDispatcher/state";
import { UnionDispatcher } from "./domains/unionDispatcher/state";

export const Dispatcher = {
  Operations: {
    DispatchAs: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: DispatchParsedType<T>,
      renderer: Renderer<T>,
      dispatcherContext: DispatcherContext<T>,
      as: string,
      isNested: boolean,
      formName?: string,
      launcherName?: string,
      api?: string | string[],
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      Dispatcher.Operations.Dispatch(
        type,
        renderer,
        dispatcherContext,
        isNested,
        formName,
        launcherName,
        api,
      ).MapErrors((errors) =>
        errors.map((error) => `${error}\n...When dispatching as: ${as}`),
      ),
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: DispatchParsedType<T>,
      renderer: Renderer<T>,
      dispatcherContext: DispatcherContext<T>,
      isNested: boolean,
      formName?: string,
      launcherName?: string,
      api?: string | string[],
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      type.kind == "lookup" && renderer.kind == "lookupRenderer"
        ? DispatchParsedType.Operations.ResolveLookupType(
            type.name,
            dispatcherContext.types,
          ).Then((lookupType) =>
            MapRepo.Operations.tryFindWithError(
              renderer.renderer,
              dispatcherContext.forms,
              () => `cannot find form ${lookupType}`,
            ).Then((formRenderer) =>
              Dispatcher.Operations.Dispatch(
                lookupType,
                formRenderer,
                dispatcherContext,
                isNested,
                formName,
                launcherName,
                api ?? renderer.api,
              ).Then((template) =>
                ValueOrErrors.Default.return(
                  template.mapContext((_: any) => ({
                    ..._,
                    type: renderer.type,
                  })),
                ),
              ),
            ),
          )
        : type.kind == "lookup"
          ? DispatchParsedType.Operations.ResolveLookupType(
              type.name,
              dispatcherContext.types,
            ).Then((lookupType) =>
              Dispatcher.Operations.Dispatch(
                lookupType,
                renderer,
                dispatcherContext,
                isNested,
                formName,
                launcherName,
                api,
              ).Then((template) =>
                ValueOrErrors.Default.return(
                  template.mapContext((_: any) => ({
                    ..._,
                    type: renderer.type,
                  })),
                ),
              ),
            )
          : renderer.kind == "recordRenderer" && type.kind == "record"
            ? RecordDispatcher.Operations.Dispatch(
                type,
                renderer,
                dispatcherContext,
                isNested,
                formName,
                launcherName,
              ).Then((template) =>
                ValueOrErrors.Default.return(
                  template.mapContext((_: any) => ({
                    ..._,
                    type: renderer.type,
                  })),
                ),
              )
            : renderer.kind == "lookupRenderer" && type.kind == "primitive"
              ? PrimitiveDispatcher.Operations.Dispatch(
                  type,
                  renderer,
                  dispatcherContext,
                ).Then((template) =>
                  ValueOrErrors.Default.return(
                    template.mapContext((_: any) => ({
                      ..._,
                      type: renderer.type,
                    })),
                  ),
                )
              : renderer.kind == "lookupRenderer" //check if case is needed
                ? MapRepo.Operations.tryFindWithError(
                    renderer.renderer,
                    dispatcherContext.forms,
                    () => `cannot find form ${renderer.renderer}`,
                  )
                    .Then((formRenderer) =>
                      Dispatcher.Operations.Dispatch(
                        type,
                        formRenderer,
                        dispatcherContext,
                        true,
                        renderer.renderer,
                        launcherName,
                        api ?? renderer.api,
                      ),
                    )
                    .Then((template) =>
                      ValueOrErrors.Default.return(
                        template.mapContext((_: any) => ({
                          ..._,
                          type: renderer.type,
                        })),
                      ),
                    )
                : renderer.kind == "listRenderer" && type.kind == "list"
                  ? ListDispatcher.Operations.Dispatch(
                      type,
                      renderer,
                      dispatcherContext,
                    ).Then((template) =>
                      ValueOrErrors.Default.return(
                        template.mapContext((_: any) => ({
                          ..._,
                          type: renderer.type,
                        })),
                      ),
                    )
                  : renderer.kind == "mapRenderer" && type.kind == "map"
                    ? MapDispatcher.Operations.Dispatch(
                        type,
                        renderer,
                        dispatcherContext,
                      ).Then((template) =>
                        ValueOrErrors.Default.return(
                          template.mapContext((_: any) => ({
                            ..._,
                            type: renderer.type,
                          })),
                        ),
                      )
                    : type.kind == "multiSelection" &&
                        (renderer.kind == "enumRenderer" ||
                          renderer.kind == "streamRenderer")
                      ? MultiSelectionDispatcher.Operations.Dispatch(
                          renderer,
                          dispatcherContext,
                        ).Then((template) =>
                          ValueOrErrors.Default.return(
                            template.mapContext((_: any) => ({
                              ..._,
                              type: renderer.type,
                            })),
                          ),
                        )
                      : type.kind == "one" && renderer.kind == "oneRenderer"
                        ? OneDispatcher.Operations.Dispatch(
                            type,
                            renderer,
                            dispatcherContext,
                          ).Then((template) =>
                            ValueOrErrors.Default.return(
                              template.mapContext((_: any) => ({
                                ..._,
                                type: renderer.type,
                              })),
                            ),
                          )
                        : type.kind == "singleSelection" &&
                            (renderer.kind == "enumRenderer" ||
                              renderer.kind == "streamRenderer")
                          ? SingleSelectionDispatcher.Operations.Dispatch(
                              renderer,
                              dispatcherContext,
                            ).Then((template) =>
                              ValueOrErrors.Default.return(
                                template.mapContext((_: any) => ({
                                  ..._,
                                  type: renderer.type,
                                })),
                              ),
                            )
                          : type.kind == "sum" &&
                              (renderer.kind == "sumRenderer" ||
                                renderer.kind == "sumUnitDateRenderer")
                            ? SumDispatcher.Operations.Dispatch(
                                renderer,
                                dispatcherContext,
                              ).Then((template) =>
                                ValueOrErrors.Default.return(
                                  template.mapContext((_: any) => ({
                                    ..._,
                                    type: renderer.type,
                                  })),
                                ),
                              )
                            : type.kind == "table" &&
                                renderer.kind == "tableRenderer"
                              ? TableDispatcher.Operations.Dispatch(
                                  type,
                                  renderer,
                                  dispatcherContext,
                                  api,
                                  isNested,
                                  formName,
                                  launcherName,
                                ).Then((template) =>
                                  ValueOrErrors.Default.return(
                                    template.mapContext((_: any) => ({
                                      ..._,
                                      type: renderer.type,
                                    })),
                                  ),
                                )
                              : type.kind == "tuple" &&
                                  renderer.kind == "tupleRenderer"
                                ? TupleDispatcher.Operations.Dispatch(
                                    type,
                                    renderer,
                                    dispatcherContext,
                                  ).Then((template) =>
                                    ValueOrErrors.Default.return(
                                      template.mapContext((_: any) => ({
                                        ..._,
                                        type: renderer.type,
                                      })),
                                    ),
                                  )
                                : type.kind == "union" &&
                                    renderer.kind == "unionRenderer"
                                  ? UnionDispatcher.Operations.Dispatch(
                                      type,
                                      renderer,
                                      dispatcherContext,
                                      isNested,
                                    ).Then((template) =>
                                      ValueOrErrors.Default.return(
                                        template.mapContext((_: any) => ({
                                          ..._,
                                          type: renderer.type,
                                        })),
                                      ),
                                    )
                                  : ValueOrErrors.Default.throwOne(
                                      `non matching renderer ${renderer.kind} and type ${type.kind}`,
                                    ),
  },
};
