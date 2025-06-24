import { apiResultStatuses } from "ballerina-core";
import { Synchronize } from "ballerina-core";
import { Synchronized } from "ballerina-core";
import { Debounce } from "ballerina-core";
import { Value } from "ballerina-core";
import { IDEApi } from "../../../apis/spec"
import { Co } from "./builder";
import {SpecEditor, ValidationResultWithPayload} from "../state";

export const specPreviewDebouncer = Co.Repeat(
  Debounce<Synchronized<Value<string>, ValidationResultWithPayload<string>>>(
    Synchronize<Value<string>, ValidationResultWithPayload<string>>(
      IDEApi.seed,
      (_: any) => (_ in apiResultStatuses ? _ : "permanent failure"),
      5,
      150,
    ),
    0,
    500,
  ).embed((parent) => parent.input, SpecEditor.Updaters.Core.input),
);
