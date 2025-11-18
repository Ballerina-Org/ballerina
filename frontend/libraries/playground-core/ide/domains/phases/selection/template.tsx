import {Maybe, Template} from "ballerina-core";
import {

    SelectionPhase,
    SelectionPhaseForeignMutationsExpected,
    SelectionPhaseView
} from "./state";

export const SelectionPhaseTemplate = Template.Default<
    Maybe<SelectionPhase>,
    Maybe<SelectionPhase>,
    SelectionPhaseForeignMutationsExpected,
    SelectionPhaseView
>((props) =>
    <props.view
        {...props}
    />).any([]);
