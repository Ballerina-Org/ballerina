import {Co} from "./builder";
import {
    ValueOrErrors,
    replaceWith,Guid, Updater, BasicFun
} from "ballerina-core";

import {CustomEntity, CustomEntityStatus, Document, Job} from "../../state"
import { JobProcessing } from "../../domains/job/state"
import {RequestValueJobResponse, ResponseWithStatus} from "../../domains/job/response/state";
import {
    constructionJob,
    getJobStatus,
    getValue,
    typeCheckingJob,
    updaterJob
} from "../../../../../api/custom-fields/client";
import {AI_Value_Mock} from "../../domains/mock";
import {UpdaterJob} from "../../domains/job/request/state";
import {listDocuments} from "../../../../../api/documents";
import {List} from "immutable";

export const documents =

    Co.Await<ValueOrErrors<Document[], any>, any>(() =>
        listDocuments(), (_err: any) => {}).then(res => {
                        if(res.kind  == "r")
                            return Co.SetState(
                                CustomEntity.Updaters.Core.result(
                                    replaceWith(ValueOrErrors.Default.throw(List(["listDocuments failed."])))))
                        if(res.value.kind == "errors")
                            return Co.SetState(
                                CustomEntity.Updaters.Core.result(
                                    replaceWith(ValueOrErrors.Default.throw(res.value.errors))))
                        const docs = res.value.value;
                        console.log(JSON.stringify(res.value.value, null, 2))
                        return Co.SetState(CustomEntity.Updaters.Core.documents(d => ({
                            ...d,
                            available: docs
                        })))
                    })