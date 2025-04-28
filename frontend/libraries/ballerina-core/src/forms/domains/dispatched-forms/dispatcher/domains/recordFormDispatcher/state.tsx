import {
  DispatcherContext,
  RecordAbstractRenderer,
  Template,
} from "../../../../../../../main";
import { ValueOrErrors } from "../../../../../../collections/domains/valueOrErrors/state";
import { List, Map } from "immutable";
import { RecordFieldDispatcher } from "./domains/recordFieldDispatcher/state";
import { Expr } from "../../../../parser/domains/predicates/state";
import { RecordType } from "../../../deserializer/domains/specification/domains/types/state";
import { RecordFormRenderer } from "../../../deserializer/domains/specification/domains/form/domains/renderers/domains/recordFormRenderer/state";

export const RecordFormDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: RecordType<T>,
      renderer: RecordFormRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
      isNested: boolean = false,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      ValueOrErrors.Operations.All(
        List<
          ValueOrErrors<
            [
              string,
              {
                template: Template<any, any, any, any>;
                visible: Expr;
                disabled: Expr;
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
              const fieldType = type.fields.get(fieldName);
              if (fieldType == undefined) {
                return ValueOrErrors.Default.throwOne(
                  `cannot find type "${fieldType}" for field "${fieldName}" in types`,
                );
              }
              return RecordFieldDispatcher.Operations.Dispatch(
                fieldName,
                fieldType,
                fieldRenderer,
                dispatcherContext,
              ).Then((template) =>
                dispatcherContext
                  .defaultState(fieldType, fieldRenderer)
                  .Then((defaultState) =>
                    ValueOrErrors.Default.return([
                      fieldName,
                      {
                        template,
                        visible: fieldRenderer.visible,
                        disabled: fieldRenderer.disabled,
                        GetDefaultState: () => defaultState,
                      },
                    ]),
                  ),
              );
            }),
        ),
      )
        .Then((fieldTemplates) =>
          dispatcherContext
            .getConcreteRenderer(
              "record",
              renderer.concreteRendererName,
              isNested,
            )
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return(
                RecordAbstractRenderer(Map(fieldTemplates), renderer.tabs)
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
