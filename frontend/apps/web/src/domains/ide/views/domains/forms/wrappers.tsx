import {
    EntityFormView,
    PredicateFormLayout,
    Unit,
    unit,
    CreateFormView,
    Template,
    CreateFormContext,
    CreateFormWritableState,
    CreateFormForeignMutationsExpected,
    SimpleCallback,
    FormParsingResult,
    FormConfigValidationAndParseResult,
    EditFormView,
    EditFormWritableState,
    EditFormContext,
    EditFormForeignMutationsExpected,
    RecordAbstractRendererView,
    LookupTypeAbstractRendererView,
} from "ballerina-core";
import {IdeFlags} from "./domains/common/ide-flags.ts";
// import {
//     DispatchPassthroughFormCustomPresentationContext,
//     DispatchPassthroughFormExtraContext,
//     DispatchPassthroughFormFlags,
// } from "./concrete-renderers";

export const DispatchEntityContainerFormView: <PresentationContext,FormExtraContext>() => RecordAbstractRendererView<
    PresentationContext,
    IdeFlags,
    FormExtraContext
> = () => (props) => {
    return (
        <>
            <table>
                <tbody
                    style={{
                        display: "flex",
                        flexDirection: "column",
                        alignItems: "center",
                    }}
                >
                {props.context.layout.valueSeq().map((tab) =>
                    tab.columns.valueSeq().map((column) => (
                        <tr style={{ display: "block", float: "left" }}>
                            {column.groups.valueSeq().map((group) =>
                                group
                                    .filter((fieldName) =>
                                        props.VisibleFieldKeys.has(fieldName),
                                    )
                                    .map((fieldName) => (
                                        <>
                                            <td style={{ display: "block" }}>
                                                {props.EmbeddedFields.get(fieldName)!(undefined)({
                                                    ...props,
                                                    context: {
                                                        ...props.context,
                                                        disabled: props.DisabledFieldKeys.has(fieldName),
                                                    },
                                                    view: unit,
                                                })}
                                            </td>
                                        </>
                                    )),
                            )}
                        </tr>
                    )),
                )}
                </tbody>
            </table>
        </>
    );
};

export const DispatchEntityNestedContainerFormView: <PresentationContext,FormExtraContext>() => RecordAbstractRendererView<
    PresentationContext,
    IdeFlags,
    FormExtraContext
> = () => (props) => {
    return (
        <>
            <table>
                <tbody>
                {props.context.layout.valueSeq().map((tab) =>
                    tab.columns.valueSeq().map((column) => (
                        <tr style={{ display: "block", float: "left" }}>
                            {column.groups.valueSeq().map((group) =>
                                group
                                    .filter((fieldName) =>
                                        props.VisibleFieldKeys.has(fieldName),
                                    )
                                    .map((fieldName) => (
                                        <td style={{ display: "block" }}>
                                            {props.EmbeddedFields.get(fieldName)!(undefined)({
                                                ...props,
                                                context: {
                                                    ...props.context,
                                                    disabled: props.DisabledFieldKeys.has(fieldName),
                                                },
                                                view: unit,
                                            })}
                                        </td>
                                    )),
                            )}
                        </tr>
                    )),
                )}
                </tbody>
            </table>
        </>
    );
};

export const DispatchLookupTypeRenderer: <PresentationContext,FormExtraContext>() => LookupTypeAbstractRendererView<
    PresentationContext,
    IdeFlags,
    FormExtraContext
> = () => (props) => {
    return (
        <>
            {props.embeddedTemplate({
                ...props,
                view: unit,
            })}
        </>
    );
};

export const CreatePersonSubmitButtonWrapper: CreateFormView<any, any> =
    Template.Default<
        CreateFormContext<any, any> & CreateFormWritableState<any, any>,
        CreateFormWritableState<any, any>,
        CreateFormForeignMutationsExpected<any, any> & {
        onSubmit: SimpleCallback<void>;
    },
        {
            actualForm: JSX.Element | undefined;
        }
    >((props) => (
        <>
            {props.view.actualForm}
            <button
                disabled={props.context.customFormState.apiRunner.dirty != "not dirty"}
                onClick={(e) => props.foreignMutations.onSubmit()}
            >
                Submit
            </button>
        </>
    ));

export const EditPersonSubmitButtonWrapper: EditFormView<any, any> =
    Template.Default<
        EditFormContext<any, any> & EditFormWritableState<any, any>,
        EditFormWritableState<any, any>,
        EditFormForeignMutationsExpected<any, any> & {
        onSubmit: SimpleCallback<void>;
    },
        {
            actualForm: JSX.Element | undefined;
        }
    >((props) => (
        <>
            {props.view.actualForm}
            <button
                disabled={props.context.customFormState.apiRunner.dirty != "not dirty"}
                onClick={(e) => props.foreignMutations.onSubmit()}
            >
                Submit
            </button>
        </>
    ));

export const PersonShowFormSetupErrors = (
    validatedFormsConfig: FormConfigValidationAndParseResult<Unit>,
    parsedFormsConfig: FormParsingResult,
) => ({
    form: Template.Default((props: any) => (
        <>
            {validatedFormsConfig.kind == "errors" &&
                JSON.stringify(validatedFormsConfig.errors)}
            {parsedFormsConfig.kind == "r" && JSON.stringify(parsedFormsConfig.value)}
        </>
    )),
    initialState: unit,
});
