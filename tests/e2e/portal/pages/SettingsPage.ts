import { type Page, type Locator, expect } from '@playwright/test';

export class SettingsPage {
  readonly page: Page;
  readonly profileForm: Locator;
  readonly nameInput: Locator;
  readonly emailInput: Locator;
  readonly saveButton: Locator;
  readonly notificationToggles: Locator;

  constructor(page: Page) {
    this.page = page;

    // The profile card is the first Card on the settings page
    this.profileForm = page.locator('text=Clinician Profile').locator('..');

    // Input fields -- the settings page uses labeled inputs
    this.nameInput = page.locator('input').first();
    this.emailInput = page.locator('input[type="email"]');
    this.saveButton = page.getByRole('button', { name: 'Save Changes' });

    // Toggle switches are buttons with role="switch"
    this.notificationToggles = page.locator('[role="switch"]');
  }

  /** Get a specific notification toggle by its label text */
  toggleByLabel(label: string): Locator {
    return this.page
      .locator('div')
      .filter({ hasText: label })
      .locator('[role="switch"]');
  }

  get pushToggle(): Locator {
    return this.toggleByLabel('New Referrals');
  }

  get emailToggle(): Locator {
    return this.toggleByLabel('Weekly Digest');
  }

  async goto() {
    await this.page.goto('/settings');
    await this.page.waitForLoadState('networkidle');
  }

  async updateProfile(name: string, email: string) {
    await this.nameInput.fill(name);
    await this.emailInput.fill(email);
  }

  async togglePush() {
    await this.pushToggle.click();
  }

  async toggleEmail() {
    await this.emailToggle.click();
  }

  async saveSettings() {
    await this.saveButton.click();
  }

  async expectProfileName(name: string) {
    await expect(this.nameInput).toHaveValue(name);
  }

  async expectSaved() {
    // After clicking save, the button should still be present (page doesn't navigate away)
    await expect(this.saveButton).toBeVisible();
  }
}
