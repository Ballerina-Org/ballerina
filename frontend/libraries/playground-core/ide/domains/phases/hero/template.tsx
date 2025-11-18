import {Maybe, Template} from "ballerina-core";
import {HeroPhase, HeroPhaseForeignMutationsExpected, HeroPhaseView} from "./state";

export const HeroPhaseTemplate = Template.Default<
    Maybe<HeroPhase>,
    Maybe<HeroPhase>,
    HeroPhaseForeignMutationsExpected,
    HeroPhaseView
>((props) =>
    <props.view
        {...props}
    />).any([]);