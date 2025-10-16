import React from "react";
import {Dropdown} from "../layout/dropdown.tsx"
import {FlatNode, getOrInitSpec, getSpec, Ide, VirtualFolders, WorkspaceState} from "playground-core";
import {AddSpec, AddSpecInner} from "./add-spec.tsx";
import {BasicFun, Updater} from "ballerina-core";
import {SpecMode, SpecOrigin} from "playground-core/ide/domains/spec/state.ts";

type SelectSpecProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const SelectSpec = (props: SelectSpecProps): React.ReactElement => {
    return props.phase == "choose" && props.specSelection.specs.length > 0  
        ?
        <Dropdown
            label={"Select spec"}
            onChange={async (name: string) => {
                const spec = await getSpec(name);
              
                if (spec.kind == "errors") {
                    props.setState(Ide.Updaters.CommonUI.chooseErrors(spec.errors))
                    return;
                }
                const mode = { mode: 'explore', entry: 'upload-zip' } as SpecMode
                const origin = { origin: 'selected' } as SpecOrigin;
                const u = 
                    Ide.Updaters.Phases.choosing.toLocked(name, spec.value, origin, mode)
                        .then(((ide: Ide) => {
                            if(ide.phase != 'locked') return ide;
debugger
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
