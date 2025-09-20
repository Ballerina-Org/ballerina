import {KnownSections, TopLevelKey} from "../locked/vfs/state";
type Path = string [];
import { Map } from "immutable";
import {FlatNode} from "../locked/vfs/upload/model";

// export const fromSlimJson = (
//     raw: any[],
//     opts?: {
//         mkTopLevels?: (name: string, path: string[], content: KnownSections) => TopLevelKey[];
//         defaultContent?: KnownSections; // used if backend omitted file content
//     }
// ): FlatNode[] => {
//     debugger
//     const defContent = opts?.defaultContent ?? ({} as KnownSections);
//     const isObj = (x: unknown): x is Record<string, unknown> => typeof x === "object" && x !== null;
//     debugger
//     return raw.map(raw => {
//         debugger
//         const content =
//             (raw as any).content !== undefined ? ((raw as any).content as KnownSections) : defContent;
//         const name = raw.name as string;
//         const vfile: FlatNode = {
//
//             name,
//             id: (raw.path as string[]).join("/"),
//             metadata: {
//                 kind: raw.kind,
//                 content: content,
//                 path: raw.path,
//                 checked: true,
//               
//             }
//             //path: (n as any).path,
//             //fileRef: undefined,
//             //content,
//             //topLevels: opts?.mkTopLevels?.(name, (n as any).path, content) ?? [],
//         };
//         return vfile;
//     })
//
// }


// export function toSlimJson(node: VirtualFolderNode, includeContent = false): unknown {
//     if (node.kind === "file") {
//         const { name, path, content } = node;
//         return includeContent ? { kind: "file", name, path, content } : { kind: "file", name, path };
//     } else {
//         return {
//             kind: "folder",
//             name: node.name,
//             path: node.path,
//             children: [...node.children.values()].map((c) => toSlimJson(c, includeContent)),
//         };
//     }
// }