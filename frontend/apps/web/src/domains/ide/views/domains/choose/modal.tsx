import {FlatNode, getOrInitSpec, Ide, NodeId, postVfsNode, VirtualFolders} from "playground-core";
import {BasicFun, BasicUpdater, Option} from "ballerina-core";
import React from "react";
import {HorizontalDropdown} from "../../dropdown.tsx";
import MultiSelectCheckboxControlled from "../vfs/example.tsx"
import {Map} from "immutable";
// async function uploadFileNode(spec: string, node: FlatNode): Promise<void> {
//     const file = node.metadata.fileRef;
//     if (!file) {
//         console.warn(`Skipping node without fileRef: ${node.name}`);
//         return;
//     }
//
//     const content = await file.text(); // or .arrayBuffer(), .stream(), etc.
//
//     node.metadata.fileRef = content;
//     const payload = {
//         path: node.metadata.path,
//         name: node.name,
//         content,
//     };
//     debugger
//     await postVfsNode(spec, node)
//
//     // await fetch(apiUrl, {
//     //     method: "POST",
//     //     headers: {
//     //         "Content-Type": "application/json",
//     //     },
//     //     body: JSON.stringify(payload),
//     // });
// }

// export async function uploadAllFileNodes(spec: string,root: FlatNode): Promise<void> {
//     const allUploads: Promise<void>[] = [];
//
//     const walk = (node: FlatNode) => {
//         debugger
//         if (node.metadata.kind === "file" && node.metadata.fileRef) {
//             allUploads.push(uploadFileNode(spec, node));
//         }
//         if (node.children) {
//             node.children.forEach(walk);
//         }
//     };
//
//     walk(root);
//     await Promise.all(allUploads); // or use `for await` if you want to throttle
// }
async function buildTree(files: FileList, insert: (acc: FlatNode, parts: string[], file: File) => Promise<FlatNode>, root: FlatNode): Promise<FlatNode> {
    return Array.from(files).reduce(
        (promiseAcc, file) =>
            promiseAcc.then(async (acc) => {
                const parts = file.webkitRelativePath.split("/").filter(Boolean);
                return await insert(acc, parts, file);
            }),
        Promise.resolve(root)
    );
}
export const fileListToFlatTree = async (files: FileList): Promise<FlatNode> => {
    const root: FlatNode = {
        id: "root",
        name: "root",
        parent: null,
        isBranch: true,
        metadata: { kind: "dir", path: "root", checked: true },
        children: [],
    };
    debugger
    const insert = async (node: FlatNode, pathParts: string[], file: File): Promise<FlatNode> => {
        if (pathParts.length === 0) return node;

        const [head, ...rest] = pathParts;

        const existing = node.children?.find(c => c.name === head);

        if (rest.length === 0) {
           
            const fileNode: FlatNode = {
                id: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
                name: head,
                parent: node.id ?? null,
                isBranch: false,
                metadata: {
                    kind: "file",
                    path: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
                    size: file.size,
                    checked: true,
                    content: JSON.parse(await file.text())
                }
            };
            node.children = [...(node.children ?? []), fileNode];
        } else {
            let dirNode = existing;

            if (!dirNode || dirNode.metadata.kind !== "dir") {
                dirNode = {
                    id: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
                    name: head,
                    parent: node.id ?? null,
                    isBranch: true,
                    metadata: {
                        kind: "dir",
                        path: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
                        checked: true
                    },
                    children: [],
                };
                node.children = [...(node.children ?? []), dirNode];
            }

            insert(dirNode, rest, file);
        }

        return node;
    };

    // return Array.from(files).reduce((acc, file) => {
    //     const parts = file.webkitRelativePath.split("/").filter(Boolean);
    //     const t = await insert(acc, parts, file);
    //     return t
    // }, root);
    debugger
    const r = await  buildTree(files, insert, root)
    return r
};

