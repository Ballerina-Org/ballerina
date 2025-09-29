import {FlatNode, FormsMode, Ide, VfsWorkspace} from "playground-core";
import React from "react";
import {VscFile, VscFolder} from "react-icons/vsc";
import {Option, replaceWith, SimpleCallback, Unit, Updater} from "ballerina-core";
import {LockedSpec} from "playground-core/ide/domains/locked/state.ts";

type MissingFilterProps = {
    folder: FlatNode;
    update?: any;
    formsMode: FormsMode;
};

export const MissingFiles = ({
     folder,
     update,
        formsMode
 }: MissingFilterProps) => {
    
    if (folder.metadata.kind !== "dir") return null;
    
    const files = (folder.children || [])?.filter(x => x.metadata.kind === "file");
    const missing = files.filter(f => f.name == "typeV2.json" || f.name == "schema.json");
    return (
 
            missing.length > 0 && formsMode.kind == 'select' && <div className="card w-full bg-base-100 card-xs shadow-sm">
                <div className="card-body">
                    <h2 className="card-title">Add missing siles</h2>
                    <p></p>
                    <div className="justify-end card-actions">
                        <button className="btn btn-primary">Buy Now</button>
                    </div>
                </div>
            </div>);
};
