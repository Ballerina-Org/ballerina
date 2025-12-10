import React from "react";
import {Dropdown} from "../layout/dropdown.tsx"
import {
    FlatNode,
    getSpec,
    Ide,
    WorkspaceState
} from "playground-core";
import {BasicFun, replaceWith, Updater, Value} from "ballerina-core";
import {SelectionPhase, Spec} from "playground-core/ide/domains/phases/selection/state.ts";
import {LocalStorage_SpecName} from "playground-core/ide/domains/storage/local.ts";

type SelectSpecProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const SelectSpec = (props: SelectSpecProps): React.ReactElement => {
    return props.phase.kind == "selection" && props.phase.selection.specs.length > 0  
        ?
        <Dropdown
            label={"Select spec"}
            onChange={async (name: Spec) => {
                if(props.phase.kind != "selection") return
                const spec = await getSpec(name.name);
                debugger
                if (spec.kind == "errors") {
                    props.setState(Ide.Updaters.Core.phase.selection(SelectionPhase.Updaters.Core.errors(replaceWith(spec.errors))))
                    return;
                }
                
                const u = 
                    Ide.Updaters.Core.phase.toLocked(name.name, props.phase.selection.variant, spec.value)
                        .then(((ide: Ide) => {
                 
                            if(ide.phase.kind != 'locked') return ide;

                            if(!FlatNode.Operations.hasSingleFolderBelowRoot(spec.value)) {
                                return ide;
                            }

                            return ({...ide,
                                name: Value.Default(name.name),
                                locked: {
                                    ...ide.phase.locked,
                                    workspace: WorkspaceState.Updater.defaultForSingleFolder()(ide.phase.locked.workspace) }
                            })
                        }))
                props.setState(u);
                LocalStorage_SpecName.set(name.name);
    
            }}
            options={props.phase.selection.specs}/>

        : <></>
}
