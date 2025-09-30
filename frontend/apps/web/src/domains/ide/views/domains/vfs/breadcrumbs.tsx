import {FlatNode, ProgressiveWorkspace} from "playground-core";
import React from "react";
import {VscFile, VscFolder} from "react-icons/vsc";
import {Option} from "ballerina-core";


type BreadcrumbsProps = { workspace: ProgressiveWorkspace };

export function Breadcrumbs({ workspace }: BreadcrumbsProps) {

    return (
        workspace.kind == 'unstale' && <div className="breadcrumbs text-sm">
            <ul>
                {workspace.current.folder.metadata.path.split('/').map((part, i) => (
                    <li key={`${part}-${i}`}>
                        <a><VscFolder size={15} />{part}</a>
                    </li>
                ))}
            </ul>
        </div>
    );
}