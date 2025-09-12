/** @jsxImportSource @emotion/react */

import React from "react";
import {style} from "./actions.styled";
import {VscCheck, VscCheckAll, VscDatabase, VscLock, VscSave, VscNewFile, VscRedo, VscPlay} from "react-icons/vsc";
import {Ide} from "playground-core";

export const Actions: React.FC<{
    context:  Ide,
    onNew?: () => void;
    onLock?: () => void;
    onSeed?: () => void;
    onReSeed?: () => void;
    onRun?: () => void;
    onRunCondition?: boolean;
    onDownload?: () => void;
    onSave?: () => void;
    onValidateBridge?: () => void;
    onValidateV1?: () => void;

}> = ({context, onSeed, onNew, onLock, onReSeed, onRun, onDownload, onSave, onRunCondition, onValidateBridge, onValidateV1}) => (
    <div css={style.layout}>
        <div css={style.buttonSection}>
            <button className="btn tooltip tooltip-bottom" data-tip="Lock spec">
                <VscLock size={20} onClick={onLock}/>
            </button>
            { context.phase == "choose" && context.activeTab == "existing" &&  <button className="btn tooltip tooltip-bottom" data-tip="New spec">
                <VscNewFile size={20} onClick={onNew}/>
            </button>}
            { context.phase == "locked" && <button className="btn tooltip tooltip-bottom" data-tip="Save changes">
                <VscSave size={20} onClick={onSave}/>
            </button>}
            { context.phase == "locked" && <button className="btn tooltip tooltip-bottom" data-tip="Seed">
                <VscDatabase size={20} onClick={onSeed}/>
            </button>}
            { context.phase == "locked" && context.step == "design" && <button className="btn tooltip tooltip-bottom" data-tip="Run Forms Engine">
                <VscPlay size={20} onClick={onRun}/>
            </button>}
            {/*<button className="btn tooltip tooltip-bottom" data-tip="ReSeed">*/}
            {/*    <VscRedo size={20} onClick={onReSeed}/>*/}
            {/*</button>*/}
            {/*<button className="btn tooltip tooltip-bottom" data-tip="Validate v1">*/}
            {/*    <VscCheck size={20} onClick={onValidateV1}/>*/}
            {/*</button>*/}
            {/*<button className="btn tooltip tooltip-bottom" data-tip="Validate bridge">*/}
            {/*    <VscCheckAll size={20} onClick={onValidateBridge}/>*/}
            {/*</button>*/}
        </div>
    </div>
);
