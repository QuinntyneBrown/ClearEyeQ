import { type Page, type Locator, expect } from '@playwright/test';
import { waitForBlazor, waitForDataLoad } from '../helpers/blazor';

export class NavMenu {
  readonly page: Page;
  readonly logo: Locator;
  readonly adminBadge: Locator;
  readonly dashboardLink: Locator;
  readonly tenantsLink: Locator;
  readonly usersLink: Locator;
  readonly subscriptionsLink: Locator;
  readonly healthLink: Locator;
  readonly featureFlagsLink: Locator;
  readonly auditLink: Locator;
  readonly activeNavItem: Locator;

  constructor(page: Page) {
    this.page = page;
    this.logo = page.locator('.sidebar-logo');
    this.adminBadge = page.locator('.sidebar-badge');
    this.dashboardLink = page.locator('.nav-item', { hasText: 'Dashboard' });
    this.tenantsLink = page.locator('.nav-item', { hasText: 'Tenants' });
    this.usersLink = page.locator('.nav-item', { hasText: 'Users' });
    this.subscriptionsLink = page.locator('.nav-item', { hasText: 'Subscriptions' });
    this.healthLink = page.locator('.nav-item', { hasText: 'System Health' });
    this.featureFlagsLink = page.locator('.nav-item', { hasText: 'Feature Flags' });
    this.auditLink = page.locator('.nav-item', { hasText: 'Audit Log' });
    this.activeNavItem = page.locator('.nav-item.active');
  }

  private getLinkBySection(section: string): Locator {
    const map: Record<string, Locator> = {
      dashboard: this.dashboardLink,
      tenants: this.tenantsLink,
      users: this.usersLink,
      subscriptions: this.subscriptionsLink,
      health: this.healthLink,
      'system health': this.healthLink,
      'feature flags': this.featureFlagsLink,
      features: this.featureFlagsLink,
      audit: this.auditLink,
      'audit log': this.auditLink,
    };
    const link = map[section.toLowerCase()];
    if (!link) {
      throw new Error(`Unknown nav section: ${section}`);
    }
    return link;
  }

  async navigateTo(section: string): Promise<void> {
    const link = this.getLinkBySection(section);
    await link.click();
    await waitForBlazor(this.page);
    await waitForDataLoad(this.page);
  }

  async expectActiveSection(name: string): Promise<void> {
    const link = this.getLinkBySection(name);
    await expect(link).toHaveClass(/active/);
  }

  async expectAllLinksVisible(): Promise<void> {
    await expect(this.dashboardLink).toBeVisible();
    await expect(this.tenantsLink).toBeVisible();
    await expect(this.usersLink).toBeVisible();
    await expect(this.subscriptionsLink).toBeVisible();
    await expect(this.healthLink).toBeVisible();
    await expect(this.featureFlagsLink).toBeVisible();
    await expect(this.auditLink).toBeVisible();
  }
}
