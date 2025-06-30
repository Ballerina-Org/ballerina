import { List, Map, OrderedMap } from "immutable";
import {
  Expr,
  RecordType,
  ValueOrErrors,
  DispatcherContext,
  Template,
  MapRepo,
  RecordAbstractRenderer,
  DispatchInjectablesTypes,
  RecordAbstractRendererView,
  StringSerializedType,
} from "../../../../../../../../../main";
import { RecordRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/record/state";
import { RecordFieldDispatcher } from "./recordField/state";

export const RecordDispatcher = {
  Operations: {
    GetRecordConcreteRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      concreteRenderer: string | undefined,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isNested: boolean,
    ): ValueOrErrors<
      RecordAbstractRendererView<
        CustomPresentationContexts,
        Flags,
        ExtraContext
      >,
      string
    > =>
      concreteRenderer == undefined
        ? ValueOrErrors.Default.return(
            dispatcherContext.getDefaultRecordRenderer(isNested),
          )
        : dispatcherContext.getConcreteRenderer("record", concreteRenderer),
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: RecordRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isNested: boolean,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
      ValueOrErrors.Operations.All(
        List<
          ValueOrErrors<
            [
              string,
              {
                template: Template<any, any, any, any>;
                visible?: Expr;
                disabled?: Expr;
                label?: string;
                GetDefaultState: () => any;
              },
              StringSerializedType,
            ],
            string
          >
        >(
          renderer.fields
            .entrySeq()
            .toArray()
            .map(([fieldName, fieldRenderer]) => {
              const res = MapRepo.Operations.tryFindWithError(
                fieldName,
                renderer.type.fields,
                () => `cannot find field "${fieldName}" in types`,
              );

              return res.Then((fieldType) =>
                RecordFieldDispatcher.Operations.Dispatch(
                  fieldName,
                  fieldRenderer,
                  dispatcherContext,
                  isInlined,
                  tableApi,
                ).Then(([template, serializedType]) =>
                  dispatcherContext
                    .defaultState(fieldType, fieldRenderer.renderer)
                    .Then((defaultState) =>
                      ValueOrErrors.Default.return([
                        fieldName,
                        {
                          template,
                          visible: fieldRenderer.visible,
                          disabled: fieldRenderer.disabled,
                          label: fieldRenderer.label,
                          GetDefaultState: () => defaultState,
                        },
                        serializedType,
                      ]),
                    ),
                ),
              );
            }),
        ),
      )
        .Then((fieldTemplates) =>
          RecordDispatcher.Operations.GetRecordConcreteRenderer(
            renderer.concreteRenderer,
            dispatcherContext,
            isNested,
          ).Then((concreteRenderer) => {
            const serializedType = RecordType.SerializeToString(
              OrderedMap(
                fieldTemplates
                  .toArray()
                  .map((template) => [template[0], template[2]]),
              ),
            );
            return ValueOrErrors.Default.return<
              [Template<any, any, any, any>, StringSerializedType],
              string
            >([
              RecordAbstractRenderer<
                CustomPresentationContexts,
                Flags,
                ExtraContext
              >(
                Map(
                  fieldTemplates.map((template) => [template[0], template[1]]),
                ),
                renderer.tabs,
                dispatcherContext.IdProvider,
                dispatcherContext.ErrorRenderer,
                isInlined,
                serializedType,
              )
                .mapContext((_: any) => ({
                  ..._,
                  type: renderer.type,
                }))
                .withView(concreteRenderer),
              serializedType,
            ]);
          }),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching as record form`),
        ),
  },
};
