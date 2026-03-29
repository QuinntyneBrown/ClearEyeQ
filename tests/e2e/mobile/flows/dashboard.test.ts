import { device } from 'detox';
import { loginAsTestUser } from '../helpers/auth';
import DashboardPage from '../pages/DashboardPage';
import ScanPage from '../pages/ScanPage';
import TabBar from '../pages/TabBar';

describe('Dashboard Flow', () => {
  beforeEach(async () => {
    await device.reloadReactNative();
    await loginAsTestUser();
  });

  it('should display the dashboard after login', async () => {
    await DashboardPage.isVisible();
  });

  it('should display the health score card with a score value', async () => {
    await DashboardPage.isVisible();
    await DashboardPage.hasHealthScore('78');
  });

  it('should display environment cards for AQI, Pollen, Humidity, and Screen Time', async () => {
    await DashboardPage.isVisible();
    await DashboardPage.hasEnvironmentCard('aqi');
    await DashboardPage.hasEnvironmentCard('pollen');
    await DashboardPage.hasEnvironmentCard('humidity');
    await DashboardPage.hasEnvironmentCard('screen');
  });

  it('should dismiss the daily tip banner when tapping dismiss', async () => {
    await DashboardPage.isVisible();
    await DashboardPage.dailyTipIsVisible();
    await DashboardPage.dismissTip();
    await DashboardPage.dailyTipIsNotVisible();
  });

  it('should navigate to scan screen when tapping Scan Now', async () => {
    await DashboardPage.isVisible();
    await DashboardPage.tapScanNow();
    await ScanPage.isVisible();
  });

  it('should return to dashboard with data after navigating back from scan', async () => {
    await DashboardPage.isVisible();
    await DashboardPage.tapScanNow();
    await ScanPage.isVisible();

    await TabBar.tapHome();
    await DashboardPage.isVisible();
    await DashboardPage.hasHealthScore('78');
  });
});
