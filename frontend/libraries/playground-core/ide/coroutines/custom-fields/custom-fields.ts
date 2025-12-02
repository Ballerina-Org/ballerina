import {Co} from "./builder";
import {
    ValueOrErrors,
    replaceWith,
    Option,
    Debounce,
    Synchronized,
    Value,
    ValidationResult,
    Synchronize, apiResultStatuses, Guid, Unit, PredicateValue
} from "ballerina-core";

import {CustomFields} from "../../domains/phases/custom-fields/state";
import {
    constructionJob,
    getConstructionJobStatus,
    getTypeCheckingJobStatus, getValue, statusSynchronizer,
    typeCheckingJob,
} from "../../api/custom-fields/client";
//import {Guid} from "../../domains/types/Guid";
import {
    ConstructionJobResponse, RequestValueJobResponse,
    TypeCheckingJobResponse, 
} from "../../domains/phases/custom-fields/domains/job/state"
import {CustomFieldsEvent} from "../../domains/phases/custom-fields/domains/event/state";
import {ParentApi} from "../../../parent/apis/mocks";
import {Parent} from "../../../parent/state";


export const  t = 
    //Co.Await(jobStatusSynchronizer, (_err: any) => {})
    Co.All([statusSynchronizer])
export const customFields =
    Co.Repeat(
        Co.GetState().then((state: CustomFields) => {
            const typeChecking = CustomFields.Updaters.Coroutine.isTypeCheckingRequested(state);
            if(typeChecking.kind == "l") return Co.SetState(_ => state);
            
            return Co.Seq([
                Co.Await<ValueOrErrors<Guid, any>, any>(() =>
                    typeCheckingJob(typeChecking.value), (_err: any) => {}).then(res => {
                    const response = CustomFields.Operations.checkResponseForErrors<Guid>(res, 'type-checking job')
                    if(response.kind == "r") return response.value
                    return Co.SetState(
                        CustomFields.Updaters.Coroutine.transition(
                            CustomFieldsEvent.typeCheckingInit(res.value.value)))
                }),
                Co.Wait(5000),
                Co.GetState().then((state: CustomFields) => {
                    const typeCheckingDispatched = CustomFields.Updaters.Coroutine.isTypeCheckingDispatched(state);
                    if(typeCheckingDispatched.kind == "l") return Co.SetState(_ => state);
                   
                    return Co.Await<ValueOrErrors<TypeCheckingJobResponse, any>, any>(() =>
                    getTypeCheckingJobStatus(typeCheckingDispatched.value), (_err: any) => {}).then(res => {
                    const response = CustomFields.Operations.checkResponseForErrors<TypeCheckingJobResponse>(res, 'type-checking status')
                    if(response.kind == "r") return response.value
                    if(res.value.value.status == 3) return Co.SetState(CustomFields.Updaters.Coroutine.errorAndFail(res.value.value.error.message));
                    if(res.value.value.status == 1) return Co.SetState(CustomFields.Updaters.Coroutine.errorAndFail("still in progress"));
                    return Co.SetState(
                    CustomFields.Updaters.Coroutine.transition(CustomFieldsEvent.typeCheckingCompleted(res.value.value)))
                })}),
                Co.GetState().then((state: CustomFields) => {
                    const typeCheckingCompleted = CustomFields.Updaters.Coroutine.isTypeCheckingCompleted(state);
                    if(typeCheckingCompleted.kind == "l") return Co.SetState(_ => state);
                    
                    return Co.Seq([
                        Co.SetState(
                            CustomFields.Updaters.Coroutine.transition(
                                CustomFieldsEvent.beginConstruction()).then(
                                CustomFields.Updaters.Coroutine.transition(CustomFieldsEvent.startConstruction(typeCheckingCompleted.value))
                            )),
                        Co.Await<ValueOrErrors<Guid, any>, any>(() =>
                            constructionJob({ ValueDescriptorId: typeCheckingCompleted.value }), (_err: any) => {}).then(res => {
                            const response = CustomFields.Operations.checkResponseForErrors<Guid>(res, 'construction job 2')
                            if(response.kind == "r") return response.value

                            return Co.SetState(
                                CustomFields.Updaters.Coroutine.transition(
                                    CustomFieldsEvent.initConstruction(res.value.value)))
                        }),
                        Co.Wait(5000),
                        Co.GetState().then((state: CustomFields) => {
                            const constructionDispatched = CustomFields.Updaters.Coroutine.isConstructionDispatched(state);
                            if(constructionDispatched.kind == "l") return Co.SetState(_ => state);
    
                            return Co.Await<ValueOrErrors<ConstructionJobResponse, any>, any>(() =>
                                getConstructionJobStatus(constructionDispatched.value), (_err: any) => {}).then(res => {
                                const response = CustomFields.Operations.checkResponseForErrors<ConstructionJobResponse>(res, 'construction status')
                                if(response.kind == "r") return response.value
                                debugger
                                if(res.value.value.status == 3) return Co.SetState(CustomFields.Updaters.Coroutine.errorAndFail(res.value.value.error.message));
                                if(res.value.value.status == 1) return Co.SetState(CustomFields.Updaters.Coroutine.errorAndFail("still in progress"));
                                return Co.SetState(
                                    CustomFields.Updaters.Coroutine.transition(
                                        CustomFieldsEvent.completeConstruction(res.value.value)))
                            })}),
                        Co.GetState().then((state: CustomFields) => {
                            const constructionCompleted = CustomFields.Updaters.Coroutine.isConstructionCompleted(state);
                            if(constructionCompleted.kind == "l") return Co.SetState(_ => state);
                            return Co.Seq([
                                Co.SetState(
                                    CustomFields.Updaters.Coroutine.transition(
                                        {
                                            kind: 'begin-value', valueId: constructionCompleted.value ,
                                        }).then(CustomFields.Updaters.Core.value(replaceWith(Option.Default.some(constructionCompleted.value)))).then(CustomFields.Updaters.Coroutine.transition(
                                        CustomFieldsEvent.requestValue(constructionCompleted.value)))),
                                Co.Await<ValueOrErrors<RequestValueJobResponse, any>, any>(() =>
                                    getValue(constructionCompleted.value), (_err: any) => {}).then(res => {
                                    const response = CustomFields.Operations.checkResponseForErrors<RequestValueJobResponse>(res, 'request value')
                                    if(response.kind == "r") return response.value
                                    if(res.value.value.status == 3) return Co.SetState(CustomFields.Updaters.Coroutine.errorAndFail(res.value.value.error.message));
                                    if(res.value.value.status == 1) return Co.SetState(CustomFields.Updaters.Coroutine.errorAndFail("still in progress"));
                                    debugger
                                    return Co.SetState(
                                        CustomFields.Updaters.Coroutine.transition(
                                            CustomFieldsEvent.valueCompleted(res.value.value))
                                            .then(CustomFields.Updaters.Coroutine.transition(
                                        CustomFieldsEvent.conclude(res.value))))
                                }),
                            ]) }),
                    ]) }),
            ])})
    );