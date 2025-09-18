import * as React from "react";
import Editor, { OnMount } from "@monaco-editor/react";
import type * as monaco from "monaco-editor";
import type * as monacoT from "monaco-editor";
import {BasicUpdater} from "ballerina-core";

type VocabJsonEditorProps = {
    value?: string;
    onChange?: (code: string) => void;
    vocab?: string[]; // keys you want highlighted as special
};
const rxEscape = (s: string) => s.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
export default function MonacoEditor( props: {content: string, onChange: BasicUpdater<any>}) {
    const editorRef = React.useRef<monaco.editor.IStandaloneCodeEditor | null>(null);

    const handleMount: OnMount = (editor, monaco) => {
        editorRef.current = editor;

        // Nice defaults
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
                    fileMatch: ["*"], // apply to this model
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
        <div className="h-[70vh] flex flex-col gap-2">
            <div className="flex gap-2 ml-5">
                <button className="btn btn-sm btn-info" onClick={format}>Format JSON</button>
            </div>

            <Editor
                height="100%"
                defaultLanguage="json"
                theme="vs-light"
                onChange={(val) => props.onChange?.(val ?? "")}
                defaultValue={props.content}
                onMount={handleMount}
            />
        </div>
    );
}
