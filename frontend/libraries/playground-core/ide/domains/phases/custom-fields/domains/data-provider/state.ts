import {TypeCheckingPayload} from "../type-checking/state";
import {Option, ValueOrErrors} from "ballerina-core";
import {FlatNode, INode, Meta} from "../../../locked/domains/folders/node";
import {List} from "immutable";

export type TypeCheckingProvider = {
    hasNode: () => boolean;
    feed: (raw: unknown) => void;  
    collect: () => ValueOrErrors<TypeCheckingPayload, string>;
    delta: () => ValueOrErrors<string, string>;
    document: () => ValueOrErrors<string, string>;
    prompt: () => ValueOrErrors<string, string>;
};
function isNode(raw: unknown): raw is INode<Meta> {
    return typeof raw === "object" && raw !== null &&
        "metadata" in raw; 
}
export const makeTypeCheckingProvider = (
    maybeNode: Option<unknown>
): TypeCheckingProvider => {

    let internalNode: INode<Meta> | null = null;

    if (maybeNode.kind == "r") {
        if (!isNode(maybeNode.value)) {
            throw new Error("Invalid node passed to provider");
        }
        internalNode = maybeNode.value;
    }

    const hasNode = () => internalNode !== null;

    const feed = (raw: unknown) => {
        if (!isNode(raw)) {
            throw new Error("Invalid node passed to provider");
        }
        internalNode = raw;
    };

    const collect: () => ValueOrErrors<TypeCheckingPayload, string> = () => {
        if (!internalNode) {
            return ValueOrErrors.Default.throw(
                List(["Node not set"])
            );
        }
        const payload = extractTypeCheckingPayload(internalNode);
       // const doc = document()
        const pt = prompt ()
        debugger
       // if(doc.kind == "errors") return ValueOrErrors.Default.throw(doc.errors)
        if(pt.kind == "errors") return ValueOrErrors.Default.throw(pt.errors)
        if(payload.kind == "errors") return ValueOrErrors.Default.throw(payload.errors)
        const currated = `${pt.value}\nin ${payload.value.Constructor}`;
        payload.value.Constructor = currated
        return payload
    };

    const delta: () => ValueOrErrors<string, string> = () => {
        if (!internalNode) {
            return ValueOrErrors.Default.throw(
                List(["Node not set"])
            );
        }
        return extractDeltaPayload(internalNode);
    };

    const document: () => ValueOrErrors<string, string> = () => {
        if (!internalNode) {
            return ValueOrErrors.Default.throw(
                List(["Node not set"])
            );
        }
        return extractFilePayload("document.bl", internalNode);
    };

    const prompt: () => ValueOrErrors<string, string> = () => {
        if (!internalNode) {
            return ValueOrErrors.Default.throw(
                List(["Node not set"])
            );
        }
        return extractFilePayload("prompt.bl", internalNode);
    };

    return { hasNode, feed, collect, delta, document, prompt };
};

function extractTypeCheckingPayload(node: INode<Meta>)
    : ValueOrErrors<TypeCheckingPayload, string>{

const f = FlatNode.Operations.findFolderByPath(
        node,
        node.metadata.path
    );

    if (f.kind === "l") {
        return ValueOrErrors.Default.throw(
            List(["Can't find 'types.bl' file"])
        );
    }

    const children = f.value.children ?? [];
    const files    = children.filter(x => x.metadata.kind === "file");

    const types         = files.find(x => x.name === "types.bl");
    const constructor   = files.find(x => x.name === "constructor.bl");
    const evidence      = files.find(x => x.name === "evidence.bl");
    const uncertainties = files.find(x => x.name === "uncertainties.bl");
    const updater       = files.find(x => x.name === "update.bl");

    if (!types || !constructor || !evidence || !uncertainties || !updater)
        return ValueOrErrors.Default.throw(
            List(["Missing required .bl files"])
        );

    const accessorsDir =
        children.find(x => x.name === "accessors" && x.metadata.kind === "dir");

    const accessors =
        Object.fromEntries(
            (accessorsDir?.children ?? [])
                .map(a => [
                    a.name.replace(/\.bl$/, ""),
                    cleanString(a.metadata.content)
                ])
        );

    return ValueOrErrors.Default.return({
        Constructor:   cleanString(constructor.metadata.content),
        Updater:       cleanString(updater.metadata.content),
        Types:         cleanString(types.metadata.content),
        Uncertainties: cleanString(uncertainties.metadata.content),
        Evidence:      cleanString(evidence.metadata.content),
        Accessors:     accessors
    } as TypeCheckingPayload);
}

