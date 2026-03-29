import { type Page, type Locator, expect } from '@playwright/test';
import { blazorNavigate, waitForDataLoad } from '../helpers/blazor';

export class AuditLogPage {
  readonly page: Page;
  readonly dateFromInput: Locator;
  readonly dateToInput: Locator;
  readonly actionFilter: Locator;
  readonly userSearchInput: Locator;
  readonly auditTable: Locator;
  readonly pagination: Locator;
  readonly nextPageButton: Locator;
  readonly prevPageButton: Locator;
  readonly pageInfo: Locator;

  constructor(page: Page) {
    this.page = page;
    this.userSearchInput = page.locator('input[placeholder*="Search by user"]');
    this.actionFilter = page.locator('.toolbar select.form-select');
    this.dateFromInput = page.locator('input[type="date"]').first();
    this.dateToInput = page.locator('input[type="date"]').nth(1);
    this.auditTable = page.locator('.data-table');
    this.pagination = page.locator('.pagination');
    this.nextPageButton = page.locator('button:has-text("Next")');
    this.prevPageButton = page.locator('button:has-text("Previous")');
    this.pageInfo = page.locator('.pagination-info');
  }

  auditRow(index: number): Locator {
    return this.auditTable.locator('tbody tr').nth(index);
  }

  timestamp(index: number): Locator {
    return this.auditRow(index).locator('td').first();
  }

  user(index: number): Locator {
    return this.auditRow(index).locator('td').nth(1);
  }

  action(index: number): Locator {
    return this.auditRow(index).locator('td').nth(2);
  }

  resource(index: number): Locator {
    return this.auditRow(index).locator('td').nth(3);
  }

  async goto(): Promise<void> {
    await blazorNavigate(this.page, '/audit');
  }

  async filterByDateRange(from: string, to: string): Promise<void> {
    await this.dateFromInput.fill(from);
    await this.page.waitForTimeout(500);
    await this.dateToInput.fill(to);
    await this.page.waitForTimeout(500);
    await waitForDataLoad(this.page);
  }

  async filterByAction(action: string): Promise<void> {
    await this.actionFilter.selectOption(action);
    await this.page.waitForTimeout(500);
    await waitForDataLoad(this.page);
  }

  async searchByUser(query: string): Promise<void> {
    await this.userSearchInput.fill(query);
    await this.page.waitForTimeout(500);
    await waitForDataLoad(this.page);
  }

  async nextPage(): Promise<void> {
    await this.nextPageButton.click();
    await waitForDataLoad(this.page);
  }

  async prevPage(): Promise<void> {
    await this.prevPageButton.click();
    await waitForDataLoad(this.page);
  }

  async expectLogCount(min: number): Promise<void> {
    const rows = this.auditTable.locator('tbody tr');
    const count = await rows.count();
    expect(count).toBeGreaterThanOrEqual(min);
  }

  async expectLogAction(index: number, action: string): Promise<void> {
    await expect(this.action(index)).toContainText(action);
  }

  async expectPageInfo(current: number, total: number): Promise<void> {
    await expect(this.pageInfo).toContainText(`Page ${current} of ${total}`);
  }

  async expectAuditTableVisible(): Promise<void> {
    await expect(this.auditTable).toBeVisible();
  }

  async getCurrentPageEntryTexts(): Promise<string[]> {
    const rows = this.auditTable.locator('tbody tr');
    const count = await rows.count();
    const texts: string[] = [];
    for (let i = 0; i < count; i++) {
      const text = await rows.nth(i).textContent();
      texts.push(text ?? '');
    }
    return texts;
  }
}
