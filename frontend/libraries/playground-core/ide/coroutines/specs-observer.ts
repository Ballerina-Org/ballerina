import {Co} from "./builder";
import {
  replaceWith,
  Value,
} from "ballerina-core";
import {IDEApi} from "../apis/spec";
import {IDE} from "../state";


export const specNames = 
  Co.Seq([
      Co.Await(() =>
        IDEApi.names(), err => {}).then(res =>
        Co.SetState(
          IDE.Updaters.Core.specNames(replaceWith(res.kind == "l"? res.value.payload : [])))),
  ]
  );