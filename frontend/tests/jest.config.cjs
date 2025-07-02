module.exports = {
    transform: {
        "^.+\\.tsx?$": ["ts-jest", {
            tsconfig: "tsconfig.json", // or your specific tsconfig path
            diagnostics: true
        }]
    },
    testEnvironment: "node",
    moduleFileExtensions: ["ts", "tsx", "js", "json"]
};