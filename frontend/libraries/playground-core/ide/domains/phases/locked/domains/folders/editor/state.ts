import {ForeignMutationsInput, Option, replaceWith, SimpleCallback, simpleUpdater, View} from "ballerina-core";


export type JsonEditor = {
    //currentPosition: { lineNumber: number, column: number },
    content: Option<string>
}

export const JsonEditor = {
    Updaters: {
        Core: {
            //...simpleUpdater<JsonEditor>()("currentPosition"),
            ...simpleUpdater<JsonEditor>()("content"),
        }
    },
    ForeignMutations: (
        _: ForeignMutationsInput<JsonEditorReadonlyContext, JsonEditorWritableState>,
    ) => ({
        // setCurrentPosition: (line: number, column: number) =>
        //     _.setState(JsonEditor.Updaters.Core.currentPosition(replaceWith({ lineNumber: line, column: column}))),
        setContent: (content: string) =>
            _.setState(JsonEditor.Updaters.Core.content(replaceWith(Option.Default.some(content))
                )),
    }),
}

export type JsonEditorReadonlyContext = {};
export type JsonEditorWritableState = JsonEditor ;

export type JsonEditorForeignMutationsExpected = {
    onChange: SimpleCallback<string>
}
export type JsonEditorForeignMutationsExposed = ReturnType<
    typeof JsonEditor.ForeignMutations
>;
export type JsonEditorView = View<
    JsonEditorWritableState,
    JsonEditorWritableState,
    JsonEditorForeignMutationsExpected,
    {
    }
>;