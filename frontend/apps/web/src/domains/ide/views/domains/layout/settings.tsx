
import React, { useState } from 'react';
import styled from '@emotion/styled';
import {getSpec, Ide, postCodegen, postVfs} from "playground-core";
import {BasicFun, Updater} from "ballerina-core";
import {CommonUI} from "playground-core/ide/domains/common-ui/state.ts";

type Props = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const SettingsPanel: React.FC<Props> = (props: Props) => {
    const [message, setMessage] = useState<string>("");
    const [jsonContent, setJsonContent] = useState<any>(null);
    const handleFile = (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = e.target.files; 
        if (!files || files.length === 0) {
            setMessage("No file selected");
            return;
        }
        if (files.length > 1) {
            setMessage("Please select only one file");
            return;
        }

        const file = files[0];
        if (file.name !== "codegen.json") {
            setMessage("File must be named codegen.json");
            return;
        }

        //setMessage(`File accepted: ${file.name}`);
        file.text().then(content => {
            const parsed = JSON.parse(content);
            setJsonContent(parsed);
            console.log("File content:", content);
        });
        setMessage("");
    };
    const apply = async () => {
        const result = await postCodegen(props.name.value, jsonContent);
        
        if(result.kind == "errors") setMessage(result.errors.toArray().join("\n"));
        const refreshed = await getSpec(props.name.value);
        if(refreshed.kind == "errors") setMessage(refreshed.errors.toArray().join("\n"));
        
        if(refreshed.kind == "value" && result.kind == "value")
        props.setState(
            Ide.Updaters.Phases.locking.refreshVfs(refreshed.value)
                .then(CommonUI.Updater.Core.toggleSettings()))
    };
    if(!props.settingsVisible) return <></>;
    return (
        <div className="card w-full bg-base-100 card-md shadow-sm">
            <div className="card-body">
                <fieldset className="fieldset bg-base-200 border-base-300 rounded-box w-xs border p-4">
                    <legend className="fieldset-legend">Settings</legend>

                    <label className="label">Codegen file</label>
                    <input
                        type="file"
                        className="file-input validator"
                        accept=".json,application/json"
                        onChange={handleFile}
                       pattern="^codegen\.json$/"
                       title="Must be named codege.json"/>
                    <p className="validator-hint">Must be 10 digits</p>
                    <div className="text-sm text-gray-700">{message}</div>
                    <label className="label mt-5">Seeds (fake data) language</label>
                    <div className="join">
                        <input className="join-item btn" type="radio" name="seed-lang" aria-label="en" defaultChecked/>
                        <input className="join-item btn" type="radio" name="seed-lang" aria-label="ch"/>
                        <input className="join-item btn" type="radio" name="seed-lang" aria-label="ru"/>
                    </div>
                    <label className="label mt-5">Styles</label>
                    <div className="join">
                        <input className="join-item btn" type="radio" name="styles" aria-label="tailwind"
                               defaultChecked/>
                        <input className="join-item btn" type="radio" name="styles" aria-label="ui-kit"/>
                    </div>
                    <div className="flex justify-center gap-2 mt-7">
                        <button
                            className="btn btn-neutral"
                            disabled={message !== ""}
                            onClick={() => apply()}
                        >
                            Apply
                        </button>

                        <button
                            className="btn"
                            type="button"
                            onClick={() => {
                                props.setState(CommonUI.Updater.Core.toggleSettings());
                            }}
                        >
                            Cancel
                        </button>
                    </div>
                </fieldset>

            </div>
        </div>)
}

