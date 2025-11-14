import {Option, Updater} from "ballerina-core";

export type Meta = {
    kind: 'dir' | 'file',
    path: string,
    size?: number,
    isLeaf?: boolean,
    content?: any,
    checked: boolean,
};

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

export type Node = INode<Meta>;

export const FlatNode = {
    Updaters: {
        Template: {
            fileContent: (content: any): Updater<Node> =>
                Updater(node =>
                    ({...node, metadata: {...node.metadata, content: content}}))
        }
    },
    Operations: {
        hasSingleFolderBelowRoot<Meta>(root: Node): boolean {
            const dir = (root.children || []).filter(x => x.metadata.kind === 'dir');
            return dir.length <= 1;
        },
        getFilesForSingleFolderBelowRoot(root: Node): Node[] {
            if (!FlatNode.Operations.hasSingleFolderBelowRoot(root)) return [];
            const main = root.children?.find(x => x.metadata?.kind === 'dir') ?? root;;
            const files = (main.children || []).filter(x => x.metadata.kind === 'file')
            return files
        },
        filterLeafFolders:(node: Node): Node =>
            node.metadata.kind == "file" 
                ? node 
                : node.metadata.isLeaf 
                ? ({...node, children: node.children?.map(n => FlatNode.Operations.filterLeafFolders(n))}) 
                : node,
        replaceFileAtPath: (root: Node, path: string, newFile: Node): Node => {
            const parts = path.split("/").filter(Boolean);

            const recurse = (node: Node, pathParts: string[]): Node => {
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
        findFileByPath: (root: Node, path: string): Node | null => {
            const parts = path.split("/").filter(Boolean);

            const recurse = (node: Node, pathParts: string[]): Node | null => {
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
        findFileByName: (root: Node, fileName: string): Option<Node> => {
            const recurse = (node: Node): Option<Node> => {
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
        
        findFolderByPath: (root: Node, path: string): Option<Node> => {
            const some = Option.Default.some<Node>;
            const none = Option.Default.none<Node>;

            const parts = path.split("/").filter(s => s.length > 0);
            if (parts.length === 0) return root.metadata.kind === "dir" ? some(root) : none();

            const rec = (node: Node, segs: string[]): Option<Node> => {
                const [head, ...tail] = segs;
                if (head === undefined) return node.metadata.kind === "dir" ? some(node) : none();
                if (node.metadata.kind !== "dir") return none();
                if (head === node.name) return rec(node, tail);

                const children = node.children ?? [];
                const child = children.find(c => c.name === head);
                if (child === undefined) return none();
                if (tail.length === 0) {
                    return child.metadata.kind === "file" ? some(node)
                        : child.metadata.kind === "dir"  ? some(child)
                            : none();
                }
                return rec(child, tail);
            };
            return rec(root, parts);
        }
    }
}
