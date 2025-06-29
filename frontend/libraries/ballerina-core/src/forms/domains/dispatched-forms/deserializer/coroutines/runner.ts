import {
  AsyncState,
  injectedPrimitivesFromConcreteRenderers,
  Sum,
  Synchronize,
  Unit,
  Specification,
  ValueOrErrors,
  DispatchInjectablesTypes,
} from "../../../../../../main";
import { CoTypedFactory } from "../../../../../coroutines/builder";
import {
  DispatchSpecificationDeserializationResult,
  DispatchFormsParserContext,
  DispatchFormsParserState,
  parseDispatchFormsToLaunchers,
} from "../state";

export const LoadAndDeserializeSpecification = <
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
>() => {
  const Co = CoTypedFactory<
    DispatchFormsParserContext<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
    DispatchFormsParserState<T, Flags, CustomPresentationContexts, ExtraContext>
  >();

  return Co.Template<Unit>(
    Co.GetState().then((current) =>
      Synchronize<
        Unit,
        DispatchSpecificationDeserializationResult<
          T,
          Flags,
          CustomPresentationContexts,
          ExtraContext
        >
      >(
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
          const injectedPrimitivesResult = current.injectedPrimitives
            ? injectedPrimitivesFromConcreteRenderers(
                current.concreteRenderers,
                current.injectedPrimitives,
              )
            : ValueOrErrors.Default.return(undefined);

          if (injectedPrimitivesResult.kind == "errors") {
            console.error(
              injectedPrimitivesResult.errors.valueSeq().toArray().join("\n"),
            );
            return ValueOrErrors.Default.throwOne(
              "Error getting injected primitives in LoadAndDeserializeSpecification: " +
                injectedPrimitivesResult.errors.valueSeq().toArray().join("\n"),
            );
          }

          const injectedPrimitives = injectedPrimitivesResult.value;

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
            current.lookupTypeRenderer,
            current.defaultRecordConcreteRenderer,
            current.defaultNestedRecordConcreteRenderer,
            current.concreteRenderers,
            current.infiniteStreamSources,
            current.enumOptionsSources,
            current.entityApis,
            current.IdWrapper,
            current.ErrorRenderer,
            current.tableApiSources,
            current.lookupSources,
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
        DispatchFormsParserState<
          T,
          Flags,
          CustomPresentationContexts,
          ExtraContext
        >().Updaters.deserializedSpecification,
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
