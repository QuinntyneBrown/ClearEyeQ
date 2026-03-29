import { device } from 'detox';
import { loginAsTestUser } from '../helpers/auth';
import DashboardPage from '../pages/DashboardPage';
import NotificationsPage from '../pages/NotificationsPage';

describe('Notifications Flow', () => {
  beforeEach(async () => {
    await device.reloadReactNative();
    await loginAsTestUser();

    // Navigate to notifications from dashboard
    await DashboardPage.isVisible();
    await DashboardPage.tapNotifications();
  });

  it('should display the notifications screen', async () => {
    await NotificationsPage.isVisible();
  });

  it('should show 72-hour forecast cards for 3 days', async () => {
    await NotificationsPage.isVisible();
    await NotificationsPage.hasForecastDays(3);
  });

  it('should display the urgent alert card', async () => {
    await NotificationsPage.isVisible();
    await NotificationsPage.urgentAlertIsVisible();
  });

  it('should display the treatment reminder', async () => {
    await NotificationsPage.isVisible();
    await NotificationsPage.treatmentReminderIsVisible();
  });

  it('should display the flare-up warning', async () => {
    await NotificationsPage.isVisible();
    await NotificationsPage.flareUpWarningIsVisible();
  });
});
