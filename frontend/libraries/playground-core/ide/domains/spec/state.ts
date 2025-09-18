import {BasicUpdater, Option, ParsedFormJSON, Updater, Value} from "ballerina-core";
import {Seeds} from "../seeds/state";

import {Ide} from "../../state";

// export type V1 = ParsedFormJSON<any>
//
// export type V2 = {
//     typesV2: any,
//     schema: any
// };
//
// export type Spec = {
//     v1: V1,
//     v2: V2,
//     seeds: Seeds,
//     config: any
// }
//
// export type SpecEditor = {
//     v1: V1,
//     v2: V2,
//     seeds: Seeds,
//     config: any
// }
//
// export type SpecVx = {
//     v1: ParsedFormJSON<any>,
//     v2: V2
// }
// export const Spec = {
//     Default: (): Spec => ({
//         v1: {} as V1,
//         v2: {} as V2,
//         seeds:  { entities: [], lookups: [] },
//         config: {}
//     }),
//
// }
