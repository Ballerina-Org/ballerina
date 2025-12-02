import {List} from "immutable";
import {
    Coroutine,
    ForeignMutationsInput,
    Maybe, Option,
    replaceWith,
    simpleUpdater, Sum, Unit,
    Updater,
    ValueOrErrors,
    View,
    Visibility
} from "ballerina-core";
import {INode, Meta} from "../locked/domains/folders/node";
import {JobFlow, JobTrace} from "./domains/job/state";
import {CustomFieldsEvent, DomainEvent, JobLifecycleEvent, transitionUpdater} from "./domains/event/state";
import {TypeCheckingProvider} from "./domains/data-provider/state";
import {Guid} from "../../types/Guid";
import {Co} from "../../../coroutines/custom-fields/builder";
import {TypeCheckingPayload} from "./domains/type-checking/state";

export type CustomFieldsProcess =
    | { kind: "idle" }
    | { kind: "type-checking" }
    | { kind: "construction" }
    | { kind: "updater" } // optional
    | { kind: "value" }
    | { kind: "result"; value: ValueOrErrors<any, any> };

export type CustomFieldsContext = {
    state: CustomFieldsProcess;
    flow: JobFlow;
};

export type SimDocument = {
    content: string,
    enabled: boolean,
}

export type CustomFields = { 
    errors: List<string>,
    status: CustomFieldsProcess,
    visibility: Visibility,
    jobFlow: JobFlow,
    document: SimDocument,
    
    value: Option<Guid>,
    delta: Option<string>
}

