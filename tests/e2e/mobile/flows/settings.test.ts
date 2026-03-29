import { device } from 'detox';
import { loginAsTestUser } from '../helpers/auth';
import SettingsPage from '../pages/SettingsPage';
import TabBar from '../pages/TabBar';

describe('Settings Flow', () => {
  beforeEach(async () => {
    await device.reloadReactNative();
    await loginAsTestUser();
    await TabBar.tapMore();
  });

  it('should display the settings screen', async () => {
    await SettingsPage.isVisible();
  });

  it('should show the profile card with user name and email', async () => {
    await SettingsPage.isVisible();
    await SettingsPage.profileIsVisible();
  });

  it('should show the connected apps list with app statuses', async () => {
    await SettingsPage.isVisible();
    await SettingsPage.hasConnectedApp('Apple Health', true);
  });

  it('should toggle passive monitoring off', async () => {
    await SettingsPage.isVisible();
    await SettingsPage.togglePassiveMonitoring();
  });

  it('should show subscription section with current plan', async () => {
    await SettingsPage.isVisible();
    await SettingsPage.hasCurrentPlan('free');
  });

  it('should display the delete account button', async () => {
    await SettingsPage.isVisible();
    await SettingsPage.deleteAccountButtonIsVisible();
  });
});
