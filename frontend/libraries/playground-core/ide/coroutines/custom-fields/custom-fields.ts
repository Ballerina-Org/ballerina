import {Co} from "./builder";
import {ValueOrErrors, replaceWith, unit} from "ballerina-core";
import {Ide} from "../../state";
import {listSpecs} from "../../api/specs"
import {List} from "immutable";
import {BootstrapPhase} from "../../domains/phases/bootstrap/state";
import {fromError, fromVoe} from "web/src/domains/ide/views/domains/layout/toaster";
import {LockedPhase} from "../../domains/phases/locked/state";
import {CustomFieldsRunner} from "./runner";
import {CustomFields, CustomFieldsProcess} from "../../domains/phases/custom-fields/state";
import {Spec} from "../../domains/phases/selection/state";
import {typeCheckingJob} from "../../api/custom-fields/client";
import {Guid} from "../../domains/types/Guid";
import {TypeCheckingJob} from "../../domains/phases/custom-fields/domains/job/state";
import {TypeCheckingPayload} from "../../domains/phases/custom-fields/domains/type-checking/state";

function narrowed (msg: string): never {
    throw new Error(msg);
}

const errors = CustomFields.Updaters.Core.errors
// export const customFields =
//     Co.GetState().then((state: Ide) => { 
//        
//         if(!(state.phase.kind == 'locked' && state.phase.locked.customFields.status.kind == "type-checking")) {
//             return narrowed("expected locked/customField requested")
//         }
//         const fields = state.phase.locked.customFields
//         const currentTrace = CustomFields.Operations.currentJobTrace(fields.jobFlow);
//        
//         if(currentTrace == undefined || currentTrace.kind != 'requested') return narrowed("At this point there should be a current trace")
//         const payload = currentTrace.job.payload as TypeCheckingPayload
//         return Co.Seq([
//             Co.Await<ValueOrErrors<Guid, any>, any>(() =>
//                 typeCheckingJob(payload), (_err: any) => {}).then(res => {
//                 if (res.kind == "r") {
//                     return Co.SetState(
//                         Ide.Updaters.Core.phase.locked(
//                             LockedPhase.Updaters.Core.customFields(errors(
//                                 replaceWith(List([`Unknown error on type checking job: ${res}`]))))))
//                 } else if (res.value.kind == "errors") {
//                     return Co.SetState(
//                         Ide.Updaters.Core.phase.locked(
//                             LockedPhase.Updaters.Core.customFields(errors(
//                                 replaceWith(List([`Unknown error on type checking job: ${res}`]))))))
//                 }
//    
//                 return Co.SetState(
//                     Ide.Updaters.Core.phase.locked(
//                         LockedPhase.Updaters.Core.customFields(
//                             CustomFields.Updaters.Coroutine.transition(
//                                 {
//                                     kind: 'initial', 
//                                     initial:  {
//                                         kind: 'type-checking',
//                                         jobId: res.value.value
//                                     }}))))
//             })
//            
//         ])});

export const customFields =
    Co.GetState().then((state: CustomFields) => {

        if(!(state.status.kind == "type-checking")) {
            return narrowed("expected locked/customField requested")
        }
        const currentTrace = CustomFields.Operations.currentJobTrace(state.jobFlow);

        if(currentTrace == undefined || currentTrace.kind != 'requested') return narrowed("At this point there should be a current trace")
        
        const payload = currentTrace.job.payload as TypeCheckingPayload
        
        return Co.Seq([
            Co.Await<ValueOrErrors<Guid, any>, any>(() =>
                typeCheckingJob(payload), (_err: any) => {}).then(res => {
                if (res.kind == "r") {
                    return Co.SetState(
                        CustomFields.Updaters.Core.errors(
                                replaceWith(List([`Unknown error on type checking job: ${JSON.stringify(res)}`]))))
                } else if (res.value.kind == "errors") {
                    return Co.SetState(
                        CustomFields.Updaters.Core.errors(
                                replaceWith(List([`Unknown error on type checking job: ${JSON.stringify(res)}`]))))
                }
                return Co.SetState(
                    CustomFields.Updaters.Coroutine.transition(
                        {
                            kind: 'initial',
                            initial:  {
                                kind: 'type-checking',
                                jobId: res.value.value
                            }}))
            })

        ])});