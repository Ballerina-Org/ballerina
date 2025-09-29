import * as React from "react";
import Editor, { OnMount } from "@monaco-editor/react";
import type * as monaco from "monaco-editor";
import type * as monacoT from "monaco-editor";
import {BasicUpdater} from "ballerina-core";
import {VirtualFolders} from "playground-core";

type VocabJsonEditorProps = {
    value?: string;
    onChange?: (code: string) => void;
    vocab?: string[];
};


export default function MonacoEditor( props: {content: string, onChange: BasicUpdater<any>}) {
    const editorRef = React.useRef<monaco.editor.IStandaloneCodeEditor | null>(null);
    const initialFormatted = React.useMemo(() => {
        try {
            const parsed = typeof props.content === "string"
                ? JSON.parse(props.content)
                : props.content;
            return JSON.stringify(parsed, null, 2);
        } catch {
            return "{}";
        }
    }, []);

    const handleEditorChange = React.useCallback((value: string | undefined) => {
        if (!value) return;

        try {
            const parsed = JSON.parse(value);
            props.onChange(VirtualFolders.Updaters.Template.selectedFileContent(parsed));
        } catch {
        }
    }, [props.onChange]);
    const handleMount: OnMount = (editor, monaco) => {
        editorRef.current = editor;
        
        editor.updateOptions({
            tabSize: 2,
            insertSpaces: true,
            formatOnPaste: true,
            formatOnType: true,
            wordWrap: "on",
            minimap: { enabled: false },
            scrollBeyondLastLine: false,
        });

        monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
            validate: true,
            allowComments: true,
            schemas: [
                {
                    uri: "inmemory://schema/known-sections.json",
                    fileMatch: ["*"], 
                    schema: {
                        type: "object",
                        additionalProperties: false,
                        properties: {
                            types: { type: "object" },
                            forms: { type: "object" },
                            apis: { type: "object" },
                            launchers: { type: "object" },
                            typesV2: { type: "object" },
                            schema: { type: "object" },
                        }
                    }
                }
            ],
        });

    };

    const format = () => {
        const ed = editorRef.current;
        if (!ed) return;
        ed.getAction("editor.action.formatDocument")?.run();
    };

    return (
        <div className="h-[90vh] flex flex-col gap-2 ">
            <div className="flex gap-2 ml-5">
                <button className="btn btn-sm btn-info" onClick={format}>Format JSON</button>
            </div>

            <Editor
                height="100%"
                defaultLanguage="json"
                theme="vs-light"

                onChange={handleEditorChange
                }
                defaultValue={initialFormatted}
                onMount={handleMount}
            />
        </div>
    );
}
