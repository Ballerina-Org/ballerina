type Path = string[];

type FileNode = {
    kind: "file";
    name: string;
    path: Path;
    content: unknown; 
};

type FolderNode = {
    kind: "folder";
    name: string;
    path: Path;
    children: Content[];
};

export type Content = FileNode | FolderNode;

type EncodedFile = {
    kind: "file";
    name: string;
    path: string[];
    content?: unknown;
};

type EncodedFolder = {
    kind: "folder";
    name: string;
    path: string[];
    children: EncodedContent[];
};

export type EncodedContent = EncodedFile | EncodedFolder;

const encodePath = (path: Path): string[] => path;

export function toSlimJson(content: Content, includeContent?: boolean): EncodedContent {
    if (content.kind === "file") {
        const base: EncodedFile = {
            kind: "file",
            name: content.name,
            path: encodePath(content.path),
        };
        return includeContent === true
            ? { ...base, content: content.content }
            : base;
    } else {
        const children = content.children.map((c) => toSlimJson(c, includeContent));
        return {
            kind: "folder",
            name: content.name,
            path: encodePath(content.path),
            children,
        };
    }
}