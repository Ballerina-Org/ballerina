/** @jsxImportSource @emotion/react */

import { style as editorStyle } from "./json-editor.styled.ts";
import {Ide, IdeView, seed, VfsWorkspace, VirtualFolderNode, isFile, validate} from "playground-core";
import {V2Editor, V1Editor, SeedEditor} from "./json-editor.tsx";
import "react-grid-layout/css/styles.css";
import React, {useState} from "react";
import {Actions} from "./actions"

import { Toaster } from 'sonner';
import {replaceWith, Value, Option, Updater} from "ballerina-core";
import LauncherSelector from "./launcher-selector.tsx";
import {HorizontalDropdown} from "./dropdown.tsx";
import {themeChange} from 'theme-change'
import {useEffect} from 'react'
import {Themes} from "./theme-selector.tsx";
import { ideToast } from "./toaster.tsx";
import { toast as sonnerToast } from 'sonner';
import {seedSpecErrorHandler, updateSpecErrorHandler} from "./error-handlers/UpdateSpec.ts";
import {DispatcherFormsApp} from "./forms.tsx";
import {Panel, PanelGroup,PanelResizeHandle} from "react-resizable-panels";
import {NoSpescInfo} from "./domains/bootstrap/no-specs-info.tsx";
import {LockedSpec} from "playground-core/ide/domains/locked/state.ts";
import {Loader} from "./domains/bootstrap/loader.tsx";
import {Navbar} from "./domains/bootstrap/navbar.tsx";
import {AddSpec} from "./domains/choose/add-spec.tsx";
import {SpecificationLabel} from "./domains/locked/specification-label.tsx";
import {VfsLayout} from "./domains/vfs/layout.tsx";
import {AddOrSelectSpec} from "./domains/choose/add-or-select.tsx";
declare const __ENV__: Record<string, string>;
console.table(__ENV__);

