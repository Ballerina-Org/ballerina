
import {
    Ide,
    IdeView,
    seed,
    update,
    validateCompose, validateExplore, getSpec, seedPath, getKeys, validateBridge, sendDelta,
    IsBootstrap, IsHero, IsChoose, IsLocked, Variant
} from "playground-core";
import "react-grid-layout/css/styles.css";
import React, {useState} from "react";
import {Actions} from "./actions"
import {themeChange} from 'theme-change'
import {useEffect} from 'react'
import {AppToaster, fromVoe, errorFromList, notify} from "./toaster.tsx";

import {DispatcherFormsApp} from "../forms/forms.tsx";
import {Panel, PanelGroup,PanelResizeHandle} from "react-resizable-panels";
import {MissingSpecsInfoAlert} from "../bootstrap/no-specs-info.tsx";
import {LockedPhase, LockedStep} from "playground-core/ide/domains/locked/state.ts";
import {Loader} from "../bootstrap/loader.tsx";
import {Navbar} from "../bootstrap/navbar.tsx";
import {AddSpec} from "../choose/add-spec.tsx";
import {SpecificationLabel} from "../locked/specification-label.tsx";
import {VfsLayout} from "../vfs/layout.tsx";
import {AddOrSelectSpec} from "../choose/layout.tsx";
import {FormSkeleton} from "../forms/skeleton.tsx";
import {SettingsPanel} from "./settings.tsx";
import {Hero} from "./hero.tsx";
import {List} from "immutable";
import {LaunchersDock} from "../forms/launchers-dock.tsx";
import {ValueOrErrors, Option} from "ballerina-core";
import {CommonUI} from "playground-core/ide/domains/common-ui/state.ts";
declare const __ENV__: Record<string, string>;
console.table(__ENV__);

