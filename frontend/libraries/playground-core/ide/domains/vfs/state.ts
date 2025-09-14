type VirtualFolders = Map<string, File[]>; // key = folder path ("" for root)

function handlePick(e: React.ChangeEvent<HTMLInputElement>) {
    const list = e.currentTarget.files;
    if (!list) return;

    const files = Array.from(list);

    // Build virtual folders ("" = root). Uses webkitRelativePath when present.
    const folders: VirtualFolders = new Map();

    for (const file of files) {
        const relPath = (file as any).webkitRelativePath || file.name; // e.g. "src/utils/a.json" or "a.json"
        const lastSlash = relPath.lastIndexOf("/");
        const dir = lastSlash >= 0 ? relPath.slice(0, lastSlash) : "";

        if (!folders.has(dir)) folders.set(dir, []);
        folders.get(dir)!.push(file);
    }

    // Example: inspect structure
    console.log("Total files:", files.length);
    console.log("Virtual folders:", Object.fromEntries(folders));

    // If you need a nested tree instead of buckets:
    // const tree = filesToTree(files);
    // console.log(tree);

    // If you’ll POST to backend and want to preserve folder paths:
    // const form = new FormData();
    // for (const f of files) {
    //   const path = (f as any).webkitRelativePath || f.name;
    //   form.append("files", f, path); // filename carries subpath
    // }
    // await fetch("/api/upload", { method: "POST", body: form });
}
