module.exports = {
    preset: 'ts-jest/presets/default-esm',
    testEnvironment: 'node',
    extensionsToTreatAsEsm: ['.ts', '.tsx'],
    transform: {
        '^.+\\.ts$': 'ts-jest',
        '^.+\\.tsx$': 'ts-jest',
    },
    globals: {
        'ts-jest': {
            useESM: true,
            tsconfig: './tsconfig.json', // 👈 make sure it's pointing correctly
        },
    },
    moduleFileExtensions: ['ts', 'tsx', 'js', 'json'],
};
