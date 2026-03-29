import LoginPage from '../pages/LoginPage';
import DashboardPage from '../pages/DashboardPage';

const TEST_USER_EMAIL = 'testuser@cleareyeq.com';
const TEST_USER_PASSWORD = 'TestPass123!';

/**
 * Reusable login helper that authenticates with test credentials
 * and waits for the dashboard to become visible.
 * Use this in non-auth tests to skip the login flow.
 */
export async function loginAsTestUser(): Promise<void> {
  await LoginPage.isVisible();
  await LoginPage.login(TEST_USER_EMAIL, TEST_USER_PASSWORD);
  await DashboardPage.isVisible();
}

export { TEST_USER_EMAIL, TEST_USER_PASSWORD };
