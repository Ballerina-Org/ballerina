

// async function buildTree(
//     files: FileList,
//     insert: (acc: FlatNode, parts: string[], file: File) => Promise<FlatNode>,
//     root: FlatNode
// ): Promise<FlatNode> {
//     return Array.from(files).reduce(
//         (promiseAcc, file) =>
//             promiseAcc.then(async (acc) => {
//                 const rel = (file as any).webkitRelativePath || file.name; // <-- fallback
//                 const parts = rel.split(/[\\/]/).filter(Boolean);
//                 return insert(acc, parts, file);
//             }),
//        
//         Promise.resolve(root)
//     );
// }
//
// // export const markLeaves = (n: FlatNode): FlatNode => {
// //     const children = (n.children ?? []).map(markLeaves);
// //     const hasDirChild = children.some(c => c.metadata.kind === "dir");
// //     const isLeaf = n.metadata.kind === "file" || (n.metadata.kind === "dir" && !hasDirChild);
// //     return { ...n, children, metadata: { ...n.metadata, isLeaf } };
// // };
// export const markLeaves = (n: FlatNode): FlatNode => {
//     if (n.metadata.kind === "file") {
//         // don't add `children` at all for files
//         return { ...n, metadata: { ...n.metadata, isLeaf: true } };
//     }
//
//     const children = (n.children ?? []).map(markLeaves);
//     const hasDirChild = children.some(c => c.metadata.kind === "dir");
//
//     return {
//         ...n,
//         ...(children.length ? { children } : {}), // keep only if non-empty (optional)
//         metadata: { ...n.metadata, isLeaf: !hasDirChild },
//     };
// };
// export const fileListToFlatTree = async (files: FileList): Promise<ValueOrErrors<FlatNode, any>> => {
//     try {
//         const root: FlatNode = {
//             id: "root",
//             name: "root",
//             parent: null,
//             isBranch: true,
//             metadata: {kind: "dir", path: "root", checked: true},
//             children: [],
//         };
//
//         const insert = async (node: FlatNode, pathParts: string[], file: File): Promise<FlatNode> => {
//             if (pathParts.length === 0) return node;
//
//             const [head, ...rest] = pathParts;
//             const existing = node.children?.find(c => c.name === head);
//            
//             if (rest.length === 0) {
//                 //TODO: make it a properly validated in the state
//                 const content = await (async () => {
//                     try {
//                         return JSON.parse(await file.text());
//                     } catch {
//                         return {};
//                     }
//                 })();
//                 const fileNode: FlatNode = {
//                     id: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
//                     name: head,
//                     parent: node.id ?? null,
//                     isBranch: false,
//                     metadata: {
//                         kind: "file",
//                         path: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
//                         size: file.size,
//                         checked: true,
//                         // if any non-JSON appears, this will throw and stop everything
//                         // wrap in try/catch if you want to keep going
//                         content: content
//                     }
//                 };
//                 node.children = [...(node.children ?? []), fileNode];
//                 return node;
//             } else {
//                 let dirNode = existing && existing.metadata.kind === "dir" ? existing : undefined;
//
//                 if (!dirNode) {
//                     dirNode = {
//                         id: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
//                         name: head,
//                         parent: node.id ?? null,
//                         isBranch: true,
//                         metadata: {
//                             kind: "dir",
//                             path: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
//                             checked: true
//                         },
//                         children: [],
//                     };
//                     node.children = [...(node.children ?? []), dirNode];
//                 }
//
//                 await insert(dirNode, rest, file); // <-- important: await recursion
//                 return node;
//             }
//         };
//
//         const tree = await buildTree(files, insert, root);
//         const result = markLeaves(tree);
//         return ValueOrErrors.Default.return(result);
//     }
//     catch(e:any) {
//         return ValueOrErrors.Default.throwOne(e);
//     }
// };
