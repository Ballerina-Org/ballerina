import { Debounced, Unit } from "ballerina-core";
import { Co } from "./builder";
import {specNames} from "./specs-observer";


export const SpecsObserver =
  Co.Template<Unit>(specNames, {
    runFilter: (props) => true,
  });
