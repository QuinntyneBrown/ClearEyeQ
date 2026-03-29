import { type Page, type Locator, expect } from '@playwright/test';

export class TopBar {
  readonly page: Page;
  readonly header: Locator;
  readonly searchInput: Locator;
  readonly notificationBell: Locator;
  readonly notificationCount: Locator;
  readonly userAvatar: Locator;
  readonly userDropdown: Locator;
  readonly logoutButton: Locator;
  readonly pageTitle: Locator;

  constructor(page: Page) {
    this.page = page;
    this.header = page.locator('header');
    this.searchInput = this.header.locator('input[placeholder="Search patients..."]');
    this.notificationBell = this.header.locator('button').filter({ has: page.locator('svg.lucide-bell') });
    this.notificationCount = this.header.locator('span.rounded-full.bg-error');
    this.userAvatar = this.header.locator('button').filter({ has: page.locator('div.rounded-full.bg-primary') });
    this.userDropdown = page.locator('[role="menu"]');
    this.logoutButton = page.getByRole('menuitem', { name: 'Logout' });
    this.pageTitle = this.header.locator('h1');
  }

  async search(query: string) {
    await this.searchInput.fill(query);
  }

  async openNotifications() {
    await this.notificationBell.click();
  }

  async openUserMenu() {
    await this.userAvatar.click();
    await this.userDropdown.waitFor({ state: 'visible' });
  }

  async logout() {
    await this.openUserMenu();
    await this.logoutButton.click();
  }
}
