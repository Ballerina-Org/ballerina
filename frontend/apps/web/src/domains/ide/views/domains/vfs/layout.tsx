import React, {Dispatch, SetStateAction} from "react";
import {Ide, isFile, VfsWorkspace, VirtualFolderNode} from "playground-core";
import {Themes} from "../../theme-selector.tsx";
import {BasicFun, BasicUpdater, Option, Updater} from "ballerina-core";
import {HorizontalDropdown} from "../../dropdown.tsx";
import {Breadcrumbs} from "./breadcrumbs.tsx";
import {FolderFilter} from "./folder-filter.tsx";
import MonacoEditor from "../../monaco.tsx";
import {Drawer} from "./drawer.tsx";

type VfsLayoutProps = Ide & { setState: BasicFun<BasicUpdater<Ide>, void> };

export const VfsLayout = (props: VfsLayoutProps): React.ReactElement => {
    const folder =
        props.phase == "locked" 
        && props.locked.virtualFolders.selectedFolder.kind == "r" 
        && props.locked.virtualFolders.selectedFolder.value.kind == "folder" ?
        <fieldset className="fieldset ml-5">
            <Breadcrumbs file={props.locked.virtualFolders.selectedFolder.value} />
            
            <div className="join">
                <FolderFilter
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
            key={props.locked.bridge.spec.left.specBody.value}
            content={props.locked.bridge.spec.left.specBody.value}/> : <></>
    
    const drawer =
        props.phase == "locked" 
        ? <Drawer 
            vfs={props.locked.virtualFolders} 
            selectNode={
                (node: VirtualFolderNode) => {
                    if (isFile(node)) return ;
                    const files = Array.from(node.children.values()).filter(x => isFile(x));//.map( x=>x.value);
            
                    const next =
                        Updater(VfsWorkspace.Updaters.Core.selectedNode(node)).then(vfs =>
                            files.length > 0
                                ? ({...vfs, selectedFile: Option.Default.some(files[0])}): ({...vfs})
                        )
            
                    props.setState(
                        VfsWorkspace.Updaters.Core.selectedFolder(
                            next
                        )
                    )
                }
            } drawerId="my-drawer" /> : <></>
    
    return <>
        {folder}
        {file}
        {drawer}</>
}