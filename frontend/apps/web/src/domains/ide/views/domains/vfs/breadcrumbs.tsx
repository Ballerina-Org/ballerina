import {Node, WorkspaceState} from "playground-core";
import React from "react";
import {VscFile, VscFolder} from "react-icons/vsc";
import {Option} from "ballerina-core";


type BreadcrumbsProps = { workspace: WorkspaceState };

export function Breadcrumbs({ workspace }: BreadcrumbsProps) {

    return (
        workspace.kind == 'selected' && <div className="breadcrumbs text-sm">
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