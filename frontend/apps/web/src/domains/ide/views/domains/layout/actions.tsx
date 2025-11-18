
import React, {useEffect} from "react";
import {
    VscCheck,
    VscDatabase,
    VscSave,
    VscNewFile,
    VscPlay,
    VscMerge,
    VscTriangleLeft, VscTriangleRight, VscFolderLibrary, VscSettings, VscTriangleUp, VscBracketError, VscHistory,
    VscInsert
} from "react-icons/vsc";


import {Ide, IdePhase, LockedPhase} from "playground-core";

const size = 22;

type ActionsProps = {
    context: Ide;
    setState: any;
    hideRight?: boolean;
    errorCount: number;
    canRun?: boolean;
    canValidate: boolean;
    onNew?: () => void;
    onLock?: () => void;
    onSeed?: () => void;
    onReSeed?: () => void;
    onRun: () => Promise<void>;
    onRunCondition?: boolean;
    onSave: () => Promise<void>;
    onMerge:  () => Promise<void>;
    onHide?: () => void;
    onHideUp: () => void;
    onSettings?: () => void;
    onErrorPanel?: () => void;
    onDeltaShow?: () => void;
    onCustomFields?: () => void;
    
};

export const Actions: React.FC<ActionsProps> = ({
    context,
    setState,
    errorCount = 0,
    hideRight = false,
    onHideUp,
    onCustomFields,
    onSettings,
    canValidate = false,
    canRun = true,
    onDeltaShow,
    onErrorPanel,
    onSeed, onNew, onRun, onMerge, onSave, onHide}) => {
    
    useEffect(() => {
        const onKeyDown = async (e: KeyboardEvent) => {
            const isMac = navigator.platform.toUpperCase().includes("MAC");
            const isCtrlOrCmd = isMac ? e.metaKey : e.ctrlKey;
            
            if (isCtrlOrCmd && /^Digit[1-9]$/.test(e.code)) {
                e.preventDefault();
                
                const numberPressed = Number(e.code.replace("Digit", ""));
                
                await onSave();
                await onMerge();
                await onRun();

                setState(
                    Ide.Updaters.Core.phase.locked(
                    LockedPhase.Updaters.Core.selectLauncherByNr(numberPressed))
                );
            }
        };

        window.addEventListener("keydown", onKeyDown);
        return () => window.removeEventListener("keydown", onKeyDown);
    }, [onSave, onMerge, onRun, setState]);

    const isWellKnownFile = context.phase.kind === "locked"
        && context.phase.locked.workspace.kind === "selected"
        && ((context.phase.locked.workspace.file.name.replace(".json","") == "go-config"
        || context.phase.locked.workspace.file.name.replace(".json","") == "seeds" 
            || context.phase.locked.workspace.file.name.replace(".json","") == "merged" ))
    return (
    <div className={"p-5 mt-10.5 flex space-x-1 w-full"}>
        { context.phase.kind == "locked" &&
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Create new"
                onClick={onNew}
            >
                <VscNewFile size={size} />
            </button>}
        { context.phase.kind == "locked" &&
            <label
                htmlFor="my-drawer" className="btn tooltip tooltip-bottom" data-tip="Workspace">
                <VscFolderLibrary className="mt-2" size={size}/>
            </label>}

        {context.phase.kind === "locked" && (
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Save"
                onClick={onSave}
            >
                <VscSave size={size} />
            </button>
        )}
        {
            context.phase.kind === "locked"  
            && !isWellKnownFile && (
            <div className="indicator">
                {context.phase.locked.validatedSpec.kind == "r"  && <span className="indicator-item indicator-top indicator-center badge badge-xs badge-secondary"><VscCheck size={10}/></span> }
                <button
                    className="btn tooltip tooltip-bottom"
                    data-tip="Validate"
                    disabled={!canValidate || isWellKnownFile}
                >
                    <VscMerge size={size} onClick={onMerge} />
                </button>
            </div>

        )}
        {context.phase.kind === "locked" && (
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Seed"
                onClick={onSeed}
                disabled={isWellKnownFile}
            >
                <VscDatabase size={size} />
            </button>
        )}

        {context.phase.kind === "locked"
            && (
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Run Forms"
                onClick={onRun}
                disabled={!canRun || isWellKnownFile}
            >
                <VscPlay size={size} />
            </button>
        )}
        <div className="ml-auto flex space-x-2">
        {context.phase.kind === "locked"
            && (
                <button
                    className="btn tooltip tooltip-bottom"
                    data-tip="Settings"
                    onClick={onSettings}
                >
                    <VscSettings size={size} />
                </button>
            )}
            {context.phase.kind === "locked"
                && context.phase.locked.step.kind === "display"
                && (
                    <button
                        className="btn tooltip tooltip-bottom"
                        data-tip="Show deltas"
                        onClick={onDeltaShow}
                    >
                        <VscHistory size={size} />
                    </button>
                )}
        {context.phase.kind === "locked" && hideRight && 
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Show Forms"
                onClick={onHide}
            >
                <VscTriangleLeft size={size} />
            </button>}
            { context.phase.kind == "locked" &&<div className="indicator">
                {/*<span className=*/}
                {/*          {errorCount > 0 ?*/}
                {/*              "indicator-item indicator-top indicator-center badge badge-xs badge-error"*/}
                {/*              :"indicator-item indicator-top indicator-center badge badge-xs badge-success"}>*/}
                {/*    {errorCount > 0 ? <VscCommentUnresolved size={size} /> : <VscCheck color="green" size={size/2} />}*/}
                {/*</span>*/}
                <button
                    disabled={errorCount == 0}
                    className="btn tooltip tooltip-bottom"
                    data-tip="Hide/Show Errors"
                    onClick={onErrorPanel}
                >
                    <VscBracketError size={size} />
                </button>
            </div>}
            { context.phase.kind == "locked" &&
                <button
                    disabled={errorCount == 0}
                    className="btn tooltip tooltip-bottom"
                    data-tip="Custom fields"
                    onClick={onCustomFields}
                >
                    <VscInsert size={size} />
                </button>}
            { context.phase.kind == "locked" &&<button
                className="btn tooltip tooltip-bottom"
                data-tip="Hide Top Menu"
                onClick={onHideUp}
            >
                <VscTriangleUp size={size} />
            </button>}
        {context.phase.kind === "locked" && !hideRight && (
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Hide Forms"
                onClick={onHide}
            >
                <VscTriangleRight size={size} />
            </button>
        )}
        </div>
    </div>
);
}

