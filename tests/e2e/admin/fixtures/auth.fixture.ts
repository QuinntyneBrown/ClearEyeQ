import { test as base, Page } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { ADMIN_CREDENTIALS } from '../helpers/test-data';
import { waitForBlazor } from '../helpers/blazor';

/**
 * Custom fixture that provides an already-authenticated admin page.
 * Authentication state is stored and reused across tests in a spec file
 * to avoid repeated logins.
 */
type AuthFixtures = {
  adminPage: Page;
};

export const test = base.extend<AuthFixtures>({
  adminPage: async ({ page }, use) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(ADMIN_CREDENTIALS.email, ADMIN_CREDENTIALS.password);
    await loginPage.expectLoggedIn();
    await waitForBlazor(page);
    await use(page);
  },
});

export { expect } from '@playwright/test';
