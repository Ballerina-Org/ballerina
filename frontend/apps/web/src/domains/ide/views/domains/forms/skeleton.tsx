import {getSpec, Ide, IdePhase} from "playground-core";
import {BasicFun, Updater} from "ballerina-core";
import React from "react";

type FormSkeletonProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const FormSkeleton = (props: FormSkeletonProps): React.ReactElement => {
    return  props.phase.kind == 'locked' && props.phase.locked.step.kind == 'design'
        ? <div className="flex w-full  h-full flex-col gap-4 p-7  shadow-sm backdrop-blur-md ">
        <div className="skeleton h-32 w-full animate-none"></div>
        <div className="skeleton h-4 w-28"></div>
        <div className="skeleton h-4 w-full animate-none"></div>
        <div className="skeleton h-4 w-full animate-none"></div>
    </div>
    :<></>
}
