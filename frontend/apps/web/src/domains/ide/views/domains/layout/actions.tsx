
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
    VscChecklist,
    VscMerge,
    VscTriangleLeft, VscTriangleRight, VscFolderLibrary, VscSettings
} from "react-icons/vsc";


import {Ide} from "playground-core";

type ActionKey =
    | "new" | "folders" | "save" | "seed" | "reseed"
    | "run" | "merge" 
    | "left" | "right" | "lock" 

const size = 22;

type ActionsProps = {
    context: Ide;
    hideRight?: boolean;
    onAction?: (action: ActionKey) => void;
    canRun?: boolean;
    canValidate: boolean;
    onNew?: () => void;
    onLock?: () => void;
    onSeed?: () => void;
    onReSeed?: () => void;
    onRun?: () => void;
    onRunCondition?: boolean;
    onSave?: () => void;
    onMerge?: () => void;
    onHide?: () => void;
    onSettings?: () => void;
    
};

export const Actions: React.FC<ActionsProps> = ({
            context,
            hideRight = false,
            onAction,
            onSettings,
            canValidate = false,
            canRun = true,
    
            onSeed, onNew, onLock, onReSeed, onRun, onMerge, onSave, onHide

        }) => {
    const isWellKnownFile = context.phase === "locked"
        && context.locked.workspace.kind === "selected"
        && (context.locked.workspace.current.kind == 'file' 
            && (context.locked.workspace.current.file.name.replace(".json","") == "go-config"
        || context.locked.workspace.current.file.name.replace(".json","") == "seeds" 
            || context.locked.workspace.current.file.name.replace(".json","") == "merged" ))
    return (
    <div className={"p-5 mt-10.5 flex space-x-1 w-full"}>
        {/*{context.phase === "choose" && context.specOrigin === "existing" && (*/}
        {/*    <button*/}
        {/*        className="btn tooltip tooltip-bottom"*/}
        {/*        data-tip="New spec"*/}
        {/*        onClick={onNew}*/}
        {/*    >*/}
        {/*        <VscNewFile size={size} />*/}
        {/*    </button>*/}
        {/*)}*/}

        { context.phase == "locked" &&
            <label
                htmlFor="my-drawer" className="btn tooltip tooltip-bottom" data-tip="Workspace">
                <VscFolderLibrary className="mt-2" size={size}/>
            </label>}

        {context.phase === "locked" && (
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Save"
                onClick={onSave}
            >
                <VscSave size={size} />
            </button>
        )}
        {
            context.phase === "locked"  
            && !isWellKnownFile && (
            <div className="indicator">
                {context.locked.validatedSpec.kind == "r"  && <span className="indicator-item indicator-top indicator-center badge badge-xs badge-secondary"><VscCheck size={10}/></span> }
                <button
                    className="btn tooltip tooltip-bottom"
                    data-tip="Validate"
                    disabled={!canValidate || isWellKnownFile}
                >
                    <VscMerge size={size} onClick={onMerge} />
                </button>
            </div>

        )}
        {context.phase === "locked" && (
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Seed"
                onClick={onSeed}
                disabled={isWellKnownFile}
            >
                <VscDatabase size={size} />
            </button>
        )}

        {context.phase === "locked" 
            //&& context.step === "design"
            //&& context.locked.seeds.kind == "r" 
            //&& context.locked.virtualFolders.merged.kind == "r" 
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
        {context.phase === "locked"
            //&& context.step === "design"
            //&& context.locked.seeds.kind == "r" 
            //&& context.locked.virtualFolders.merged.kind == "r" 
            && (
                <button
                    className="btn tooltip tooltip-bottom"
                    data-tip="Settings"
                    onClick={onSettings}
                >
                    <VscSettings size={size} />
                </button>
            )}
        {context.phase === "locked" && hideRight && 
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Show Forms"
                onClick={onHide}
            >
                <VscTriangleLeft size={size} />
            </button>}
        {context.phase === "locked" && !hideRight && (
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

