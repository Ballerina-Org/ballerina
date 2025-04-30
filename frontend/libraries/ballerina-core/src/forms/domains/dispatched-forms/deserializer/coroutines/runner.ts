import {
  AsyncState,
  injectablesFromFieldViews,
  Sum,
  Synchronize,
  Unit,
  Specification,
  ValueOrErrors,
} from "../../../../../../main";
import { CoTypedFactory } from "../../../../../coroutines/builder";
import {
  DispatchSpecificationDeserializationResult,
  DispatchFormsParserContext,
  DispatchFormsParserState,
  parseDispatchFormsToLaunchers,
} from "../state";

export const LoadAndDeserializeSpecification = <
  T extends { [key in keyof T]: { type: any; state: any } },
>() => {
  const Co = CoTypedFactory<
    DispatchFormsParserContext<T>,
    DispatchFormsParserState<T>
  >();

  return Co.Template<Unit>(
    Co.GetState().then((current) =>
      Synchronize<Unit, DispatchSpecificationDeserializationResult<T>>(
        async () => {
          const serializedSpecifications = await current
            .getFormsConfig()
            .catch((_) => {
              console.error(
                `Error getting forms config in LoadAndDeserializeSpecification: ${_}`,
              );
              return undefined;
            });
          if (serializedSpecifications == undefined) {
            return ValueOrErrors.Default.throwOne(
              "Error getting forms config in LoadAndDeserializeSpecification",
            );
          }
          const injectedPrimitives = current.injectedPrimitives
            ? injectablesFromFieldViews(
                current.concreteRenderers,
                current.injectedPrimitives,
              )
            : undefined;

          const deserializationResult = Specification.Operations.Deserialize(
            current.fieldTypeConverters,
            current.concreteRenderers,
            injectedPrimitives,
          )(serializedSpecifications);

          if (deserializationResult.kind == "errors") {
            console.error(
              deserializationResult.errors.valueSeq().toArray().join("\n"),
            );
            return deserializationResult;
          }

          const result = parseDispatchFormsToLaunchers(
            injectedPrimitives,
            current.fieldTypeConverters,
            current.defaultRecordConcreteRenderer,
            current.defaultNestedRecordConcreteRenderer,
            current.concreteRenderers,
            current.infiniteStreamSources,
            current.enumOptionsSources,
            current.entityApis,
            current.tableApiSources,
          )(deserializationResult.value);

          if (result.kind == "errors") {
            console.error(result.errors.valueSeq().toArray().join("\n"));
            return result;
          }
          return result;
        },
        (_) => "transient failure",
        5,
        50,
      ).embed(
        (_) => _.deserializedSpecification,
        DispatchFormsParserState<T>().Updaters.deserializedSpecification,
      ),
    ),
    {
      interval: 15,
      runFilter: (props) =>
        !AsyncState.Operations.hasValue(
          props.context.deserializedSpecification.sync,
        ),
    },
  );
};
