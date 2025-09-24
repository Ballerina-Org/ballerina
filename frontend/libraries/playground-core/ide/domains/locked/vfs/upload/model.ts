
import {KnownSections} from "../state";
import {Option} from "ballerina-core";

export type Meta =  {
    kind: "dir" | "file";
    path: string;
    size?: number;
    isLeaf?: boolean;
    content?: any;
    checked: boolean;
};
//
export type NodeId = number | string;

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
                : node,
        replaceFileAtPath: (root: FlatNode, path: string, newFile: FlatNode): FlatNode => {
            const parts = path.split("/").filter(Boolean);

            const recurse = (node: FlatNode, pathParts: string[]): FlatNode => {
                if (pathParts.length === 0) return node;

                const [current, ...rest] = pathParts;
                if (node.metadata.kind !== "dir" || !node.children) return node;

                return {
                    ...node,
                    children: node.children.map(child => {
                        if (child.name !== current) return child;
                        return rest.length === 0 ? newFile : recurse(child, rest)
                    }),
                };
            };

            return recurse(root, parts);
        },
        findFileByPath: (root: FlatNode, path: string): FlatNode | null => {
            const parts = path.split("/").filter(Boolean);

            const recurse = (node: FlatNode, pathParts: string[]): FlatNode | null => {
                if (pathParts.length === 0) return null;

                const [current, ...rest] = pathParts;

                if (node.metadata.kind !== "dir" || !node.children) return null;

                const child = node.children.find(c => c.name === current);
                if (!child) return null;

                if (rest.length === 0) {
                    return child.metadata.kind === "file" ? child : null;
                }

                return recurse(child, rest);
            };

            return recurse(root, parts);
        },
        findFileByName: (root: FlatNode, fileName: string): Option<FlatNode> => {
            const recurse = (node: FlatNode): Option<FlatNode> => {
                if (node.metadata.kind === "file" && node.name === fileName) {
                    return Option.Default.some(node);
                }

                if (node.metadata.kind === "dir" && node.children) {
                    for (const child of node.children) {
                        const found = recurse(child);
                        if (found.kind == "r") return found;
                    }
                }

                return Option.Default.none();
            };

            return recurse(root);
        },
        findFolderByPath: (root: FlatNode, path: string): FlatNode | null => {
            const parts = path.split("/").filter(Boolean);

            const recurse = (node: FlatNode, pathParts: string[]): FlatNode | null => {
                if (pathParts.length === 0) return node.metadata.kind === "dir" ? node : null;

                if (node.metadata.kind !== "dir" || !node.children) return null;

                const [current, ...rest] = pathParts;

                const child = node.children.find(c => c.name === current);
                if (!child) return null;

                return recurse(child, rest);
            };

            return recurse(root, parts);
        },
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