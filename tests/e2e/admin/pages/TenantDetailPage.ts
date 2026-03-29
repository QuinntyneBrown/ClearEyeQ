import { type Page, type Locator, expect } from '@playwright/test';
import { blazorNavigate, waitForDataLoad } from '../helpers/blazor';

export class TenantDetailPage {
  readonly page: Page;
  readonly nameInput: Locator;
  readonly statusToggle: Locator;
  readonly usersList: Locator;
  readonly saveButton: Locator;
  readonly cancelButton: Locator;
  readonly successMessage: Locator;
  readonly backButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.nameInput = page.locator('.form-group').filter({ hasText: 'Name' }).locator('.form-input');
    this.statusToggle = page.locator('.form-group').filter({ hasText: 'Status' }).locator('.form-select');
    this.usersList = page.locator('.data-table').last();
    this.saveButton = page.locator('button:has-text("Save Changes")');
    this.cancelButton = page.locator('button:has-text("Cancel")');
    this.successMessage = page.locator('.text-success');
    this.backButton = page.locator('button:has-text("Back to Tenants")');
  }

  async goto(id: string): Promise<void> {
    await blazorNavigate(this.page, `/tenants/${id}`);
  }

  async updateName(name: string): Promise<void> {
    await this.nameInput.clear();
    await this.nameInput.fill(name);
  }

  async toggleStatus(): Promise<void> {
    // Toggle between Active/Suspended by selecting the other option
    const current = await this.statusToggle.inputValue();
    const newStatus = current === 'Active' ? 'Suspended' : 'Active';
    await this.statusToggle.selectOption(newStatus);
  }

  async save(): Promise<void> {
    await this.saveButton.click();
    await this.page.waitForTimeout(500);
  }

  async cancel(): Promise<void> {
    await this.cancelButton.click();
  }

  async navigateBack(): Promise<void> {
    await this.backButton.click();
    await this.page.waitForLoadState('domcontentloaded');
    await waitForDataLoad(this.page);
  }

  async expectName(name: string): Promise<void> {
    await expect(this.nameInput).toHaveValue(name);
  }

  async expectUserCount(min: number): Promise<void> {
    const usersCard = this.page.locator('.card-title', { hasText: 'Tenant Users' });
    await expect(usersCard).toBeVisible();
    const text = await usersCard.textContent();
    const match = text?.match(/\((\d+)\)/);
    const count = match ? parseInt(match[1], 10) : 0;
    expect(count).toBeGreaterThanOrEqual(min);
  }

  async expectSaveSuccess(): Promise<void> {
    await expect(this.successMessage).toBeVisible({ timeout: 10_000 });
    await expect(this.successMessage).toContainText('updated successfully');
  }
}