export type DockItem = 'folders' | 'about'
export const IdeLayout: IdeView = (props) =>{
    const [theme, setTheme] = useState("lofi");
    const [hideRight, setHideRight] = useState(false);
    useEffect(() => {
        themeChange(false)
    }, [])
    useEffect(() => {
        if(props.context.bootstrappingError.size > 0){
            
            ideToast({
                title: 'IDE Bootstrap Error',
                dismissible: false,
                duration: Infinity,
                description: props.context.bootstrappingError,
                // button: {
                //     label: 'Ok',
                //     onClick: () => sonnerToast.dismiss(),
                // },
            });
        }
    }, [props.context.bootstrappingError]);
    
    const noErrors = 
        props.context.bootstrappingError.size == 0 
        && props.context.lockingError.size == 0 
        && props.context.choosingError.size == 0;
    return (
        <div data-theme={theme} className="flex flex-col min-h-screen">
            <Toaster />
            <NoSpescInfo {...props.context} />
            <Loader {...props.context} />
            <Navbar {...props.context} theme={theme} setTheme={setTheme} />

            {props.context.phase !== "bootstrap" && 
                <PanelGroup className={"flex-1 min-h-0"} autoSaveId="example" direction="horizontal">

                    <Panel minSize={20} defaultSize={50}>
                    <aside className="relative h-full">
                        <AddSpec {...props.context} setState={props.setState} />
                        <AddOrSelectSpec {...props.context} setState={props.setState} />
                        <div className="w-full flex">
                            <SpecificationLabel {...props.context} />
                            <Actions
                                hideRight={hideRight}
                                context={props.context}
                                onLeft={() => setHideRight(!hideRight)}
                                onRight={() => setHideRight(hideRight)}
                                //onNew={() => props.setState(Ide.Updaters.Template.choosePhase('create'))}
                                onSave={
                                    async () => {
                                        if(props.context.phase != "locked") {
                                            ideToast({
                                                title: 'UX bad state',
                                                description: 'Saving should be possible only in a locked phase',
                                                button: {
                                                    label: 'Ok',
                                                    onClick: () => sonnerToast.dismiss(),
                                                },
                                            });
                                            return
                                        }
                                        //const call = await update(props.context.create.name.value, Bridge.Operations.toVSpec(props.context.locked.bridge.spec))
                                        //updateSpecErrorHandler(call);
                                    }
                                }
                                onMerge={ async ()=>{
                                    const json = await validate(props.context.create.name.value)

                                    const u =
                                        json.kind == "errors"
                                            ? Ide.Updaters.CommonUI.lockingErrors(json.errors)
                                            : LockedSpec.Operations.merge(json.value);
                                    props.setState(u);
                                }}
                                onRun={() => props.setState(Ide.Updaters.Template.lockedOutcomePhase())}
                                onSeed={
                                    async () => {
                                        if(props.context.phase != "locked") {
                                            ideToast({
                                                title: 'UX bad state',
                                                description: 'Seeding should be possible only in a locked phase',
                                                button: {
                                                    label: 'Ok',
                                                    onClick: () => sonnerToast.dismiss(),
                                                },
                                            });
                                            return
                                        }
                                        const call = await seed(props.context.create.name.value)
                                        seedSpecErrorHandler(call);
                                        if(call.kind == "value")
                                            props.setState(LockedSpec.Updaters.Core.seed(call.value));
                                    }
                                }
                            />
                        </div>

                         
    
                            <VfsLayout {...props.context} setState={props.setState} />
                    </aside>
                </Panel>
                <PanelResizeHandle  className="w-[1px] bg-neutral text-neutral-content" />
                 {!hideRight && 
                    <Panel>
                    <PanelGroup direction="vertical">
                        <Panel>
                            <div className="mockup-window border border-base-300 w-full h-full">
                                      
                            
                        <aside className="relative h-full">
                            
                            {!(props.context.phase == 'locked' && props.context.step == 'outcome') && <div className="flex w-full  h-full flex-col gap-4 p-7  shadow-sm backdrop-blur-md ">
                                <div className="skeleton h-32 w-full animate-none"></div>
                                <div className="skeleton h-4 w-28"></div>
                                <div className="skeleton h-4 w-full animate-none"></div>
                                <div className="skeleton h-4 w-full animate-none"></div>
                                <div className="navbar bg-base-100 shadow-sm backdrop-blur-md">
                                    <div className="flex-1 px-4 gap-4 items-center">
                                        <div className="skeleton bg-base-300/20 opacity-40 blur h-6 w-48 rounded" />
                                        <div className="skeleton bg-base-300/20 opacity-40 blur h-4 w-32 rounded" />
                                    </div>
                                    <div className="flex-none gap-3 pr-4 items-center">
                                        <div className="skeleton bg-base-300/20 opacity-40 blur h-10 w-64 rounded" />
                                        <div className="skeleton bg-base-300/20 opacity-40 blur h-10 w-10 rounded-full" />
                                    </div>
                                </div>
                            </div>}

                            { props.context.phase == "locked" && props.context.step == "outcome" && <div className="card bg-base-100 w-full mt-5">
                                <div className="card-body w-full">
                                    {props.context.locked.launchers && <><LauncherSelector
                                        onChange={async (value: string) => {

                                            props.setState(
                                                LockedSpec.Updaters.Core.selectLauncher(value)
                                            )
                                        }}
                                        options={props.context.locked.launchers}

                                    />
                                        { props.context.locked.virtualFolders.merged.kind == "r" && <DispatcherFormsApp
                                            key={props.context.locked.virtualFolders.merged.value as any}
                                            specName={props.context.create.name.value}
                                            entityName={"People"}
                                            typeName={"Person"}
                                            spec={props.context.locked.virtualFolders.merged.value}/>
                                        }
                                    </>
                                    }
                                </div>
                        
                            </div>}
       
       

                        </aside>        </div>
                                </Panel>
                                <PanelResizeHandle  className="h-[1px] bg-neutral text-neutral-content" />
                                <Panel minSize={10} defaultSize={40} maxSize={90}>
                                    <div className="no-radius w-full mx-auto">
                                        <div className="inset-0 top-0 z-20 m-0 p-0">
                                            <div className="space-y-2  w-full">
                                                {
                                                    props.context.bootstrappingError.map(error => (<pre data-prefix="6"
                                                                                                        className="m-0 pl-3 bg-rose-700 text-warning-content"><code>Error!</code>{error}</pre>))
                                                }
                                                {
                                                    props.context.choosingError.map(error => (<pre data-prefix="6"
                                                                                                   className="m-0 pl-3 bg-rose-500 text-warning-content"><code>Error!</code>{error}</pre>))
                                                }
                                                {
                                                    props.context.lockingError.map(error => (<pre data-prefix="6"
                                                                                                  className="m-0 pl-3 bg-rose-300 text-warning-content"><code>Error!</code>{error}</pre>))
                                                }
                                            </div>
                                        </div>
                                        <div className="relative w-120 mx-auto rounded-lg overflow-hidden">
                                            <img style={{opacity: noErrors? 0.8 : 0.2}}
                                                src="https://framerusercontent.com/images/umluhwUKaIcQzUGEWAe9SRafnc4.png?width=1024&height=1024"
                                                alt="Descriptive alt"
                                                className="w-full object-cover rounded-lg"/>
                                            {noErrors && <div className="absolute inset-0 grid place-items-center">
                                                <span className="px-3 py-1 rounded-full bg-black/60 text-white text-sm">No issues so far</span>
                                            </div>}
                                        </div>
                                    </div>

                    </Panel>
                     </PanelGroup>
                     </Panel>}
                </PanelGroup>}
</div>
)
};