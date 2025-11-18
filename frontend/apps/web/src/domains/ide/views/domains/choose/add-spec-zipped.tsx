import React, {useState} from "react";
import {initSpec, postVfs, VirtualFolders, Ide} from "playground-core";
import {BasicFun, replaceWith, Updater, ValueOrErrors} from "ballerina-core";

import {LocalStorage_SpecName} from "playground-core/ide/domains/storage/local.ts";
import {SelectionPhase} from "playground-core/ide/domains/phases/selection/state.ts";
import {List} from "immutable";

type AddSpecProps = SelectionPhase  & { setState: BasicFun<Updater<Ide>, void> };

export const AddSpecUploadZipped = (props: AddSpecProps): React.ReactElement => {
    if(props.variant.kind != 'explore') return <></>
    const [result, setResult] = 
        useState<ValueOrErrors<Array<{ path: string[]; content: string }>, string>>(ValueOrErrors.Default.throw(List(["No file selected"])));

    const handlePick = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const f = e.target.files?.[0] || null;
        const content = await SelectionPhase.Operations.handleZip(f);
        setResult(content);
    };

    return (props.kind == 'upload-started')
        ? <div className="card bg-gray-200 text-black m-5">
                <div className="card-body items-start gap-3">

                    <div className="card-body items-start gap-3">
                        <h2 className="card-title">Select zip file</h2>
                        <div className="flex flex-wrap gap-3">
                            <input type="file" accept=".zip" className="file-input file-input-ghost" onChange={handlePick} />
                        </div>
                    </div>
                    {result.kind == "errors" && <div role="alert" className="alert alert-secondary">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 shrink-0 stroke-current" fill="none" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                        <span>{result.errors}</span>
                    </div>}
                    <div className="card-actions justify-end">
                        <button
                            onClick={ async ()=> {
                               
                                const vfs = await initSpec(props.name.value, props.variant);
                                if(vfs.kind == "errors") {
                                    props.setState(Ide.Updaters.Core.phase.selection(SelectionPhase.Updaters.Core.errors(replaceWith(vfs.errors))))
                                    return;
                                }
                                if(result.kind == "value") {
                                    const vfs = await VirtualFolders.Operations.fileArrayToTree(result.value);
                                    if(vfs.kind == "value") {
                                        // const u = Ide.Updaters.Phases.choosing.progressUpload()
                                        // props.setState(u)

                                        const d = await postVfs(props.name.value, vfs.value);

                                        if (d.kind == "errors") {
                                            props.setState(
                                                Ide.Updaters.Core.phase.selection(
                                                    SelectionPhase.Updaters.Core.errors(replaceWith(d.errors)))
                                                    .then(Ide.Updaters.Core.phase.selection(s=> ({ ...s, kind: 'upload-started'})))
                                                )
                                            return;
                                        }

                                        const u2 =
                                            Ide.Updaters.Core.phase.selection(s=> ({ ...s, kind: 'upload-finished'}))
                                                .then(
                                                    Ide.Updaters.Core.phase.toLocked(props.name.value, props.variant, vfs.value))
                                        props.setState(u2)
                                        LocalStorage_SpecName.set(props.name.value);

                                    }
                                }
        
                            }}
                            disabled={result.kind == "errors"}
                            className="btn btn-primary">Upload</button>
                    </div>

                </div>
            </div>
        : <></>
}
