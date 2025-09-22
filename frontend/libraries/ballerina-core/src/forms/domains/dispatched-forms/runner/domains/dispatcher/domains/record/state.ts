import { List, Map } from "immutable";
import {
  Expr,
  ValueOrErrors,
  Template,
  MapRepo,
  RecordAbstractRenderer,
  DispatchInjectablesTypes,
  RecordAbstractRendererView,
} from "../../../../../../../../../main";
import { RecordRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/record/state";
import { RecordFieldDispatcher } from "./recordField/state";
import { DispatcherContextWithApiSources } from "../../../../coroutines/runner";

export const RecordDispatcher = {
  Operations: {
    GetRecordConcreteRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      concreteRenderer: string | undefined,
      dispatcherContext: DispatcherContextWithApiSources<
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
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isNested: boolean,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
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
                ).Then((template) =>
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
          ).Then((concreteRenderer) =>
            ValueOrErrors.Default.return<Template<any, any, any, any>, string>(
              RecordAbstractRenderer<
                CustomPresentationContexts,
                Flags,
                ExtraContext
              >(
                Map(
                  fieldTemplates.map((template) => [template[0], template[1]]),
                ),
                renderer.tabs,
                renderer.disabledFields,
                dispatcherContext.IdProvider,
                dispatcherContext.ErrorRenderer,
                isInlined,
              )
                .mapContext((_: any) => ({
                  ..._,
                  type: renderer.type,
                }))
                .withView(concreteRenderer),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching as record form`),
        ),
  },
};
