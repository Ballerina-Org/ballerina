import React, {Dispatch, SetStateAction} from "react";
import {FlatNode, Ide, LockedSpec, VfsWorkspace} from "playground-core";
import {Themes} from "../../theme-selector.tsx";
import {BasicFun, BasicUpdater, Option, replaceWith, Updater} from "ballerina-core";
import {HorizontalDropdown} from "../../dropdown.tsx";
import {Breadcrumbs} from "./breadcrumbs.tsx";
import {FolderFilter} from "./folder-filter.tsx";
import MonacoEditor from "../../monaco.tsx";
import {Drawer} from "./drawer.tsx";

type VfsLayoutProps = Ide & { setState: BasicFun<BasicUpdater<Ide>, void> };

export const VfsLayout = (props: VfsLayoutProps): React.ReactElement => {
    debugger
    const folder =
        props.phase == "locked" 
        && props.locked.virtualFolders.selectedFolder.kind == "r"  ?
        <fieldset className="fieldset ml-5">
            <Breadcrumbs file={props.locked.virtualFolders.selectedFolder.value} />
            
            <div className="join">
                <FolderFilter
                    nodes={props.locked.virtualFolders.nodes}
                    folder={props.locked.virtualFolders.selectedFolder.value}
                    selected={props.locked.virtualFolders.selectedFile}
                    update={props.setState} />
            </div>
        </fieldset> : <></>
    const file =
        props.phase == "locked" 
        && props.locked.virtualFolders.selectedFolder.kind == "r" 
        && props.locked.virtualFolders.selectedFile.kind == "r"
        ? <MonacoEditor
            onChange={()=>{}}
            key={JSON.stringify(props.locked.virtualFolders.selectedFile.value.metadata.content)}
            content={JSON.stringify(props.locked.virtualFolders.selectedFile.value.metadata.content)}/> : <></>
    
    const drawer =
        props.phase == "locked" 
        ? <Drawer 
            mode={props.specOrigin == 'existing' ? 'select-current-folder' : 'upload'}
            vfs={props.locked.virtualFolders} 
            selectFolder={(folder) =>
                LockedSpec.Updaters.Core.vfs(
                    Updater(
                        vfs => ({ ...vfs,
                            selectedFolder: Option.Default.some(folder),
                        }),
                    )
                )
            }
            selectNodes={
                (nodes: FlatNode[]) => {
                    // const next =
                    //     LockedSpec.Updaters.Core.vfs(
                    //         Updater(
                    //             vfs => ({ ...vfs, 
                    //                 nodes: nodes
                    //             }),
                    //         )
                    //     )
                    // props.setState(next)
                }
            } drawerId="my-drawer" /> : <></>
    
    return <>
        {folder}
        {file}
        {drawer}</>
}