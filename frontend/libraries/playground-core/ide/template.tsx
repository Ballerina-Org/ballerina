import {Template} from "ballerina-core";
import {IDEForeignMutationsExpected, IDEReadonlyContext, IDEView, IDEWritableState} from "./state";
import {SpecsSubscriptionDebouncerRunner} from "./coroutines/runner";

export const IDETemplate = Template.Default<
    IDEReadonlyContext,
    IDEWritableState,
    IDEForeignMutationsExpected,
    IDEView
>((props) => <props.view {...props} />).any([
    SpecsSubscriptionDebouncerRunner.mapContext((_) => ({ ..._, events: [] })),
]);