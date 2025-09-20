import React from "react";
import {HorizontalDropdown} from "../../dropdown.tsx";
import {getOrInitSpec, Ide, VirtualFolders} from "playground-core";
import {AddSpec, AddSpecInner} from "./add-spec.tsx";
import {BasicFun, Updater} from "ballerina-core";

type AddOrSelectSpecProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const AddOrSelectSpec = (props: AddOrSelectSpecProps): React.ReactElement => {
    return props.phase == "choose" && props.specOrigin == 'existing'  ?
        <div className="flex w-full p-5">
            <div className="card no-radius bg-base-300 rounded-box grid h-20 grow place-items-center">
                <HorizontalDropdown
                    label={"Select spec"}
                    onChange={async (name: string) => {
                        const vfs = await getOrInitSpec('existing',name);
                     
                        if (vfs.kind == "errors") {
                            props.setState(Ide.Updaters.CommonUI.chooseErrors(vfs.errors))
                            return;
                        }
                        const u =
                            Ide.Updaters.Template.lockedPhase('existing','manual', name, VirtualFolders.Operations.buildWorkspaceFromRoot('existing', vfs.value))

                        props.setState(u)

                    }}
                    options={props.existing.specs}/>
            </div>
            <div className="divider divider-horizontal">OR</div>
            <div className="card bg-base-300 rounded-box grid h-20 grow place-items-center">
                <AddSpecInner {...props} />
            </div>
        </div>
        : <></>
}
