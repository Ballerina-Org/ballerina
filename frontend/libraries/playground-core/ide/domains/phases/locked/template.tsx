import {Maybe, Template} from "ballerina-core";
import {LockedPhase, LockedPhaseForeignMutationsExpected, LockedPhaseView} from "./state";
import {
    WorkspaceForeignMutationsExpected,
    WorkspaceState,
    WorkspaceView
} from "./domains/folders/state";
import {WorkspaceTemplate} from "./domains/folders/template";

export const WorkspaceEmbedded: Template<Maybe<LockedPhase>, Maybe<LockedPhase>, WorkspaceForeignMutationsExpected, WorkspaceView> =
    WorkspaceTemplate
        .mapContext<Maybe<LockedPhase>>(p =>(p!.workspace))
        .mapState(WorkspaceState.Updater.maybeSelected);

export const LockedPhaseTemplate = Template.Default<
    Maybe<LockedPhase>,
    Maybe<LockedPhase>,
    LockedPhaseForeignMutationsExpected,
    LockedPhaseView
>((props) => 
    <props.view 
        Workspace={WorkspaceEmbedded}
        {...props} 
    />).any([]);
