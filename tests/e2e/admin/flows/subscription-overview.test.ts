import { test, expect } from '../fixtures/auth.fixture';
import { NavMenu } from '../pages/NavMenu';
import { SubscriptionOverviewPage } from '../pages/SubscriptionOverviewPage';

test.describe('Subscription Overview Flow', () => {
  test('should navigate to Subscriptions page', async ({ adminPage }) => {
    const nav = new NavMenu(adminPage);
    await nav.navigateTo('subscriptions');
    await nav.expectActiveSection('subscriptions');

    await expect(adminPage.locator('h2:has-text("Subscription Overview")')).toBeVisible();
  });

  test('should display all 4 plan distribution cards', async ({ adminPage }) => {
    const subPage = new SubscriptionOverviewPage(adminPage);
    await subPage.goto();

    await subPage.expectAllPlanCardsVisible();

    // Verify each card has the correct tier label
    await expect(subPage.freeCard).toContainText('Free');
    await expect(subPage.proCard).toContainText('Pro');
    await expect(subPage.premiumCard).toContainText('Premium');
    await expect(subPage.autonomousCard).toContainText('Autonomous');
  });

  test('should show numeric counts on each plan card', async ({ adminPage }) => {
    const subPage = new SubscriptionOverviewPage(adminPage);
    await subPage.goto();

    await subPage.expectPlanCardsHaveCounts();
  });

  test('should display revenue metrics section', async ({ adminPage }) => {
    const subPage = new SubscriptionOverviewPage(adminPage);
    await subPage.goto();

    await subPage.expectRevenueVisible();
  });

  test('should display recent subscription changes table', async ({ adminPage }) => {
    const subPage = new SubscriptionOverviewPage(adminPage);
    await subPage.goto();

    await subPage.expectRecentChangesTable();
  });
});
