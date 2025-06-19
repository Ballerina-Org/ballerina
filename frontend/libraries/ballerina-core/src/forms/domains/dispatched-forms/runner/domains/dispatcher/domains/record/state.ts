import { List, Map } from "immutable";
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
} from "../../../../../../../../../main";
import { RecordRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/record/state";
import { RecordFieldDispatcher } from "./recordField/state";
import { Renderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/state";

export const RecordDispatcher = {
  Operations: {
    GetRecordConcreteRenderer: <T extends DispatchInjectablesTypes<T>, Flags, CustomPresentationContexts>(
      renderer: Renderer<T> | undefined,
      dispatcherContext: DispatcherContext<T, Flags, CustomPresentationContexts>,
      isNested: boolean,
    ): ValueOrErrors<RecordAbstractRendererView<any, any>, string> =>
      renderer != undefined && renderer.kind != "lookupRenderer"
        ? ValueOrErrors.Default.throwOne(
            "expected renderer.kind == 'lookupRenderer' but got " +
              renderer.kind,
          )
        : renderer == undefined
          ? ValueOrErrors.Default.return(
              dispatcherContext.getDefaultRecordRenderer(isNested),
            )
          : dispatcherContext.getConcreteRenderer("record", renderer.renderer),
    Dispatch: <T extends DispatchInjectablesTypes<T>, Flags, CustomPresentationContexts>(
      type: RecordType<T>,
      renderer: RecordRenderer<T>,
      dispatcherContext: DispatcherContext<T, Flags, CustomPresentationContexts>,
      isNested: boolean,
      formName?: string,
      launcherName?: string,
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
                type.fields,
                () => `cannot find field "${fieldName}" in types`,
              );

              return res.Then((fieldType) =>
                RecordFieldDispatcher.Operations.Dispatch(
                  fieldName,
                  fieldRenderer,
                  dispatcherContext,
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
            renderer.renderer,
            dispatcherContext,
            isNested,
          ).Then((concreteRenderer) =>
            !isNested && launcherName == undefined
              ? ValueOrErrors.Default.throwOne<
                  Template<any, any, any, any>,
                  string
                >(
                  "internal error: launcherName is required for top level forms",
                )
              : formName == undefined
                ? ValueOrErrors.Default.throwOne<
                    Template<any, any, any, any>,
                    string
                  >("internal error: formName is required for all forms")
                : ValueOrErrors.Default.return(
                    RecordAbstractRenderer(
                      Map(fieldTemplates),
                      renderer.tabs,
                      dispatcherContext.IdProvider,
                      dispatcherContext.ErrorRenderer,
                      renderer.isInlined,
                    )
                      .mapContext((_: any) => ({
                        ..._,
                        type: renderer.type,
                        ...(!isNested && launcherName
                          ? {
                              identifiers: {
                                // withLauncher: `[${launcherName}][${formName}]`,
                                // withoutLauncher: `[${formName}]`,
                                withLauncher: `[${type.name}]`,
                                withoutLauncher: `[${type.name}]`,
                              },
                            }
                          : {}),
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
