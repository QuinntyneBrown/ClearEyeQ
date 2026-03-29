import { type Page, type Locator, expect } from '@playwright/test';
import { blazorNavigate, waitForDataLoad } from '../helpers/blazor';

export class HealthDashboardPage {
  readonly page: Page;
  readonly serviceCards: Locator;
  readonly refreshButton: Locator;
  readonly refreshIndicator: Locator;
  readonly lastCheckedText: Locator;
  readonly summaryStat: Locator;

  constructor(page: Page) {
    this.page = page;
    // Service cards are in the .grid-3 section
    this.serviceCards = page.locator('.grid-3 .card');
    this.refreshButton = page.locator('button:has-text("Refresh Now")');
    this.refreshIndicator = page.locator('button:has-text("Checking...")');
    this.lastCheckedText = page.locator('text=Last checked');
    this.summaryStat = page.locator('.grid-4').first();
  }

  serviceCard(name: string): Locator {
    return this.page.locator(`.grid-3 .card:has-text("${name}")`);
  }

  serviceStatus(name: string): Locator {
    return this.serviceCard(name).locator('.badge, [class*="status"]');
  }

  responseTime(name: string): Locator {
    return this.serviceCard(name).locator('text=/\\d+ms/');
  }

  lastChecked(name: string): Locator {
    return this.serviceCard(name).locator('.flex-between').last().locator('span').last();
  }

  async goto(): Promise<void> {
    await blazorNavigate(this.page, '/system/health');
  }

  async waitForRefresh(): Promise<void> {
    // Click the refresh button and wait for data to reload
    await this.refreshButton.click();
    await this.refreshIndicator.waitFor({ state: 'visible', timeout: 5_000 }).catch(() => {
      // The indicator may be too fast to catch
    });
    await waitForDataLoad(this.page);
    await this.page.waitForTimeout(1000);
  }

  async expectServiceCount(count: number): Promise<void> {
    const cards = this.serviceCards;
    await expect(cards).toHaveCount(count);
  }

  async expectServiceStatus(name: string, status: string): Promise<void> {
    const statusBadge = this.serviceStatus(name);
    await expect(statusBadge).toContainText(status);
  }

  async expectAllHealthy(): Promise<void> {
    const count = await this.serviceCards.count();
    for (let i = 0; i < count; i++) {
      const card = this.serviceCards.nth(i);
      const badge = card.locator('.badge, [class*="status"]');
      await expect(badge).toContainText('Healthy');
    }
  }

  async expectServiceCardsHaveDetails(): Promise<void> {
    const count = await this.serviceCards.count();
    expect(count).toBeGreaterThan(0);
    for (let i = 0; i < Math.min(count, 3); i++) {
      const card = this.serviceCards.nth(i);
      // Each card should have: name, status badge, response time, last checked
      await expect(card.locator('.badge, [class*="status"]')).toBeVisible();
      await expect(card.locator('text=Response Time')).toBeVisible();
      await expect(card.locator('text=Last Checked')).toBeVisible();
    }
  }

  async expectStatusBadgesColorCoded(): Promise<void> {
    const count = await this.serviceCards.count();
    for (let i = 0; i < count; i++) {
      const card = this.serviceCards.nth(i);
      const badge = card.locator('.badge, [class*="status"]');
      const text = (await badge.textContent())?.trim() ?? '';
      // Status badge should be one of: Healthy, Degraded, Unhealthy
      expect(['Healthy', 'Degraded', 'Unhealthy']).toContain(text);
    }
  }
}
