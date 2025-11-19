import {Updater, caseUpdater, simpleUpdater, Maybe, BasicUpdater, replaceWith} from "ballerina-core";
import { Template, View } from "ballerina-core";
import { ForeignMutationsInput } from "ballerina-core";
import {
    BootstrapPhase,
    BootstrapPhaseForeignMutationsExpected,
    BootstrapPhaseView
} from "./domains/phases/bootstrap/state";
import {LockedPhase, LockedPhaseForeignMutationsExpected, LockedPhaseView} from "./domains/phases/locked/state";
import {
    SelectionPhase,
    SelectionPhaseForeignMutationsExpected,
    SelectionPhaseView, Spec
} from "./domains/phases/selection/state";
import {Node} from "./domains/phases/locked/domains/folders/node"
import {WorkspaceVariant} from "./domains/phases/locked/domains/folders/state";
import {HeroPhase, HeroPhaseForeignMutationsExpected, HeroPhaseView} from "./domains/phases/hero/state";
import {CustomFields} from "./domains/phases/custom-fields/state";

export type IdePhase =
    |  { kind: 'hero',              hero:      HeroPhase }
    |  { kind: 'bootstrap',         bootstrap: BootstrapPhase }
    |  { kind: 'selection',         selection: SelectionPhase }
    |  { kind: 'locked',            locked:    LockedPhase }

export type Ide = {
    phase: IdePhase;
};

export const Ide = {
    Default: (): Ide =>
        ({
            phase: { kind: 'hero', hero: HeroPhase.Default() }
        }),
    Updaters: {
        Core: {
            phase: {
                ...caseUpdater<Ide>()("phase")("bootstrap"),
                ...caseUpdater<Ide>()("phase")("locked"),
                ...caseUpdater<Ide>()("phase")("hero"),
                ...caseUpdater<Ide>()("phase")("selection"),
                maybeLocked: (u:BasicUpdater<Maybe<LockedPhase>>): Updater<Ide> =>
                    Updater(ide => {
                        if(ide.phase.kind != 'locked') return ide;
                        return ({...ide, phase: {...ide.phase,  locked: u(Maybe.Default(ide.phase.locked))!}} satisfies Ide)}),
                maybeHero: (u:BasicUpdater<Maybe<HeroPhase>>): Updater<Ide> =>
                    Updater(ide => {
                        if(ide.phase.kind != 'hero') return ide;
                        return ({...ide, phase: {...ide.phase, hero: u(Maybe.Default(ide.phase.hero))!}} satisfies Ide)}),
                maybeBootstrap: (u:BasicUpdater<Maybe<BootstrapPhase>>): Updater<Ide> =>
                    Updater(ide => {
                        if(ide.phase.kind != 'bootstrap') return ide;
                        return ({...ide, phase: {...ide.phase,  bootstrap: u(Maybe.Default(ide.phase.bootstrap))!}} satisfies Ide)}),
                maybeSelection: (u:BasicUpdater<Maybe<SelectionPhase>>): Updater<Ide> =>
                    Updater(ide => {
                        if(ide.phase.kind != 'selection') return ide;
                        return ({...ide, phase: {...ide.phase,  selection: u(Maybe.Default(ide.phase.selection))!}} satisfies Ide)}),
                toBootstrap: (variant: WorkspaceVariant): Updater<Ide> => 
                    Updater(ide =>
                    ({
                        phase: { kind: 'bootstrap', bootstrap: BootstrapPhase.Default(variant) },
                    } satisfies Ide)),
                toChoosePhase: (variant: WorkspaceVariant, specs: Spec []): Updater<Ide> => Updater(ide =>
                    ({
                        phase: { kind: 'selection', selection: SelectionPhase.Default(specs, variant) }})),
                toLocked: (name: string, variant: WorkspaceVariant, node: Node): Updater<Ide> => Updater(ide => 
                    ({
                        phase:  { 
                            kind: 'locked',
                            locked: LockedPhase.Default(name, variant, node)
                        }
                    } as Ide)),
            },
        }
    },

    ForeignMutations: (
        _: ForeignMutationsInput<IdeReadonlyContext, IdeWritableState>,
    ) => ({
    }),
};


export type IdeReadonlyContext = {};
export type IdeWritableState = Ide;

export type IdeForeignMutationsExpected = {} 

export type IdeView = View<
    IdeReadonlyContext & IdeWritableState,
    IdeWritableState,
    IdeForeignMutationsExpected,
    {
        LockedPhase: Template<
            Ide,
            Ide,
            LockedPhaseForeignMutationsExpected,
            LockedPhaseView
        >,
        HeroPhase: Template<
            Ide,
            Ide,
            HeroPhaseForeignMutationsExpected,
            HeroPhaseView
        >,
        SelectionPhase: Template<
            Ide,
            Ide,
            SelectionPhaseForeignMutationsExpected,
            SelectionPhaseView
        >,
        BootstrapPhase: Template<
            Ide,
            Ide,
            BootstrapPhaseForeignMutationsExpected,
            BootstrapPhaseView
        >;
    }
>;
