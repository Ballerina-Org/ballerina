/** @jsxImportSource @emotion/react */

import { style as editorStyle } from "./json-editor.styled.ts";
import {Ide, IdeView} from "playground-core";
import {V2Editor} from "./json-editor.tsx";
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
declare const __ENV__: Record<string, string>;
console.table(__ENV__);
export const IdeLayout: IdeView = (props) =>{
    const [theme, setTheme] = useState("fantasy");
    useEffect(() => {
        themeChange(false)
    }, [])
    return (
        <><Toaster />
            {props.context.phase == "bootstrap"
                && props.context.bootstrap.kind == "initializing"
                && <div className="w-screen h-screen  flex items-center justify-center">
                    <span className="loading loading-bars loading-xl"></span>
                    <p className="text-center">{props.context.bootstrap.message}</p>

                </div> }
        <Grid
            theme={theme}
            left={
                <>

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
                                
                                        // if (props.context.bridge.spec.right.right.kind == "value" && props.context.bridge.spec.left.right.kind == "value") {
                                        //     const spec = {
                                        //         name: props.context.specName.value,
                                        //         fields: props.context.bridge.spec.right.right.value
                                        //     };
                                        //     const updated = await Api.updateBridge(props.context.bridge.spec.left.right.value, spec);
                                        // }
                                    }
                                }
                                onReSeed={
                                    async () => {
                                        // const r = await reseed(props.context.specName.value,props.context.bridge.spec.left.left.specBody.value);
                                        // const t = await Api.getSeed(props.context.specName.value);
                                        // switch (t.kind) {
                                        //     case "value": {
                                        //         void props.setState(
                                        //             Ide.Updaters.Core.bridge(Bridge.Updaters.Core.seeds(replaceWith(t.value)))
                                        //         );
                                        //         break;
                                        //     }
                                        //     case "errors": {
                                        //         break;
                                        //     }
                                        //     default: {
                                        //         break;
                                        //     }
                                        // }
                                        }}
                                onSeed={
                                    async () => {
                                        // const t = await Api.getSeed(props.context.specName.value);
                                        // switch (t.kind) {
                                        //     case "value": {
                                        //         void props.setState(Ide.Updaters.Core.bridge(Bridge.Updaters.Core.seeds(replaceWith(t.value))));
                                        //         break;
                                        //     }
                                        //     case "errors": {
                                        //         break;
                                        //     }
                                        //     default: {
                                        //         break;
                                        //     }
                                        // }

                                    }
                                }
                            />
                        </div>
                    </div>

                    {/*<props.JsonEditor{...props} view={V1Editor} />*/}
                    {props.context.phase == "locked" &&  <props.JsonEditor{...props} view={V2Editor}/>}
                    {/*<props.JsonEditor{...props} view={SeedEditor}/>*/}
                </>}
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
                    
                    {props.context.phase == "bootstrap" && <p> {props.context.bootstrap.kind}</p> }
                    {props.context.bootstrappingError.kind == "r" && <p> {props.context.bootstrappingError.value}</p> }
                    <div className="flex w-full flex-col gap-4 p-7">
                        <div className="skeleton h-32 w-full"></div>
                        <div className="skeleton h-4 w-28"></div>
                        <div className="skeleton h-4 w-full"></div>
                        <div className="skeleton h-4 w-full"></div>
                    </div>
                   {/* <Messages*/}
                   {/*     clientErrors={props.context.bridge.spec.left.right.kind == "errors" ? props.context.bridge.spec.left.right.errors.toArray() : []}*/}
                   {/*     bridgeErrors={props.context.bridge.errors}*/}
                   {/* />*/}
                   {/*<>*/}
                   {/*     <div className="card bg-base-100 w-full mt-5">*/}
                   {/*         <div className="card-body w-full">*/}
                   {/*             {props.context.bridge.spec.left.right.kind == "value" && <><LauncherSelector*/}
                   {/*                 onChange={async (value: string) => {*/}
                   
                   {/*                     props.setState(*/}
                   {/*                         Ide.Updaters.Core.selectedLauncher(*/}
                   {/*                             replaceWith(*/}
                   {/*                                 Option.Default.some(*/}
                   {/*                                     Value.Default(value)*/}
                   {/*                                 )*/}
                   {/*                             )*/}
                   {/*                         )*/}
                   {/*                     )*/}
                   {/*                 }}*/}
                   {/*                 options={props.context.launchers}*/}
                   {/*    */}
                   {/*             />*/}
                   {/*                 <DispatcherFormsApp*/}
                   {/*                     key={props.context.bridge.spec.left.left.specBody.value}*/}
                   {/*                     specName={props.context.specName.value}*/}
                   {/*                     entityName={"People"}*/}
                   {/*                     typeName={"Person"}*/}
                   {/*                     spec={props.context.bridge.spec.left.left.specBody.value}/></>*/}
                   {/*             }*/}
                   {/*         </div>*/}
                   {/*     </div>*/}
                   {/*</>*/}
                </>
            }/>
            </>
    )
};