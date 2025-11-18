import {Template, Option} from "ballerina-core";
import {WorkspaceForeignMutationsExpected, WorkspaceState, WorkspaceView} from "./state";
import {JsonEditorTemplate} from "./editor/template";
import {JsonEditor, JsonEditorForeignMutationsExpected, JsonEditorView} from "./editor/state";



const JsonEditorEmbedded: Template<WorkspaceState, WorkspaceState, JsonEditorForeignMutationsExpected, JsonEditorView> = 
    JsonEditorTemplate
        .mapContext<WorkspaceState>((p: WorkspaceState) => {
            const n = p.kind == 'selected'
                ? { content: Option.Default.some(p.file.metadata.content as string)}
                : { content: Option.Default.none() }
            return n as JsonEditor;
        })
        .mapState(WorkspaceState.Updater.changeFileContent);

export const WorkspaceTemplate = Template.Default<
    WorkspaceState,
    WorkspaceState,
    WorkspaceForeignMutationsExpected,
    WorkspaceView
>((props) => <props.view
    JsonEditor={JsonEditorEmbedded}
    {...props} />).any([]);