import {
  AsyncState,
  builtInsFromFieldViews,
  injectablesFromFieldViews,
  Sum,
  Synchronize,
  Unit,
  FormsConfig,
} from "../../../../../main";
import { CoTypedFactory } from "../../../../coroutines/builder";
import {
  FormParsingResult,
  FormsParserContext,
  FormsParserState,
  parseFormsToLaunchers,
} from "../state";

export const LoadValidateAndParseFormsConfig = <
  T extends { [key in keyof T]: { type: any; state: any } },
>() => {
  const Co = CoTypedFactory<FormsParserContext<T>, FormsParserState>();

  return Co.Template<Unit>(
    Co.GetState().then((current) =>
      Synchronize<Unit, FormParsingResult>(
        async () => {
          const rawFormsConfig = await current.getFormsConfig();
          const builtIns = builtInsFromFieldViews(current.fieldViews);
          const injectedPrimitives = current.injectedPrimitives
            ? injectablesFromFieldViews(
                current.fieldViews,
                current.injectedPrimitives,
              )
            : undefined;
          const validationResult =
            FormsConfig.Default.validateAndParseFormConfig(
              builtIns,
              current.fieldTypeConverters,
              injectedPrimitives,
            )(rawFormsConfig);
          if (validationResult.kind == "errors")
            return Sum.Default.right(validationResult.errors);

          return parseFormsToLaunchers(
            builtIns,
            injectedPrimitives,
            current.fieldTypeConverters,
            current.containerFormView,
            current.nestedContainerFormView,
            current.fieldViews,
            current.infiniteStreamSources,
            current.enumOptionsSources,
            current.entityApis,
            current.tableApiSources,
          )(validationResult.value);
        },
        (_) => "transient failure",
        5,
        50,
      ).embed((_) => _.formsConfig, FormsParserState.Updaters.formsConfig),
    ),
    {
      interval: 15,
      runFilter: (props) =>
        !AsyncState.Operations.hasValue(props.context.formsConfig.sync),
    },
  );
};
