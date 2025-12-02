import {CoTypedFactory, Option} from "ballerina-core";
import {
    CustomFieldsCtx,
    CustomFields
} from "../../domains/phases/custom-fields/state"
import {INode, Meta} from "../../domains/phases/locked/domains/folders/node";

export const Co = CoTypedFactory<CustomFieldsCtx, CustomFields>();