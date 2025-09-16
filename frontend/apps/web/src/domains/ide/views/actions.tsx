/** @jsxImportSource @emotion/react */

import React from "react";
import {
    VscCheck,
    VscCheckAll,
    VscDatabase,
    VscLock,
    VscSave,
    VscNewFile,
    VscRedo,
    VscPlay,
    VscTriangleLeft, VscTriangleRight, VscFolderLibrary
} from "react-icons/vsc";


import {Ide} from "playground-core";



type ActionKey =
    | "new" | "folders" | "save" | "seed" | "reseed"
    | "run" | "validateBridge" | "validateV1"
    | "left" | "right" | "lock" | "download";

type ActionsProps = {
    context: Ide;
    hideRight?: boolean;
    onAction?: (action: ActionKey) => void;
    canRun?: boolean;
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
    onLeft?: () => void;
    onRight?: () => void;
    
};

export const Actions: React.FC<ActionsProps> = ({
            context,
            hideRight = false,
            onAction,
            canRun = true,
            onSeed, onNew, onLock, onReSeed, onRun, onDownload, onSave, onLeft, onRight

        }) => (
    <div className={"p-5 mt-7 flex space-x-1"}>
        {context.phase === "choose" && context.specOrigin === "existing" && (
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="New spec"
                onClick={onNew}
            >
                <VscNewFile size={20} />
            </button>
        )}

        { context.phase == "locked" &&
            <label
                htmlFor="my-drawer" className="btn tooltip tooltip-bottom" data-tip="Virtual Folders">
                <VscFolderLibrary className="mt-2" size={20}/>
            </label>}

        {context.phase === "locked" && (
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Save changes"
                onClick={onSave}
            >
                <VscSave size={20} />
            </button>
        )}

        {context.phase === "locked" && (
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Seed"
                onClick={onSeed}
            >
                <VscDatabase size={20} />
            </button>
        )}

        {context.phase === "locked" && context.step === "design" && (
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Run Forms Engine"
                onClick={onRun}
                disabled={!canRun}
            >
                <VscPlay size={20} />
            </button>
        )}

        {context.phase === "locked" && !hideRight && 
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Hide Forms"
                onClick={onRight}
            >
                <VscTriangleLeft size={20} />
            </button>}
        {context.phase === "locked" && !hideRight && (
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Show Forms"
                onClick={onLeft}
            >
                <VscTriangleRight size={20} />
            </button>
        )}
    </div>
);

