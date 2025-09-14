import { DockItem } from "../../layout.tsx";
import React from "react";
import { Option } from "ballerina-core";
import TreeView, { INode, NodeId } from "react-accessible-treeview";
import {VscFile, VscFolder} from "react-icons/vsc";


type Node =
    | { kind: "dir"; name: string; children: Record<string, Node> }
    | { kind: "file"; name: string; file: File };

type DirNode = Extract<Node, { kind: "dir" }>;


function filesToTree(files: File[]): Node {
    const root: Node = { kind: "dir", name: "/", children: {} };
    const filtered = files.filter(isJson);
    for (const f of filtered) {
        const rel = (f as any).webkitRelativePath || f.name;
        const parts = rel.split("/").filter(Boolean);
        let cur = root as DirNode;

        for (let i = 0; i < parts.length; i++) {
            const name = parts[i];
            const isLeaf = i === parts.length - 1;

            if (isLeaf) {
                cur.children[name] = { kind: "file", name, file: f };
            } else {
                if (!cur.children[name]) {
                    cur.children[name] = { kind: "dir", name, children: {} };
                }
                cur = cur.children[name] as DirNode;
            }
        }
    }

    return root;
}

function formatBytes(n?: number) {
    if (!n) return "";
    const k = 1024,
        u = ["B", "KB", "MB", "GB", "TB"];
    const i = Math.min(u.length - 1, Math.floor(Math.log(n) / Math.log(k)));
    return `${(n / Math.pow(k, i)).toFixed(1)} ${u[i]}`;
}


type Meta = { path: string; kind: "dir" | "file"; size?: number };

function toTreeViewData(root: DirNode): INode<Meta>[] {
    const items: INode<Meta>[] = [];

    function walk(node: Node, path: string[], parent: NodeId | null) {
        const id: NodeId = path.join("/");

        if (node.kind === "dir") {
            const names = Object.keys(node.children).sort();
            const childIds: NodeId[] = names.map((n) => [...path, n].join("/"));

            items.push({
                id,
                name: node.name,
                parent,
                children: childIds, // leafs must use [], not undefined
                metadata: { path: String(id), kind: "dir" },
            });

            for (const n of names) {
                walk(node.children[n], [...path, n], id);
            }
        } else {
            items.push({
                id,
                name: node.name,
                parent,
                children: [], // leaf
                metadata: { path: String(id), kind: "file", size: node.file.size },
            });
        }
    }

    // multiple roots: children of "/" become parent=null
    for (const name of Object.keys(root.children).sort()) {
        walk(root.children[name], [name], null);
    }
    return items;
}


function AccessibleTree({ root }: { root: DirNode }) {
    const data = React.useMemo(() => toTreeViewData(root), [root]);
    
    const folderIds = React.useMemo<NodeId[]>(
        () => data.filter((d) => d.children.length > 0).map((d) => d.id),
        [data]
    );
    
    const [checked, setChecked] = React.useState<Set<NodeId>>(() => new Set([]))//; folderIds));
    React.useEffect(() => setChecked(new Set([])), []); // folderIds)), [folderIds]);

    return (
        <div className="card bg-base-100 shadow w-full">
            <div className="card-body p-3">
                <TreeView
                    data={data}
                    aria-label="Files"
                    className="text-sm"
                    defaultExpandedIds={folderIds}
                    nodeRenderer={({ element, getNodeProps, level, isBranch, isExpanded, handleExpand }) => {
                        const isFolder = element.metadata?.kind === "dir";
                        return (
                            <div
                                {...getNodeProps({ onClick: handleExpand })}
                                className="flex items-center gap-2 py-1"
                                style={{ marginLeft: (level - 1) * 16 }}
                            >
                                {isBranch ? (
                                    <span className={`transition-transform ${isExpanded ? "rotate-90" : ""}`}>▸</span>
                                ) : (
                                    <span className="w-3 inline-block" />
                                )}

                                {isFolder ? <VscFolder size={15} /> : <VscFile size={15} />}

                                <span className={isFolder ? "font-medium" : "opacity-80"}>{element.name}</span>


                                {isFolder && (
                                    <input
                                        type="checkbox"
                                        className="toggle toggle-xs ml-1"
                                        checked={checked.has(element.id)}
                                        onChange={(e) => {
                                            e.stopPropagation();
                                            const id = element.id as NodeId;
                                            setChecked((prev) => {
                                                const next = new Set(prev);
                                                e.target.checked ? next.add(id) : next.delete(id);
                                                return next;
                                            });
                                        }}
                                        onClick={(e) => e.stopPropagation()}
                                    />
                                )}
                                
                                {!isFolder && element.metadata?.size != null && (
                                    <span className="badge badge-ghost badge-xs ml-1">
                    {formatBytes(element.metadata.size as any)}
                  </span>
                                )}
                            </div>
                        );
                    }}
                />
            </div>
        </div>
    );
}
const isJson = (f: File) =>
    f.type === "application/json" || /\.json$/i.test(f.name);


