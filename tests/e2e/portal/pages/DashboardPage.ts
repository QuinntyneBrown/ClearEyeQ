import { type Page, type Locator, expect } from '@playwright/test';

export class DashboardPage {
  readonly page: Page;
  readonly statCards: Locator;
  readonly totalPatientsCard: Locator;
  readonly flaggedCasesCard: Locator;
  readonly pendingReferralsCard: Locator;
  readonly treatmentReviewsCard: Locator;
  readonly recentActivityFeed: Locator;

  constructor(page: Page) {
    this.page = page;

    // StatCards renders a grid with 4 cards.  Each card has a label paragraph.
    this.statCards = page.locator('[class*="grid"] > div').filter({
      has: page.locator('p.text-2xl'),
    });
    this.totalPatientsCard = this.cardByLabel('Total Patients');
    this.flaggedCasesCard = this.cardByLabel('Flagged Cases');
    this.pendingReferralsCard = this.cardByLabel('Pending Referrals');
    this.treatmentReviewsCard = this.cardByLabel('Treatment Reviews');

    // RecentActivity sections
    this.recentActivityFeed = page
      .locator('[class*="grid"]')
      .filter({ has: page.locator('text=minutes ago').or(page.locator('text=hour ago')).or(page.locator('text=hours ago')) });
  }

  private cardByLabel(label: string): Locator {
    return this.page
      .locator('[class*="grid"] > div')
      .filter({ hasText: label });
  }

  activityItem(index: number): Locator {
    return this.recentActivityFeed.locator('> div > div').nth(index);
  }

  async goto() {
    await this.page.goto('/');
    await this.page.waitForLoadState('networkidle');
  }

  async getStatValue(name: string): Promise<string> {
    const card = this.cardByLabel(name);
    const valueEl = card.locator('p.text-2xl');
    return (await valueEl.textContent()) ?? '';
  }

  async expectStatCardValue(name: string, value: string) {
    const card = this.cardByLabel(name);
    const valueEl = card.locator('p.text-2xl');
    await expect(valueEl).toHaveText(value);
  }

  async expectActivityCount(min: number) {
    // Count all activity description elements across both RecentActivity panels
    const items = this.page.locator('text=minutes ago, text=hour ago, text=hours ago');
    const count = await items.count();
    expect(count).toBeGreaterThanOrEqual(min);
  }
}
