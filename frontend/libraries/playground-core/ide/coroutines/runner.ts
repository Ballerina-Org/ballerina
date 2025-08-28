import { Debounced, Unit } from "ballerina-core";
import { Co } from "./builder";
import {specNames} from "./specs-observer";
import {liveUpdatesCounter} from "./live-updates-counter";


export const SpecsObserver =
    Co.Template<Unit>(specNames, {
        runFilter: (props) => true,
    });


export const LiveUpdatesCounter =
    Co.Template<Unit>(liveUpdatesCounter, {
        runFilter: (props) => props.context.liveUpdates.kind == "r" && props.context.liveUpdates.value >= 0,
    });