import React from "react";
import {Ide} from "playground-core";
import {BasicFun, Updater, Value} from "ballerina-core";
import {AddSpecButtons} from "./add-spec-buttons.tsx";
import {AddSpecUploadFolder} from "./add-spec-folders.tsx";
import {AddSpecUploadZipped} from "./add-spec-zipped.tsx";
import {CommonUI} from "playground-core/ide/domains/common-ui/state.ts";


type AddSpecProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const AddSpecInner = (props: AddSpecProps): React.ReactElement => {
    return <fieldset className="fieldset w-full">
        <AddSpecButtons
            name={props.name.value}
            variant={props.variant}
            onNameChange={(name) => props.setState(CommonUI.Updater.Core.specName(Value.Default(name)))}
            onErrors={(errors) => props.setState(CommonUI.Updater.Core.chooseErrors(errors))}
            startUpload={() => props.setState(Ide.Updaters.Phases.choosing.startUpload())}
            setState={props.setState}
        />
        {props.phase == 'selectionOrCreation' && props.variant.kind != 'scratch' && props.variant.upload == 'upload-started' 
            && <progress className="progress progress-success w-56" value="100" max="100"></progress>}
        <AddSpecUploadFolder {...props} />
        <AddSpecUploadZipped {...props} />
        </fieldset> 
}

export const AddSpec = (props: AddSpecProps): React.ReactElement => {
    return props.phase == "selectionOrCreation" && props.origin == 'creating' && props.specSelection.specs.length == 0  ? 
        <AddSpecInner {...props} /> : <></>
}

