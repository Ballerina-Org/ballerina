import { Debounced } from "ballerina-core";
import { IDEForeignMutationsExpected } from "../state";
import { Co } from "./builder";
import { specsSubscription } from "./subscribe-for-new-specs";
import {runEditor} from "./run-editor";

export const SpecsSubscriptionDebouncerRunner =
    Co.Template<IDEForeignMutationsExpected>( specsSubscription , {
        // runFilter: (props) =>
        //     Debounced.Operations.shouldCoroutineRun(props.contex),
    });

export const RunEditor =
    Co.Template<IDEForeignMutationsExpected>( runEditor , {
    });
