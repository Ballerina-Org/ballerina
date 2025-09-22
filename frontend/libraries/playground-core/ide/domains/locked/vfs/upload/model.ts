
import {KnownSections} from "../state";

//TODO: distinguish with proper DU payloads for dir and file
export type Meta =  {
    kind: "dir" | "file";
    path: string;
    size?: number;
    isLeaf?: boolean;
    //fileRef? :any;
    content?: any;
    checked: boolean;
};
//

const split = (path: string) : string[] => path.split("/");


export type NodeId = number | string;
// export type IFlatMetadata = Record<
//     string,
//     string | number | boolean | undefined | null
// >;

//same as INode from "react-accessible-treeview" in web 
export interface INode<Meta> {
    id?: NodeId;
    name: string;
    children?: INode<Meta>[]; 
    parent?: NodeId | null;
    isBranch?: boolean;
    metadata: Meta;
}

export type FlatNode = INode<Meta>;

export const FlatNode = {
    Operations: {
        filterLeafFolders:(node: FlatNode): FlatNode =>
            node.metadata.kind == "file" 
                ? node 
                : node.metadata.isLeaf 
                ? ({...node, children: node.children?.map(n => FlatNode.Operations.filterLeafFolders(n))}) 
                : node
    }
}
// export const takeRoot = (nodes: FlatNode[]): FlatNode => {
//     let root: FlatNode | undefined;
//
//     for (const n of nodes) {
//         if (n.parent === null) {
//             if (root) throw new Error("Multiple roots found");
//             root = n;
//         }
//     }
//
//     if (!root) throw new Error("Root not found");
//     return root;
// }