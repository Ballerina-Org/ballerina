import {Co} from "./builder";
import {apiResultStatuses, Debounce, Synchronize, Synchronized, Unit} from "ballerina-core";
import {SpecEditor, ValidationResultWithPayload} from "../domains/spec-editor/state";
import {IDEApi} from "../apis/spec";
import {IDE} from "../state";

export const specNames = Co.Repeat(
  Co.Seq([
      Co.Wait(5000),
      Synchronize<Unit, ValidationResultWithPayload<string[]>>(
        IDEApi.names,
        (_: any) => (_ in apiResultStatuses ? _ : "permanent failure"),
        5,
        150,
      ).embed( x => x.specNames,  IDE.Updaters.Core.specNames)
  ])

);