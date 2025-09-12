/** @jsxImportSource @emotion/react */

import { style as editorStyle } from "./json-editor.styled.ts";
import {Ide, IdeView, update, seed, Bridge} from "playground-core";
import {V2Editor, V1Editor, SeedEditor} from "./json-editor.tsx";
import "react-grid-layout/css/styles.css";
import React, {useState} from "react";
import {Actions} from "./actions"
import {Messages} from "./messages";
import { Toaster } from 'sonner';
import {replaceWith,Value, Option} from "ballerina-core";
import {Grid} from "./grid.tsx";
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
declare const __ENV__: Record<string, string>;
console.table(__ENV__);
export const IdeLayout: IdeView = (props) =>{
    const [theme, setTheme] = useState("fantasy");
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
        <><Toaster />
            {props.context.phase == "bootstrap"
                && props.context.bootstrap.kind == "initializing"
                && <div className="w-screen h-screen  flex items-center justify-center">
                    <span className="loading loading-bars loading-xl"></span>
                    <p className="text-center p-7">{props.context.bootstrap.message}</p>

                </div> }
        <Grid
            theme={theme}
            left={
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
                                                button: {
                                                    label: 'Undo',
                                                    onClick: () => sonnerToast.dismiss(),
                                                },
                                            });
                                        }
                                    }
                                    >GO</button>
                                    <fieldset className="fieldset join-item">
                              
                                        <input type="file" className="file-input" />
                                        <label className="label">Max size 2MB</label>
                                    </fieldset>
                                </div>
                            </fieldset>}
                            { props.context.phase == "choose" && props.context.activeTab == "existing" && <HorizontalDropdown
                                label={"Select spec"}
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
                            <Actions
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
                                onNew={() => props.setState(Ide.Updaters.chooseNew())}
                                onLock={() =>
                                {

                                    ideToast({
                                        title: 'This is a headless toast',
                                        description: 'You have full control of styles and jsx, while still having the animations.',
                                        button: {
                                            label: 'Reply',
                                            onClick: () => sonnerToast.dismiss(),
                                        },
                                    });
    
                                    return;
                                }}
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

                    {/*<props.JsonEditor{...props} view={V1Editor} />*/}
                    {props.context.phase == "locked" &&  <props.JsonEditor{...props} view={V1Editor}/>}
                    {props.context.phase == "locked" &&  <props.JsonEditor{...props} view={V2Editor}/>}
                    {props.context.phase == "locked" &&  <props.JsonEditor{...props} view={SeedEditor}/>}
                    {/*<props.JsonEditor{...props} view={SeedEditor}/>*/}
                    <div className="drawer">
                        <input id="my-drawer" type="checkbox" className="drawer-toggle" />
                        <div className="drawer-content">
                            {/* Page content here */}
                            
                        </div>
                        <div className="drawer-side">
                            <label htmlFor="my-drawer" aria-label="close sidebar" className="drawer-overlay"></label>
                            <ul className="menu bg-base-200 text-base-content min-h-full w-80 p-4">
                                {/* Sidebar content here */}
                                <li><a>Sidebar Item 1</a></li>
                                <li><a>Sidebar Item 2</a></li>
                            </ul>
                        </div>
                    </div>
                    <div className="dock  bg-neutral text-neutral-content absolute bottom-0 left-0 right-0">
                        <button>
                            <svg className="size-[1.2em]" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g fill="currentColor" strokeLinejoin="miter" strokeLinecap="butt"><polyline points="1 11 12 2 23 11" fill="none" stroke="currentColor" stroke-miterlimit="10" strokeWidth="2"></polyline><path d="m5,13v7c0,1.105.895,2,2,2h10c1.105,0,2-.895,2-2v-7" fill="none" stroke="currentColor" strokeLinecap="square" stroke-miterlimit="10" strokeWidth="2"></path><line x1="12" y1="22" x2="12" y2="18" fill="none" stroke="currentColor" strokeLinecap="square" stroke-miterlimit="10" strokeWidth="2"></line></g></svg>
                            <label htmlFor="my-drawer" className="drawer-button"><span className="dock-label">Home</span></label>
                        </button>

                        <button className="dock-active">
                            <svg className="size-[1.2em]" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g fill="currentColor" strokeLinejoin="miter" strokeLinecap="butt"><polyline points="3 14 9 14 9 17 15 17 15 14 21 14" fill="none" stroke="currentColor" stroke-miterlimit="10" strokeWidth="2"></polyline><rect x="3" y="3" width="18" height="18" rx="2" ry="2" fill="none" stroke="currentColor" strokeLinecap="square" stroke-miterlimit="10" strokeWidth="2"></rect></g></svg>
                            <span className="dock-label">Inbox</span>
                        </button>

                        <button>
                            <svg className="size-[1.2em]" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g fill="currentColor" strokeLinejoin="miter" strokeLinecap="butt"><circle cx="12" cy="12" r="3" fill="none" stroke="currentColor" strokeLinecap="square" stroke-miterlimit="10" strokeWidth="2"></circle><path d="m22,13.25v-2.5l-2.318-.966c-.167-.581-.395-1.135-.682-1.654l.954-2.318-1.768-1.768-2.318.954c-.518-.287-1.073-.515-1.654-.682l-.966-2.318h-2.5l-.966,2.318c-.581.167-1.135.395-1.654.682l-2.318-.954-1.768,1.768.954,2.318c-.287.518-.515,1.073-.682,1.654l-2.318.966v2.5l2.318.966c.167.581.395,1.135.682,1.654l-.954,2.318,1.768,1.768,2.318-.954c.518.287,1.073.515,1.654.682l.966,2.318h2.5l.966-2.318c.581-.167,1.135-.395,1.654-.682l2.318.954,1.768-1.768-.954-2.318c.287-.518.515-1.073.682-1.654l2.318-.966Z" fill="none" stroke="currentColor" strokeLinecap="square" stroke-miterlimit="10" strokeWidth="2"></path></g></svg>
                            <span className="dock-label">Settings</span>
                        </button>
                    </div>
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
                }
            header={
                <div className="navbar bg-base-100 shadow-sm">
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
                            <li><a>docs</a></li>
                            <li>
                                {Themes.dropdown(theme, setTheme)}
                            </li>
                        </ul>
                    </div>
                </div>
                }
            right={
                <>
                    <PanelGroup autoSaveId="example" direction="horizontal">
                        <Panel defaultSize={25}>
                            <div className="bg-base-100">sdas as dasd</div>
                        </Panel>
                        <PanelResizeHandle  className="w-1 bg-neutral text-neutral-content" />
                        <Panel>
                            <div className="bg-base-100">sdas as dasd</div>
                        </Panel>
                        <PanelResizeHandle  className="w-1 bg-base-200/80 backdrop-blur border-l border-base-300"/>
                        <Panel defaultSize={25}>
                            <div className="bg-base-100">sdas as dasd</div>
                        </Panel>
                    </PanelGroup>;
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
                
                </>
            }
            bottom={
                <></>
        }/>
            </>
    )
};