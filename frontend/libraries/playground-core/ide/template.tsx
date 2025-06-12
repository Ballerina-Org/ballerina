import {Template} from "ballerina-core";
import {IDE, IDEForeignMutationsExpected, IDEReadonlyContext, IDEView, IDEWritableState} from "./state";
import {SpecsSubscriptionDebouncerRunner} from "./coroutines/runner";
import {RawJsonEditorTemplate} from "./domains/raw-json-editor/template";


export const ChildTemplateEmbedded = RawJsonEditorTemplate.mapContext<
    IDEReadonlyContext & IDEWritableState
>((p) => p.rawEditor).mapState(IDE.Updaters.Core.rawEditor);

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
    SpecsSubscriptionDebouncerRunner //.mapContext((_) => ({ ..._, events: [] })),
]);
