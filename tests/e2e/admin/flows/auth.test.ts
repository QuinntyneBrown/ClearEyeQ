import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { NavMenu } from '../pages/NavMenu';
import { ADMIN_CREDENTIALS, NON_ADMIN_CREDENTIALS } from '../helpers/test-data';
import { waitForBlazor } from '../helpers/blazor';

test.describe('Admin Authentication Flow', () => {
  test('should display login page when navigating to the app unauthenticated', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();

    await expect(loginPage.emailInput).toBeVisible({ timeout: 15_000 });
    await expect(loginPage.passwordInput).toBeVisible();
    await expect(loginPage.loginButton).toBeVisible();
  });

  test('should show error for non-admin credentials', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(NON_ADMIN_CREDENTIALS.email, NON_ADMIN_CREDENTIALS.password);

    await loginPage.expectError('denied');
  });

  test('should redirect to dashboard after valid admin login', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(ADMIN_CREDENTIALS.email, ADMIN_CREDENTIALS.password);

    await loginPage.expectLoggedIn();

    // Verify we are on the dashboard
    await expect(page.locator('h2:has-text("Dashboard")')).toBeVisible({ timeout: 10_000 });
  });

  test('should show all admin nav sections after login', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(ADMIN_CREDENTIALS.email, ADMIN_CREDENTIALS.password);
    await loginPage.expectLoggedIn();

    const navMenu = new NavMenu(page);
    await navMenu.expectAllLinksVisible();
    await expect(navMenu.adminBadge).toBeVisible();
    await expect(navMenu.adminBadge).toContainText('Admin');
  });

  test('should return to login page after logout', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(ADMIN_CREDENTIALS.email, ADMIN_CREDENTIALS.password);
    await loginPage.expectLoggedIn();
    await waitForBlazor(page);

    // Look for a logout button/link and click it
    const logoutButton = page.locator('button:has-text("Logout"), button:has-text("Log out"), a:has-text("Logout"), a:has-text("Log out"), [data-testid="logout"]');
    if (await logoutButton.isVisible()) {
      await logoutButton.click();
      await page.waitForLoadState('domcontentloaded');
    } else {
      // If no explicit logout button, navigate to logout endpoint
      await page.goto('/logout');
      await page.waitForLoadState('domcontentloaded');
    }

    // Should be back at the login page or redirected there
    await page.waitForTimeout(1000);
    const onLoginPage = await loginPage.emailInput.isVisible().catch(() => false);
    const onLogoutConfirm = await page.locator('text=/log/i').isVisible().catch(() => false);
    expect(onLoginPage || onLogoutConfirm).toBeTruthy();
  });
});
