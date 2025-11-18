import {Template, Maybe} from "ballerina-core";
import {
    IdeForeignMutationsExpected,
    IdeReadonlyContext,
    IdeView,
    IdeWritableState,
    Ide
} from "./state";
import {Bootstrap} from "./coroutines/runner";
import {LockedPhaseTemplate} from "./domains/phases/locked/template";
import {LockedPhase, LockedPhaseForeignMutationsExpected, LockedPhaseView} from "./domains/phases/locked/state";
import {HeroPhase, HeroPhaseForeignMutationsExpected, HeroPhaseView} from "./domains/phases/hero/state";
import {HeroPhaseTemplate} from "./domains/phases/hero/template";
import {BootstrapPhase, BootstrapPhaseForeignMutationsExpected, BootstrapPhaseView} from "./domains/phases/bootstrap/state";
import { SelectionPhase, SelectionPhaseForeignMutationsExpected, SelectionPhaseView } from "./domains/phases/selection/state";
import {SelectionPhaseTemplate} from "./domains/phases/selection/template";
import {BootstrapPhaseTemplate} from "./domains/phases/bootstrap/template";


/*

Important: Hero/Bootstrap/Selecting/Locked embeddings are currently not used 
in favour of phases tags processed with caseUpdaters

*/

export const LockedPhaseEmbedded: Template<Ide, Ide, LockedPhaseForeignMutationsExpected, LockedPhaseView> =
    LockedPhaseTemplate
        .mapContext<Ide>(p =>(p.phase.kind == 'locked' ? p.phase.locked: undefined) as Maybe<LockedPhase>)
        .mapState(Ide.Updaters.Core.phase.maybeLocked);

export const HeroPhaseEmbedded: Template<Ide, Ide, HeroPhaseForeignMutationsExpected, HeroPhaseView> =
    HeroPhaseTemplate
        .mapContext<Ide>(p =>(p.phase.kind == 'hero' ? p.phase.hero: undefined) as Maybe<HeroPhase>)
        .mapState(Ide.Updaters.Core.phase.maybeHero);

export const BootstrapPhaseEmbedded: Template<Ide, Ide, BootstrapPhaseForeignMutationsExpected, BootstrapPhaseView> =
    BootstrapPhaseTemplate
        .mapContext<Ide>(p =>(p.phase.kind == 'bootstrap' ? p.phase.bootstrap: undefined) as Maybe<BootstrapPhase>)
        .mapState(Ide.Updaters.Core.phase.maybeBootstrap);

export const SelectionPhaseEmbedded: Template<Ide, Ide, SelectionPhaseForeignMutationsExpected, SelectionPhaseView> =
    SelectionPhaseTemplate
        .mapContext<Ide>(p =>(p.phase.kind == 'selection' ? p.phase.selection: undefined) as Maybe<SelectionPhase>)
        .mapState(Ide.Updaters.Core.phase.maybeSelection);

export const IdeTemplate = Template.Default<
    IdeReadonlyContext,
    IdeWritableState,
    IdeForeignMutationsExpected,
    IdeView
>((props) =>
    <props.view
        {...props}
        LockedPhase={LockedPhaseEmbedded}
        HeroPhase={HeroPhaseEmbedded}
        BootstrapPhase={BootstrapPhaseEmbedded}
        SelectionPhase={SelectionPhaseEmbedded}
    />
).any([Bootstrap]);
