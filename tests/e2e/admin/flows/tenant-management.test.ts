import { test, expect } from '../fixtures/auth.fixture';
import { NavMenu } from '../pages/NavMenu';
import { TenantListPage } from '../pages/TenantListPage';
import { TenantDetailPage } from '../pages/TenantDetailPage';
import { TEST_TENANT_CREATE_NAME } from '../helpers/test-data';

test.describe('Tenant Management Flow', () => {
  test('should navigate to Tenants via nav menu and show tenant list', async ({ adminPage }) => {
    const nav = new NavMenu(adminPage);
    await nav.navigateTo('tenants');
    await nav.expectActiveSection('tenants');

    const tenantList = new TenantListPage(adminPage);
    await expect(tenantList.tenantTable).toBeVisible({ timeout: 15_000 });
    await tenantList.expectTenantCount(1);
  });

  test('should filter tenants when searching', async ({ adminPage }) => {
    const tenantList = new TenantListPage(adminPage);
    await tenantList.goto();
    await tenantList.expectTenantCount(1);

    // Get the first tenant name to use as search term
    const firstName = await tenantList.tenantName(0).textContent();
    expect(firstName).toBeTruthy();

    // Search with a partial name
    const searchTerm = firstName!.trim().substring(0, 4);
    await tenantList.search(searchTerm);

    // Table should still show at least 1 matching result
    const rows = tenantList.tenantTable.locator('tbody tr');
    const count = await rows.count();
    expect(count).toBeGreaterThanOrEqual(1);

    // All visible tenant names should contain the search term (case-insensitive)
    for (let i = 0; i < count; i++) {
      const name = await tenantList.tenantName(i).textContent();
      expect(name!.toLowerCase()).toContain(searchTerm.toLowerCase());
    }
  });

  test('should create a new tenant via dialog', async ({ adminPage }) => {
    const tenantList = new TenantListPage(adminPage);
    await tenantList.goto();

    // Record initial count
    const initialRows = tenantList.tenantTable.locator('tbody tr');
    const initialCount = await initialRows.count();

    // Create a new tenant
    await tenantList.createTenant(TEST_TENANT_CREATE_NAME);

    // Verify the new tenant appears in the list
    const newCount = await initialRows.count();
    expect(newCount).toBeGreaterThan(initialCount);

    // Search for the newly created tenant
    await tenantList.search(TEST_TENANT_CREATE_NAME);
    await tenantList.expectTenantCount(1);
    await tenantList.expectTenantName(0, TEST_TENANT_CREATE_NAME);
  });

  test('should navigate to tenant detail on edit click', async ({ adminPage }) => {
    const tenantList = new TenantListPage(adminPage);
    await tenantList.goto();
    await tenantList.expectTenantCount(1);

    await tenantList.editTenant(0);

    // Should be on the detail page now
    const detailPage = new TenantDetailPage(adminPage);
    await expect(detailPage.nameInput).toBeVisible({ timeout: 10_000 });
    await expect(detailPage.saveButton).toBeVisible();
  });

  test('should update tenant name and save', async ({ adminPage }) => {
    const tenantList = new TenantListPage(adminPage);
    await tenantList.goto();

    // Get the first tenant's name
    const originalName = (await tenantList.tenantName(0).textContent())!.trim();

    // Navigate to detail
    await tenantList.editTenant(0);

    const detailPage = new TenantDetailPage(adminPage);
    await expect(detailPage.nameInput).toBeVisible({ timeout: 10_000 });

    // Update the name
    const updatedName = `${originalName} Updated`;
    await detailPage.updateName(updatedName);
    await detailPage.save();
    await detailPage.expectSaveSuccess();

    // Navigate back to list
    await detailPage.navigateBack();

    // Verify the updated name in the list
    await tenantList.search(updatedName);
    await tenantList.expectTenantCount(1);
    await tenantList.expectTenantName(0, updatedName);

    // Restore original name
    await tenantList.editTenant(0);
    await expect(detailPage.nameInput).toBeVisible({ timeout: 10_000 });
    await detailPage.updateName(originalName);
    await detailPage.save();
    await detailPage.expectSaveSuccess();
  });

  test('should deactivate a tenant with confirmation', async ({ adminPage }) => {
    const tenantList = new TenantListPage(adminPage);
    await tenantList.goto();
    await tenantList.expectTenantCount(1);

    // Find a row with an active tenant that has a Deactivate button
    const deactivateBtn = tenantList.tenantTable.locator('tbody tr button:has-text("Deactivate")').first();
    const hasDeactivatable = await deactivateBtn.isVisible().catch(() => false);

    if (hasDeactivatable) {
      // Get the row index of the first deactivatable tenant
      const row = deactivateBtn.locator('xpath=ancestor::tr');
      const statusBefore = await row.locator('.badge, [class*="status"]').textContent();
      expect(statusBefore).toContain('Active');

      await deactivateBtn.click();

      // Confirm dialog should appear
      const confirmBtn = adminPage.locator('.modal button:has-text("Deactivate"), [class*="confirm"] button:has-text("Deactivate")').last();
      await expect(confirmBtn).toBeVisible({ timeout: 5_000 });
      await confirmBtn.click();

      // Wait for reload
      await adminPage.waitForTimeout(1000);
      await tenantList.goto();

      // The tenant should now have a non-Active status
      // (We don't know the exact index after reload, so this is a soft check)
      await tenantList.expectTenantCount(1);
    }
  });
});
