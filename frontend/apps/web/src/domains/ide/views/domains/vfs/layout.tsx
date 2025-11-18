import React from "react";
import {
    FlatNode,
    getSpec,
    Ide,
    LockedPhase,
    moveIntoOwnFolder,
    WorkspaceState
} from "playground-core";
import {BasicFun, BasicUpdater} from "ballerina-core";
import {Breadcrumbs} from "./breadcrumbs.tsx";
import {FolderFilter} from "./folder-filter.tsx";
import MonacoEditor, {SupportedLanguage} from "../editor/monaco.tsx";
import {Drawer} from "./drawer.tsx";



type VfsLayoutProps = Ide & { setState: BasicFun<BasicUpdater<Ide>, void> };

export const VfsLayout = (props: VfsLayoutProps): React.ReactElement => {
    
    const folder =
        props.phase.kind == "locked" 
        && props.phase.locked.workspace.kind == 'selected' ?
        <fieldset className="fieldset ml-5">
            <Breadcrumbs workspace={props.phase.locked.workspace} />
            <div className="join">
                <FolderFilter
                    variant={props.phase.locked.workspace.variant}
                    workspace={props.phase.locked.workspace}
                    update={props.setState}
                    name={props.phase.locked.name}
                    moveIntoOwnFolder={async () => {
                        if(props.phase.kind != "locked") return 
                        if(props.phase.locked.workspace.kind != 'selected') return;
                        debugger
                        const added = await moveIntoOwnFolder(props.phase.locked.name, props.phase.locked.workspace.file.metadata.path.split("/"));
                        if(added.kind == 'value') {
                            const spec = await getSpec(props.phase.locked.name);
                            
                            if (spec.kind == 'value') {
                                const file = FlatNode.Operations.findFileByName(spec.value, props.phase.locked.workspace.file.name);
                                if(file.kind == "r") {
                                    props.setState(
                                        Ide.Updaters.Core.phase.locked(LockedPhase.Updaters.Core.workspace(WorkspaceState.Updater.reloadContent(spec.value)))
                                            .then(Ide.Updaters.Core.phase.locked(LockedPhase.Updaters.Core.workspace(WorkspaceState.Updater.selectFile(file.value)))))
                                }
                            }
                        }
                    }}
                />
            </div>
        </fieldset> : <></>
    const file =
        props.phase.kind == "locked"
        && props.phase.locked.workspace.kind == 'selected'
            ? <MonacoEditor
            fileName={props.phase.locked.workspace.file.name}
            onChange={(next:any)=> props.setState(next)}
            key={props.phase.locked.workspace.file.name}
            content={JSON.stringify(props.phase.locked.workspace.file.metadata.content)}/> : <></>
    
    const drawer =
        props.phase.kind == "locked" 
        ? <Drawer
            name={props.phase.locked.name}
            workspace={props.phase.locked.workspace}
            onSelectedFile={(file) => 
                props.setState(

                        Ide.Updaters.Core.phase.locked(LockedPhase.Updaters.Core.workspace(WorkspaceState.Updater.selectFile(file))))
            }
            drawerId="my-drawer" /> : <></>
    
    return <>
        {folder}
        {file}
        {drawer}</>
}
