import React, {Dispatch, SetStateAction} from "react";
import {FlatNode, Ide, LockedSpec, VfsWorkspace} from "playground-core";
import {Themes} from "../../theme-selector.tsx";
import {BasicFun, BasicUpdater, Option, replaceWith, Updater} from "ballerina-core";
import {HorizontalDropdown} from "../../dropdown.tsx";
import {Breadcrumbs} from "./breadcrumbs.tsx";
import {FolderFilter} from "./folder-filter.tsx";
import MonacoEditor from "../../monaco.tsx";
import {Drawer} from "./drawer.tsx";
import {MissingFiles} from "./add-missing-files.tsx";

type VfsLayoutProps = Ide & { setState: BasicFun<BasicUpdater<Ide>, void> };

export const VfsLayout = (props: VfsLayoutProps): React.ReactElement => {
    
    const folder =
        props.phase == "locked" 
        && props.locked.virtualFolders.selectedFolder.kind == "r" ?
        <fieldset className="fieldset ml-5">
            <Breadcrumbs selected={props.locked.virtualFolders.selectedFolder} />
            <MissingFiles
                formsMode={props.locked.formsMode}
                folder={props.locked.virtualFolders.selectedFolder.value}
                update={props.setState} />
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
            onChange={(next)=> props.setState(next)}
            key={JSON.stringify(props.locked.virtualFolders.selectedFile.value.name)}
            content={JSON.stringify(props.locked.virtualFolders.selectedFile.value.metadata.content)}/> : <></>
    
    const drawer =
        props.phase == "locked" && !props.locked.virtualFolders.nodes.metadata?.isLeaf
        ? <Drawer 
            formsMode={props.locked.formsMode}
            mode={props.specOrigin == 'existing' ? 'select-current-folder' : 'upload'}
            vfs={props.locked.virtualFolders} 
            onSelectedFolder={(folder) => {
                props.setState(LockedSpec.Updaters.Core.vfs(
                    Updater(
                        vfs => ({
                            ...vfs,
                            selectedFolder: Option.Default.some(folder),
                            selectedFile: folder.children?.length || 0 > 0 ? Option.Default.some(folder.children![0]) : vfs.selectedFolder,
                        }),
                    )
                ))
            }
            }

            onSelectedFile={(file) => {
                props.setState(LockedSpec.Updaters.Core.vfs(
                    Updater(
                        vfs => ({
                            ...vfs,
                            selectedFile: Option.Default.some(file),
                        }),
                    )
                ))
            }
            }

            drawerId="my-drawer" /> : <></>
    
    return <>
        {folder}
        {file}
        {drawer}</>
}
