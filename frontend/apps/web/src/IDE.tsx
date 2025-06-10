import { useEffect, useState } from "react";

export const IDEApp = (props: {}) => {
    const [text, setText] = useState(`
{
  "name": "Alice",
  "age": 30,
  "active": true,
  "fun": "extend",
  "tags": ["json", "monarch", "test"],
  "nullValue": null
}`);
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
        { regex: /\b(extend)\b/g, className: "token-keyword2" }
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
        <div className="IDE">
            <div className="editor-wrapper">
                <pre className="editor-highlighted" id="highlighted"></pre>
                <textarea
                    className="editor-input"
                    value={text}
                    onChange={(e) => setText(e.target.value)}
                    id="editor">
                </textarea>
            </div>
        </div>)
}