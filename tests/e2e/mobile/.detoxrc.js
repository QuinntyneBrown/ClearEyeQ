/** @type {Detox.DetoxConfig} */
module.exports = {
  testRunner: {
    args: {
      $0: 'jest',
      config: './jest.config.ts',
    },
    jest: {
      setupTimeout: 120000,
    },
  },
  apps: {
    'ios.debug': {
      type: 'ios.app',
      binaryPath:
        '../../src/mobile/ios/build/Build/Products/Debug-iphonesimulator/ClearEyeQ.app',
      build:
        'cd ../../src/mobile && npx expo run:ios --configuration Debug',
    },
    'android.debug': {
      type: 'android.apk',
      binaryPath:
        '../../src/mobile/android/app/build/outputs/apk/debug/app-debug.apk',
      build:
        'cd ../../src/mobile && npx expo run:android --variant debug',
    },
  },
  devices: {
    simulator: {
      type: 'ios.simulator',
      device: {
        type: 'iPhone 16',
      },
    },
    emulator: {
      type: 'android.emulator',
      device: {
        avdName: 'Pixel_7',
      },
    },
  },
  configurations: {
    'ios.sim.debug': {
      device: 'simulator',
      app: 'ios.debug',
    },
    'android.emu.debug': {
      device: 'emulator',
      app: 'android.debug',
    },
  },
};
