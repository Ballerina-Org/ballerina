import {Synchronized, Unit} from "ballerina-core";
import {Set} from "immutable";
import {LocalizationState} from "./localization-state.ts";
import {Namespace} from "./namespace.ts";

export type FieldExtraContext = {
    isSuperAdmin: boolean;
    locale: LocalizationState;
    headers: any; //AuthHeaders;
    docId: string;
    foreignMutations: any;// BaseCardSharedForeignMutationsExpected;
    downloadExampleAccountingCsv: Synchronized<Unit, Unit>;
    customLocks: Set<string>;
    namespace: Namespace;
};
