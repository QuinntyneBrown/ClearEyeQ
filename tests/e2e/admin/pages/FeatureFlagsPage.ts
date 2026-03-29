import { type Page, type Locator, expect } from '@playwright/test';
import { blazorNavigate, waitForDataLoad } from '../helpers/blazor';

export class FeatureFlagsPage {
  readonly page: Page;
  readonly flagTable: Locator;

  constructor(page: Page) {
    this.page = page;
    this.flagTable = page.locator('.data-table');
  }

  flagRow(name: string): Locator {
    return this.flagTable.locator(`tr:has-text("${name}")`);
  }

  globalToggle(name: string): Locator {
    // The global toggle is in the 3rd <td> of the flag row
    return this.flagRow(name).locator('td').nth(2).locator('input[type="checkbox"]');
  }

  tenantOverrideSection(name: string): Locator {
    // Tenant overrides are in the 4th <td> of the flag row
    return this.flagRow(name).locator('td').nth(3);
  }

  addOverrideButton(name: string): Locator {
    return this.flagRow(name).locator('button:has-text("Add Override"), button:has-text("Add")');
  }

  async goto(): Promise<void> {
    await blazorNavigate(this.page, '/system/features');
  }

  async toggleGlobalFlag(name: string): Promise<void> {
    const toggle = this.globalToggle(name);
    await toggle.click();
    await this.page.waitForTimeout(500);
  }

  async addTenantOverride(name: string, tenantId: string, enabled: boolean): Promise<void> {
    const addBtn = this.addOverrideButton(name);
    await addBtn.click();
    // Fill the tenant override form if it appears
    const dialog = this.page.locator('.modal, [class*="dialog"]');
    if (await dialog.isVisible()) {
      const tenantInput = dialog.locator('input, select').first();
      await tenantInput.fill(tenantId);
      if (!enabled) {
        const enabledToggle = dialog.locator('input[type="checkbox"]');
        await enabledToggle.uncheck();
      }
      await dialog.locator('button:has-text("Save"), button:has-text("Add"), button.btn-primary').click();
      await this.page.waitForTimeout(500);
    }
    await waitForDataLoad(this.page);
  }

  async expectFlagEnabled(name: string, enabled: boolean): Promise<void> {
    const toggle = this.globalToggle(name);
    if (enabled) {
      await expect(toggle).toBeChecked();
    } else {
      await expect(toggle).not.toBeChecked();
    }
  }

  async expectOverrideCount(name: string, count: number): Promise<void> {
    const section = this.tenantOverrideSection(name);
    if (count === 0) {
      await expect(section).toContainText('None');
    } else {
      const overrides = section.locator('.flex.gap-sm');
      const actual = await overrides.count();
      expect(actual).toBeGreaterThanOrEqual(count);
    }
  }

  async expectFlagTableVisible(): Promise<void> {
    await expect(this.flagTable).toBeVisible();
    const rows = this.flagTable.locator('tbody tr');
    const count = await rows.count();
    expect(count).toBeGreaterThan(0);
  }
}
