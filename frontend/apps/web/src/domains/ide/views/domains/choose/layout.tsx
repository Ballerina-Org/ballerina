import React from "react";

import {getOrInitSpec, getSpec, Ide, VirtualFolders} from "playground-core";
import {AddSpec, AddSpecInner} from "./add-spec.tsx";
import {BasicFun, Updater} from "ballerina-core";
import {SelectSpec} from "./select.tsx";

type AddOrSelectSpecProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const AddOrSelectSpec = (props: AddOrSelectSpecProps): React.ReactElement => {
    return props.phase == "choose" && props.specSelection.specs.length > 0 ?
        <div className="flex w-full p-5">
            <div className="card no-radius bg-base-300 rounded-box grid h-20 grow place-items-center">
                <SelectSpec {...props} />
            </div>
            <div className="divider divider-horizontal">OR</div>
            <div className="card bg-base-300 rounded-box grid h-20 grow place-items-center">
                <AddSpecInner {...props} />
            </div>
        </div>
        : <></>
}
