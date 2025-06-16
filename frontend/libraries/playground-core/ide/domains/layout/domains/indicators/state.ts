import {simpleUpdater} from "ballerina-core";

export type EditorStep =
    | { kind : "loading-spec-body" }
    | { kind : "editing" }
    | { kind : "validating" }
    | { kind : "parsing" }
    | { kind : "running" }
    | { kind : "output" };

export const EditorStep = {
    loadingSpecBody: (): EditorStep => ({ kind: "loading-spec-body" }),
    editing: (): EditorStep => ({ kind: "editing" }),
    validating: (): EditorStep => ({ kind: "validating" }),
    parsing: (): EditorStep => ({ kind: "parsing" }),
    running: (): EditorStep => ({ kind: "running" }),
    output: (): EditorStep => ({ kind: "output" }),
}

export type LayoutIndicators = {
    step: EditorStep;
}

const CoreUpdaters = {
    ...simpleUpdater<LayoutIndicators>()("step"),
}

export const LayoutIndicators = {
    Default: (): LayoutIndicators => ({
        step: EditorStep.loadingSpecBody(),
    }),
    Updaters: CoreUpdaters
}