import {Guid} from "ballerina-core";

export type TypeCheckingPayload = {
    Constructor: any,
    Updater: any,
    Types: any,
    Uncertainties: any,
    Evidence: any,
    Accessors: any,
    CuratedContext: string
}

export type ConstructionJob = {
    ValueDescriptorId: Guid
}

export type UpdaterJob = {
    ValueId: Guid,
    Parameter: { Delta: string}
}
