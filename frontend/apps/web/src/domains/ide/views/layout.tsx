/** @jsxImportSource @emotion/react */

import { style as editorStyle } from "./json-editor.styled.ts";
import {Ide, IdeView, update, seed, Bridge, VfsWorkspace} from "playground-core";
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
import {seedSpecErrorHandler, updateSpecErrorHandler} from "./ErrorHandlers/UpdateSpec.ts";
import {DispatcherFormsApp} from "./forms.tsx";
import {Panel, PanelGroup,PanelResizeHandle} from "react-resizable-panels";
import {drawer} from "./domains/vfs/drawer.tsx";
import {VscDatabase, VscFolder, VscGithub} from "react-icons/vsc";
import {NoSpescInfo} from "./domains/bootstrap/NoSpecsInfo.tsx";
import * as repl from "node:repl";
import {breadcrumbs} from "./domains/vfs/breadcrumbs.tsx";
import {folderFilter} from "./domains/vfs/folder-filter.tsx";
declare const __ENV__: Record<string, string>;
console.table(__ENV__);

export type DockItem = 'folders' | 'about'
export const IdeLayout: IdeView = (props) =>{
    const [theme, setTheme] = useState("lofi");
    const [dockItem, setDockItem] = useState('folders' as DockItem);
    //const [hideLeft, setHideLeft] = useState(false);
    const [hideRight, setHideRight] = useState(false);
    useEffect(() => {
        themeChange(false)
    }, [])
    useEffect(() => {
        if(props.context.bootstrappingError.kind == "r"){
            ideToast({
                title: 'Error',
                description: `${props.context.bootstrappingError.value}`,
                button: {
                    label: 'Ok',
                    onClick: () => sonnerToast.dismiss(),
                },
            });
        }
    }, [props.context.bootstrappingError])
    return (
        <div data-theme={theme} className="flex flex-col min-h-screen">
            <Toaster />
            {props.context.phase == "choose" && props.context.existing.specs.length == 0 && <NoSpescInfo />}
            {props.context.phase == "bootstrap"
                && props.context.bootstrap.kind == "initializing"
                && <div className="w-screen h-screen  flex items-center justify-center">
                    <p className="text-center p-7">{props.context.bootstrap.message}</p>

                </div> }
            <div className="navbar bg-base-100 shadow-sm sticky top-0 z-50 h-16  ">
                <img
                    style={{height: 80}}
                    src="https://github.com/Ballerina-Org/ballerina/raw/main/docs/pics/Ballerina_logo-04.svg"
                    alt="Ballerina"
                />
                <div className="flex-1">
                    <a className="btn btn-ghost text-xl">IDE</a>
                </div>

                <div className="flex-none mr-32">
                    <ul className="menu menu-horizontal px-1"  style={{zIndex: 10000, position:"relative"}}>
                        <li>
                            {Themes.dropdown(theme, setTheme)}
                        </li>
                    </ul>
                </div>
            </div>
           
                <PanelGroup className={"flex-1 min-h-0"} autoSaveId="example" direction="horizontal">
                    {<Panel  minSize={20} defaultSize={50}>
                        <aside className="relative h-full">

                            <div css={editorStyle.container}>
                                <div css={editorStyle.row}>
                                    { props.context.phase == "choose" && props.context.activeTab == "new" && <fieldset className="fieldset ml-4">
                                        <div className="join">
                                            <input
                                                type="text"
                                                className="input join-item"
                                                placeholder="Spec name"
                                                value={props.context.create.name.value}
                                                onChange={(e) =>
                                                    props.setState(
                                                        Ide.Updaters.specName(e.target.value))
                                                }
                                            />
                                            <button
                                                className="btn join-item"
                                                onClick={ async () => {
                                                    const u = await Ide.Operations.toLockedSpec('new', props.context.create.name.value);

                                                    props.setState(u);
                                                    ideToast({
                                                        title: `${props.context.create.name.value} has been locked`,
                                                        description: 'From now on your focus is on playing with that spec',
                                                        position: 'top-center',
                                                        button: {
                                                            label: 'Undo',
                                                            onClick: () => sonnerToast.dismiss(),
                                                        },
                                                    });
                                                }
                                                }
                                            >GO</button>
                 
                                        </div>
                                    </fieldset>}
                                    { props.context.phase == "choose" && props.context.activeTab == "existing" && <HorizontalDropdown
                                        label={"Select spec 2"}
                                        onChange={async (name: string) => {
                                            const u = await Ide.Operations.toLockedSpec('existing', name);
                                            ideToast({
                                                title: 'This is a headless toast',
                                                description: 'You have full control of styles and jsx, while still having the animations.',
                                                button: {
                                                    label: 'Reply',
                                                    onClick: () => sonnerToast.dismiss(),
                                                },
                                            });
                                            props.setState(u)

                                        }}
                                        options={props.context.existing.specs}/>
                                    }
                                    {props.context.phase == 'locked' && <fieldset className="fieldset pl-5">
                                        <legend className="fieldset-legend">Specification name</legend>
                                        <input disabled={true} type="text" className="input" value={props.context.create.name.value} placeholder="My awesome page" />
                                        
                                    </fieldset>}
                              
                                    <Actions
                                        //hideLeft={hideLeft}
                                        hideRight={hideRight}
                                        context={props.context}
                                        onValidateBridge={async () => {
                                            // if (props.context.selectedLauncher.kind == "r") {
                                            //     const res = await validate(props.context.specName.value, props.context.selectedLauncher.value.value);
                                            //     if (res.kind == "errors")
                                            //         props.setState(Ide.Updaters.Core.bridge(
                                            //                 Bridge.Operations.errors(res.errors as unknown as string [])
                                            //             )
                                            //         );
                                            // }
                                        }}
                                        
                                        onValidateV1={async () => {
                                            // const u = await Ide.Operations.validateV1(props.context);
                                            // props.setState(u);
                                        }}
                                        //onLeft={() => setHideLeft(!hideLeft)}
                                        onRight={() => setHideRight(!hideRight)}
                                        onNew={() => props.setState(Ide.Updaters.chooseNew())}
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
                                                const call = await update(props.context.create.name.value, Bridge.Operations.toVSpec(props.context.locked.bridge.spec))
                                                updateSpecErrorHandler(call);
                                            }
                                        }
                                        onRun={() => props.setState(Ide.Updaters.runForms())}
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
                                                    props.setState(Ide.Updaters.lockedSpec.seed(call.value));
                                            }
                                        }
                                    />
                                </div>
                            </div>
                           
                            {props.context.phase == "locked" && props.context.locked.virtualFolders.selectedFolder.kind == "r" &&props.context.locked.virtualFolders.selectedFolder.value.folder.kind == "folder" &&
                                <fieldset className="fieldset ml-5">
                                    {breadcrumbs(props.context.locked.virtualFolders.selectedFolder.value.folder)}
        
                                    <div className="join">
                                        <div className="filter mb-7  join-item">
                                            <input className="btn filter-reset" type="radio" name="virtual-files" aria-label="All"/>
                                            {props.context.locked.virtualFolders.selectedFolder.value.files.map(f =>
                                                (<div className="tooltip tooltip-bottom" data-tip={f.path}>
                                                    <input
                                                        className="btn"
                                                        type="radio"
                                                        name="virtual-files"
                                                        checked={
                                                            props.context.phase == 'locked'
                                                            && props.context.locked.virtualFolders.selectedFile.kind == "r"
                                                            && props.context.locked.virtualFolders.selectedFile.value == f.name}
                                                        onClick={()=>
                                                            props.setState(
                                                                Ide.Updaters.lockedSpec.vfs.selectedFolder(
                                                                    (Updater(VfsWorkspace.Updaters.Core.selectedFile(

                                                                        f.name

                                                                    )))
                                                                )
                                                            )} aria-label={f.name?.replace(/\.json$/,"")}/>
                                                </div>))}

                                        </div>
                                        {folderFilter(props.context.locked.virtualFolders.selectedFolder.value,props.context.locked.virtualFolders.selectedFile)}
                                        <div className="ml-5  join-item">
                                            <button 
                                                className="btn join-item"
                                                onClick={async () =>{
                                                    if(!(props.context.phase == "locked" && props.context.locked.virtualFolders.selectedFolder.kind == "r" &&props.context.locked.virtualFolders.selectedFolder.value.folder.kind == "folder" && props.context.locked.virtualFolders.selectedFile.kind == "r"))
                                                        return;
                                                    const file = props.context.locked.virtualFolders.selectedFolder.value.folder.children.get(props.context.locked.virtualFolders.selectedFile.value)!;
                                                    if(file.kind == "file"){
                                                        const content = await file.value.fileRef?.text()!;
                                                        props.setState(Ide.Updaters.lockedSpec.bridge.v1(content));
                                                    } }}
                                            >Load</button>
                                        </div>
                                    </div>
                                </fieldset>
                                }

                            {props.context.phase == "locked" &&  <props.JsonEditor{...props} view={V1Editor}/>}
                            {props.context.phase == "locked" &&  <props.JsonEditor{...props} view={V2Editor}/>}
                            {props.context.phase == "locked" &&  <props.JsonEditor{...props} view={SeedEditor}/>}
                            {/*<props.JsonEditor{...props} view={SeedEditor}/>*/}
                            { drawer(dockItem, 
                                node => {
                                    const next = 
                                        Updater(VfsWorkspace.Updaters.Core.selectedNode(node))
                                    
                                    props.setState(
                                        Ide.Updaters.lockedSpec.vfs.selectedFolder(
                                            next
                                        )
                                    )
                                }
                            )}
                            {/*<div className="dock  bg-neutral text-neutral-content absolute bottom-0 left-0 right-0">*/}
                            {/*    <label*/}
                            {/*        htmlFor="my-drawer" onClick={() => setDockItem('folders')} */}
                            {/*        className="flex flex-col items-center justify-center gap-1"*/}
                            {/*    >*/}
                            {/*        <VscFolder size={20}  />*/}
                            {/*        <span className="dock-label">Folders</span>*/}
                            {/*    </label>*/}
                            
                            {/*    <label*/}
                            {/*        onClick={() => setDockItem('about')}*/}
                            {/*        htmlFor="my-drawer"*/}
                            {/*        className="flex flex-col items-center justify-center gap-1"*/}
                            {/*    >*/}
                            {/*        <VscGithub size={20}  className="drawer-button" />*/}
                            {/*        <span className="dock-label">About</span>*/}
                            {/*    </label>*/}
                            
                            {/*</div>*/}
                            {/*<div className="dock absolute bottom-0 left-0 right-0">*/}

                            {/*    <label htmlFor="my-drawer" className="drawer-button">       <svg className="size-[1.2em]" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g fill="currentColor" strokeLinejoin="miter" strokeLinecap="butt"><polyline points="3 14 9 14 9 17 15 17 15 14 21 14" fill="none" stroke="currentColor" stroke-miterlimit="10" strokeWidth="2"></polyline><rect x="3" y="3" width="18" height="18" rx="2" ry="2" fill="none" stroke="currentColor" strokeLinecap="square" stroke-miterlimit="10" strokeWidth="2"></rect></g></svg>*/}
                            {/*        <span className="dock-label">Inbox</span></label>*/}
                            {/*    <button className="dock-active">*/}
                            {/*        <svg className="size-[1.2em]" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g fill="currentColor" strokeLinejoin="miter" strokeLinecap="butt"><polyline points="3 14 9 14 9 17 15 17 15 14 21 14" fill="none" stroke="currentColor" stroke-miterlimit="10" strokeWidth="2"></polyline><rect x="3" y="3" width="18" height="18" rx="2" ry="2" fill="none" stroke="currentColor" strokeLinecap="square" stroke-miterlimit="10" strokeWidth="2"></rect></g></svg>*/}
                            {/*        <span className="dock-label">Inbox</span>*/}
                            {/*    </button>*/}

                            {/*    <button>*/}
                            {/*        <svg className="size-[1.2em]" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g fill="currentColor" strokeLinejoin="miter" strokeLinecap="butt"><circle cx="12" cy="12" r="3" fill="none" stroke="currentColor" strokeLinecap="square" stroke-miterlimit="10" strokeWidth="2"></circle><path d="m22,13.25v-2.5l-2.318-.966c-.167-.581-.395-1.135-.682-1.654l.954-2.318-1.768-1.768-2.318.954c-.518-.287-1.073-.515-1.654-.682l-.966-2.318h-2.5l-.966,2.318c-.581.167-1.135.395-1.654.682l-2.318-.954-1.768,1.768.954,2.318c-.287.518-.515,1.073-.682,1.654l-2.318.966v2.5l2.318.966c.167.581.395,1.135.682,1.654l-.954,2.318,1.768,1.768,2.318-.954c.518.287,1.073.515,1.654.682l.966,2.318h2.5l.966-2.318c.581-.167,1.135-.395,1.654-.682l2.318.954,1.768-1.768-.954-2.318c.287-.518.515-1.073.682-1.654l2.318-.966Z" fill="none" stroke="currentColor" strokeLinecap="square" stroke-miterlimit="10" strokeWidth="2"></path></g></svg>*/}
                            {/*        <span className="dock-label">Settings</span>*/}
                            {/*    </button>*/}
                            {/*</div>*/}
                        </aside>
                    </Panel> }
                    <PanelResizeHandle  className="w-0.5 bg-neutral text-neutral-content" />
                    {!hideRight && 
                        <Panel>
                            <PanelGroup direction="vertical">
                                <Panel>
                                    <div className="mockup-window border border-base-300 w-full">
                                      
                            
                        <aside className="relative h-full">

                            {props.context.phase == "bootstrap" && <p> {props.context.bootstrap.kind}</p> }
                            {props.context.bootstrappingError.kind == "r" && <p> {props.context.bootstrappingError.value}</p> }
                            {!(props.context.phase == 'locked' && props.context.step == 'outcome') && <div className="flex w-full flex-col gap-4 p-7  shadow-sm backdrop-blur-md">
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
                                                Ide.Updaters.lockedSpec.selectLauncher(value)
                                            )
                                        }}
                                        options={props.context.locked.launchers}

                                    />
                                        { props.context.locked.selectedLauncher.kind == "r" && <DispatcherFormsApp
                                            key={props.context.locked.bridge.spec.left.specBody.value}
                                            specName={props.context.create.name.value}
                                            entityName={"People"}
                                            typeName={"Person"}
                                            spec={props.context.locked.bridge.spec.left.specBody.value}/>
                                        }
                                    </>
                                    }
                                </div>
                        
                            </div>}
       
       

                        </aside>        </div>
                                </Panel>
                                <PanelResizeHandle  className="h-0.5 bg-neutral text-neutral-content" />
                                <Panel minSize={10} defaultSize={20} maxSize={30}><div className="no-radius w-full">
                                    <pre data-prefix="1" className="pl-3 bg-gray-300 text-warning-content"><code>Error!</code> Can't find the ...</pre>
                                    <pre data-prefix="2" className="pl-3 bg-gray-300 text-warning-content"><code>Error!</code> Can't find the ...</pre>
                                    <pre data-prefix="3" className="pl-3 bg-accent text-primary-content"><code>Error!</code> Can't find the ...</pre>
                                    <pre data-prefix="4" className="pl-3 bg-warning text-warning-content"><code>Error!</code> Can't find the ...</pre>
                                    <pre data-prefix="5" className="pl-3 bg-gray-300 text-warning-content"><code>Error!</code> Can't find the ...</pre>
                                    <pre data-prefix="6" className="pl-3 bg-gray-300 text-warning-content"><code>Error!</code> Can't find the ...</pre>
                                </div></Panel>
                            </PanelGroup>
                    </Panel>}

                </PanelGroup>
         
           

            </div>
    )
};