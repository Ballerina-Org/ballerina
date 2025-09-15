import {Ide, VfsWorkspace, VirtualFolderNode, VirtualJsonFile} from "playground-core";
import React from "react";
import {VscFile, VscFolder} from "react-icons/vsc";
import {Option, Updater} from "ballerina-core";


export const folderFilter = (data: {
    folder: VirtualFolderNode
    files: VirtualJsonFile[]
}, selected: Option<string>) => {
    if(data.folder.kind == 'file') return <></>
    const values = data.folder.children.values();
    debugger
    return ( <div className="filter mb-7  join-item">
        <input className="btn filter-reset" type="radio" name="virtual-files" aria-label="All"/>
        {values.filter(x => x.kind == 'file').map(f => f.value).map( f =>
            (<div className="tooltip tooltip-bottom" data-tip={f.path}>
                <input
                    className="btn"
                    type="radio"
                    name="virtual-files"
                    checked={
                        selected.kind == "r" && selected.value == f.fileRef?.name}
                    // onClick={()=>
                    //     props.setState(
                    //         Ide.Updaters.lockedSpec.vfs.selectedFolder(
                    //             (Updater(VfsWorkspace.Updaters.Core.selectedFile(
                    //
                    //                 f.name
                    //
                    //             )))
                    //         )
                    //     )} 
                    aria-label={f.fileRef?.name.replace(/\.json$/,"")}/>
            </div>))}

    </div>)}