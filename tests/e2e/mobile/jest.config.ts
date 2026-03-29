import type { Config } from 'jest';

const config: Config = {
  preset: 'ts-jest',
  testEnvironment: 'node',
  testTimeout: 120000,
  testMatch: ['<rootDir>/flows/**/*.test.ts'],
  setupFilesAfterSetup: ['<rootDir>/setup.ts'],
  verbose: true,
  bail: 0,
  maxWorkers: 1,
  globals: {
    'ts-jest': {
      tsconfig: '<rootDir>/tsconfig.json',
    },
  },
};

export default config;
