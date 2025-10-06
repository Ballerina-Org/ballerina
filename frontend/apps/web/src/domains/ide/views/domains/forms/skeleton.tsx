import {getSpec, Ide} from "playground-core";
import {BasicFun, Updater} from "ballerina-core";
import React from "react";

type FormSkeletonProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const FormSkeleton = (props: FormSkeletonProps): React.ReactElement => {
    return  props.phase == 'locked' && props.locked.progress.kind == 'preDisplay'
        ? <div className="flex w-full  h-full flex-col gap-4 p-7  shadow-sm backdrop-blur-md ">
        <div className="skeleton h-32 w-full animate-none"></div>
        <div className="skeleton h-4 w-28"></div>
        <div className="skeleton h-4 w-full animate-none"></div>
        <div className="skeleton h-4 w-full animate-none"></div>
        <div className="navbar bg-base-100 shadow-sm backdrop-blur-md">
            <div className="flex-1 px-4 gap-4 items-center">
                <div className="skeleton bg-base-300/20 opacity-40 blur h-6 w-48 rounded" />
                <div className="skeleton bg-base-300/20 opacity-40 blur h-4 w-32 rounded" />
            </div>
            <div className="flex-none gap-3 pr-4 items-center">
                <div className="skeleton bg-base-300/20 opacity-40 blur h-10 w-64 rounded" />
                <div className="skeleton bg-base-300/20 opacity-40 blur h-10 w-10 rounded-full" />
            </div>
        </div>
    </div>
    :<></>
}
