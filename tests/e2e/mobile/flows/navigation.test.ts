import { device } from 'detox';
import { loginAsTestUser } from '../helpers/auth';
import DashboardPage from '../pages/DashboardPage';
import ScanPage from '../pages/ScanPage';
import TimelinePage from '../pages/TimelinePage';
import TreatmentPage from '../pages/TreatmentPage';
import SettingsPage from '../pages/SettingsPage';
import TabBar from '../pages/TabBar';

describe('Tab Navigation Flow', () => {
  beforeEach(async () => {
    await device.reloadReactNative();
    await loginAsTestUser();
  });

  it('should start on the Home tab with the dashboard visible', async () => {
    await DashboardPage.isVisible();
    await TabBar.isActiveTab('home');
  });

  it('should navigate to the Scan tab', async () => {
    await TabBar.tapScan();
    await ScanPage.isVisible();
    await TabBar.isActiveTab('scan');
  });

  it('should navigate to the Trends tab', async () => {
    await TabBar.tapTrends();
    await TimelinePage.isVisible();
    await TabBar.isActiveTab('trends');
  });

  it('should navigate to the Treat tab', async () => {
    await TabBar.tapTreat();
    await TreatmentPage.isVisible();
    await TabBar.isActiveTab('treat');
  });

  it('should navigate to the More/Settings tab', async () => {
    await TabBar.tapMore();
    await SettingsPage.isVisible();
    await TabBar.isActiveTab('more');
  });

  it('should return to Home after navigating through all tabs', async () => {
    await TabBar.tapScan();
    await ScanPage.isVisible();

    await TabBar.tapTrends();
    await TimelinePage.isVisible();

    await TabBar.tapTreat();
    await TreatmentPage.isVisible();

    await TabBar.tapMore();
    await SettingsPage.isVisible();

    await TabBar.tapHome();
    await DashboardPage.isVisible();
    await TabBar.isActiveTab('home');
  });
});
