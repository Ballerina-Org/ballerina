import {
    VscAdd,
    VscArrowSmallRight,
    VscDiffAdded,
    VscDiffRemoved,
    VscFile,
    VscFolder, VscNewFile,
    VscPrimitiveSquare
} from "react-icons/vsc";
import React, { useState } from "react";
import TreeView, { flattenTree, INode} from "react-accessible-treeview";
import {VscArrowSmallDown} from "react-icons/vsc";
import {Node, Meta, NodeId, WorkspaceState, VirtualFolders} from "playground-core";
import {BasicFun} from "ballerina-core";
import {Set} from "immutable";

function cx(...args: Array<string | Record<string, boolean> | undefined | null | false>): string {
    return args
        .flatMap(arg => {
            if (!arg) return [];
            if (typeof arg === "string") return [arg];
            if (typeof arg === "object") {
                return Object.entries(arg)
                    .filter(([_, value]) => Boolean(value))
                    .map(([key]) => key);
            }
            return [];
        })
        .join(" ");
}

const SizeBadge: React.FC<{ bytes?: number }> = ({ bytes = 0 }) => {
    const size = Number(bytes);
    let tone: "badge-secondary" | "badge-warning" | "badge-error";

    switch (true) {
        case size <= 10 * 1024: tone = "badge-secondary"; break;
        case size < 102400: tone = "badge-warning"; break;
        default: tone = "badge-error";   break;
    }

    return (
        <div className={`ml-5 mr-5 badge badge-sm ${tone} ml-auto`}>
            {VirtualFolders.Operations.formatBytes(size)}
        </div>
    );
};

export type MultiSelectCheckboxControlledProps = {
    mode: 'reader' | 'uploader',
    workspace: WorkspaceState,
    onSelectedFolder?: BasicFun<Node, void>,
    onSelectedFile?: BasicFun<Node, void>
    onAcceptedNodes?: BasicFun<Node, void>
    onAddNewFile?: BasicFun<string, void>
}


export function MultiSelectCheckboxControlled(props:MultiSelectCheckboxControlledProps) {

    const data: INode<Meta>[] =
        flattenTree(props.workspace.nodes);

    const [selectedIds, setSelectedIds] = useState(data.map(x => x.id));
    const [chosenIds, setChosenIds] = useState<Set<NodeId>>(Set(data.map(x => x.id)));
    const folders = data.filter(x => chosenIds.has(x.id)).filter(x => x.metadata?.kind == "dir")?.length - 1 || 0;
    return (
        <div>
            <div className="">
                <TreeView
                    data={data}
                    aria-label="Checkbox tree"
                    multiSelect
                    selectedIds={selectedIds}
                    defaultExpandedIds={[1]}
                    propagateSelect
                    propagateSelectUpwards
                    togglableSelect
                    onSelect={(props) => {

                        setChosenIds(Set(props.treeState.selectedIds))
                    }
                    }
                    onNodeSelect={(props) => console.log('onNodeSelect callback: ', props)}
                    nodeRenderer={({
                                       element,
                                       isBranch,
                                       isExpanded,
                                       isSelected,
                                       isHalfSelected,
                                       isDisabled,
                                       getNodeProps,
                                       level,
                                       handleSelect,
                                       handleExpand,
                                   }) => {

                        return (
                            <div
                                {...getNodeProps({ onClick: handleExpand })}
                                style={{
                                    marginLeft: 40 * (level - 1),
                                    opacity: isDisabled ? 0.5 : 1,
                                }}
                            >

                                <div className="w-full group inline-flex items-center h-6">
                                    <div
                                        className={
                                            element.metadata?.kind == "dir"
                                                ? "w-full flex items-center justify-between mt-3"
                                                : "contents mt-3"
                                        }
                                    >
                                        {isBranch && (
                                            <div className="flex items-center gap-2 text-base-content font-medium">
                                                <VscFolder size={20} />
                                                <span>{element.name}</span>
                                            </div>
                                        )}

                                        {!isBranch && (
                                            <div
                                                className="flex items-center gap-2 text-base-content text-sm cursor-pointer"
                                                onClick={() => {
                                                    const selected = element as unknown as Node;
                                                    props.onSelectedFile?.(selected);
                                                }}
                                            >
                                                <VscFile size={20} />
                                                <span>{element.name}</span>
                                            </div>
                                        )}

                                        {element.metadata?.kind === "dir" && (
                                            <button
                                                className="ml-2 opacity-0 group-hover:opacity-100 transition-opacity duration-200 text-base-content/70 hover:text-primary"
                                                title="Add new file"
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                   // element.metadata?.path && props.onAddNewFile(element.metadata?.path as string)
                                                }}
                                            >
                                                <VscNewFile size={18} />
                                            </button>
                                        )}

                                        {element.metadata?.kind === "file" && (
                                            <div className="ml-2 hidden group-hover:inline text-sm text-base-content/80">
                                                <SizeBadge bytes={(element.metadata as Meta)?.size} />
                                            </div>
                                        )}
                                    </div>
                                </div>
                            </div>
                        );
                    }}
                />
            </div>
        </div>
    );
}

const ArrowIcon = ({ isOpen, className }: { isOpen: any; [key: string]: any }) => {
    const baseClass = "arrow";
    const classes = cx(
        baseClass,
        { [`${baseClass}--closed`]: !isOpen },
        { [`${baseClass}--open`]: isOpen },
        className
    );
    return isOpen ? <VscArrowSmallDown size={20} className={classes} />
        :<VscArrowSmallRight size={20} className={classes} />;
};

const CheckBoxIcon = ({ variant,mode, ...rest }: { variant: any; mode: 'reader' | 'uploader', [key: string]: any }) => {
    switch (variant) {
        case "all":
            return <VscDiffAdded size={20} {...rest} />;
        case "none":
            return <VscPrimitiveSquare size={20} {...rest} />;
        case "some":
            return <VscDiffRemoved size={20} {...rest} />;
        default:
            return null;
    }
};
