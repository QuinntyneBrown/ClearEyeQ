import { device } from 'detox';
import { loginAsTestUser } from '../helpers/auth';
import TimelinePage from '../pages/TimelinePage';
import TabBar from '../pages/TabBar';
import { waitForElement } from '../helpers/wait';

describe('Timeline & Trends Flow', () => {
  beforeEach(async () => {
    await device.reloadReactNative();
    await loginAsTestUser();
    await TabBar.tapTrends();
  });

  it('should display the trends screen with default 7D range', async () => {
    await TimelinePage.isVisible();
    await TimelinePage.hasTimeRangeSelected('7D');
  });

  it('should switch to 30D time range and update the chart', async () => {
    await TimelinePage.isVisible();
    await TimelinePage.selectTimeRange('30D');
    await TimelinePage.chartIsVisible();
  });

  it('should toggle the Pollen correlation chip to active', async () => {
    await TimelinePage.isVisible();
    await TimelinePage.toggleCorrelation('Pollen');
    await TimelinePage.correlationChipIsActive('Pollen');
  });

  it('should display insight cards below the chart', async () => {
    await TimelinePage.isVisible();
    await TimelinePage.insightCardsAreVisible();
  });

  it('should show at least one insight card with content', async () => {
    await TimelinePage.isVisible();
    await TimelinePage.hasInsightCards(1);
  });
});
