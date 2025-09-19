import {FlatNode, Ide, VfsWorkspace} from "playground-core";
import React from "react";
import {VscFile, VscFolder} from "react-icons/vsc";
import {Option, replaceWith, SimpleCallback, Unit, Updater} from "ballerina-core";
import {LockedSpec} from "playground-core/ide/domains/locked/state.ts";


// export const folderFilter = (data: {
//     folder: VirtualFolderNode
//     files: VirtualJsonFile[]
// }, selected: Option<string>) => {
//     if(data.folder.kind == 'file') return <p></p>
//     const values = data.folder.children.values();
//     debugger
//     return ( <div className="filter mb-7  join-item">
//         <input className="btn filter-reset" type="radio" name="virtual-files" aria-label="All"/>
//
//         {values.filter(x => x.kind == 'file').map(f => f.value).map( f =>
//             (<div className="tooltip tooltip-bottom" data-tip="v">
//                 <p>bbbbb</p>
//                 <input
//                     className="btn"
//                     type="radio"
//                     name="virtual-files"
//                     checked={
//                         selected.kind == "r" && selected.value == f.fileRef?.name}
//                     // onClick={()=>
//                     //     props.setState(
//                     //         Ide.Updaters.lockedSpec.vfs.selectedFolder(
//                     //             (Updater(VfsWorkspace.Updaters.Core.selectedFile(
//                     //
//                     //                 f.name
//                     //
//                     //             )))
//                     //         )
//                     //     )} 
//                     aria-label={f.fileRef ? f.fileRef.name.replace(/\.json$/,"") : f.name?.replace(/\.json$/,"") }/>
//             </div>))}
//
//     </div>)}

type FolderFilterProps = {
    folder: FlatNode;
    nodes: FlatNode;
    selected: Option<FlatNode>;
    update?: any
};

export const FolderFilter = ({
     folder,
     selected,
     nodes,
     update,
 }: FolderFilterProps) => {
    
    if (folder.metadata.kind !== "dir") return null;
    
    const allFiles = (nodes.children || [])?.filter(x => x?.metadata.kind == 'file');
    
    const ids = (folder.children || []).map(x => x.id);
    const files = allFiles.filter(x => ids.includes(x.id));

    return (
        <div className="w-full">
            <div className="mt-3 flex w-full">
                <div className="filter mb-7 join-item space-x-1">
                    <input
                        className="btn filter-reset"
                        type="radio"
                        name="virtual-files"
                        aria-label="All"
                    />

                    {files.map((f) => {
                        

                        return (
                            <div
                                key={f.metadata.path}
                                className="tooltip tooltip-bottom"
                                data-tip={`Load ${f.name} into editor`}
                            >
                                <input
                                    className="btn"
                                    type="radio"
                                    name="virtual-files"
                                    aria-label={f.name}
                                    onChange={async () => {
                                        const s_u = LockedSpec.Updaters.Core.vfs(
                                            VfsWorkspace.Updaters.Core.selectedFile(
                                                replaceWith(
                                                    Option.Default.some(f)))
                                        );
                                        //const b_u = LockedSpec.Updaters.Core.bridge.v1(content);
                                        update(s_u);
                                    }}
                                />
                            </div>
                        );
                    })}
                </div>
            </div>
        </div>
    );
};
