/** @type {import('jest').Config} */
module.exports = {
  preset: "ts-jest",
  testEnvironment: "node",
  testMatch: ["<rootDir>/tests/**/*.test.ts"],
  cacheDirectory: "/app/.cache/jest",
  moduleNameMapper: {
    "^src/(.*)$": "<rootDir>/../../libraries/ballerina-core/src/$1",
    "^ballerina-core$": "<rootDir>/../../libraries/ballerina-core/main.ts",
    "^ballerina-core/(.*)$": "<rootDir>/../../libraries/ballerina-core/$1",
  },
  transform: {
    "^.+\\.tsx?$": [
      "ts-jest",
      {
        tsconfig: "<rootDir>/tsconfig.json",
        diagnostics: false,
      },
    ],
  },
};
