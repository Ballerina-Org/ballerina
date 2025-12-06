import {Guid} from "ballerina-core";

export type TypeCheckingPayload = {
    Constructor: any,
    Updater: any,
    Types: any,
    Uncertainties: any,
    Evidence: any,
    Accessors: any
}

export type ConstructionJob = {
    ValueDescriptorId: Guid
}
