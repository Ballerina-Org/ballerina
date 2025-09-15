import {
    CollectionReference, DeserializedDispatchSpecification, DispatchLookupSources,
    Errors,
    Guid,
    Identifiable, LookupApis, SpecificationApis,
    Unit,
    ValidationResult,
    Value,
    ValueOrErrors
} from "ballerina-core";
import axios from "axios";
import {
    DispatchPassthroughFormInjectedTypes
} from "web/src/domains/dispatched-passthrough-form/injected-forms/category";
import {
    DispatchPassthroughFormCustomPresentationContext, DispatchPassthroughFormExtraContext,
    DispatchPassthroughFormFlags
} from "web/src/domains/dispatched-passthrough-form/views/concrete-renderers";
import {axiosVOE} from "./api";
import {FullSpec} from "../state";
import {FormsSeedEntity} from "../domains/seeds/state";
import {VirtualFolderNode} from "../domains/vfs/state";

export const getVirtualFolders= async (specName: string) =>
    Promise.resolve(null as VirtualFolderNode)
