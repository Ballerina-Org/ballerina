import {TypeCheckingPayload} from "../type-checking/state";
import {ValueOrErrors, Maybe} from "ballerina-core";
import {FlatNode, INode, Meta} from "../../../locked/domains/folders/node";
import {List} from "immutable";

export type TypeCheckingDataProvider = {
    collect: () => ValueOrErrors<TypeCheckingPayload, string>;
    prompt: () => ValueOrErrors<string, string>;
};

export const makeTypeCheckingProviderFromWorkspace = (
    /**
     * Node must be a selected file in a workspace, that has custom entity code files as a siblings.
     * Ideally it should be the main spec file json
     * (PoC conception, likely to change)
     * @node
     */
    node: Maybe<INode<Meta>>
): TypeCheckingDataProvider => {
    const collect: () => ValueOrErrors<TypeCheckingPayload, string> = () => {
        if(node == undefined) {
            return ValueOrErrors.Default.throw(List(["TypeCheckingDataProvider hasn't got a valid node"]))
        }
        const payload = extractTypeCheckingPayload(node);

        const pt = prompt ()
        if(pt.kind == "errors") return ValueOrErrors.Default.throw(pt.errors)
        if(payload.kind == "errors") return ValueOrErrors.Default.throw(payload.errors)
        payload.value.Constructor =  `${pt.value}\nin ${payload.value.Constructor}`;
        return payload
    }

    const prompt: () => ValueOrErrors<string, string> = () => extractFilePayload("prompt.bl", node);

    return { collect, prompt };
};

const extractTypeCheckingPayload = (node: INode<Meta>)
    : ValueOrErrors<TypeCheckingPayload, string> => {

const f: Maybe<INode<Meta>> = FlatNode.Operations.findFolderByPath(
        node,
        node.metadata.path
    );

    if (!f) {
        return ValueOrErrors.Default.throw(
            List(["Can't locate custom entity files"])
        );
    }

    const children = f.children ?? [];
    const files    = children.filter(x => x.metadata.kind === "file");

    const types: Maybe<INode<Meta>>         = files.find(x => x.name === "types.bl");
    const constructor: Maybe<INode<Meta>>   = files.find(x => x.name === "constructor.bl");
    const evidence: Maybe<INode<Meta>>      = files.find(x => x.name === "evidence.bl");
    const uncertainties: Maybe<INode<Meta>> = files.find(x => x.name === "uncertainties.bl");
    const updater: Maybe<INode<Meta>>       = files.find(x => x.name === "update.bl");

    if (!types || !constructor || !evidence || !uncertainties || !updater)
        return ValueOrErrors.Default.throw(
            List(["Missing required *.bl files"])
        );

    const accessorsDir =
        children.find(x => x.name === "accessors" && x.metadata.kind === "dir");

    const accessors =
        Object.fromEntries(
            (accessorsDir?.children ?? [])
                .map(a => [
                    a.name.replace(/\.bl$/, ""),
                    a.metadata.content
                ])
        );

    return ValueOrErrors.Default.return({
        Constructor:   constructor.metadata.content,
        Updater:       updater.metadata.content,
        Types:         types.metadata.content,
        Uncertainties: uncertainties.metadata.content,
        Evidence:      evidence.metadata.content,
        Accessors:     accessors
    } as TypeCheckingPayload);
}

function extractFilePayload(name: string, node: Maybe<INode<Meta>>)
    : ValueOrErrors<string, string>{
    if(node == undefined) {
        return ValueOrErrors.Default.throw(List(["TypeCheckingDataProvider hasn't got a valid node"]))
    }
    const f: Maybe<INode<Meta>> = FlatNode.Operations.findFolderByPath(
        node,
        node.metadata.path
    );

    if (!f) {
        return ValueOrErrors.Default.throw(
            List([`Can't find '${name}' file`])
        );
    }

    const children = f.children ?? [];
    const files    = children.filter(x => x.metadata.kind === "file");

    const document         = files.find(x => x.name === name);
    if (!document)
        return ValueOrErrors.Default.throw(
            List([`Missing required '${name}' file`])
        );
    return ValueOrErrors.Default.return(document.metadata.content);
}

// function extractDeltaPayload(node: INode<Meta>)
//     : ValueOrErrors<string, string>{
//
//     const f = FlatNode.Operations.findFolderByPath(
//         node,
//         node.metadata.path
//     );
//
//     if (f.kind === "l") {
//         return ValueOrErrors.Default.throw(
//             List(["Can't find 'delta.json' file"])
//         );
//     }
//
//     const children = f.value.children ?? [];
//     const files    = children.filter(x => x.metadata.kind === "file");
//
//     const delta         = files.find(x => x.name === "delta.json");
//     if (!delta)
//         return ValueOrErrors.Default.throw(
//             List(["Missing required delta file"])
//         );
//     return ValueOrErrors.Default.return(cleanString(delta.metadata.content));
// }

// function unescapeEscapedQuotes(s: string): string {
//     return s.replace(/\\\\\\"/g, '"')   // \\\"
//         .replace(/\\\\"/g, '"')    // \\"
//         .replace(/\\"/g, '"');     // \"
// }
// export function stripBOM(text: string): string {
//     if (text.charCodeAt(0) === 0xFEFF) {
//         return text.slice(1);
//     }
//     return text;
// }

// export function unquote(input: string): string {
//     if (!input) return input;
//
//     let s = input.trim();
//
//     // 1) Try JSON.parse safely (only unwrap if it returns a string)
//     try {
//         const parsed = JSON.parse(s);
//         if (typeof parsed === "string") {
//             s = parsed;
//         }
//     } catch {
//         // ignore JSON failures
//     }
//
//     // 2) Remove *matching outer quotes* repeatedly
//     while (
//         s.length >= 2 &&
//         ((s.startsWith('"') && s.endsWith('"')) ||
//             (s.startsWith("'") && s.endsWith("'")))
//         ) {
//         s = s.slice(1, -1);
//     }
//
//     return  cleanString(s)
// }