import { test, expect } from '../fixtures/auth.fixture';
import { NavMenu } from '../pages/NavMenu';
import { NAV_SECTIONS } from '../helpers/test-data';
import { waitForBlazor, waitForDataLoad } from '../helpers/blazor';

test.describe('Full Navigation Flow', () => {
  test('should display all 7 nav items in the sidebar', async ({ adminPage }) => {
    const nav = new NavMenu(adminPage);

    // Navigate to dashboard first to ensure full layout is loaded
    await adminPage.goto('/');
    await waitForBlazor(adminPage);
    await waitForDataLoad(adminPage);

    await nav.expectAllLinksVisible();
  });

  test('should navigate to each section and highlight active item', async ({ adminPage }) => {
    const nav = new NavMenu(adminPage);

    // Navigate to dashboard first
    await adminPage.goto('/');
    await waitForBlazor(adminPage);
    await waitForDataLoad(adminPage);

    // Test Dashboard
    await nav.navigateTo('dashboard');
    await nav.expectActiveSection('dashboard');
    await expect(adminPage.locator('h2:has-text("Dashboard")')).toBeVisible({ timeout: 10_000 });

    // Test Tenants
    await nav.navigateTo('tenants');
    await nav.expectActiveSection('tenants');
    await expect(adminPage.locator('h2:has-text("Tenants")')).toBeVisible({ timeout: 10_000 });

    // Test Users
    await nav.navigateTo('users');
    await nav.expectActiveSection('users');
    await expect(adminPage.locator('h2:has-text("User Management")')).toBeVisible({ timeout: 10_000 });

    // Test Subscriptions
    await nav.navigateTo('subscriptions');
    await nav.expectActiveSection('subscriptions');
    await expect(adminPage.locator('h2:has-text("Subscription Overview")')).toBeVisible({ timeout: 10_000 });

    // Test System Health
    await nav.navigateTo('system health');
    await nav.expectActiveSection('system health');
    await expect(adminPage.locator('h2:has-text("System Health")')).toBeVisible({ timeout: 10_000 });

    // Test Feature Flags
    await nav.navigateTo('feature flags');
    await nav.expectActiveSection('feature flags');
    await expect(adminPage.locator('h2:has-text("Feature Flags")')).toBeVisible({ timeout: 10_000 });

    // Test Audit Log
    await nav.navigateTo('audit');
    await nav.expectActiveSection('audit');
    await expect(adminPage.locator('h2:has-text("Audit Log")')).toBeVisible({ timeout: 10_000 });
  });

  test('should show Admin badge next to logo', async ({ adminPage }) => {
    const nav = new NavMenu(adminPage);

    await adminPage.goto('/');
    await waitForBlazor(adminPage);

    await expect(nav.logo).toBeVisible();
    await expect(nav.logo).toContainText('ClearEyeQ');
    await expect(nav.adminBadge).toBeVisible();
    await expect(nav.adminBadge).toContainText('Admin');
  });

  test('should handle deep link to /tenants with authentication', async ({ adminPage }) => {
    // Direct navigation to /tenants should work when authenticated
    await adminPage.goto('/tenants');
    await waitForBlazor(adminPage);
    await waitForDataLoad(adminPage);

    await expect(adminPage.locator('h2:has-text("Tenants")')).toBeVisible({ timeout: 15_000 });

    // Nav should show tenants as active
    const nav = new NavMenu(adminPage);
    await nav.expectActiveSection('tenants');
  });

  test('should handle deep link to /system/health', async ({ adminPage }) => {
    await adminPage.goto('/system/health');
    await waitForBlazor(adminPage);
    await waitForDataLoad(adminPage);

    await expect(adminPage.locator('h2:has-text("System Health")')).toBeVisible({ timeout: 15_000 });

    const nav = new NavMenu(adminPage);
    await nav.expectActiveSection('system health');
  });

  test('should handle deep link to /audit', async ({ adminPage }) => {
    await adminPage.goto('/audit');
    await waitForBlazor(adminPage);
    await waitForDataLoad(adminPage);

    await expect(adminPage.locator('h2:has-text("Audit Log")')).toBeVisible({ timeout: 15_000 });

    const nav = new NavMenu(adminPage);
    await nav.expectActiveSection('audit');
  });
});
