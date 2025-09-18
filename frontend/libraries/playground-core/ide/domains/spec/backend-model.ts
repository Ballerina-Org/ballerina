import {KnownSections, TopLevelKey, VirtualFolderNode, VirtualJsonFile} from "../locked/vfs/state";

type Path = string[];

// type FileNode = {
//     kind: "file";
//     name: string;
//     path: Path;
//     content: unknown; 
// };
//
// type FolderNode = {
//     kind: "folder";
//     name: string;
//     path: Path;
//     children: Content[];
// };
//
// export type Content = FileNode | FolderNode;
//
// type EncodedFile = {
//     kind: "file";
//     name: string;
//     path: string[];
//     content?: unknown;
// };
//
// type EncodedFolder = {
//     kind: "folder";
//     name: string;
//     path: string[];
//     children: EncodedContent[];
// };
//
// export type EncodedContent = EncodedFile | EncodedFolder;
//
// const encodePath = (path: Path): string[] => path;
//
// export function toSlimJson(content: Content, includeContent?: boolean): EncodedContent {
//     if (content.kind === "file") {
//         const base: EncodedFile = {
//             kind: "file",
//             name: content.name,
//             path: encodePath(content.path),
//         };
//         return includeContent === true
//             ? { ...base, content: content.content }
//             : base;
//     } else {
//         const children = content.children.map((c) => toSlimJson(c, includeContent));
//         return {
//             kind: "folder",
//             name: content.name,
//             path: encodePath(content.path),
//             children,
//         };
//     }
// }

import { Map } from "immutable";
export function fromSlimJson(
    raw: unknown,
    opts?: {
        mkTopLevels?: (name: string, path: string[], content: KnownSections) => TopLevelKey[];
        defaultContent?: KnownSections; // used if backend omitted file content
    }
): VirtualFolderNode {
    const defContent = opts?.defaultContent ?? ({} as KnownSections);
    const isObj = (x: unknown): x is Record<string, unknown> => typeof x === "object" && x !== null;

    const go = (n: unknown): VirtualFolderNode => {
        if (!isObj(n) || typeof n.kind !== "string" || typeof n.name !== "string") {
            throw new Error("Invalid node");
        }


        if (n.kind === "file") {
            const content =
                "content" in n && (n as any).content !== undefined ? ((n as any).content as KnownSections) : defContent;
            const name = n.name as string;
            const vfile: VirtualJsonFile = {
                name,
                path: (n as any).path,
                fileRef: undefined,
                content,
                topLevels: opts?.mkTopLevels?.(name, (n as any).path, content) ?? [],
            };
            return VirtualFolderNode.fromFile(vfile);
        }

        if (n.kind === "folder") {
            if (!Array.isArray((n as any).children)) throw new Error("Invalid children");
            const childrenArr = (n as any).children as unknown[];
            const children = Map<string, VirtualFolderNode>();
            for (const chRaw of childrenArr) {
                const child = go(chRaw);
                const key = child.kind === "folder" ? child.name : child.name;
                children.set(key, child);
            }
            return { kind: "folder", name: n.name as string, path: (n as any).path, children };
        }

        throw new Error("Unknown kind");
    };

    return go(raw);
}


export function toSlimJson(node: VirtualFolderNode, includeContent = false): unknown {
    if (node.kind === "file") {
        const { name, path, content } = node;
        return includeContent ? { kind: "file", name, path, content } : { kind: "file", name, path };
    } else {
        return {
            kind: "folder",
            name: node.name,
            path: node.path,
            children: [...node.children.values()].map((c) => toSlimJson(c, includeContent)),
        };
    }
}