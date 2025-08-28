import {Template} from "ballerina-core";
import {Ide, IdeForeignMutationsExpected, IdeReadonlyContext, IdeView, IdeWritableState} from "./state";
import {JsonEditorTemplate} from "./domains/editor/template";
import {LiveUpdatesCounter, SpecsObserver} from "./coroutines/runner";

export const ChildTemplateEmbedded = JsonEditorTemplate.mapContext<
    IdeReadonlyContext & IdeWritableState
>((p) => p.bridge).mapState(Ide.Updaters.Core.bridge)

export const IdeTemplate = Template.Default<
    IdeReadonlyContext,
    IdeWritableState,
    IdeForeignMutationsExpected,
    IdeView
>((props) =>
    <props.view
        {...props}
        JsonEditor={ChildTemplateEmbedded}
    />
).any([SpecsObserver,LiveUpdatesCounter]);
