import React from "react";
import {BootstrapPhase} from "playground-core";

type LoaderProps = BootstrapPhase;

export const Loader = (props: LoaderProps) : React.ReactElement => {
    return props.kind == "initializing" ? <div className="w-screen h-screen flex items-center justify-center">
        <div className="relative w-120  mx-auto">
            <span className="loading loading-infinity loading-xl"></span>
            <div className="absolute inset-0 grid place-items-center">
                <span className="px-3 py-1 rounded-full bg-black/60 text-white text-sm">{props.message}</span>
            </div>
        </div>
    </div> : <></>
}
