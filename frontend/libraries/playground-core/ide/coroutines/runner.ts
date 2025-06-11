import { Debounced } from "ballerina-core";
import { IDEForeignMutationsExpected } from "../state";
import { Co } from "./builder";
import { specsSubscription } from "./subscribe-for-new-specs";

export const SpecsSubscriptionDebouncerRunner =
    Co.Template<IDEForeignMutationsExpected>( specsSubscription , {
        // runFilter: (props) =>
        //     Debounced.Operations.shouldCoroutineRun(props.contex),
    });
