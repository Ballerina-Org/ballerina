import {WorkspaceState} from "playground-core";
import React from "react";
import {VscFolder} from "react-icons/vsc";

type BreadcrumbsProps = { workspace: WorkspaceState };

export function Breadcrumbs({ workspace }: BreadcrumbsProps) {

    return (
        workspace.kind == 'selected' && workspace.file.metadata.path.split("/").length > 2 && <div className="breadcrumbs text-sm">
            <ul>
                {workspace.file.metadata.path.split('/').slice(0, -1).map((part, i) => (
                    <li key={`${part}-${i}`}>
                        <a><VscFolder size={15} />{part}</a>
                    </li>
                ))}
            </ul>
        </div>
    );
}