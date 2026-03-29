import { type Page, type Locator, expect } from '@playwright/test';
import { blazorNavigate, waitForDataLoad } from '../helpers/blazor';

export class DashboardPage {
  readonly page: Page;
  readonly statCards: Locator;
  readonly totalTenantsCard: Locator;
  readonly totalUsersCard: Locator;
  readonly activeSubscriptionsCard: Locator;
  readonly systemHealthCard: Locator;
  readonly subscriptionBars: Locator;
  readonly serviceStatusGrid: Locator;

  constructor(page: Page) {
    this.page = page;
    this.statCards = page.locator('.grid-4 .card, .grid-4 [class*="stat"]');
    this.totalTenantsCard = page.locator('.grid-4').first().locator(':nth-child(1)');
    this.totalUsersCard = page.locator('.grid-4').first().locator(':nth-child(2)');
    this.activeSubscriptionsCard = page.locator('.grid-4').first().locator(':nth-child(3)');
    this.systemHealthCard = page.locator('.grid-4').first().locator(':nth-child(4)');
    this.subscriptionBars = page.locator('.grid-2 .card').first();
    this.serviceStatusGrid = page.locator('.grid-2 .card').nth(1);
  }

  async goto(): Promise<void> {
    await blazorNavigate(this.page, '/');
  }

  async getStatValue(name: string): Promise<string> {
    const cardMap: Record<string, Locator> = {
      'Total Tenants': this.totalTenantsCard,
      'Total Users': this.totalUsersCard,
      'Active Subscriptions': this.activeSubscriptionsCard,
      'System Health': this.systemHealthCard,
    };
    const card = cardMap[name];
    if (!card) {
      throw new Error(`Unknown stat card: ${name}`);
    }
    // StatCard component renders a value prominently
    const valueEl = card.locator('[class*="value"], [style*="font-size: 28px"], [style*="font-weight: 700"]').first();
    return (await valueEl.textContent()) ?? '';
  }

  async expectStatValue(name: string, value: string): Promise<void> {
    const actual = await this.getStatValue(name);
    expect(actual.trim()).toBe(value);
  }

  async expectAllStatCardsHaveNumericValues(): Promise<void> {
    await waitForDataLoad(this.page);
    const cards = [
      this.totalTenantsCard,
      this.totalUsersCard,
      this.activeSubscriptionsCard,
    ];
    for (const card of cards) {
      const text = await card.textContent();
      // The card should contain at least one numeric digit
      expect(text).toMatch(/\d/);
    }
    // System health card may show "Healthy" or a number
    const healthText = await this.systemHealthCard.textContent();
    expect(healthText).toBeTruthy();
  }

  async expectSubscriptionBarsVisible(): Promise<void> {
    await expect(this.subscriptionBars).toBeVisible();
    await expect(this.subscriptionBars.locator('.card-title')).toContainText('Subscription Distribution');
    // Verify bars exist for each tier
    const tierLabels = ['Free', 'Pro', 'Premium', 'Autonomous'];
    for (const tier of tierLabels) {
      await expect(this.subscriptionBars.locator(`text=${tier}`)).toBeVisible();
    }
  }

  async expectServiceStatusGrid(): Promise<void> {
    await expect(this.serviceStatusGrid).toBeVisible();
    await expect(this.serviceStatusGrid.locator('.card-title')).toContainText('Service Status');
    // There should be at least one service entry
    const rows = this.serviceStatusGrid.locator('.flex-between');
    await expect(rows.first()).toBeVisible();
  }
}
