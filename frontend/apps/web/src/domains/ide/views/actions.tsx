
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
    VscTriangleLeft, VscTriangleRight, VscFolderLibrary
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
    onNew?: () => void;
    onLock?: () => void;
    onSeed?: () => void;
    onReSeed?: () => void;
    onRun?: () => void;
    onRunCondition?: boolean;
    onSave?: () => void;
    onMerge?: () => void;
    onHide?: () => void;
    
};

export const Actions: React.FC<ActionsProps> = ({
            context,
            hideRight = false,
            onAction,
            canRun = true,
    
            onSeed, onNew, onLock, onReSeed, onRun, onMerge, onSave, onHide

        }) => (
    <div className={"p-5 mt-3.5 flex space-x-1 w-full"}>
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
                htmlFor="my-drawer" className="btn tooltip tooltip-bottom" data-tip="Virtual Folders">
                <VscFolderLibrary className="mt-2" size={size}/>
            </label>}

        {context.phase === "locked" && (
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Save changes"
                onClick={onSave}
            >
                <VscSave size={size} />
            </button>
        )}
        {context.phase === "locked"  && (
            <div className="indicator">
                {context.locked.virtualFolders.merged.kind == "r"  && <span className="indicator-item indicator-top indicator-center badge badge-xs badge-secondary"><VscCheck size={10}/></span> }
                <button
                    className="btn tooltip tooltip-bottom"
                    data-tip="Merge and validate"
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
                data-tip="Run Forms Engine"
                onClick={onRun}
                disabled={!canRun}
            >
                <VscPlay size={size} />
            </button>
        )}

        {context.phase === "locked" && hideRight && 
            <button
                className="btn tooltip tooltip-bottom"
                data-tip="Hide Forms"
                onClick={onHide}
            >
                <VscTriangleLeft size={size} />
            </button>}
        {context.phase === "locked" && !hideRight && (
            <button
                className="btn tooltip tooltip-bottom ml-auto"
                data-tip="Show Forms"
                onClick={onHide}
            >
                <VscTriangleRight size={size} />
            </button>
        )}
    </div>
);

