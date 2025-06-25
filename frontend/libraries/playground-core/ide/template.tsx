import {Template} from "ballerina-core";
import {IDE, IDEForeignMutationsExpected, IDEReadonlyContext, IDEView, IDEWritableState} from "./state";
import {RawJsonEditorTemplate} from "./domains/spec-editor/template";
import {SpecsObserver} from "./coroutines/runner";
export const ChildTemplateEmbedded = RawJsonEditorTemplate.mapContext<
    IDEReadonlyContext & IDEWritableState
>((p) => p.editor).mapState(IDE.Updaters.Core.editor)

export const IDETemplate = Template.Default<
    IDEReadonlyContext,
    IDEWritableState,
    IDEForeignMutationsExpected,
    IDEView
>((props) =>        
        <props.view 
            {...props}
            RawJsonEditor={ChildTemplateEmbedded}
        />
  ).any([
 SpecsObserver
]);
