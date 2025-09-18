import {Ide, VfsWorkspace, VirtualFolderNode, VirtualJsonFile} from "playground-core";
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
    folder: VirtualFolderNode;
    selected: Option<VirtualJsonFile>;
    update?: any
};

export const FolderFilter = ({
                                 folder,
                                 selected,
                                 update,
                             }: FolderFilterProps) => {
    if (folder.kind !== "folder") return null;
    const fs = Array.from(folder.children.values());
    const ft = fs.filter(x => x.kind == 'file');
    const files: VirtualJsonFile[] = ft.map(x=> x as VirtualJsonFile);
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
                        
                        const fileName =
                            f.fileRef?.name || f.name
                        const label = fileName?.replace(/\.json$/, "");

                        return (
                            <div
                                key={f.path.join("-")}
                                className="tooltip tooltip-bottom"
                                data-tip={`Load ${f.name} into editor`}
                            >
                                <input
                                    className="btn"
                                    type="radio"
                                    name="virtual-files"
                                    aria-label={label}
                                    onChange={async () => {
                                        const content: any = f.content || await f.fileRef?.text()!;
                                        debugger
                                        const s_u = LockedSpec.Updaters.Core.vfs(
                                            VfsWorkspace.Updaters.Core.selectedFile(
                                                replaceWith(
                                                    Option.Default.some({...f, content: content })))
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