export const IdeLayout: IdeView = (props) => {

    const [theme, setTheme] = useState("lofi");
    const [version, setVersion] = useState(0);
    
    const [hideForms, setHideForms] = useState(false);
    const [hideNavbar, setHideNavbar] = useState(false);
    const [hideErrors, setHideErrors] = useState(true);

    const forceRerender = () => setVersion(v => v + 1);
    
    useEffect(() => {
        themeChange(false)
    }, [props.context.bootstrappingError, props.context.choosingError, props.context.lockingError,props.context.phase == 'locked'])
    
    // useEffect(() => {
    //     if(props.context.bootstrappingError.size > 0){
    //        
    //         notify.error('IDE Bootstrap Error', errorFromList(props.context.bootstrappingError))
    //     }
    // }, [props.context.bootstrappingError, props.context.choosingError, props.context.lockingError]);
    
    const noErrors = 
        props.context.bootstrappingError.size == 0 
        && props.context.lockingError.size == 0 
        && props.context.choosingError.size == 0;
    return (
        <div data-theme="lofi"  className="flex flex-col min-h-screen">
            <AppToaster />
            {IsChoose(props.context) && <MissingSpecsInfoAlert specs={props.context.specSelection.specs.length} /> }
            {IsBootstrap(props.context) && <Loader {...props.context.bootstrap} />}
            {IsHero(props.context) 
                && <Hero 
                    heroVisible={props.context.heroVisible}
                    onSelection1={() =>
                        props.setState(
                            Ide.Updaters.Phases.hero.toBootstrap({kind: 'explore', upload: 'upload-not-started'} as Variant)
                                .then(CommonUI.Updater.Core.toggleHero()))}
                    onSelection2={() => {} }
                />}
            
            {(IsChoose(props.context) || IsLocked(props.context)) && 
                <>
                {!hideNavbar  && <Navbar theme={theme} setTheme={setTheme} />}
                    <PanelGroup className={"flex-1 min-h-0"} autoSaveId="example" direction="horizontal">
                    <Panel minSize={20} defaultSize={50}>
                        <aside className="relative h-full">
                        <AddSpec {...props.context} setState={props.setState} />
                        <AddOrSelectSpec {...props.context} setState={props.setState} />
                        <div className="w-full flex">
                            { IsLocked(props.context) && <SpecificationLabel name={props.context.name.value} /> }
                            <Actions
                                onNew={()=> props.setState(Ide.Updaters.Phases.hero.toBootstrap(props.context.variant))}
                                errorCount={5}
                                canValidate={props.context.phase == 'locked' && props.context.locked.workspace.kind == 'selected'}
                                onDeltaShow={() =>
                                    props.setState(Ide.Updaters.Phases.locking.progress(LockedStep.Updaters.Core.toggleDeltas()))
                                }
                                hideRight={hideForms}
                                context={props.context}
                                setState={props.setState}
                                onHide={() => setHideForms(!hideForms)}
                                onHideUp={() => setHideNavbar(!hideNavbar)}
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
                                             props.setState(Ide.Updaters.Phases.locking.refreshVfs(updated.value));
                                        }
                                        
                                    }
                                }
                                onMerge={ async ()=>{
                                    if(!(props.context.phase == 'locked' && props.context.locked.workspace.kind == 'selected' && props.context.locked.workspace.current.kind == 'file')) return;
                                    props.setState(
                                        CommonUI.Updater.Core.clearAllErrors())
                                    const json =
                                        props.context.variant.kind == 'compose' || props.context.variant.kind == 'scratch'
                                            ? await validateCompose(props.context.name.value)
                                            : await validateExplore(props.context.name.value, props.context.locked.workspace.current.file.metadata.path.split("/"))
                                    
                                    if(json.kind == "errors") {
                                        props.setState(CommonUI.Updater.Core.lockingErrors(json.errors));
                                        return
                                    }
          
                                    const bridge =
                                        props.context.variant.kind  == 'compose' || props.context.variant.kind  == 'scratch'
                                        ? ValueOrErrors.Default.throw(List(["compose or scratch bridge validator not implemented yet"]))    
                                        : await validateBridge(props.context.name.value,props.context.locked.workspace.current.file.metadata.path.split("/"));
                                    
                                    if(bridge.kind == "errors") {
                                        props.setState(CommonUI.Updater.Core.lockingErrors(bridge.errors));
                                        return
                                    }
                                    props.setState(LockedPhase.Updaters.Core.validated(json.value));
                                    notify.success("Validation succeeds")
                                    
                                }}
                                onSettings={()=> props.setState(CommonUI.Updater.Core.toggleSettings())}
                                onRun={async () =>{
                                    if(!(props.context.phase == "locked" && props.context.locked.workspace.kind == 'selected' && props.context.locked.workspace.current.kind == 'file')) { return;}
                                    const launchers = await getKeys(props.context.name.value, "launchers", props.context.locked.workspace.current.file.metadata.path.split("/"));
                                    
                                    if(launchers.kind == "errors") {
                                        props.setState(CommonUI.Updater.Core.lockingErrors(launchers.errors));
                                        return;
                                    }
                                    
                                    props.setState(LockedPhase.Operations.enableRun(launchers.value));
                                    forceRerender();
                                }}
                                onErrorPanel={() => setHideErrors(!hideErrors)}
                                onSeed={
                                    async () => {
                                        if(props.context.phase != "locked") {
                                            notify.error('UX bad state','Seeding should be possible only in a locked phase');
                                            return
                                        }
                                        
                                        const call =
                                            props.context.variant.kind  == 'explore'
                                            && props.context.locked.workspace.kind == 'selected'
                                            && props.context.locked.workspace.current.kind == 'file'
                                            ? await seedPath(props.context.name.value, props.context.locked.workspace.current.file.metadata.path.split("/")) 
                                            : await seed(props.context.name.value)
                                        if(call.kind == "errors") notify.error("Seeding failed")
                                        if(call.kind != "errors") notify.success("Seeding succeed")
          
                                        if(call.kind == "errors") props.setState(CommonUI.Updater.Core.lockingErrors(call.errors));
                                    }
                                }
                            />
                        </div>
                            <SettingsPanel  {...props.context} setState={props.setState}/>
                            <VfsLayout {...props.context} setState={props.setState} />
                      
                    </aside>
                    </Panel>
                    <PanelResizeHandle  className="w-[1px] bg-neutral text-neutral-content" />
                 {!hideForms && 
                    <Panel>
                        <PanelGroup direction="vertical">
                            <Panel minSize={0} defaultSize={50}  >
                                {/*<Panel minSize={0} defaultSize={50} style={{overflow: 'auto !important'}} >*/}
                                
                                <div data-theme={theme}  className="mockup-window border border-base-300 w-full  rounded-none">
                                    <aside className="relative h-full">
                                        <FormSkeleton {...props.context} setState={props.setState} />
                     
                                        { props.context.phase == "locked" 
                                            &&  props.context.locked.progress.kind == 'preDisplay' 
                                            && props.context.locked.progress.dataEntry.kind == 'launchers'
                                            && props.context.locked.workspace.kind == 'selected' && props.context.locked.workspace.current.kind == 'file'
                                            && <div className="card bg-base-100 w-full mt-1">
                                            <div className="card-body w-full">
                                                { props.context.locked.validatedSpec.kind == "r" 
                                                    && props.context.locked.progress.dataEntry.kind == 'launchers' 
                                                    && props.context.locked.progress.dataEntry.selected.kind == "r"
                                                    
                                                    && <>
                                
                                                        <DispatcherFormsApp
                                                        key={version}
                                                        showDeltas={props.context.locked.progress.showDeltas}
                                                        deltas={props.context.locked.progress.deltas}
                                                        specName={props.context.name.value}
                                                        path={props.context.locked.workspace.current.file.metadata.path.split("/")}
                                                        launcher={props.context.locked.progress.dataEntry.selected.value}
                                                        setState={props.setState}
                                                        //typeName={"Person"}
                                                        spec={props.context.locked.validatedSpec.value}/></>
                                                }
                                            </div>
                                            </div>
                                            }
                              
                                    </aside>
                                  
                                        {props.context.phase == "locked" && props.context.locked.progress.kind == "preDisplay" &&
                                            props.context.locked.progress.dataEntry.kind == 'launchers' &&
                                            <div className="flex justify-center items-start h-full  mt-auto p-4">
                                            <LaunchersDock
                                                launchers={props.context.locked.progress.dataEntry.launchers}
                                                selected={props.context.locked.progress.dataEntry.selected}
                                                onSelect={(launcher:string) => props.setState(LockedPhase.Updaters.Step.selectLauncher(launcher))}
                                            />
                                    </div>}

                                </div>

                            </Panel>
                            <PanelResizeHandle  className="h-[1px] bg-neutral text-neutral-content" />
                            {hideErrors && <Panel minSize={30} defaultSize={50} maxSize={90}>
                                <div className="no-radius w-full mx-auto">
                                    <div className="inset-0 top-0 z-20 m-0 p-0">
                                        <div className="space-y-2  w-full">
                                            {
                                                props.context.bootstrappingError.map(error => 
                                                    (<pre 
                                                        data-prefix="6"
                                                        className="m-0 pl-3 bg-rose-700 text-warning-content">
                                                        {error}
                                                    </pre>))
                                            }
                                            {
                                                props.context.choosingError.map(error => 
                                                    (<pre data-prefix="6"
                                                          className="m-0 pl-3 bg-rose-500 text-warning-content">
                                                        {error}
                                                    </pre>))
                                            }
                                            {
                                                props.context.lockingError.map(error => 
                                                    (<pre data-prefix="6"
                                                          className="m-0 pl-3 bg-rose-300 text-warning-content">
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

                            </Panel> }
                        </PanelGroup>
                     </Panel>}
                </PanelGroup></>}
        </div>)
};