import {simpleUpdater} from "ballerina-core";


export type LayoutIndicators = {
    //step: EditorStep;
}

const CoreUpdaters = {
    //...simpleUpdater<LayoutIndicators>()("step"),
}

export const LayoutIndicators = {
    Default: (): LayoutIndicators => ({
        //step: EditorStep.loadingSpecBody(),
    }),
    Updaters: CoreUpdaters
}