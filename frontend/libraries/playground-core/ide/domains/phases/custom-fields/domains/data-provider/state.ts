import {ValueOrErrors, Maybe} from "ballerina-core";
import {FlatNode, INode, Meta} from "../../../locked/domains/folders/node";
import {List} from "immutable";
import { TypeCheckingPayload } from "../job/request/state";

export type TypeCheckingDataProvider = {
    collect: (curatedContext: string) => ValueOrErrors<TypeCheckingPayload, string>;
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
    const collect: (curatedContext: string) => ValueOrErrors<TypeCheckingPayload, string> = (curatedContext: string) => {
        if(node == undefined) {
            return ValueOrErrors.Default.throw(List(["TypeCheckingDataProvider hasn't got a valid node"]))
        }
        const payload = extractTypeCheckingPayload(node, curatedContext);

        // const pt = prompt ()
        // if(pt.kind == "errors") return ValueOrErrors.Default.throw(pt.errors)
        // if(payload.kind == "errors") return ValueOrErrors.Default.throw(payload.errors)
        // payload.value.Constructor =  `${pt.value}\nin ${payload.value.Constructor}`;
        return payload
    }

    const prompt: () => ValueOrErrors<string, string> = () => extractFilePayload("prompt.bl", node);

    return { collect, prompt };
};

const extractTypeCheckingPayload = (node: INode<Meta>, curatedContext: string)
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
        Accessors:     accessors,
        CuratedContext: curatedContext,
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
