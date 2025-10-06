import React from "react";
import {Ide, LockedSpec, WorkspaceState} from "playground-core";
import {BasicFun, BasicUpdater} from "ballerina-core";
import {Breadcrumbs} from "./breadcrumbs.tsx";
import {FolderFilter} from "./folder-filter.tsx";
import MonacoEditor from "../editor/monaco.tsx";
import {Drawer} from "./drawer.tsx";

type VfsLayoutProps = Ide & { setState: BasicFun<BasicUpdater<Ide>, void> };

export const VfsLayout = (props: VfsLayoutProps): React.ReactElement => {
    
    const folder =
        props.phase == "locked" 
        && props.locked.workspace.kind == 'selected' ?
        <fieldset className="fieldset ml-5">
            <Breadcrumbs workspace={props.locked.workspace} />
            <div className="join">
                <FolderFilter
                    workspace={props.locked.workspace}
                    update={props.setState} />
            </div>
        </fieldset> : <></>
    const file =
        props.phase == "locked"
        && props.locked.workspace.kind == 'selected' && props.locked.workspace.current.kind == 'file'
            ? <MonacoEditor
            onChange={(next:any)=> props.setState(next)}
            key={props.locked.workspace.current.file.name}
            content={JSON.stringify(props.locked.workspace.current.file.metadata.content)}/> : <></>
    
    const drawer =
        props.phase == "locked" 
        ? <Drawer 
            name={props.name.value}
            workspace={props.locked.workspace}
            mode={props.locked.workspace.origin == 'selected' ? 'select-current-folder' : 'upload'}
            onSelectedFolder={(folder) =>
                props.setState(LockedSpec.Updaters.Core.workspace(WorkspaceState.Updater.selectFolder(folder)))
            }
            onSelectedFile={(file) => 
                props.setState(LockedSpec.Updaters.Core.workspace(WorkspaceState.Updater.selectFile(file)))
            }
            drawerId="my-drawer" /> : <></>
    
    return <>
        {folder}
        {file}
        {drawer}</>
}
