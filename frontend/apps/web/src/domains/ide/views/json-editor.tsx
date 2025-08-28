/** @jsxImportSource @emotion/react */
import {
    Ide,
    JsonEditorView,
    JsonEditorReadonlyContext
} from "playground-core";
import {HeadlessTemplateProps, replaceWith, Value} from "ballerina-core";
import { JsonEditor, githubLightTheme,githubDarkTheme,psychedelicTheme,monoLightTheme } from 'json-edit-react';
import {myTheme, style} from "./json-editor.styled.ts";
import React from "react";
import {CheckCircle, Edit, Delete, Check, SquareX, ArrowDown} from "lucide-react";
import {Bridge} from "playground-core";
import {
    VscCopilot, VscDiffRemoved, VscExpandAll, VscClose, VscDiffAdded, VscEdit, VscArrowSmallRight, VscCheck,
    VscCopy, VscDash
} from "react-icons/vsc";

export const V2Editor: JsonEditorView = (props) => (
    <>

        <div css={style.editor.container}>
            <div css={style.editor.left}>
                <JsonEditor
                    icons={{
                        delete: <VscDiffRemoved size={20} />,
                        edit:<VscEdit size={20}/>,
                        ok: <VscCheck size={20}/>,
                        cancel: <VscClose  size={20}/>,
                        add:<VscDiffAdded size={20}/>,
                        chevron:<VscDash size={20}/>,
                        copy:<VscCopy size={20}/>,
                    }}

                    collapse={1}
                    rootName="V2"
                    data={JSON.parse(props.context.bridge.right.left.specBody.value)}
                    theme={monoLightTheme}
                    onDelete={ x =>
                    {
                        const value = JSON.stringify(x.newData)
                        props.setState(
                            Bridge.Updaters.Template.setV2Body(
                                Value.Default(value)
                            )
                        );
                    }}

                    // onEditEvent={ x => {}},
                    onChange={x => {
                        // const value = JSON.stringify(x.newValue)
                        // debugger
                        // props.setState(
                        //     Bridge.Updaters.Template.setV2Body(
                        //         Value.Default(value)
                        //     )
                        // );
                        return x.newValue}}
                    onUpdate={x =>
                    {
                        const value = JSON.stringify(x.newData)
                        props.setState(
                            Bridge.Updaters.Template.setV2Body(
                                Value.Default(value)
                            )
                        );
                    }}
                />
            </div>
        </div>
    </>
);
export const V1Editor: JsonEditorView  = (props) => (
    <>

        <div css={style.editor.container}>
            {/*<p>{props.context.indicator.kind}</p>*/}
            <div css={style.editor.left}>
                <JsonEditor
                    icons={{
                        delete: <VscDiffRemoved size={20} />,
                        edit:<VscEdit size={20}/>,
                        ok: <VscCheck size={20}/>,
                        cancel: <VscClose  size={20}/>,
                        add:<VscDiffAdded size={20}/>,
                        chevron:<VscDash size={20}/>,
                        copy:<VscCopy size={20}/>,
                    }}

                    collapse={1}
                    rootName="V1"
                    data={JSON.parse(props.context.bridge.left.left.specBody.value)}
                    theme={monoLightTheme}
                    onDelete={ x =>
                    {
                        const value = JSON.stringify(x.newData)
                        props.setState(
                            Bridge.Updaters.Template.setV1Body(
                                Value.Default(value)
                            )
                        );
                    }}

                    // onEditEvent={ x => {}},
                    onChange={x => {
                        return x.newValue}}
                    onUpdate={x =>
                    {
               
                        const value = JSON.stringify(x.newData)
                        props.setState(
                            Bridge.Updaters.Template.setV1Body(
                                Value.Default(value)
                            )
                        );
                        
                    }}
                />
            </div>
        </div>
    </>
);
export const SeedEditor: JsonEditorView  = (props) => (
    <>

        <div css={style.editor.container}>
            {/*<p>{props.context.indicator.kind}</p>*/}
            <div css={style.editor.left}>
                <JsonEditor
                    icons={{
                        delete: <VscDiffRemoved size={20} />,
                        edit:<VscEdit size={20}/>,
                        ok: <VscCheck size={20}/>,
                        cancel: <VscClose  size={20}/>,
                        add:<VscDiffAdded size={20}/>,
                        chevron:<VscDash size={20}/>,
                        copy:<VscCopy size={20}/>,
                    }}

                    collapse={1}
                    rootName="Seeds"
                    data={props.context.seeds}
                    theme={monoLightTheme}
                    onDelete={ x =>
                    {
                        const value = JSON.stringify(x.newData)
                        props.setState(
                            Bridge.Updaters.Template.setV2Body(
                                Value.Default(value)
                            )
                        );
                    }}

                    // onEditEvent={ x => {}},
                    onChange={x => {
                        return x.newValue}}
                    onUpdate={x =>
                    {   const value = x.newValue as any
                        const id = value.id
                        const entityName = "SourceTable";
                        //SpecEditor.Operations.updateEntity(props.context.name.value,entityName,id,value.value)
                    }}
                />
            </div>
        </div>
    </>
);

