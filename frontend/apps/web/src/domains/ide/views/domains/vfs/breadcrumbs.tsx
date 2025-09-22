import {FlatNode} from "playground-core";
import React from "react";
import {VscFile, VscFolder} from "react-icons/vsc";


type BreadcrumbsProps = { selected: FlatNode };

export function Breadcrumbs({ selected }: BreadcrumbsProps) {
    if (selected.metadata?.kind === 'file') return <></>;

    return (
        <div className="breadcrumbs text-sm">
            <ul>
                {selected.metadata.path.split('/').map((part, i) => (
                    <li key={`${part}-${i}`}>
                        <a><VscFolder size={15} />{part}</a>
                    </li>
                ))}
            </ul>
        </div>
    );
}