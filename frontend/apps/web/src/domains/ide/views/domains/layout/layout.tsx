
import {
    Ide,
    IdeView,
    seed,

    update,
    VirtualFolders,
    validateCompose, validateExplore, getSpec, seedPath
} from "playground-core";
import "react-grid-layout/css/styles.css";
import React, {useState} from "react";
import {Actions} from "./actions"
import {themeChange} from 'theme-change'
import {useEffect} from 'react'
import {AppToaster, fromVoe, errorFromList, notify} from "./toaster.tsx";

import {DispatcherFormsApp} from "../forms/forms.tsx";
import {Panel, PanelGroup,PanelResizeHandle} from "react-resizable-panels";
import {NoSpescInfo} from "../bootstrap/no-specs-info.tsx";
import {LockedSpec} from "playground-core/ide/domains/locked/state.ts";
import {Loader} from "../bootstrap/loader.tsx";
import {Navbar} from "../bootstrap/navbar.tsx";
import {AddSpec} from "../choose/add-spec.tsx";
import {SpecificationLabel} from "../locked/specification-label.tsx";
import {VfsLayout} from "../vfs/layout.tsx";
import {AddOrSelectSpec} from "../choose/layout.tsx";
import {EntitiesSelector} from "../forms/entities-selector.tsx";
import {FormSkeleton} from "../forms/skeleton.tsx";
import {LauncherAndEntity} from "../forms/launcher-entity.tsx";
import {SettingsPanel} from "./settings.tsx";
import {languages} from "monaco-editor";
import json = languages.json;
import {Hero} from "./hero.tsx";
import {List} from "immutable";
import {SelectEntityAndLookups} from "../lookups/selector/layout.tsx";
declare const __ENV__: Record<string, string>;
console.table(__ENV__);

