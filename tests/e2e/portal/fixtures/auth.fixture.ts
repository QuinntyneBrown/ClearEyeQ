import { test as base, expect, type Page } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { TEST_CLINICIAN_EMAIL, TEST_CLINICIAN_PASSWORD } from '../helpers/test-data';

/**
 * Custom fixture that provides an authenticated page.
 * Logs in before each test and stores the session so subsequent
 * navigations remain authenticated.
 */
type AuthFixtures = {
  authenticatedPage: Page;
};

export const test = base.extend<AuthFixtures>({
  authenticatedPage: async ({ page }, use) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();

    // Attempt login -- if the app redirects straight to dashboard
    // (no auth gate in dev mode), we just proceed.
    try {
      await page.waitForSelector('[data-testid="login-button"]', {
        timeout: 3_000,
      });
      await loginPage.login(TEST_CLINICIAN_EMAIL, TEST_CLINICIAN_PASSWORD);
      // Wait for redirect away from /login
      await page.waitForURL((url) => !url.pathname.includes('/login'), {
        timeout: 10_000,
      });
    } catch {
      // No login page present -- app is running without auth gate.
      // Navigate to the dashboard so tests start from a known location.
      await page.goto('/');
    }

    await page.waitForLoadState('networkidle');
    await use(page);
  },
});

export { expect };
