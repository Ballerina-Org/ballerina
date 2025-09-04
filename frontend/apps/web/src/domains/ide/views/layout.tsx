/** @jsxImportSource @emotion/react */
import {style} from "./ide.styled.ts";
import { style as editorStyle } from "./json-editor.styled.ts";
import {Ide, IdeView, validateV1} from "playground-core";
import {V1Editor, SeedEditor, V2Editor} from "./json-editor.tsx";
import "react-grid-layout/css/styles.css";
import React, {useState} from "react";
import {Actions} from "./actions"
import {Messages} from "./messages";

import {
    replaceWith,
    Value,
    Option,
    ValueOrErrors,
    Debounced,
    Synchronized,
    Updater,
    PassthroughLauncher, DispatchFormsParserState
} from "ballerina-core";
import {Grid} from "./grid.tsx";
import RadioButtons from "./radio-buttons.tsx";
import {HorizontalDropdown} from "./dropdown.tsx";
import {themeChange} from 'theme-change'
import {useEffect} from 'react'
import {getSpecs, validate} from "playground-core"

import * as Api from "playground-core/ide/api/specs";
import {Bridge, BridgeState} from "playground-core/ide/domains/bridge/state.ts";
import { Map } from "immutable";
import {DispatchPassthroughFormInjectedTypes} from "../../dispatched-passthrough-form/injected-forms/category.tsx";
import {
    DispatchPassthroughFormCustomPresentationContext, DispatchPassthroughFormExtraContext,
    DispatchPassthroughFormFlags
} from "../../dispatched-passthrough-form/views/concrete-renderers.tsx";
import {DispatcherFormsApp} from "./forms.tsx";
export const IdeLayout: IdeView = (props) =>{
    const [key, setKey] = useState(0);
    const [theme, setTheme] = useState("fantasy");

    useEffect(() => {

        themeChange(false)
    }, [])
    return (
        <Grid
            theme={theme}
            left={
                <>
                    <div css={editorStyle.container}>
                        <div css={editorStyle.row}>
                            <fieldset className="fieldset ml-4">
                      
                                <div className="join">
                                    <input 
                                        type="text" 
                                        className="input join-item" 
                                        placeholder="Product name"
              
                                        value={props.context.specName.value}
                                        onChange={(e) =>
                                            props.setState(
                                                Ide.Updaters.Core.specName(replaceWith({value: e.target.value})))}
                                    />
                                    <button className="btn join-item">save</button>
                                </div>
                            </fieldset>
         

                            <Actions
                                onValidateBridge={async () => {

                                    if (props.context.launcherName.kind == "r") {
                                        const res = await validate(props.context.specName.value, props.context.launcherName.value.value);
                                        if (res.kind == "errors")
                                            props.setState(Ide.Updaters.Core.bridge(
                                                    Bridge.Operations.errors(res.errors as unknown as string [])
                                                )
                                            );
                                    }
                                }}
                                onValidateV1={async () => {
                                    const u = await Ide.Operations.validateV1(props.context);
                                    props.setState(u);

                                }}
                                onSave={
                                    async () => {

                                        //debugger
                                        if (props.context.bridge.bridge.right.right.kind == "value" && props.context.bridge.bridge.left.right.kind == "value") {
                                            const spec = {
                                                name: props.context.specName.value,
                                                fields: props.context.bridge.bridge.right.right.value
                                            };
                                            debugger
                                            const t = await Api.updateBridge(props.context.bridge.bridge.left.right.value, spec);
                                        }
                                    }
                                }
                                // onReSeed={
                                //     async () => {
                                //         const t1 = await NextApi.reseed(props.context.specName.value);
                                //         const t = await NextApi.getSeed(props.context.specName.value);
                                //         switch (t.kind) {
                                //
                                //             case "value":
                                //             {
                                //
                                //                 void props.setState(
                                //                     IDE.Updaters.Core.editor(SpecEditor.Operations.seed(t.value))
                                //                 );
                                //                 break;
                                //             }
                                //             case "errors": {
                                //                 break;
                                //             }
                                //             default: {break;}
                                //         }}}
                                onSeed={
                                    async () => {
                                        const t = await Api.getSeed(props.context.specName.value);
                                        switch (t.kind) {

                                            case "value": {


                                                void props.setState(
                                                    Ide.Updaters.Core.bridge(Bridge.Updaters.Core.seeds(replaceWith(t.value)))
                                                );
                                                break;
                                            }
                                            case "errors": {
                                                break;
                                            }
                                            default: {
                                                break;
                                            }
                                        }
                                        // const ste= props.setState(
                                        //   IDE.Updaters.Core.loading(replaceWith(true)
                                        //   )
                                        // )
                                        // const res = await IDEApi.validateSpec({value: props.context.editor.input.value});
                                        //
                                        // if (res.isValid) {
                                        //   const entity = await IDEApi.save(props.context.editor.name.value, props.context.editor.input.value)
                                        //   const entityNames = await IDEApi.entity_names(props.context.specName.value);
                                        //   props.setState(
                                        //     IDE.Updaters.Core.runner(SpecRunner.Updaters.Core.validation(
                                        //       replaceWith(
                                        //         Option.Default.some(ValueOrErrors.Default.return(props.context.editor.input.value))
                                        //       )
                                        //     )).then(IDE.Updaters.Core.entityNames(
                                        //       replaceWith(entityNames.payload)
                                        //     ).then( IDE.Updaters.Core.loading(replaceWith(false)
                                        //     )).then( IDE.Updaters.Core.step(replaceWith(IdeStep.Default.entity())
                                        //     )))
                                        //   )
                                        // } else {
                                        //   props.setState(IDE.Updaters.Core.runner(SpecRunner.Updaters.Core.validation(
                                        //     replaceWith(
                                        //       Option.Default.some(ValueOrErrors.Default.throwOne(res.errors))
                                        //     )
                                        //   )).then( IDE.Updaters.Core.loading(replaceWith(false)
                                        //   )));
                                        // }

                                    }
                                }

                            />
                        </div>

                    </div>

                    <props.JsonEditor{...props} view={V1Editor} />
                    <props.JsonEditor{...props} view={V2Editor}/>
                    <props.JsonEditor{...props} view={SeedEditor}/>
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
                             <HorizontalDropdown
                                 label={"Select spec"}
                                 onChange={async (name: string) => {
                    
                                     const t = await Api.getSpec(name);
                                    props.setState(
                                         Ide.Updaters.Core.bridge(
                                             Bridge.Operations.load(t)
                                         ).then(Ide.Updaters.Core.specName(replaceWith(Value.Default(name))))
                                     )
                    
                                 }}
                                 options={props.context.specNames}/>
                    <div className="flex-none mr-32">
                        <ul className="menu menu-horizontal px-1"  style={{zIndex: 10000, position:"relative"}}>
                            <li><a>docs</a></li>
                            <li>
                                <details>
                                    <summary>{theme}</summary>
                                             <ul className="menu dropdown-content bg-base-100 rounded-box z-1 w-52 p-2 shadow-sm">
                                                 <li><a onClick={(e) => setTheme("wireframe")}>wireframe</a></li>
                                                 <li><a onClick={(e) => setTheme("fantasy")}>fantasy</a></li>
                                                 <li><a onClick={(e) => setTheme("winter")}>winter</a></li>
                                                 <li><a onClick={(e) => setTheme("lofi")}>lofi</a></li>
                                                 <li><a onClick={(e) => setTheme("dark")}>dark</a></li>
                                                 <li><a onClick={(e) => setTheme("dracula")}>dracula</a></li>
                                                 <li><a onClick={(e) => setTheme("bumblebee")}>bumblebee</a></li>
                                                 <li><a onClick={(e) => setTheme("emerald")}>emerald</a></li>
                                                 <li><a onClick={(e) => setTheme("halloween")}>halloween</a></li>
                                                 <li><a onClick={(e) => setTheme("retro")}>retro</a></li>
                                                 <li><a onClick={(e) => setTheme("cyberpunk")}>cyberpunk</a></li>
                                                <li><a onClick={(e) => setTheme("abyss")}>abyss</a></li>

                                                 <li><a onClick={(e) => setTheme("sunset")}>sunset</a></li>
                                                 <li><a onClick={(e) => setTheme("dim")}>dim</a></li>
                                                 <li><a onClick={(e) => setTheme("business")}>business</a></li>
                                                 <li><a onClick={(e) => setTheme("luxury")}>luxury</a></li>
                                                 <li><a onClick={(e) => setTheme("black")}>black</a></li>
                                                 <li><a onClick={(e) => setTheme("pastel")}>pastel</a></li>
                                                 <li><a onClick={(e) => setTheme("aqua")}>aqua</a></li>
                                                 <li><a onClick={(e) => setTheme("forest")}>forest</a></li>
                                                 
                                            </ul>
                                </details>
                            </li>
                        </ul>
                    </div>
                </div>
                // <div css={style.headerParent}>
                //     <div css={style.logoParent}>
                //         <img
                //             style={{height: 80}}
                //             src="https://github.com/Ballerina-Org/ballerina/raw/main/docs/pics/Ballerina_logo-04.svg"
                //             alt="Ballerina"
                //         />
                //         <p>IDE</p>
                //         <HorizontalDropdown
                //             label={"Select spec"}
                //             onChange={async (name: string) => {
                //
                //                 const t = await Api.getSpec(name);
                //                 props.setState(
                //                     Ide.Updaters.Core.bridge(
                //                         Bridge.Operations.load(t)
                //                     ).then(Ide.Updaters.Core.specName(replaceWith(Value.Default(name))))
                //                 )
                //
                //             }}
                //             options={props.context.specNames}/>
                //     </div>
                //     <div css={{flex: 1}}/>
                //     <details className="dropdown">
                //         <summary className="btn m-1 mr-24">{theme}</summary>
                //         <ul className="menu dropdown-content bg-base-100 rounded-box z-1 w-52 p-2 shadow-sm">
                //             <li><a onClick={(e) => setTheme("wireframe")}>wireframe</a></li>
                //             <li><a onClick={(e) => setTheme("fantasy")}>fantasy</a></li>
                //             <li><a onClick={(e) => setTheme("winter")}>winter</a></li>
                //             <li><a onClick={(e) => setTheme("lofi")}>lofi</a></li>
                //             <li><a onClick={(e) => setTheme("dark")}>dark</a></li>
                //             <li><a onClick={(e) => setTheme("dracula")}>dracula</a></li>
                //             <li><a onClick={(e) => setTheme("bumblebee")}>bumblebee</a></li>
                //             <li><a onClick={(e) => setTheme("emerald")}>emerald</a></li>
                //             <li><a onClick={(e) => setTheme("halloween")}>halloween</a></li>
                //             <li><a onClick={(e) => setTheme("retro")}>retro</a></li>
                //             <li><a onClick={(e) => setTheme("cyberpunk")}>cyberpunk</a></li>
                //             <li><a onClick={(e) => setTheme("abyss")}>abyss</a></li>
                //         </ul>
                //     </details>
                //
                //
                // </div>
                }
            right={
                <>
                    <Messages
                        clientErrors={props.context.bridge.bridge.left.right.kind == "errors" ? props.context.bridge.bridge.left.right.errors.toArray() : []}
                        bridgeErrors={props.context.bridge.errors}
                    />

                   <>
                        <div className="card bg-base-100 w-full mt-5">
                            <div className="card-body w-full">
                              
                                    <div className="join">
                                        <label className="input">
                                            <span className="label">Live Update</span>
                                            <input
                                                type="checkbox"
                                                onChange={e => 
                                                    props.setState(Ide.Updaters.Core.liveUpdates(
                                                        replaceWith(
                                                            props.context.liveUpdates.kind == "l" ? Option.Default.some(3): Option.Default.none()
                                                        )
                                                    ))
                                                } 
                                                checked={props.context.liveUpdates.kind == "r"}
                                                className="toggle"
                                            />
                                            {props.context.liveUpdates.kind == "r" &&  <span className="countdown">
  <span style={{"--value":props.context.liveUpdates.value} /* as React.CSSProperties */ } aria-live="polite" aria-label={props.context.liveUpdates.value}>{props.context.liveUpdates.value}</span>
</span>}
                                        </label>
                                  
                                        

                                    </div>
                          

                                {props.context.bridge.bridge.left.right.kind == "value" && <><RadioButtons
                                    onChange={async (value: string) => {

                                        props.setState(
                                            Ide.Updaters.Core.launcherName(
                                                replaceWith(
                                                    Option.Default.some(
                                                        Value.Default(value)
                                                    )
                                                )
                                            )
                                        )
                                    }}
                                    options={props.context.launchers}


                                />
                                    <DispatcherFormsApp
                                        specName={props.context.specName.value}
                                        entityName={"People"}
                                        typeName={"Person"}
                                        spec={props.context.bridge.bridge.left.right.value}/></>
                                }

                            </div>
                        </div>

                   </>


                </>

            }/>
    )
};