export const drawer = (dockItem: DockItem) => {
    const [node, setNode] = React.useState<Option<Node>>(Option.Default.none());

    const handlePick = (e: React.ChangeEvent<HTMLInputElement>) => {
        const list = e.currentTarget.files;
        if (!list) return;

        const files = Array.from(list);
        const tree = filesToTree(files);

        console.log(`Total files: ${files.length}`);
        console.log("/");
        
        const printTree = (n: Node, prefix = ""): void => {
            if (n.kind === "dir") {
                const entries = Object.values(n.children).sort((a, b) => {
                    if (a.kind !== b.kind) return a.kind === "dir" ? -1 : 1;
                    return a.name.localeCompare(b.name);
                });
                entries.forEach((child, idx) => {
                    const isLast = idx === entries.length - 1;
                    const branch = isLast ? "└─ " : "├─ ";
                    const nextPrefix = prefix + (isLast ? "   " : "│  ");
                    if (child.kind === "dir") {
                        console.log(prefix + branch + child.name + "/");
                        printTree(child, nextPrefix);
                    } else {
                        console.log(prefix + branch + `${child.name} (${formatBytes(child.file.size)})`);
                    }
                });
            }
        };
        printTree(tree);

        setNode(Option.Default.some(tree));
    };

    return (
        <div className="drawer pt-16">
            <input id="my-drawer" type="checkbox" className="drawer-toggle" />
            <div className="drawer-content" />

            <div className="drawer-side top-16 h-[calc(100vh-4rem)] z-40">
                <label htmlFor="my-drawer" aria-label="close sidebar" className="drawer-overlay"></label>

                <ul className="menu bg-base-200 text-base-content min-h-full w-[50vw] p-4">
                    {dockItem == "about" && (
                        <>
                            <div className="hero bg-base-200 min-h-screen">
                                <div className="hero-content flex-col lg:flex-row">
                                    <img
                                        src="https://framerusercontent.com/images/0YOFGJQT6BszWKZYV9kMTT629JA.png?scale-down-to=1024"
                                        className="max-w-sm rounded-lg shadow-2xl"
                                    />
                                    <div>
                                        <h1 className="text-5xl font-bold">Created at BLP</h1>
                                        <p className="py-6">
                                            Native-AI for ERP automation
                                            At BLP, we design and engineer our own AI models for true, complex, end-to-end ERP automation.
                                        </p>
                                        <button className="btn btn-primary">Join us!</button>
                                    </div>
                                </div>
                            </div>

                            <footer className="footer sm:footer-horizontal bg-neutral text-neutral-content p-10">
                                <nav>
                                    <h6 className="footer-title">Services</h6>
                                    <a className="link link-hover">Branding</a>
                                    <a className="link link-hover">Design</a>
                                    <a className="link link-hover">Marketing</a>
                                    <a className="link link-hover">Advertisement</a>
                                </nav>
                                <nav>
                                    <h6 className="footer-title">Company</h6>
                                    <a className="link link-hover">About us</a>
                                    <a className="link link-hover">Contact</a>
                                    <a className="link link-hover">Jobs</a>
                                    <a className="link link-hover">Press kit</a>
                                </nav>
                                <nav>
                                    <h6 className="footer-title">Legal</h6>
                                    <a className="link link-hover">Terms of use</a>
                                    <a className="link link-hover">Privacy policy</a>
                                    <a className="link link-hover">Cookie policy</a>
                                </nav>
                            </footer>
                        </>
                    )}

                    {dockItem == "folders" && (
                        <>
                            <div className="flex w-full">
                                <div className="card bg-warning text-neutral-content w-1/2">
                                    <div className="card-body items-start gap-3">
                                        <h2 className="card-title">Select files</h2>
                                        <div className="flex flex-wrap gap-3">
                                            <input
                                                type="file"
                                                multiple
                                                onChange={handlePick}
                                                className="file-input file-input-accent"
                                            />
                                     
                                        </div>
                                    </div>
                                </div>
                                <div className="divider divider-horizontal">OR</div>
                             <div className="card bg-warning text-neutral-content w-1/2">
                                    <div className="card-body items-start gap-3">
                                        <h2 className="card-title">Select folder</h2>
                                        <div className="flex flex-wrap gap-3">
                                       
                                            <input
                                                type="file"
                                                multiple
                                                onChange={handlePick}
                                                className="file-input file-input-accent"
                                                {...{ webkitdirectory: "", directory: "" }}
                                            />
                                        </div>
                                    </div>
                                </div>
                            </div>
                      
                            <div className="indicator w-full mt-12 ">
                                <span className="indicator-item indicator-center badge badge-accent">{checked.length}</span>
                                <button className="btn btn-block btn-accent w-full">Accept selected</button>
                            </div>
                                
                           
                           
                          
                            {/* render the accessible tree when we have data */}
                            {node.kind === "r" && node.value.kind === "dir" && (
                                <div className="mt-4">
                                    <AccessibleTree root={node.value} />
                                </div>
                            )}
                        </>
                    )}
                </ul>
            </div>
        </div>
    );
};
