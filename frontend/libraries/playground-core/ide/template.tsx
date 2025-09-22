import {Template, Option} from "ballerina-core";
import {Ide, IdeForeignMutationsExpected, IdeReadonlyContext, IdeView, IdeWritableState} from "./state";
import {JsonEditorTemplate} from "./domains/editor/template";
import {Bootstrap} from "./coroutines/runner";

/*
*  TODO: split on templates will be done after initial usage as UX design changes every day, 
*  for now there is a split on Layout and JsonEditor but with the same state shared
* 
* */

export const JsonEditorEmbedded = JsonEditorTemplate

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
