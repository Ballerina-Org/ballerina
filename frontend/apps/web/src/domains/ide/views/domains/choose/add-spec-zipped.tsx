import React, {Dispatch, SetStateAction, useState} from "react";
import {FlatNode, FormsMode, getOrInitSpec, Ide, postVfs, VirtualFolders} from "playground-core";
import {BasicFun, BasicUpdater, Option, Updater, Value, ValueOrErrors} from "ballerina-core";

import {LocalStorage_SpecName} from "playground-core/ide/domains/storage/local.ts";
import {ChooseState} from "playground-core/ide/domains/choose/state.ts";

type AddSpecProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const AddSpecUploadZipped = (props: AddSpecProps): React.ReactElement => {
    const [result, setResult] = useState<ValueOrErrors<Array<{ path: string[]; content: Record<string, unknown> }>, string>>(ValueOrErrors.Default.return([]));

    const handlePick = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const f = e.target.files?.[0] || null;
        const content = await ChooseState.Operations.handleZip(f);
        setResult(content);
    };
    const origin = 'create';
    return (props.phase == 'choose' && props.source == 'upload-zip' && props.progressIndicator == 'upload-started')
        ? <div className="card bg-gray-200 text-black w-full  mt-12">
                <div className="card-body items-start gap-3">

                    <div className="card-body items-start gap-3">
                        <h2 className="card-title">Select zip file</h2>
                        <div className="flex flex-wrap gap-3">
                            <input
                                type="file"
                                multiple
                                onChange={handlePick}
                                className="file-input file-input-ghost"
                            />
                            <input type="file" accept=".zip" onChange={handlePick} />
                        </div>
                    </div>
                    {result.kind == "errors" && <div role="alert" className="alert alert-error">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 shrink-0 stroke-current" fill="none" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                        <span>{result.errors}</span>
                    </div>}
                    {result.kind == "value" && <div className="card-actions justify-end">
                        <button
                            onClick={async ()=> {
                                const formsMode: FormsMode = { kind: 'select'};
                                debugger
                                const vfs = await getOrInitSpec(origin, formsMode, props.create.name.value);
                                if(vfs.kind == "errors") {
                                    props.setState(Ide.Updaters.CommonUI.chooseErrors(vfs.errors))
                                    return;
                                }
                                if(result.kind == "value") {
                                    const vfs = await VirtualFolders.Operations.fileArrayToTree(result.value);
                                    if(vfs.kind == "value") {
                                        const u = Ide.Updaters.Phases.progressUpload()
                                        props.setState(u)

                                        const d = await postVfs(props.create.name.value, vfs.value);

                                        if (d.kind == "errors") {
                                            props.setState(Ide.Updaters.CommonUI.chooseErrors(d.errors).then(Ide.Updaters.Phases.finishUpload()))
                                            return;
                                        }

                                        const u2 =
                                            Ide.Updaters.Phases.finishUpload()
                                                .then(
                                                    Ide.Updaters.Phases.lockedPhase(
                                                        'create',
                                                        'upload',
                                                        props.create.name.value,
                                                        VirtualFolders.Operations.buildWorkspaceFromRoot('create', vfs.value),
                                                        formsMode
                                                    ))
                                        props.setState(u2)
                                        LocalStorage_SpecName.set(props.create.name.value);

                                    }
                                }
        
                            }}
                            //disabled={node.kind == "l"}
                            className="btn btn-primary">Upload</button>
                    </div>}

                </div>
            </div>
        : <></>   

}
