import {BasicFun, Option} from "ballerina-core";
import React from "react";

import {VscAdd, VscCodeReview, VscEdit} from "react-icons/vsc";

type LaunchersDockProps = {
    launchers: string []
    selected: Option<string>
    onSelect: BasicFun<string, void>
}

interface LauncherIconProps {
    launcher: string
    size?: number;
}
export const LauncherIcon: React.FC<LauncherIconProps> = ({ launcher, size = 20 }) => {
    switch (launcher) {
        case "edit":
            return <VscEdit size={size} />;
        case "create":
            return <VscAdd size={size} />;
        default:
            return <VscCodeReview size={size} />;
    }
};
export const LaunchersDock = (props: LaunchersDockProps): React.ReactElement => {
    
    return <div className="dock bg-neutral text-neutral-content fixed bottom-0 left-0 w-full flex p-4 ">
        {props.launchers.map(launcher =>
            <button 
                className={props.selected.kind == "r" && launcher == props.selected.value ? "dock-active" :"" }
                onClick={() => props.onSelect(launcher)}
            >   <LauncherIcon launcher={launcher} />
               
                <span className="dock-label text-md">{launcher}</span>
            </button>
        )}

    </div>
}
