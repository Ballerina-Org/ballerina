import React from "react";

import {Ide} from "playground-core";
import {AddSpecInner} from "./add-spec.tsx";
import {BasicFun, Updater} from "ballerina-core";
import {SelectSpec} from "./select.tsx";
import {MissingSpecsInfoAlert} from "../bootstrap/no-specs-info.tsx";

type AddOrSelectSpecProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const AddOrSelectSpec = (props: AddOrSelectSpecProps): React.ReactElement => {
    return props.phase.kind == "selection" && props.phase.selection.specs.length > 0 ?
        <div className="flex w-full p-5">
            <MissingSpecsInfoAlert specs={props.phase.selection.specs.length} />
            <div className="card no-radius bg-base-300 rounded-box grid h-20 grow place-items-center">
                <SelectSpec {...props} />
            </div>
            <div className="divider divider-horizontal">OR</div>
            <div className="card bg-base-300 rounded-box grid h-20 grow place-items-center">
                <AddSpecInner setState={props.setState} {...props.phase.selection } />
            </div>
        </div>
        : <></>
}
