import {LayoutActions} from "./domains/actions/state";
import {SpecEditor} from "../spec-editor/state";
import {LayoutIndicators} from "./domains/indicators/state";

export type Layout = {
    actions: LayoutActions,
    tabs: string [],
    indicators: LayoutIndicators
};

export const Layout = {
    Default: () => {
        return {
            actions: LayoutActions.Default(),
            tabs: [ "tab1", "tab2", "tab3" ],
            indicators: LayoutIndicators.Default()
        };
    },
}
