import {
    seed,
    update,
    validateCompose, validateExplore, getSpec, seedPath, getKeys, validateBridge, WorkspaceVariant,
    Ide, WorkspaceState, LockedDisplay,
    IdeView, HeroPhase, BootstrapPhase,
    Deltas, Delta
} from "playground-core";
import "react-grid-layout/css/styles.css";
import React, {useState} from "react";
import {Actions} from "./actions"
import {themeChange} from 'theme-change'
import {useEffect} from 'react'
import {AppToaster, fromVoe,  notify} from "./toaster.tsx";
import {DispatcherFormsApp} from "../forms/forms.tsx";
import {Panel, PanelGroup,PanelResizeHandle} from "react-resizable-panels";
import {MissingSpecsInfoAlert} from "../bootstrap/no-specs-info.tsx";
import {LockedPhase} from "playground-core/ide/domains/phases/locked/state.ts";
import {Bootstrap} from "../bootstrap/loader.tsx";
import {Navbar} from "../bootstrap/navbar.tsx";
import {AddSpec} from "../choose/add-spec.tsx";
import {SpecificationLabel} from "../locked/specification-label.tsx";
import {VfsLayout} from "../vfs/layout.tsx";
import {AddOrSelectSpec} from "../choose/layout.tsx";
import {FormSkeleton} from "../forms/skeleton.tsx";
import {SettingsPanel} from "./settings.tsx";
import {List} from "immutable";
import {LaunchersDock} from "../forms/launchers-dock.tsx";
import {Maybe, ValueOrErrors, replaceWith} from "ballerina-core";
import {ErrorsPanel} from "./errors.tsx";
import {Hero} from "../hero/hero.tsx";
import {languages} from "monaco-editor";
import json = languages.json;

declare const __ENV__: Record<string, string>;
console.table(__ENV__);

