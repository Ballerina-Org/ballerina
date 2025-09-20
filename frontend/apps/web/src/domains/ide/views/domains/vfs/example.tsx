import { VscArrowSmallRight } from "react-icons/vsc";
import React, { useState } from "react";

import TreeView, { flattenTree, INode} from "react-accessible-treeview";
import {VscArrowSmallDown} from "react-icons/vsc";
import {FlatNode, VirtualFolders} from "playground-core";
import {BasicFun, Unit} from "ballerina-core";


export type MultiSelectCheckboxControlledProps = { nodes: FlatNode, onAccepted: BasicFun<FlatNode, void> }
export function MultiSelectCheckboxControlled(props:MultiSelectCheckboxControlledProps) {
    
    const data = flattenTree(props.nodes as any)
    const [selectedIds, setSelectedIds] = useState(data.map(x => x.id));
    // const onKeyDown = (e) => {
    //     if (e.key === "Enter") {
    //         getAndSetIds();
    //     }
    // };
    

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
                    onSelect={(props) => console.log('onSelect callback: ', props)}
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
                     
                            ><div className={"flex p-1"}>
                                {isBranch && <ArrowIcon isOpen={isExpanded} />}


                                <label className="label pr-3 ">

                                    {element.name}
                                </label>
                                
                                
                                <CheckBoxIcon
                                    onClick={(e:any) => {
                                        handleSelect(e);
                                        e.stopPropagation();
                                    }}
                                    isChecked={element.metadata?.checked}
                                    variant={
                                        isHalfSelected ? "some" : isSelected ? "all" : "none"
                                    }
                                />
                                {element.metadata?.kind == "file" && <label className={"label pl-2"} >{VirtualFolders.Operations.formatBytes(element.metadata?.size as any)}</label> }
                            </div>
                               
                            
                            </div>
                        )
                    }}
                />
                <button className="btn btn-wide" onClick={() => props.onAccepted(props.nodes)}>Accept</button>
            </div>
        </div>
    );
}

const ArrowIcon = (props: {isOpen: boolean}) => {

    return props.isOpen ? <VscArrowSmallDown size={25}/> : <VscArrowSmallRight size={25}/>
};

const CheckBoxIcon = (props:any) => {
    switch (props.variant) {
        case "all":
            return <input
                    type="checkbox"
                    className="toggle toggle-xs ml-1 mt-1"
                    onChange={(e:any) => props.onClick(e)}
                    defaultChecked={props.isChecked}
                    disabled={false}

                />;
        case "none":
            return <input
                type="checkbox"
                className="toggle toggle-xs ml-1 mt-1"
                checked={true}
                defaultChecked={props.isChecked}
                disabled={false}

            />;
        case "some":
            return <input
                type="checkbox"
                className="toggle toggle-xs ml-1 mt-1"
                checked={true}
                defaultChecked={props.isChecked}
                disabled={false}

            />;
        default:
            return null;
    }
};

export default MultiSelectCheckboxControlled;