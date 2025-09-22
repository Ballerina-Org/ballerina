import {VscArrowSmallRight, VscDiffAdded, VscDiffRemoved, VscPrimitiveSquare} from "react-icons/vsc";
import React, { useState } from "react";

import TreeView, { flattenTree, INode} from "react-accessible-treeview";
import {VscArrowSmallDown, VscCheck} from "react-icons/vsc";
import {FlatNode, Meta, NodeId, VirtualFolders} from "playground-core";
import {BasicFun, Unit} from "ballerina-core";
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
    let tone: "badge-success" | "badge-warning" | "badge-error";

    switch (true) {
        case size <= 10 * 1024: tone = "badge-success"; break;
        case size < 102400:    tone = "badge-warning"; break;
        default:               tone = "badge-error";   break;
    }

    return (
        <div className={`ml-3 badge badge-sm ${tone} ml-auto`}>
            {VirtualFolders.Operations.formatBytes(size)}
        </div>
    );
};


export type MultiSelectCheckboxControlledProps = { 
    mode: 'reader' | 'uploader',
    nodes: FlatNode, 
    onSelectedFolder?: BasicFun<FlatNode, void>
    onAcceptedNodes?: BasicFun<FlatNode, void> }



export function MultiSelectCheckboxControlled(props:MultiSelectCheckboxControlledProps) {

    const data: INode<Meta>[] = 
        flattenTree(props.nodes);


    const [selectedIds, setSelectedIds] = useState(data.map(x => x.id));
    const [chosenIds, setChosenIds] = useState<Set<NodeId>>(Set(data.map(x => x.id)));
   
    return (
        <div>
            <div className="indicator">
                <span className="indicator-item badge badge-secondary">all items: {chosenIds.size}</span>
                <button className="btn">Selected folders: {5}</button>
            </div>
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
      
                                <div className={"w-full flex mt-3"}>
                                {isBranch && <ArrowIcon isOpen={isExpanded} />}
                                <CheckBoxIcon
                                    mode={props.mode}
                                    className="checkbox-icon mr-3"
                                    onClick={(e:any) => {
                                        handleSelect(e);
                                        e.stopPropagation();
                                    }}
                                    variant={
                                        isHalfSelected ? "some" : isSelected ? "all" : "none"
                                    }
                                />
                                <label className="label">
                                  {element.name}
                                </label>
                                {props.mode == 'reader' && element.metadata?.isLeaf
                                    &&  <button 
                                        className="ml-3 btn btn-neutral btn-dash"
                                        onClick={() => { 
                                            const selected = element as unknown as FlatNode
                                            if (props.onSelectedFolder) {
                                                let item =
                                                    ({ ...selected, children: selected.children?.map((child: any) => 
                                                        data.find(x => x.id === child)
                                                    )})
                                       
                                                props.onSelectedFolder(item as any);
                                            }
                                        }}
                                    >select</button>}
                                {element.metadata?.kind == "file" 
                                    &&
                                    <SizeBadge bytes={(element.metadata as Meta)?.size} />
                                        
                                 }
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
    return  isOpen ? <VscArrowSmallDown size={20} className={classes} /> 
        :<VscArrowSmallRight size={20} className={classes} />;
};

const CheckBoxIcon = ({ variant,mode, ...rest }: { variant: any; mode: 'reader' | 'uploader', [key: string]: any }) => {
    
    if(mode == 'reader') return <VscCheck size={20}/>;
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

export default MultiSelectCheckboxControlled;