export const IdeLayout: IdeView = (props) => {

    const [theme, setTheme] = useState("lofi");
    const [version, setVersion] = useState(0);
    
    const [hideForms, setHideForms] = useState(false);
    const [hideNavbar, setHideNavbar] = useState(false);
    const [hideErrors, setHideErrors] = useState(true);

    
    const [hero, setHero] = useState<Maybe<HeroPhase>>(HeroPhase.Default());
    const forceRerender = () => setVersion(v => v + 1);
    
    useEffect(() => {
        themeChange(false)
    }, [props.context.phase.kind == 'locked'])
    

    return (
        <div data-theme="lofi"  className="flex flex-col min-h-screen">
            <AppToaster />
            {props.context.phase.kind == 'hero'
                && <Hero
                    context={hero}
                    setState={setHero}
                    foreignMutations={
                        {
                            onSelect1: () =>
                                props.setState(
                                    Ide.Updaters.Core.phase.toBootstrap(
                                        {kind: 'explore'} as WorkspaceVariant)
                                ),
                            onSelect2: () =>  {},
                        }
                    }
                />}
            {props.context.phase.kind == 'bootstrap' && 
                <Bootstrap {...props.context.phase.bootstrap} />
            }

            {(props.context.phase.kind == 'locked' || props.context.phase.kind == 'selection') && 
                <>
                {!hideNavbar  && <Navbar theme={theme} setTheme={setTheme} />}
                    <PanelGroup className={"flex-1 min-h-0"} autoSaveId="example" direction="horizontal">
                    <Panel minSize={20} defaultSize={50}>
                        <aside className="relative h-full">
                            {props.context.phase.kind == 'selection' 
                                && <AddSpec {...props.context.phase.selection} setState={props.setState} />}
                            <AddOrSelectSpec {...props.context} setState={props.setState} />
                            <div className="w-full flex">
                                {props.context.phase.kind == 'locked' && <SpecificationLabel name={props.context.phase.locked.name} /> }
                                <Actions
                                    onNew={()=> {
                                        if (props.context.phase.kind == 'locked')
                                            props.setState(Ide.Updaters.Core.phase.toBootstrap(props.context.phase.locked.workspace.variant))
                                    }
                                    }
                                    errorCount={5}
                                    canValidate={props.context.phase.kind == 'locked' && props.context.phase.locked.workspace.kind == 'selected'}
                                    onDeltaShow={() =>
                                        props.setState(
                                            Ide.Updaters.Core.phase.locked(
                                                LockedPhase.Updaters.Core.display(
                                                    LockedDisplay.Updaters.Core.deltas(d => ({
                                                        ...d, 
                                                        visibility: d.visibility == 'fully-visible' ? 'fully-invisible' : 'fully-visible'} satisfies Delta)))))
                                    }
                                    hideRight={hideForms}
                                    context={props.context}
                                    setState={props.setState}
                                    onHide={() => setHideForms(!hideForms)}
                                    onHideUp={() => setHideNavbar(!hideNavbar)}
                                    onSave={
                                        async () => {
                                            if(!(props.context.phase.kind == "locked" 
                                                && props.context.phase.locked.workspace.kind == 'selected')) {
                                                notify.error('UX bad state','Saving should be possible only in a locked phase');
                                                return
                                            }
                              
                                            const currentFile = props.context.phase.locked.workspace.file;
                                            const call = 
                                                await update(
                                                    props.context.phase.locked.name, 
                                                    currentFile.metadata.path, currentFile.metadata.content);
                                            
                                            
                                            fromVoe(call,'Specification save');
                                            const updated = await getSpec(props.context.phase.locked.name);
                                            if(updated.kind == "value") {
                                                debugger
                                                 props.setState(
                                                     Ide.Updaters.Core.phase.locked(LockedPhase.Updaters.Core.workspace(
                                                         WorkspaceState.Updater.reloadContent(updated.value)
                                                     ))
                                                     )
                                            }
                                            else {

                                                props.setState(
                                                    Ide.Updaters.Core.phase.locked(LockedPhase.Updaters.Core.errors(
                                                        replaceWith(updated.errors)
                                                    ))
                                                )
                                            }
                                        }
                                    }
                                    onMerge={ async ()=>{
                                        if(!(props.context.phase.kind === 'locked' && props.context.phase.locked.workspace.kind === 'selected')) return;
                                        
                                        const locked = props.context.phase.locked;
                                        const variant = locked.workspace.variant.kind
                                        
                                        props.setState(Ide.Updaters.Core.phase.locked(
                                            LockedPhase.Updaters.Core.errors(replaceWith(List()))
                                        ))
                                        
                                        const json =
                                            variant == 'compose' || variant == 'scratch'
                                                ? await validateCompose(locked.name)
                                                : await validateExplore(locked.name, props.context.phase.locked.workspace.file.metadata.path.split("/"))
                                        
                                        if(json.kind == "errors") {
                                            props.setState(Ide.Updaters.Core.phase.locked(
                                                LockedPhase.Updaters.Core.errors(replaceWith(json.errors))))
                                            return
                                        }
              
                                        const bridge =
                                            locked.workspace.variant.kind  == 'compose' || props.context.phase.locked.workspace.variant.kind  == 'scratch'
                                            ? ValueOrErrors.Default.throw(List(["compose or scratch bridge validator not implemented yet"]))    
                                            : await validateBridge(props.context.phase.locked.name,props.context.phase.locked.workspace.file.metadata.path.split("/"));
                                        
                                        if(bridge.kind == "errors") {
                                            props.setState(Ide.Updaters.Core.phase.locked(
                                                LockedPhase.Updaters.Core.errors(replaceWith(bridge.errors))))
                                            return
                                        }
                                        props.setState(Ide.Updaters.Core.phase.locked(LockedPhase.Updaters.Core.validated(json.value)));
                                        notify.success("Validation succeeds")
                                        
                                    }}
                                    onSettings={()=> 
                                        props.setState(
                                            Ide.Updaters.Core.phase.locked(LockedPhase.Updaters.Core.toggleSettings())
                                            
                                        )}
                                    onRun={async () =>{
                                        if(!(props.context.phase.kind == "locked" && props.context.phase.locked.workspace.kind == 'selected')) { return;}
                                        const launchers = await getKeys(props.context.phase.locked.name, "launchers", props.context.phase.locked.workspace.file.metadata.path.split("/"));
                                        
                                        if(launchers.kind == "errors") {
                                            props.setState(Ide.Updaters.Core.phase.locked(
                                                LockedPhase.Updaters.Core.errors(replaceWith(launchers.errors))))
                                            return;
                                        }
                                        debugger
                                        props.setState(
                                            Ide.Updaters.Core.phase.locked(LockedPhase.Updaters.Core.toDisplay(launchers.value))
                                            );
                                        forceRerender();
                                    }}
                                    onErrorPanel={() => setHideErrors(!hideErrors)}
                                    onSeed={
                                        async () => {
                                            if(props.context.phase.kind != "locked") {
                                                notify.error('UX bad state','Seeding should be possible only in a locked phase');
                                                return
                                            }
                                            
                                            const call =
                                                props.context.phase.locked.workspace.variant.kind  == 'explore'
                                                && props.context.phase.locked.workspace.kind == 'selected'
                                                ? await seedPath(props.context.phase.locked.name, props.context.phase.locked.workspace.file.metadata.path.split("/")) 
                                                : await seed(props.context.phase.locked.name)
                                            if(call.kind == "errors") notify.error("Seeding failed")
                                            if(call.kind != "errors") notify.success("Seeding succeed")
              
                                            if(call.kind == "errors") props.setState(Ide.Updaters.Core.phase.locked(
                                                LockedPhase.Updaters.Core.errors(replaceWith(call.errors))))
                                        }
                                    }
                                />
                            </div>
                            {props.context.phase.kind == 'locked' && <SettingsPanel  {...props.context.phase.locked} setState={props.setState}/> }
                                <VfsLayout {...props.context} setState={props.setState} />
                                {props.context.phase.kind == "locked" && props.context.phase.locked.step.kind == "display" 
                                    && 
                                    <LaunchersDock
                                    launchers={props.context.phase.locked.step.display.launchers.names}
                                    selected={props.context.phase.locked.step.display.launchers.selected}
                                    onSelect={(launcher:string) =>
                                        props.setState(
                                            Ide.Updaters.Core.phase.locked(LockedPhase.Updaters.Core.selectLauncher(launcher))
                                        )}
                                />}
                      
                    </aside>
                    </Panel>
                    <PanelResizeHandle  className="w-[1px] bg-neutral text-neutral-content" />
                 {!hideForms && 
                    <Panel>
                        <PanelGroup direction="vertical">
                            <Panel minSize={0} defaultSize={50}  >
                                {/*<Panel minSize={0} defaultSize={50} style={{overflow: 'auto !important'}} >*/}
                                
                                <div data-theme={theme}  className="overflow-auto mockup-window border border-base-300 w-full h-full rounded-none relative flex flex-col">
                                    <aside className="flex-1">
                                        <FormSkeleton {...props.context} setState={props.setState} />
                     
                                        { props.context.phase.kind == "locked" 
                                            &&  props.context.phase.locked.step.kind == 'display'
                                            && props.context.phase.locked.workspace.kind == 'selected'
                                            && <div className="m-5 h-full">
                                       
                                                { props.context.phase.locked.validatedSpec.kind == "r" 
                                                    && props.context.phase.locked.step.display.launchers.selected.kind == "r"
                                                    
                                                    && <>
                                                        <div role="tablist" className="tabs tabs-lift w-full">
                                                            <a role="tab" className={`tab ${props.context.phase.locked.step.display.ui.kind === "ui-kit" ? "tab-active" : ""}`}
                                                                onClick={()=> {
                                                                    props.setState(
                                                                        Ide.Updaters.Core.phase.locked(
                                                                            LockedPhase.Updaters.Core.display(
                                                                                LockedDisplay.Updaters.Core.change ('ui-kit', 'blp')
                                                                            )
                                                                        )
                                                                    )
                                                                    forceRerender();
                                                                }
                                                                }>ui-kit</a>
                                                            <a role="tab" className={`tab ${props.context.phase.locked.step.display.ui.kind === "tailwind" ? "tab-active" : ""}`}
                                                               onClick={()=> {
                                                                   props.setState(
                                                                       Ide.Updaters.Core.phase.locked(
                                                                           LockedPhase.Updaters.Core.display(
                                                                               LockedDisplay.Updaters.Core.change ('tailwind', 'lofi')
                                                                           )
                                                                       )
                                                                   )
                                                                   forceRerender();
                                                               }
                                                               }>tailwind</a>
                                            
                                                        </div>
                                                        <DispatcherFormsApp
                                                        key={version}
                                                        ui={props.context.phase.locked.step.display.ui}
                                                        showDeltas={props.context.phase.locked.step.display.deltas.visibility == 'fully-visible'}
                                                        deltas={props.context.phase.locked.step.display.deltas.drain}
                                                        specName={props.context.phase.locked.name}
                                                        path={props.context.phase.locked.workspace.file.metadata.path.split("/")}
                                                        launcher={props.context.phase.locked.step.display.launchers.selected.value}
                                                        setState={props.setState}
                                                        //typeName={"Person"}
                                                        spec={props.context.phase.locked.validatedSpec.value}/></>
                                                }
                                            </div>
                                            }
                                    </aside>
                                    
                                </div>

                            </Panel>
                            <PanelResizeHandle  className="h-[1px] bg-neutral text-neutral-content" />
                            {hideErrors && 
                                <Panel minSize={30} defaultSize={50} maxSize={90}>
                                    <ErrorsPanel {...props.context} />
                                </Panel>}
                        </PanelGroup>

                     </Panel>}
                </PanelGroup></>}
        </div>)
};