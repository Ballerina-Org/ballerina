import type * as monaco from "monaco-editor";

export function registerBallerina(monacoInstance: typeof monaco) {
    monacoInstance.languages.register({
        id: "ballerina",
        extensions: [".fs", ".fsi", ".fsx"],
        aliases: ["F#", "fsharp", "FSharp"],
    });
}

export function configureBallerina(monacoInstance: typeof monaco) {
    monacoInstance.languages.setLanguageConfiguration("ballerina", {
        comments: {
            lineComment: "//",
            blockComment: ["/*", "*/"]
        },
        brackets: [
            ["{", "}"],
            ["[", "]"],
            ["(", ")"]
        ],
        autoClosingPairs: [
            {open: "{", close: "}"},
            {open: "[", close: "]"},
            {open: "(", close: ")"},
            {open: "\"", close: "\""}
        ]
    });
}
export function setMonarchForBallerina(monacoInstance: typeof monaco) {
    const ballerinaLanguage: monaco.languages.IMonarchLanguage = {
        defaultToken: "invalid",

        controlKeywords: ["match", "with"],
        functionKeywords: ["fun"],
        letInKeywords: ["let", "in"],

        keywords: [
            "function", "let", "in", "Value",
            "type", "module", "open", "namespace",
            "if", "then", "else", "elif",
            "try", "finally", "do", "yield", "return",
            "member", "override", "abstract",
            "mutable"
        ],

        typeKeywords: ["Some", "None", "Value", "Error", "Ok"],

        operators: [
            "->", "|", "::", "=", ">", "<", "<=", ">=", "<>",
            "+", "-", "*", "/", "%", "&&", "||"
        ],

        symbols: /[=><!~?:&|+\-*\/\^%]+/,
        escapes: /\\(?:[abfnrtv\\"']|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4})/,

        tokenizer: {
            root: [
                //
                // HKT binders
                // [f:*->*]
                //
                [
                    /(\[)([a-zA-Z_]\w*)(\s*:\s*)(\*)(\s*->\s*)(\*)(\])/,
                    [
                        "meta.hkt.bracket",   // [
                        "meta.hkt.var",       // f
                        "meta.hkt.colon",     // :
                        "meta.hkt.universe",  // *
                        "meta.hkt.arrow",     // ->
                        "meta.hkt.universe",  // *
                        "meta.hkt.bracket"    // ]
                    ]
                ],

                //
                // [a:*] or [b:*] etc.
                //
                [
                    /(\[)([a-zA-Z_]\w*)(\s*:\s*)(\*)(\])/,
                    [
                        "meta.hkt.bracket",   // [
                        "meta.hkt.var",       // a / b / ...
                        "meta.hkt.colon",     // :
                        "meta.hkt.universe",  // *
                        "meta.hkt.bracket"    // ]
                    ]
                ],

                //
                // Your existing delimiters
                //
                [/[()]/, "delimiter.paren"],
                [/[\[\]]/, "delimiter.bracket"],
                
                
                [/(\|)(\s*)(Value)\b/, ["operator", "white", "keyword.matchValue"]],
                [/(\|)(\s*)(None)\b/, ["operator", "white", "keyword.matchNone"]],

                [
                    /[a-zA-Z_][\w']*/,
                    {
                        cases: {
                            "@controlKeywords": "keyword.control",
                            "@functionKeywords": "keyword.function",
                            "@letInKeywords": "keyword.letIn",
                            "@keywords": "keyword",
                            "@typeKeywords": "type",
                            "@default": "identifier"
                        }
                    }
                ],
                [/\[[a-zA-Z_]\s*:\s*\*+\s*->\s*\*+\]/, "type.hkt"],
                [/\d+(\.\d+)?([eE][\-+]?\d+)?[fFmM]?/, "number"],

                [/"/, { token: "string.quote", bracket: "@open", next: "@string" }],

                [/\/\/.*$/, "comment"],
                [/\/\*/, "comment", "@comment"],

                [
                    /@symbols/,
                    {
                        cases: {
                            "@operators": "operator",
                            "@default": ""
                        }
                    }
                ],

                // Parentheses
                [/[()]/, "delimiter.paren"],

// Brackets
                [/[\[\]]/, "delimiter.bracket"],
            ],

            string: [
                [/[^\\"]+/, "string"],
                [/@escapes/, "string.escape"],
                [/\\./, "string.escape.invalid"],
                [/"/, { token: "string.quote", bracket: "@close", next: "@pop" }]
            ],

            comment: [
                [/[^/*]+/, "comment"],
                
                [/\/\*/, "comment", "@push"],
                [/\*\//, "comment", "@pop"],
                [/./, "comment"]
            ]
        }
    };

    monacoInstance.languages.setMonarchTokensProvider(
        "ballerina",
        ballerinaLanguage
    );
}