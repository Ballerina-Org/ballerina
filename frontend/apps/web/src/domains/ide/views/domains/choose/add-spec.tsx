import React from "react";
import {Ide, IdePhase} from "playground-core";
import {BasicFun, replaceWith, Updater, Value} from "ballerina-core";
import {AddSpecButtons} from "./add-spec-buttons.tsx";
import {AddSpecUploadFolder} from "./add-spec-folders.tsx";
import {AddSpecUploadZipped} from "./add-spec-zipped.tsx";
import {SelectionPhase} from "playground-core/ide/domains/phases/selection/state.ts";
import {MissingSpecsInfoAlert} from "../bootstrap/no-specs-info.tsx";


type AddSpecProps = SelectionPhase & { setState: BasicFun<Updater<Ide>, void> };

export const AddSpecInner = (props: AddSpecProps): React.ReactElement => {

    return <fieldset className="fieldset w-full">
        <AddSpecButtons
            name={props.name.value}
            variant={props.variant}
            onNameChange={(name) => 
                props.setState(
                    Ide.Updaters.Core.phase.selection(SelectionPhase.Updaters.Core.name(replaceWith(Value.Default(name)))))}
            onErrors={(errors) => 
                props.setState(Ide.Updaters.Core.phase.selection(SelectionPhase.Updaters.Core.errors(replaceWith(errors))))}
            startUpload={() => 
                props.setState(Ide.Updaters.Core.phase.selection( s => ({...s, kind: 'upload-started' })))}
            setState={props.setState}
        />
        <AddSpecUploadFolder {...props} />
        <AddSpecUploadZipped {...props} />
        </fieldset> 
}

export const AddSpec = (props: AddSpecProps): React.ReactElement => {
    return props.specs.length == 0  ? 
        <>
            <MissingSpecsInfoAlert specs={props.specs.length} />
            <AddSpecInner {...props} /></> : <></>
}

