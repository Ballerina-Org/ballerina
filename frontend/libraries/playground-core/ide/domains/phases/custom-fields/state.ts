import {List} from "immutable";
import {
    ForeignMutationsInput,
    Guid,
    Maybe, Option,
    replaceWith,
    SimpleCallback,
    simpleUpdater, Template, Unit,
    Updater, Value,
    ValueOrErrors,
    View,
    Visibility
} from "ballerina-core";
import {VirtualFolders, WorkspaceState} from "../locked/domains/folders/state";
import {FlatNode, INode, Meta} from "../locked/domains/folders/node";
import {TypeCheckingPayload} from "./domains/type-checking/state";
import {JobFlow, JobTrace, TypeCheckingJob} from "./domains/job/state";
import {CustomFieldsEvent, DomainEvent, JobLifecycleEvent, transitionUpdater} from "./domains/event/state";
import * as repl from "node:repl";
import {LockedPhaseForeignMutationsExpected, LockedPhaseView} from "../locked/state";
import {HeroPhaseForeignMutationsExpected, HeroPhaseView} from "../hero/state";
import {SelectionPhaseForeignMutationsExpected, SelectionPhaseView} from "../selection/state";
import {BootstrapPhaseForeignMutationsExpected, BootstrapPhaseView} from "../bootstrap/state";

/*

Domain state machine moves between macro steps (type checking, construction, updater, ...)
JobFlow manages micro job steps (protocol) (request, initial response, response)

*/

export type CustomFieldsProcess =
    | { kind: "idle" }
    | { kind: "type-checking" }
    | { kind: "construction" }
    | { kind: "updater" } // optional
    | { kind: "result"; value: ValueOrErrors<any, any> };

export type CustomFieldsContext = {
    state: CustomFieldsProcess;
    flow: JobFlow;
};

export type CustomFields = { 
    errors: List<string>,
    status: CustomFieldsProcess,
    visibility: Visibility,
    folder: Option<INode<Meta>>,
    jobFlow: JobFlow
}

export const CustomFields = {
    Default: (): CustomFields => ({
        errors: List<string>(),
        visibility: "fully-invisible",
        folder: Option.Default.none(),
        status: { kind: 'idle' },
        jobFlow: {
            traces: [],
            kind: "in-progress"
        }
    }),
    Updaters: {
        Core: {
            ...simpleUpdater<CustomFields>()("errors"),
            ...simpleUpdater<CustomFields>()("status"),
            ...simpleUpdater<CustomFields>()("jobFlow"),
            ...simpleUpdater<CustomFields>()("visibility"),
            ...simpleUpdater<CustomFields>()("folder"),
        },
        Coroutine: {
            //start: () => CustomFields.Updaters.Core.collectTypeCheckingData()
            transition: (event: CustomFieldsEvent): Updater<CustomFields> =>
                Updater(fields => {
                    console.log("event:" + JSON.stringify(event, null, 2));
                    const { state, flow } = transitionUpdater(event)({ flow: fields.jobFlow, state: fields.status});
                    const u = 
                        CustomFields.Updaters.Core.status(replaceWith(state))
                            .then(CustomFields.Updaters.Core.jobFlow(replaceWith(flow)))
                    return u(fields)
                })
        },
        Template: {
            toggle: (): Updater<CustomFields> => 
                Updater(cf =>
                    CustomFields.Updaters.Core.visibility(
                        replaceWith(cf.visibility == 'fully-invisible' ? 'fully-visible' : 'fully-invisible' as Visibility))(cf)
                ),
            start: (): Updater<CustomFields> =>
                Updater(fields => {
                    if(fields.folder.kind == 'l') return ({...fields, errors: List(["Can't start custom fields without set folder"])})
                    const payload = CustomFields.Operations.collectTypeCheckingData(fields.folder.value);
                    if(payload.kind == "errors") return ({...fields, errors: payload.errors});
                    
                    const u =
                        CustomFields.Updaters.Coroutine.transition({kind: "begin-type-checking"} satisfies DomainEvent)
                            .then(CustomFields.Updaters.Coroutine.transition(
                                {
                                    kind: "start-job",
                                    job: {kind: 'type-checking', payload: payload.value}
                                } satisfies JobLifecycleEvent))
                    return u(fields)
                })
        }
    },
    Operations: {
        idle: (): CustomFieldsProcess => ({ kind: 'idle' }),
        isAvailable: (node: INode<Meta>): boolean => {
            //debugger
            return true //node.name == "customfields"
        } ,// !node.isBranch && node.name.endsWith(".fs"),
        currentJobTrace: (flow: JobFlow): Maybe<JobTrace> => {
            const traces = flow.traces;
            return traces.length === 0 ? undefined : traces[traces.length - 1];
        },
        // collecting type checking data depends on how the editor is implemented, it can be a simple text box(es)
        // here we re-use virtual folders for each entry
        collectTypeCheckingData: (folder: INode<Meta>) : ValueOrErrors<TypeCheckingPayload, string> => {
            const f = FlatNode.Operations.findFolderByPath(folder, folder.metadata.path)
            debugger
            if (f.kind == 'l') return ValueOrErrors.Default.throw(List(["Can't find 'types.fs' file"]));
            
            const files = (f.value.children || []).filter( x => x.metadata.kind == 'file')
            const types = files.filter(x => x.name == 'types.fs')[0]
            const constructor = files.filter(x => x.name == 'constructor.fs')[0]
            const evidence = files.filter(x => x.name == 'evidence.fs')[0]
            const uncertainties = files.filter(x => x.name == 'uncertainties.fs')[0]
            const updater = files.filter(x => x.name == 'update.fs')[0]
            
            const accessors = (f.value.children || []).filter(x => x.name == 'accessors' && x.metadata.kind == 'dir')[0].children
            const a = Object.fromEntries(
                (accessors || []).map(a => [a.name.replace(/\.fs$/, ""), a.metadata.content])
            );
            
            const payload = {
                Constructor: constructor.metadata.content,
                Updater: updater.metadata.content,
                Types: types.metadata.content,
                Uncertainties: uncertainties.metadata.content,
                Evidence: evidence.metadata.content,
                Accessors: a,
            } satisfies TypeCheckingPayload
            debugger
            console.log(JSON.stringify(payload, null, 2));
            return ValueOrErrors.Default.return(payload)
        
        }
    },
    ForeignMutations: (
        _: ForeignMutationsInput<Unit, CustomFields>,
    ) => ({
    }),
};

export type CustomFieldsForeignMutationsExpected = {}

export type CustomFieldsView = View<
    CustomFields & {node: Option<INode<Meta>>},
    CustomFields,
    CustomFieldsForeignMutationsExpected,
    {
    }
>;