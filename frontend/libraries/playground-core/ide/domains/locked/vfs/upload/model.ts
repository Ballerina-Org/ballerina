/*
* 1. new spec - simple vfs ( with top nodes) made on the backend
* 2. user works on a simple vfs or
* 3. user upload folders which can be filtered (with toggles) and sync that structure to the backend
* 4. from now on only updating the files works (no upload)
* 
* 
* */


import {KnownSections} from "../state";

export type Meta = {
    kind: "dir" | "file";
    path: string;
    size?: number;
    isLeafFolder?: boolean;
    fileRef? :any;
    content?: any;
    checked: boolean;
};
//

const split = (path: string) : string[] => path.split("/");


export type NodeId = number | string;
export type IFlatMetadata = Record<
    string,
    string | number | boolean | undefined | null
>;

//same as INode from "react-accessible-treeview" in web 
export interface INode<M extends IFlatMetadata = IFlatMetadata> {
    id?: NodeId;
    name: string;
    children?: INode<Meta>[]; 
    parent?: NodeId | null;
    isBranch?: boolean;
    metadata: M;

}

export type FlatNode = INode<Meta>;
export const takeRoot = (nodes: FlatNode[]): FlatNode => {
    let root: FlatNode | undefined;

    for (const n of nodes) {
        if (n.parent === null) {
            if (root) throw new Error("Multiple roots found");
            root = n;
        }
    }

    if (!root) throw new Error("Root not found");
    return root;
}