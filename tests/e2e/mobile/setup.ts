import { device } from 'detox';

beforeAll(async () => {
  await device.launchApp({
    newInstance: true,
    permissions: {
      camera: 'YES',
      notifications: 'YES',
    },
  });
});

afterAll(async () => {
  await device.terminateApp();
});
