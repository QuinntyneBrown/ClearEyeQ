import { test, expect } from '../fixtures/auth.fixture';
import { DashboardPage } from '../pages/DashboardPage';

test.describe('Dashboard Overview Flow', () => {
  test('should display dashboard with all stat cards after login', async ({ adminPage }) => {
    const dashboard = new DashboardPage(adminPage);
    await dashboard.goto();

    await expect(adminPage.locator('h2:has-text("Dashboard")')).toBeVisible();
  });

  test('should show all 4 stat cards with numeric values', async ({ adminPage }) => {
    const dashboard = new DashboardPage(adminPage);
    await dashboard.goto();

    await dashboard.expectAllStatCardsHaveNumericValues();

    // Verify the system health card has meaningful content
    const healthText = await dashboard.systemHealthCard.textContent();
    expect(healthText).toBeTruthy();
    expect(healthText!.length).toBeGreaterThan(0);
  });

  test('should display subscription distribution bars', async ({ adminPage }) => {
    const dashboard = new DashboardPage(adminPage);
    await dashboard.goto();

    await dashboard.expectSubscriptionBarsVisible();
  });

  test('should display service status grid with services', async ({ adminPage }) => {
    const dashboard = new DashboardPage(adminPage);
    await dashboard.goto();

    await dashboard.expectServiceStatusGrid();
  });
});
