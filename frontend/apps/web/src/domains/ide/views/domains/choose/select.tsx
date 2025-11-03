import React from "react";
import {Dropdown} from "../layout/dropdown.tsx"
import {CommonUI, FlatNode, getOrInitSpec, getSpec, Ide, VirtualFolders, WorkspaceState} from "playground-core";
import {BasicFun, Updater} from "ballerina-core";

type SelectSpecProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const SelectSpec = (props: SelectSpecProps): React.ReactElement => {
    return props.phase == "selectionOrCreation" && props.specSelection.specs.length > 0  
        ?
        <Dropdown
            label={"Select spec"}
            onChange={async (name: string) => {
                const spec = await getSpec(name);
              
                if (spec.kind == "errors") {
                    props.setState(CommonUI.Updater.Core.chooseErrors(spec.errors))
                    return;
                }

                const origin =  'selected'
                const u = 
                    Ide.Updaters.Phases.choosing.toLocked(spec.value)
                        .then(((ide: Ide) => {
                            debugger
                            if(ide.phase != 'locked') return ide;

                            if(!FlatNode.Operations.hasSingleFolderBelowRoot(spec.value)) {
                                return ide;
                            }

                            return ({...ide,
                                locked: {
                                    ...ide.locked,
                                    workspace: WorkspaceState.Updater.defaultForSingleFolder()(ide.locked.workspace) }
                            })
                        }))
                props.setState(u)
    
            }}
            options={props.specSelection.specs}/>

        : <></>
}
