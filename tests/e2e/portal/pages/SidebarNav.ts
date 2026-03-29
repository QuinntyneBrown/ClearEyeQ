import { type Page, type Locator, expect } from '@playwright/test';

export class SidebarNav {
  readonly page: Page;
  readonly sidebar: Locator;
  readonly logo: Locator;
  readonly dashboardLink: Locator;
  readonly patientsLink: Locator;
  readonly referralsLink: Locator;
  readonly treatmentReviewsLink: Locator;
  readonly settingsLink: Locator;

  constructor(page: Page) {
    this.page = page;
    this.sidebar = page.locator('aside');
    this.logo = this.sidebar.locator('text=ClearEyeQ');
    this.dashboardLink = this.sidebar.getByRole('link', { name: 'Dashboard' });
    this.patientsLink = this.sidebar.getByRole('link', { name: 'Patients' });
    this.referralsLink = this.sidebar.getByRole('link', { name: 'Referrals' });
    this.treatmentReviewsLink = this.sidebar.getByRole('link', {
      name: 'Treatment Reviews',
    });
    this.settingsLink = this.sidebar.getByRole('link', { name: 'Settings' });
  }

  private linkForSection(section: string): Locator {
    switch (section) {
      case 'Dashboard':
        return this.dashboardLink;
      case 'Patients':
        return this.patientsLink;
      case 'Referrals':
        return this.referralsLink;
      case 'Treatment Reviews':
        return this.treatmentReviewsLink;
      case 'Settings':
        return this.settingsLink;
      default:
        throw new Error(`Unknown sidebar section: ${section}`);
    }
  }

  async navigateTo(section: string) {
    const link = this.linkForSection(section);
    await link.click();
    await this.page.waitForLoadState('networkidle');
  }

  /** The active item is the one with the active background class */
  get activeItem(): Locator {
    return this.sidebar.locator('a.bg-primary-light');
  }

  async expectActiveSection(name: string) {
    const link = this.linkForSection(name);
    await expect(link).toHaveClass(/bg-primary-light/);
  }
}
