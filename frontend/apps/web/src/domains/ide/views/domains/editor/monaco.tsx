import * as React from "react";
import Editor, { OnMount } from "@monaco-editor/react";
import type * as monaco from "monaco-editor";
import {BasicUpdater, ValueOrErrors} from "ballerina-core";
import { LockedPhase, WorkspaceState,  Ide} from "playground-core";
import {configureBallerina, registerBallerina, setMonarchForBallerina} from "./language.ts";
import {List} from "immutable";
import {cleanString, unquote} from "playground-core/ide/domains/phases/custom-fields/domains/data-provider/state.ts";

export type SupportedLanguage = 'json' | 'ballerina' | 'fsharp'

const getExtension = (fileName: string): string =>
    fileName.split(".").pop() ?? "";

const extToLang: Record<string, SupportedLanguage> = {
    json: "json",
    fs: "fsharp",
    bl: "ballerina",
};

const getLanguage = (fileName: string): ValueOrErrors<SupportedLanguage, string> => {
    const ext = getExtension(fileName);
    return ext in extToLang
        ? ValueOrErrors.Default.return(extToLang[ext])
        : ValueOrErrors.Default.throw(
            List([`Unsupported fileName extension for monaco editor: ${ext}`])
        );
};

export default function MonacoEditor(props: {
    content: string;
    fileName: string;
    onChange: BasicUpdater<any>;
}) {
    const editorRef =
        React.useRef<monaco.editor.IStandaloneCodeEditor | null>(null);
    
    const [errors, setErrors] = React.useState<List<string>>(List());
    const [language, setLanguage] = React.useState<SupportedLanguage>("json");
    
    React.useEffect(() => {
        const lang = getLanguage(props.fileName);

        if (lang.kind === "errors") {
            setErrors(lang.errors);
            return;
        }

        setErrors(List());
        setLanguage(lang.value);
    }, [props.fileName]);

    const lastContentRef = React.useRef<string>(props.content);

    const initialFormatted = React.useMemo(() => {
        if (language !== "json") return props.content;

        // Only pretty-format when content actually changes
        if (props.content === lastContentRef.current) {
            return editorRef.current?.getValue() ?? props.content;
        }

        lastContentRef.current = props.content;

        try {
            return JSON.stringify(JSON.parse(props.content), null, 2);
        } catch {
            return props.content;
        }
    }, [props.content, language]);
    const handleEditorChange = React.useCallback(
        (value: string | undefined) => {
            if (!value) return;
            if (language === "json") {
                try {
                    const parsed = JSON.parse(value);
                    props.onChange(
                        Ide.Updaters.Core.phase.locked(
                            LockedPhase.Updaters.Core.workspace(
                                WorkspaceState.Updater.changeFileContent(value)
                            )
                        )
                    );
                } catch {
                    /* ignore invalid JSON */
                }
            } else {
                props.onChange(
                    Ide.Updaters.Core.phase.locked(
                        LockedPhase.Updaters.Core.workspace(
                            WorkspaceState.Updater.changeFileContent(value)
                        )
                    )
                );
            }
        },
        [language, props.onChange]
    );


    const handleMount: OnMount = (editor, monacoInstance) => {
        editorRef.current = editor;

        editor.updateOptions({
            tabSize: 2,
            insertSpaces: true,
            formatOnPaste: true,
            formatOnType: true,
            wordWrap: "on",
            minimap: { enabled: false },
            scrollBeyondLastLine: false
        });


        monacoInstance.languages.json.jsonDefaults.setDiagnosticsOptions({
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
                            schema: { type: "object" }
                        }
                    }
                }
            ]
        });

        //
        // Register Ballerina language
        //
        monacoInstance.editor.defineTheme(
            "ballerina-red",
            {
                base: "vs",
                inherit: false,
                colors: {},

                rules: [
                    { token: "keyword.control", foreground: "FFD700", fontStyle: "bold" },
                    { token: "keyword", foreground: "0000cc" },

                    // 🔴 Operators (if you want pipes/arrows red)
                    { token: "operator", foreground: "c72e2e" },

                    // 🟤 DU cases (Some, None, Value)
                    { token: "type", foreground: "a31515" },

                    // other optional rules
                    { token: "comment", foreground: "008000" },
                    { token: "string", foreground: "a31515" },
                    { token: "number", foreground: "098658" }
                ],

                // Monaco supports this at runtime; TS does not
                /** @ts-ignore */
                semanticHighlighting: false
            } as monaco.editor.IStandaloneThemeData
        );
        registerBallerina(monacoInstance);
        setMonarchForBallerina(monacoInstance);
        configureBallerina(monacoInstance);
        format()
    };

    const format = () => {
        if (language !== "json") return;
        editorRef.current?.getAction("editor.action.formatDocument")?.run();
    };

    return (
        <div className="h-screen flex flex-col gap-2">
            <div className="flex gap-2 ml-5">
                <button className="btn btn-sm btn-info" onClick={format}>
                    Format JSON
                </button>
            </div>

            <Editor
                height="100%"
                theme="ballerina-red" //"vs-light"
                language={language} 
                value={initialFormatted}
                onMount={handleMount}
                onChange={handleEditorChange}
            />
        </div>
    );
}