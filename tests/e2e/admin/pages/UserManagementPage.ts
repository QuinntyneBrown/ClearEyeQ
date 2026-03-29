import { type Page, type Locator, expect } from '@playwright/test';
import { blazorNavigate, waitForDataLoad } from '../helpers/blazor';

export class UserManagementPage {
  readonly page: Page;
  readonly searchBar: Locator;
  readonly roleFilter: Locator;
  readonly statusFilter: Locator;
  readonly userTable: Locator;
  readonly paginationInfo: Locator;

  constructor(page: Page) {
    this.page = page;
    this.searchBar = page.locator('input[placeholder*="Search users"]');
    this.roleFilter = page.locator('.toolbar select.form-select').first();
    this.statusFilter = page.locator('.toolbar select.form-select').nth(1);
    this.userTable = page.locator('.data-table');
    this.paginationInfo = page.locator('.pagination-info');
  }

  userRow(index: number): Locator {
    return this.userTable.locator('tbody tr').nth(index);
  }

  userName(index: number): Locator {
    return this.userRow(index).locator('td').first();
  }

  userEmail(index: number): Locator {
    return this.userRow(index).locator('td').nth(1);
  }

  roleDropdown(index: number): Locator {
    return this.userRow(index).locator('select.form-select');
  }

  statusToggle(index: number): Locator {
    return this.userRow(index).locator('button.btn-ghost');
  }

  userStatusBadge(index: number): Locator {
    return this.userRow(index).locator('.badge, [class*="status"]').first();
  }

  async goto(): Promise<void> {
    await blazorNavigate(this.page, '/users');
  }

  async search(query: string): Promise<void> {
    await this.searchBar.fill(query);
    await this.page.waitForTimeout(500);
    await waitForDataLoad(this.page);
  }

  async filterByRole(role: string): Promise<void> {
    await this.roleFilter.selectOption(role);
    await this.page.waitForTimeout(500);
    await waitForDataLoad(this.page);
  }

  async filterByStatus(status: string): Promise<void> {
    await this.statusFilter.selectOption(status);
    await this.page.waitForTimeout(500);
    await waitForDataLoad(this.page);
  }

  async changeRole(index: number, role: string): Promise<void> {
    await this.roleDropdown(index).selectOption(role);
    await this.page.waitForTimeout(500);
  }

  async toggleUserStatus(index: number): Promise<void> {
    await this.statusToggle(index).click();
    await this.page.waitForTimeout(500);
  }

  async expectUserCount(min: number): Promise<void> {
    const rows = this.userTable.locator('tbody tr');
    const count = await rows.count();
    expect(count).toBeGreaterThanOrEqual(min);
  }

  async expectUserRole(index: number, role: string): Promise<void> {
    await expect(this.roleDropdown(index)).toHaveValue(role);
  }

  async expectUserStatus(index: number, status: string): Promise<void> {
    // Check the status badge or the toggle button text
    const row = this.userRow(index);
    const rowText = await row.textContent();
    expect(rowText).toContain(status);
  }
}
