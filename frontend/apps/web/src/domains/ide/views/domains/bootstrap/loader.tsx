import React, {Dispatch, SetStateAction} from "react";
import {Ide} from "playground-core";
import {Themes} from "../../theme-selector.tsx";

type LoaderProps = Ide;

export const Loader = (props: LoaderProps): React.ReactElement => {
    return props.phase == "bootstrap" && props.bootstrap.kind == "initializing" ? <div className="w-screen h-screen  flex items-center justify-center">
        <div className="relative w-120  mx-auto">
            <span className="loading loading-infinity loading-xl"></span>
            <div className="absolute inset-0 grid place-items-center">
                <span className="px-3 py-1 rounded-full bg-black/60 text-white text-sm">{props.bootstrap.message}</span>
            </div>
        </div>
    </div> : <></>
}