function extractFilePayload(name: string, node: INode<Meta>)
    : ValueOrErrors<string, string>{

    const f = FlatNode.Operations.findFolderByPath(
        node,
        node.metadata.path
    );

    if (f.kind === "l") {
        return ValueOrErrors.Default.throw(
            List([`Can't find '${name}' file`])
        );
    }

    const children = f.value.children ?? [];
    const files    = children.filter(x => x.metadata.kind === "file");

    const document         = files.find(x => x.name === name);
    if (!document)
        return ValueOrErrors.Default.throw(
            List([`Missing required '${name}' file`])
        );
    return ValueOrErrors.Default.return(cleanString(document.metadata.content));
}

function extractDeltaPayload(node: INode<Meta>)
    : ValueOrErrors<string, string>{

    const f = FlatNode.Operations.findFolderByPath(
        node,
        node.metadata.path
    );

    if (f.kind === "l") {
        return ValueOrErrors.Default.throw(
            List(["Can't find 'delta.json' file"])
        );
    }

    const children = f.value.children ?? [];
    const files    = children.filter(x => x.metadata.kind === "file");

    const delta         = files.find(x => x.name === "delta.json");
    if (!delta)
        return ValueOrErrors.Default.throw(
            List(["Missing required delta file"])
        );
    return ValueOrErrors.Default.return(cleanString(delta.metadata.content));
}

function unescapeEscapedQuotes(s: string): string {
    return s.replace(/\\\\\\"/g, '"')   // \\\"
        .replace(/\\\\"/g, '"')    // \\"
        .replace(/\\"/g, '"');     // \"
}
export function stripBOM(text: string): string {
    if (text.charCodeAt(0) === 0xFEFF) {
        return text.slice(1);
    }
    return text;
}
export const cleanString = (raw: string) => raw
export const cleanString2 = (raw: string) => {
    if (!raw) return raw;

    let s = raw;

    // 1. Turn escaped newlines into real newlines (if any still left)
    //    If you already see real line breaks, this will effectively do nothing.
    s = s.replace(/\\r\\n/g, "\n")
        .replace(/\\n/g, "\n")
        .replace(/\\r/g, "\r");

    // 2. Strip repeated outer quotes like ""foo""
    while (
        s.length >= 4 &&
        ((s.startsWith('""') && s.endsWith('""')) ||
            (s.startsWith("''") && s.endsWith("''")))
        ) {
        s = s.slice(1, -1);
    }

    // 3. Optionally strip a single pair of outer quotes
    //    If you want bare code (no wrapping "...")
    if (
        s.length >= 2 &&
        ((s.startsWith('"') && s.endsWith('"')) ||
            (s.startsWith("'") && s.endsWith("'")))
    ) {
        s = s.slice(1, -1);
    }

    return (stripBOM(unescapeEscapedQuotes(s)));

};
export function unquote(input: string): string {
    if (!input) return input;

    let s = input.trim();

    // 1) Try JSON.parse safely (only unwrap if it returns a string)
    try {
        const parsed = JSON.parse(s);
        if (typeof parsed === "string") {
            s = parsed;
        }
    } catch {
        // ignore JSON failures
    }

    // 2) Remove *matching outer quotes* repeatedly
    while (
        s.length >= 2 &&
        ((s.startsWith('"') && s.endsWith('"')) ||
            (s.startsWith("'") && s.endsWith("'")))
        ) {
        s = s.slice(1, -1);
    }

    return  cleanString(s)
}