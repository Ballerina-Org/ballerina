import {Bridge, Ide, VfsWorkspace, VirtualFolderNode, VirtualJsonFile} from "playground-core";
import React from "react";
import {VscFile, VscFolder} from "react-icons/vsc";
import {Option, SimpleCallback, Unit, Updater} from "ballerina-core";
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
    data: { folder: VirtualFolderNode; files: VirtualJsonFile[] };
    selected: Option<string>;
    mode: "spec" | "schema";
    setMode: (m: "spec" | "schema") => void;
    update?: any
};

export const FolderFilter = ({
                                 data,
                                 selected,
                                 mode,
                                 setMode,
                                 update,
                             }: FolderFilterProps) => {
    if (data.folder.kind !== "folder") return null;

    const files: VirtualJsonFile[] =
        data.files && data.files.length > 0
            ? data.files
            : (() => {
                const acc: VirtualJsonFile[] = [];
                data.folder.children.forEach((child) => {
                    if (child.kind === "file") acc.push(child.value);
                });
                return acc;
            })();

    return (
        <div className="w-full">
            <div role="tablist" className="tabs tabs-lift">
                <a
                    role="tab"
                    className={mode == "spec" ? "tab tab-active" : "tab"}
                    onClick={() => setMode("spec")}
                >
                    Spec
                </a>
                <a
                    role="tab"
                    className={mode == "schema" ? "tab tab-active" : "tab"}
                    onClick={() => setMode("schema")}
                >
                    Schema
                </a>
            </div>
            <div className="mt-3 flex w-full">
                <div className="filter mb-7 join-item space-x-1">
                    {/* Reset ("All") */}
                    <input
                        className="btn filter-reset"
                        type="radio"
                        name="virtual-files"
                        aria-label="All"
                        //checked={selected.kind == "r"}
                    />

                    {files.map((f) => {
                        const fileName =
                            f.fileRef?.name ?? (f.path.split("/").pop() || f.path);
                        const label = fileName.replace(/\.json$/, "");
                        const isChecked =
                            selected.kind === "r" && selected.value === f.path;

                        return (
                            <div
                                key={f.path}
                                className="tooltip tooltip-bottom"
                                data-tip={`Load ${f.name} into editor`}
                            >
                                <input
                                    className="btn"
                                    type="radio"
                                    name="virtual-files"
                                    value={f.path}
                                    //checked={isChecked}
                                    aria-label={label}
                                    onChange={async () => {
                                        const content = await f.fileRef?.text()!;
                                        const s_u = VfsWorkspace.Updaters.Core.selectedFolder(
                                            VfsWorkspace.Updaters.Core.selectedFile(f.name)
                                        );
                                        const b_u = LockedSpec.Updaters.Core.bridge.v1(content);
                                        update(s_u.then(b_u));
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
