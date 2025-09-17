import {VirtualFolderNode, VirtualJsonFile} from "playground-core";
import React from "react";
import {VscFile, VscFolder} from "react-icons/vsc";


type BreadcrumbsProps = { file: VirtualFolderNode };

export function Breadcrumbs({ file }: BreadcrumbsProps) {
    if (file.kind === 'file') return <></>;

    return (
        <div className="breadcrumbs text-sm">
            <ul>
                {file.path.map((part, i) => (
                    <li key={`${part}-${i}`}>
                        <a><VscFolder size={15} />{part}</a>
                    </li>
                ))}
            </ul>
        </div>
    );
}