export const CustomFields = {
    Default: (): CustomFields => ({
        errors: List<string>(),
        visibility: "fully-invisible",
        document: { content: "", enabled : false },
        status: { kind: 'idle' },
        jobFlow: {
            traces: [],
            kind: "in-progress"
        },
        value: Option.Default.none(),
        delta: Option.Default.none()
    }),
    Updaters: {
        Core: {
            ...simpleUpdater<CustomFields>()("errors"),
            ...simpleUpdater<CustomFields>()("status"),
            ...simpleUpdater<CustomFields>()("jobFlow"),
            ...simpleUpdater<CustomFields>()("visibility"),
            ...simpleUpdater<CustomFields>()("value"),
            ...simpleUpdater<CustomFields>()("delta"),
            ...simpleUpdater<CustomFields>()("document"),
        },
        Coroutine: {
            errorAndFail: (msg: string) =>
                CustomFields.Updaters.Core.errors(
                    replaceWith(
                        List([msg])
                    )
                ).then(
                    CustomFields.Updaters.Core.jobFlow(
                        (flow: JobFlow) => ({
                            ...flow,
                            kind: "finished",
                            result: ValueOrErrors.Default.throw(List([msg]))
                        })
                    )
                ),
            transition: (event: CustomFieldsEvent): Updater<CustomFields> =>
                Updater(fields => {
                    //console.log("event:" + JSON.stringify(event, null, 2));
                    const {state, flow} = transitionUpdater(event)({flow: fields.jobFlow, state: fields.status});
                    const u =
                        CustomFields.Updaters.Core.status(replaceWith(state))
                            .then(CustomFields.Updaters.Core.jobFlow(replaceWith(flow)))
                    return u(fields)
                }),
            isTypeCheckingRequested(
                fields: CustomFields
            ): Option<TypeCheckingPayload> {
                if (fields.status.kind === "type-checking") {
                    const currentTrace = CustomFields.Operations.currentJobTrace(fields.jobFlow);
                    if(currentTrace !== undefined && currentTrace.kind === "requested") return Option.Default.some(currentTrace.job.payload as TypeCheckingPayload)
                }

                return Option.Default.none();
            },
            isTypeCheckingDispatched(
                fields: CustomFields
            ): Option<Guid> {
                if (fields.status.kind === "type-checking") {
                    const currentTrace = CustomFields.Operations.currentJobTrace(fields.jobFlow);
                    if(currentTrace !== undefined && currentTrace.kind === "dispatched" && currentTrace.initial.kind == "r") 
                        return Option.Default.some( currentTrace.initial.value.jobId)
                }

                return Option.Default.none();
            },
            isTypeCheckingCompleted(
                fields: CustomFields
            ): Option<Guid> {
                if (fields.status.kind === "type-checking") {
                    const currentTrace = CustomFields.Operations.currentJobTrace(fields.jobFlow);
                    if(currentTrace !== undefined && currentTrace.kind === "completed" && currentTrace.result.kind == 'type-checking')
                        return Option.Default.some( currentTrace.result.result.result.valueDescriptorId)
                }

                return Option.Default.none();
            },
            isConstructionCompleted(
                fields: CustomFields
            ): Option<Guid> {
                if (fields.status.kind === "construction") {
                    const currentTrace = CustomFields.Operations.currentJobTrace(fields.jobFlow);
                    if(currentTrace !== undefined && currentTrace.kind === "completed" && currentTrace.result.kind == 'construction')
                        return Option.Default.some( currentTrace.result.result.result.valueId)
                }

                return Option.Default.none();
            },
            isConstructionDispatched(
                fields: CustomFields
            ): Option<Guid> {
                if (fields.status.kind === "construction") {
                    const currentTrace = CustomFields.Operations.currentJobTrace(fields.jobFlow);
                    if(currentTrace !== undefined && currentTrace.kind === "dispatched" && currentTrace.initial.kind == "r")
                        return Option.Default.some( currentTrace.initial.value.jobId)
                }
    
                return Option.Default.none();
            }
            
        },
        Template: {
            toggle: (): Updater<CustomFields> => 
                Updater(cf =>
                    CustomFields.Updaters.Core.visibility(
                        replaceWith(cf.visibility == 'fully-invisible' ? 'fully-visible' : 'fully-invisible' as Visibility))(cf)
                ),
            start: (provider: TypeCheckingProvider): Updater<CustomFields> =>
                Updater(fields => {
                    const payload = provider.collect()
                    if(payload.kind == "errors") return ({...fields, errors: payload.errors});
                    
                    const u =
                        CustomFields.Updaters.Coroutine.transition({kind: "begin-type-checking"} satisfies DomainEvent)
                            .then(CustomFields.Updaters.Coroutine.transition(
                                {
                                    kind: "start-job",
                                    job: {kind: 'type-checking', payload: payload.value}
                                } satisfies JobLifecycleEvent))
                    return u(fields)
                }),
            update: (provider: TypeCheckingProvider): Updater<CustomFields> =>
                Updater(fields => {
                    const delta = provider.delta()
                    debugger
                    if(delta.kind == "errors") return ({...fields, errors: delta.errors});
                    const u =
                        CustomFields.Updaters.Core.delta(replaceWith(Option.Default.some(delta.value))).then(
                        CustomFields.Updaters.Coroutine.transition({kind: "begin-updater"} satisfies DomainEvent))
                  
                    return u(fields)
                })
        }
    },
    Operations: {
        idle: (): CustomFieldsProcess => ({ kind: 'idle' }),
        isAvailable: (node: INode<Meta>): boolean => {
            return true
        },
        currentJobTrace: (flow: JobFlow): Maybe<JobTrace> => {
            const traces = flow.traces;
            return traces.length === 0 ? undefined : traces[traces.length - 1];
        },
        checkResponseForErrors:
            <response>(res: Sum<ValueOrErrors<response, any>, any>, name: string): Option<Coroutine<CustomFields & {
                provider: TypeCheckingProvider
            }, CustomFields, Unit>> => {
                if (res.kind == "r") {
                    return Option.Default.some(Co.SetState(CustomFields.Updaters.Coroutine.errorAndFail(`Unknown error on ${name}  status, sum: ${JSON.stringify(res)}`)))
                } else if (res.value.kind == "errors")

                    return Option.Default.some(Co.SetState(CustomFields.Updaters.Coroutine.errorAndFail(`Unknown error on ${name}  status, voe: ${JSON.stringify(res)}`)))

                return Option.Default.none()
            },
    },
    ForeignMutations: (
        _: ForeignMutationsInput<Unit, CustomFields>,
    ) => ({
    }),
};

export type CustomFieldsForeignMutationsExpected = {}

export type CustomFieldsCtx = CustomFields & { provider: TypeCheckingProvider }

export type CustomFieldsView = View<
    CustomFieldsCtx,
    CustomFields,
    CustomFieldsForeignMutationsExpected,
    {
    }
>;