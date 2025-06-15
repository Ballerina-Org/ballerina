import {RawJsonEditor, RawJsonEditorView} from "playground-core";
import {replaceWith} from "ballerina-core";
import React, {useEffect} from "react";

export const RawEditor: RawJsonEditorView = (props) => {
    function escapeHtml(str: string): string {
        const map: Record<string, string> = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#39;'
        };

        return str.replace(/[&<>"']/g, (c: string) => map[c] ?? c);
    }
    function highlightJSON(text: string): string {
        let escaped = escapeHtml(text);
        for (const { regex, className } of rules) {
            escaped = escaped.replace(regex, match => `<span class="${className}">${match}</span>`);
        }
        return escaped;
    }
    function update(highlighted: any,editor: any) {
        highlighted.innerHTML = highlightJSON(editor.value);
    }

    const rules = [
        { regex: /"(\\.|[^"\\])*"(?=\s*:)/g, className: "token-key" },
        { regex: /"(\\.|[^"\\])*"/g, className: "token-string" },
        { regex: /\b(true|false|null)\b/g, className: "token-literal" },
        { regex: /-?\d+(\.\d+)?([eE][+-]?\d+)?/g, className: "token-number" },
        { regex: /[{}\[\],:]/g, className: "token-punct" },

        { regex: /\b(args|fun)\b/g, className: "token-keyword" },
        { regex: /\b(types)\b/g, className: "token-keyword2" },
        { regex: /\b(fields|extends|caseName|Value)\b/g, className: "token-keyword3" }
    ];

    useEffect(() => {
        const editor = document.getElementById('editor') as HTMLTextAreaElement;
        const highlighted = document.getElementById('highlighted') as HTMLPreElement;
        editor.addEventListener('input', () => update(highlighted, editor));
        editor.addEventListener('scroll', () => {
            highlighted.scrollTop = editor.scrollTop;
            highlighted.scrollLeft = editor.scrollLeft;
        });

        update(highlighted, editor)
    }, []);
    
    return (
    <>
        <div className="editor-wrapper">
            <pre className="editor-highlighted" id="highlighted"></pre>
            <textarea
                spellCheck={false}
                className="editor-input"
                value={props.context.inputString.value}
                onChange={(e) => {
                    props.setState(RawJsonEditor.Updaters.Template.inputString(replaceWith({ value: e.target.value})))
                }}
                id="editor">
        </textarea>
        </div>
    </>
)};