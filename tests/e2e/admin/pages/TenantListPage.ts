import { type Page, type Locator, expect } from '@playwright/test';
import { blazorNavigate, waitForDataLoad } from '../helpers/blazor';

export class TenantListPage {
  readonly page: Page;
  readonly searchBar: Locator;
  readonly createButton: Locator;
  readonly tenantTable: Locator;
  readonly createDialog: Locator;
  readonly createNameInput: Locator;
  readonly createSubmitButton: Locator;
  readonly confirmDialog: Locator;
  readonly confirmButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.searchBar = page.locator('input[placeholder*="Search tenants"]');
    this.createButton = page.locator('button:has-text("Create Tenant")');
    this.tenantTable = page.locator('.data-table');
    this.createDialog = page.locator('.modal');
    this.createNameInput = page.locator('.modal .form-input');
    this.createSubmitButton = page.locator('.modal button.btn-primary:has-text("Create")');
    this.confirmDialog = page.locator('.modal:has-text("Deactivate"), [class*="confirm"]');
    this.confirmButton = page.locator('button:has-text("Deactivate")').last();
  }

  tenantRow(index: number): Locator {
    return this.tenantTable.locator('tbody tr').nth(index);
  }

  tenantName(index: number): Locator {
    return this.tenantRow(index).locator('td').first().locator('a');
  }

  tenantStatus(index: number): Locator {
    return this.tenantRow(index).locator('.badge, [class*="status"]');
  }

  editButton(index: number): Locator {
    return this.tenantRow(index).locator('button:has-text("Edit")');
  }

  deactivateButton(index: number): Locator {
    return this.tenantRow(index).locator('button:has-text("Deactivate")');
  }

  async goto(): Promise<void> {
    await blazorNavigate(this.page, '/tenants');
  }

  async search(query: string): Promise<void> {
    await this.searchBar.fill(query);
    // Blazor SearchBar triggers on input or debounce; give time for re-render
    await this.page.waitForTimeout(500);
    await waitForDataLoad(this.page);
  }

  async createTenant(name: string): Promise<void> {
    await this.createButton.click();
    await expect(this.createDialog).toBeVisible();
    await this.createNameInput.fill(name);
    await this.createSubmitButton.click();
    await expect(this.createDialog).toBeHidden({ timeout: 10_000 });
    await waitForDataLoad(this.page);
  }

  async editTenant(index: number): Promise<void> {
    await this.editButton(index).click();
    await this.page.waitForLoadState('domcontentloaded');
    await waitForDataLoad(this.page);
  }

  async deactivateTenant(index: number): Promise<void> {
    await this.deactivateButton(index).click();
  }

  async confirmAction(): Promise<void> {
    await expect(this.confirmButton).toBeVisible({ timeout: 5_000 });
    await this.confirmButton.click();
    await waitForDataLoad(this.page);
  }

  async expectTenantCount(min: number): Promise<void> {
    const rows = this.tenantTable.locator('tbody tr');
    await expect(rows).toHaveCount(await rows.count());
    const count = await rows.count();
    expect(count).toBeGreaterThanOrEqual(min);
  }

  async expectTenantName(index: number, name: string): Promise<void> {
    await expect(this.tenantName(index)).toContainText(name);
  }

  async expectTenantStatus(index: number, status: string): Promise<void> {
    await expect(this.tenantStatus(index)).toContainText(status);
  }
}
