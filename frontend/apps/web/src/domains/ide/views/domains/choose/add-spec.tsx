import React from "react";
import {Ide} from "playground-core";
import {BasicFun, Updater, Value} from "ballerina-core";
import {AddSpecButtons} from "./add-spec-buttons.tsx";
import {AddSpecUploadFolder} from "./add-spec-folders.tsx";
import {AddSpecUploadZipped} from "./add-spec-zipped.tsx";

type AddSpecProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const AddSpecInner = (props: AddSpecProps): React.ReactElement => {
    return <fieldset className="fieldset w-full">
        <AddSpecButtons {...props}/>
        {props.phase == 'choose' && props.progressIndicator == 'upload-in-progress' 
            && <progress className="progress progress-success w-56" value="100" max="100"></progress>}
        <AddSpecUploadFolder {...props} />
        <AddSpecUploadZipped {...props} />
        </fieldset> 
}

export const AddSpec = (props: AddSpecProps): React.ReactElement => {
    return props.phase == "choose" && props.specOrigin.origin == 'creating' && props.existing.specs.length == 0  ? 
        <AddSpecInner {...props} /> : <></>
}

