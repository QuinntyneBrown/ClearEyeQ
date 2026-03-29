import { type Page, type Locator, expect } from '@playwright/test';
import { blazorNavigate } from '../helpers/blazor';

export class SubscriptionOverviewPage {
  readonly page: Page;
  readonly planCards: Locator;
  readonly freeCard: Locator;
  readonly proCard: Locator;
  readonly premiumCard: Locator;
  readonly autonomousCard: Locator;
  readonly revenueMetrics: Locator;
  readonly recentChangesTable: Locator;

  constructor(page: Page) {
    this.page = page;
    // The plan distribution cards are in the first .grid-4
    this.planCards = page.locator('.grid-4').first();
    this.freeCard = this.planCards.locator(':nth-child(1)');
    this.proCard = this.planCards.locator(':nth-child(2)');
    this.premiumCard = this.planCards.locator(':nth-child(3)');
    this.autonomousCard = this.planCards.locator(':nth-child(4)');
    // Revenue metrics are in the .grid-3
    this.revenueMetrics = page.locator('.grid-3');
    this.recentChangesTable = page.locator('.card:has(.card-title:has-text("Recent Subscription Changes")) .data-table');
  }

  async goto(): Promise<void> {
    await blazorNavigate(this.page, '/subscriptions');
  }

  async expectPlanCount(tier: string, count: string): Promise<void> {
    const cardMap: Record<string, Locator> = {
      Free: this.freeCard,
      Pro: this.proCard,
      Premium: this.premiumCard,
      Autonomous: this.autonomousCard,
    };
    const card = cardMap[tier];
    if (!card) {
      throw new Error(`Unknown tier: ${tier}`);
    }
    const text = await card.textContent();
    expect(text).toContain(count);
  }

  async expectAllPlanCardsVisible(): Promise<void> {
    await expect(this.freeCard).toBeVisible();
    await expect(this.proCard).toBeVisible();
    await expect(this.premiumCard).toBeVisible();
    await expect(this.autonomousCard).toBeVisible();
  }

  async expectPlanCardsHaveCounts(): Promise<void> {
    for (const card of [this.freeCard, this.proCard, this.premiumCard, this.autonomousCard]) {
      const text = await card.textContent();
      expect(text).toMatch(/\d/);
    }
  }

  async expectRevenueVisible(): Promise<void> {
    await expect(this.revenueMetrics).toBeVisible();
    // Check for the three revenue cards
    await expect(this.revenueMetrics.locator('.card-title:has-text("Monthly Recurring Revenue")')).toBeVisible();
    await expect(this.revenueMetrics.locator('.card-title:has-text("Avg Revenue Per User")')).toBeVisible();
    await expect(this.revenueMetrics.locator('.card-title:has-text("Churn Rate")')).toBeVisible();
  }

  async expectRecentChangesTable(): Promise<void> {
    const changesSection = this.page.locator('.card-title:has-text("Recent Subscription Changes")');
    await expect(changesSection).toBeVisible();
  }
}