export const IdeLayout: IdeView = (props) =>{
    const [theme, setTheme] = useState("lofi");
    const [hideRight, setHideRight] = useState(false);
    const [version, setVersion] = useState(0);
    const forceRerender = () => setVersion(v => v + 1);
    
    useEffect(() => {
        themeChange(false)
    }, [props.context.bootstrappingError, props.context.choosingError, props.context.lockingError,props.context.phase == 'locked'])
    useEffect(() => {
        if(props.context.bootstrappingError.size > 0){
            
            notify.error('IDE Bootstrap Error', errorFromList(props.context.bootstrappingError))
        }
    }, [props.context.bootstrappingError, props.context.choosingError, props.context.lockingError]);
    
    const noErrors = 
        props.context.bootstrappingError.size == 0 
        && props.context.lockingError.size == 0 
        && props.context.choosingError.size == 0;
    return (
        <div data-theme={theme} className="flex flex-col min-h-screen">
            <AppToaster />
            <NoSpescInfo {...props.context} />
            <Loader {...props.context} />
            <Hero {...props.context} setState={props.setState}/>
            
            {props.context.phase !== "bootstrap" && props.context.phase !== "hero" && 
                <>
                    <Navbar {...props.context} theme={theme} setTheme={setTheme} />
                    <PanelGroup className={"flex-1 min-h-0"} autoSaveId="example" direction="horizontal">
                    <Panel minSize={20} defaultSize={50}>
                        <aside className="relative h-full">
                        <AddSpec {...props.context} setState={props.setState} />
                        <AddOrSelectSpec {...props.context} setState={props.setState} />
                        <div className="w-full flex">
                            <SpecificationLabel {...props.context} />
                            <Actions
                                canValidate={
                                props.context.phase == 'locked' 
                                    && props.context.locked.workspace.kind == 'selected'
       
                                }
                                hideRight={hideRight}
                                context={props.context}
                                onHide={() => setHideRight(!hideRight)}
                                onSave={
                                    async () => {
                                        if(!(props.context.phase == "locked" 
                                            && props.context.locked.workspace.kind == 'selected' 
                                            && props.context.locked.workspace.current.kind == 'file')) {
                                            notify.error('UX bad state','Saving should be possible only in a locked phase');
                                            return
                                        }
                                        
                                        const currentFile = props.context.locked.workspace.current.file;
                                        const call = 
                                            await update(
                                                props.context.name.value, 
                                                currentFile.metadata.path, currentFile.metadata.content);
                                        
                                        
                                        fromVoe(call,'Specification save');
                                        const updated = await getSpec(props.context.name.value);
                                        if(updated.kind == "value") {
                                             props.setState(Ide.Updaters.Phases.locking.refreshVfs(updated.value.folders));
                                        }
                                        
                                    }
                                }
                                onMerge={ async ()=>{
                                    if(!(props.context.phase == 'locked' && props.context.locked.workspace.kind == 'selected' && props.context.locked.workspace.current.kind == 'file')) return;
                                    props.setState(
                                        Ide.Operations.clearErrors())
                                    const json =
                                        props.context.locked.workspace.mode == 'compose' || props.context.locked.workspace.mode == 'scratch'
                                            ? await validateCompose(props.context.name.value)
                                            : await validateExplore(props.context.name.value, props.context.locked.workspace.current.file.metadata.path.split("/"))
                                    const u =
                                        json.kind == "errors"
                                            ? Ide.Updaters.CommonUI.lockingErrors(json.errors)
                                            : LockedSpec.Updaters.Core.validated(json.value);
                                    
                                    props.setState(u);
                                    if(json.kind != "errors") {
                                        notify.success("Validation succeeds")
                                        return
                                    }
                                }}
                                onSettings={()=> props.setState(Ide.Updaters.CommonUI.toggleSettings())}
                                onRun={() =>{
                                    props.setState(LockedSpec.Operations.enableRun());
                                    forceRerender();
                                }}
                                onSeed={
                                    async () => {
                                        if(props.context.phase != "locked") {
                                            notify.error('UX bad state','Seeding should be possible only in a locked phase');
                                            return
                                        }
                                        
                                        const call =
                                            props.context.locked.workspace.mode == 'explore'
                                            && props.context.locked.workspace.kind == 'selected'
                                            && props.context.locked.workspace.current.kind == 'file'
                                            ? await seedPath(props.context.name.value, props.context.locked.workspace.current.file.metadata.path.split("/")) 
                                            : await seed(props.context.name.value)
                                        if(call.kind == "errors") notify.error("Seeding failed")
                                        if(call.kind != "errors") notify.success("Seeding succeed")
          
                                        if(call.kind == "errors") props.setState(Ide.Updaters.CommonUI.lockingErrors(call.errors));
                                    }
                                }
                            />
                        </div>
            
                            <SettingsPanel  {...props.context} setState={props.setState}/>
                            <VfsLayout {...props.context} setState={props.setState} />
                      
                    </aside>
                    </Panel>
                    <PanelResizeHandle  className="w-[1px] bg-neutral text-neutral-content" />
                 {!hideRight && 
                    <Panel>
                        <PanelGroup direction="vertical">
                            <Panel minSize={20} defaultSize={50}>
                                <div className="mockup-window border border-base-300 w-full h-full">
                                    <aside className="relative h-full">
                                        <FormSkeleton {...props.context} setState={props.setState} />
                                        
                                        { props.context.phase == "locked" &&  props.context.locked.progress.kind == 'preDisplay' 
                                            && <div className="card bg-base-100 w-full mt-5">
                                            <div className="card-body w-full">
                                                <LauncherAndEntity {...props.context.locked.progress.selectEntityFromLauncher} setState={props.setState} />
                                                {/*<SelectEntityAndLookups />*/}
                                                { props.context.locked.progress.selectEntityFromLauncher.kind == 'done'
                                                    && props.context.locked.validatedSpec.kind == "r"
                                                    && <><DispatcherFormsApp
                                                        key={version}
                                                        launcherName={props.context.locked.progress.selectEntityFromLauncher.a.key}
                                                        launcherConfigName={props.context.locked.progress.selectEntityFromLauncher.a.configType}
                                                        specName={props.context.name.value}
                                                        entityName={props.context.locked.progress.selectEntityFromLauncher.b}
                                                        setState={props.setState}
                                                        typeName={"Person"}
                                                        spec={props.context.locked.validatedSpec.value}/></>
                                                }
                                            </div>
                                            </div>
                                            }
                              
                                    </aside>
                                </div>
                            </Panel>
                            <PanelResizeHandle  className="h-[1px] bg-neutral text-neutral-content" />
                            <Panel minSize={10} defaultSize={40} maxSize={90}>
                                <div className="no-radius w-full mx-auto">
                                    <div className="inset-0 top-0 z-20 m-0 p-0">
                                        <div className="space-y-2  w-full">
                                            {
                                                props.context.bootstrappingError.map(error => 
                                                    (<pre 
                                                        data-prefix="6"
                                                        className="m-0 pl-3 bg-rose-700 text-warning-content">
                                                        <code>Error!</code>
                                                        {error}
                                                    </pre>))
                                            }
                                            {
                                                props.context.choosingError.map(error => 
                                                    (<pre data-prefix="6"
                                                          className="m-0 pl-3 bg-rose-500 text-warning-content">
                                                        <code>Error!</code>
                                                        {error}
                                                    </pre>))
                                            }
                                            {
                                                props.context.lockingError.map(error => 
                                                    (<pre data-prefix="6"
                                                          className="m-0 pl-3 bg-rose-300 text-warning-content">
                                                        <code>Error!</code>
                                                        {error}</pre>))
                                            }
                                            {
                                                props.context.formsError.map(error => 
                                                    (<pre data-prefix="6"
                                                          className="m-0 pl-3 bg-rose-300 text-warning-content">
                                                        <code>Forms!</code>
                                                        {error}</pre>))
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
                </PanelGroup></>}
        </div>)
};