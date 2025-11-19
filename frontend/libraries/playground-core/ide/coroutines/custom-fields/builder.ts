import {CoTypedFactory, Option} from "ballerina-core";
import {
    CustomFields
} from "../../domains/phases/custom-fields/state"
import {INode, Meta} from "../../domains/phases/locked/domains/folders/node";

export const Co = CoTypedFactory<CustomFields & { node: Option<INode<Meta>>}, CustomFields>();