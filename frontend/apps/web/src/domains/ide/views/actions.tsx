/** @jsxImportSource @emotion/react */

import React from "react";
import {style} from "./actions.styled";
import {VscCheck, VscCheckAll, VscDatabase, VscLock, VscSave, VscNewFile, VscRedo} from "react-icons/vsc";

export const Actions: React.FC<{
    onSeed?: () => void;
    onReSeed?: () => void;
    onRun?: () => void;
    onRunCondition?: boolean;
    onDownload?: () => void;
    onSave?: () => void;
    onValidateBridge?: () => void;
    onValidateV1?: () => void;

}> = ({onSeed, onReSeed, onRun, onDownload, onSave, onRunCondition, onValidateBridge, onValidateV1}) => (
    <div css={style.layout}>
        <div css={style.buttonSection}>
            <button className="btn tooltip tooltip-bottom" data-tip="Lock spec">
                <VscLock size={20} onClick={()=>{}}/>
            </button>
            <button className="btn tooltip tooltip-bottom" data-tip="New spec">
                <VscNewFile size={20} />
            </button>
            <button className="btn tooltip tooltip-bottom" data-tip="Save changes">
                <VscSave size={20} onClick={onSave}/>
            </button>
            <button className="btn tooltip tooltip-bottom" data-tip="Seed">
                <VscDatabase size={20} onClick={onSeed}/>
            </button>
            <button className="btn tooltip tooltip-bottom" data-tip="ReSeed">
                <VscRedo size={20} onClick={onReSeed}/>
            </button>
            <button className="btn tooltip tooltip-bottom" data-tip="Validate v1">
                <VscCheck size={20} onClick={onValidateV1}/>
            </button>
            <button className="btn tooltip tooltip-bottom" data-tip="Validate bridge">
                <VscCheckAll size={20} onClick={onValidateBridge}/>
            </button>
        </div>
    </div>
);
