import { VscArrowSmallRight } from "react-icons/vsc";
import React, { useState } from "react";

import TreeView, { flattenTree, INode} from "react-accessible-treeview";
import {VscArrowSmallDown} from "react-icons/vsc";
import {FlatNode, VirtualFolders} from "playground-core";
import {BasicFun, Unit} from "ballerina-core";


const folder = {
    name: "",
    children: [
        {
            name: "Fruits",
            children: [
                { name: "Avocados" },
                { name: "Bananas" },
                { name: "Berries" },
                { name: "Oranges" },
                { name: "Pears" },
            ],
        },
        {
            name: "Drinks",
            children: [
                { name: "Apple Juice" },
                { name: "Chocolate" },
                { name: "Coffee" },
                {
                    name: "Tea",
                    children: [
                        { name: "Black Tea" },
                        { name: "Green Tea" },
                        { name: "Red Tea" },
                        { name: "Matcha" },
                    ],
                },
            ],
        },
        {
            name: "Vegetables",
            children: [
                { name: "Beets" },
                { name: "Carrots" },
                { name: "Celery" },
                { name: "Lettuce" },
                { name: "Onions" },
            ],
        },
    ],
};

const data2 = flattenTree(folder);


export type MultiSelectCheckboxControlledProps = { nodes: FlatNode, onAccepted: BasicFun<FlatNode, void> }
export function MultiSelectCheckboxControlled(props:MultiSelectCheckboxControlledProps) {
    const [selectedIds, setSelectedIds] = useState([]);
    const data = flattenTree(props.nodes as any)
    // const onKeyDown = (e) => {
    //     if (e.key === "Enter") {
    //         getAndSetIds();
    //     }
    // };

    // const getAndSetIds = () => {
    //     setSelectedIds(
    //         document
    //             .querySelector("#txtIdsToSelect")
    //             .value.split(",")
    //             .filter(val => !!val.trim())
    //             .map((x) => {
    //                 if (isNaN(parseInt(x.trim()))) {
    //                     return x;
    //                 }
    //                 return parseInt(x.trim());
    //             })
    //     );
    // };

    return (
        <div>
            {/*<div>*/}
            {/*    <label htmlFor="txtIdsToSelect">*/}
            {/*        Comma-delimited list of IDs to set:*/}
            {/*    </label>*/}
            {/*    <input id="txtIdsToSelect" type="text" onKeyDown={onKeyDown} />*/}
            {/*    <button onClick={() => getAndSetIds()}>Set</button>*/}
            {/*</div>*/}
            {/*<div>*/}
            {/*    <button onClick={() => setSelectedIds([])}>Clear Selected Nodes</button>*/}
            {/*</div>*/}
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
                    checked={true}
                    disabled={false}

                />;
        case "none":
            return <input
                type="checkbox"
                className="toggle toggle-xs ml-1 mt-1"
                checked={true}
                disabled={false}

            />;
        case "some":
            return <input
                type="checkbox"
                className="toggle toggle-xs ml-1 mt-1"
                checked={true}
                disabled={false}

            />;
        default:
            return null;
    }
};

export default MultiSelectCheckboxControlled;