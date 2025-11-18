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

        //
        // ONLY match + with should be red → classify as keyword.control
        //
        controlKeywords: ["match", "with"],

        //
        // Normal keywords (blue)
        //
        keywords: [
            "fun", "function", "let", "in",
            "type", "module", "open", "namespace",
            "if", "then", "else", "elif",
            "try", "finally", "do", "yield", "return",
            "member", "override", "abstract",
            "mutable"
        ],

        //
        // DU case names (brown/red)
        //
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
                // THIS IS WHERE IDENTIFIER & KEYWORD COLORING HAPPENS
                //
                [
                    /[a-zA-Z_][\w']*/,
                    {
                        cases: {
                            "@controlKeywords": "keyword.control",  // match, with
                            "@keywords": "keyword",                 // fun, let, in, type...
                            "@typeKeywords": "type",                // Some/None/Value
                            "@default": "identifier"
                        }
                    }
                ],

                // numbers
                [/\d+(\.\d+)?([eE][\-+]?\d+)?[fFmM]?/, "number"],

                // strings
                [/"/, { token: "string.quote", bracket: "@open", next: "@string" }],

                // comments
                [/\/\/.*$/, "comment"],
                [/\/\*/, "comment", "@comment"],

                // operators
                [
                    /@symbols/,
                    {
                        cases: {
                            "@operators": "operator",
                            "@default": ""
                        }
                    }
                ],

                // parentheses
                [/[()]/, "delimiter.parenthesis"]
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