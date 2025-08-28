/** @jsxImportSource @emotion/react */
import React from "react";
import {CheckCircle, Play, SaveIcon, Lock, Download, Plus, RefreshCw, Sprout, Copy, GitCompare, ShieldCheck, Check} from "lucide-react";
import {style} from "./actions.styled";
import { VscDebugAll, VscCheck, VscCheckAll, VscDatabase, VscLock, VscSave, VscNewFile } from "react-icons/vsc";
export const Actions: React.FC<{
    onReload?: () => void;
    onSeed?: () => void;
    onReSeed?: () => void;
    onRun?: () => void;
    onRunCondition?: boolean;
    onDownload?: () => void;
    onSave?: () => void;

    onValidateBridge?: () => void;
    onValidateV1?: () => void;

}> = ({onSeed, onReSeed, onReload,onRun,onDownload,onSave, onRunCondition, onValidateBridge,onValidateV1}) => (
    <div css={style.layout}>
        <div css={style.buttonSection}>
            {/*<button css={style.iconButton} onClick={onReload}>*/}
            {/*  <RefreshCw size={14}  />*/}
            {/*</button>*/}

            <button className="btn tooltip tooltip-bottom" data-tip="Lock spec">
                <VscLock size={20} onClick={onReload}/>
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
<button className="btn tooltip tooltip-bottom" data-tip="Validate v1">
                <VscCheck size={20} onClick={onValidateV1}/></button>
<button className="btn tooltip tooltip-bottom" data-tip="Validate bridge">
                <VscCheckAll size={20} onClick={onValidateBridge}/>
            </button>
            {/*<button css={style.iconButton} onClick={onReSeed}>*/}
            {/*    <Sprout size={14}  />*/}
            {/*</button>*/}
            {/*{onRun && onRunCondition && <button css={style.iconButton} onClick={onRun}>*/}
            {/*      <Play size={14} />*/}
            {/*  </button>}*/}

        </div>
    </div>
);
