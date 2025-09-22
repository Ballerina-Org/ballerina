import React, {Dispatch, SetStateAction} from "react";
import {Ide} from "playground-core";
import {Themes} from "../../theme-selector.tsx";

type NavbarProps = Ide & { theme: string, setTheme: Dispatch<SetStateAction<string>> };

export const Navbar = (props: NavbarProps): React.ReactElement => {
    return props.phase !== "bootstrap" ?
        <div className="navbar bg-base-100 shadow-sm sticky top-0 z-50 h-16">
            <img
                style={{height: 80}}
                src="https://github.com/Ballerina-Org/ballerina/raw/main/docs/pics/Ballerina_logo-04.svg"
                alt="Ballerina"
            />
            <div className="flex-1">
                <a className="btn btn-ghost text-xl">IDE</a>
            </div>

            <div className="flex-none mr-32">
                <ul className="menu menu-horizontal px-1" style={{zIndex: 10000, position: "relative"}}>
                    <li>
                        {Themes.dropdown(props.theme, props.setTheme)}
                    </li>
                </ul>
            </div>
            <div className="avatar avatar-online avatar-placeholder">
                <div className="bg-neutral text-neutral-content w-12 rounded-full">
                    <span className="text-xl">PS</span>
                </div>
            </div></div> : <></>
}