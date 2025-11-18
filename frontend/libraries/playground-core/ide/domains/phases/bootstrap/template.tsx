import {Maybe, Template} from "ballerina-core";
import {
    BootstrapPhase,
    BootstrapPhaseForeignMutationsExpected,
    BootstrapPhaseView
} from "./state";

export const BootstrapPhaseTemplate = Template.Default<
    Maybe<BootstrapPhase>,
    Maybe<BootstrapPhase>,
    BootstrapPhaseForeignMutationsExpected,
    BootstrapPhaseView
>((props) =>
    <props.view
        {...props}
    />).any([]);
