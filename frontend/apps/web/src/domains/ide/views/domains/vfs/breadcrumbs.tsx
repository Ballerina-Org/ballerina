import {FlatNode} from "playground-core";
import React from "react";
import {VscFile, VscFolder} from "react-icons/vsc";
import {Option} from "ballerina-core";


type BreadcrumbsProps = { selected: Option<FlatNode> };

export function Breadcrumbs({ selected }: BreadcrumbsProps) {

    return (
        selected.kind == "r" && <div className="breadcrumbs text-sm">
            <ul>
                {selected.value.metadata.path.split('/').map((part, i) => (
                    <li key={`${part}-${i}`}>
                        <a><VscFolder size={15} />{part}</a>
                    </li>
                ))}
            </ul>
        </div>
    );
}