import {Template, Option} from "ballerina-core";
import {Ide, IdeForeignMutationsExpected, IdeReadonlyContext, IdeView, IdeWritableState} from "./state";
import {JsonEditorTemplate} from "./domains/editor/template";
import {Bootstrap} from "./coroutines/runner";

export const JsonEditorEmbedded = JsonEditorTemplate.mapContext<
    IdeReadonlyContext & IdeWritableState
>((p) =>
    p.phase == "locked"
        ? Option.Default.some(p.locked.bridge)
        : Option.Default.none()
).mapState(Ide.Template.bridge.id)

export const IdeTemplate = Template.Default<
    IdeReadonlyContext,
    IdeWritableState,
    IdeForeignMutationsExpected,
    IdeView
>((props) =>
    <props.view
        {...props}
        JsonEditor={JsonEditorEmbedded}
    />
).any([Bootstrap]);
