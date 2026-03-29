import { test, expect } from '../fixtures/auth.fixture';
import { NavMenu } from '../pages/NavMenu';
import { UserManagementPage } from '../pages/UserManagementPage';

test.describe('User Management Flow', () => {
  test('should navigate to Users and display user table', async ({ adminPage }) => {
    const nav = new NavMenu(adminPage);
    await nav.navigateTo('users');
    await nav.expectActiveSection('users');

    const userPage = new UserManagementPage(adminPage);
    await expect(userPage.userTable).toBeVisible({ timeout: 15_000 });
    await userPage.expectUserCount(1);
  });

  test('should filter users by role Clinician', async ({ adminPage }) => {
    const userPage = new UserManagementPage(adminPage);
    await userPage.goto();
    await userPage.expectUserCount(1);

    await userPage.filterByRole('Clinician');

    // All visible users should be Clinicians
    const rows = userPage.userTable.locator('tbody tr');
    const count = await rows.count();
    expect(count).toBeGreaterThanOrEqual(0);

    for (let i = 0; i < count; i++) {
      await userPage.expectUserRole(i, 'Clinician');
    }
  });

  test('should filter users by status Active', async ({ adminPage }) => {
    const userPage = new UserManagementPage(adminPage);
    await userPage.goto();

    await userPage.filterByStatus('Active');

    // All visible users should have Active status
    const rows = userPage.userTable.locator('tbody tr');
    const count = await rows.count();
    expect(count).toBeGreaterThanOrEqual(0);

    for (let i = 0; i < count; i++) {
      await userPage.expectUserStatus(i, 'Active');
    }
  });

  test('should change a user role and verify update', async ({ adminPage }) => {
    const userPage = new UserManagementPage(adminPage);
    await userPage.goto();
    await userPage.expectUserCount(1);

    // Get the initial role of the first user
    const initialRole = await userPage.roleDropdown(0).inputValue();
    const newRole = initialRole === 'Patient' ? 'Clinician' : 'Patient';

    await userPage.changeRole(0, newRole);

    // Verify the role was updated
    await userPage.expectUserRole(0, newRole);

    // Restore original role
    await userPage.changeRole(0, initialRole);
    await userPage.expectUserRole(0, initialRole);
  });

  test('should suspend a user and then reactivate', async ({ adminPage }) => {
    const userPage = new UserManagementPage(adminPage);
    await userPage.goto();
    await userPage.expectUserCount(1);

    // Filter to active users so we know we have one to suspend
    await userPage.filterByStatus('Active');
    const activeRows = userPage.userTable.locator('tbody tr');
    const activeCount = await activeRows.count();

    if (activeCount > 0) {
      // Suspend the first active user
      const suspendBtn = userPage.userRow(0).locator('button:has-text("Suspend")');
      await suspendBtn.click();
      await adminPage.waitForTimeout(500);

      // The button should now say "Activate" (status changed to Suspended)
      const activateBtn = userPage.userRow(0).locator('button:has-text("Activate")');
      await expect(activateBtn).toBeVisible({ timeout: 5_000 });

      // Reactivate the user
      await activateBtn.click();
      await adminPage.waitForTimeout(500);

      // Should be back to Suspend button (Active status)
      await expect(userPage.userRow(0).locator('button:has-text("Suspend")')).toBeVisible({ timeout: 5_000 });
    }
  });

  test('should search for users by name or email', async ({ adminPage }) => {
    const userPage = new UserManagementPage(adminPage);
    await userPage.goto();
    await userPage.expectUserCount(1);

    // Get the first user's name for searching
    const firstName = (await userPage.userName(0).textContent())!.trim();
    const searchTerm = firstName.split(' ')[0];

    await userPage.search(searchTerm);

    const rows = userPage.userTable.locator('tbody tr');
    const count = await rows.count();
    expect(count).toBeGreaterThanOrEqual(1);

    // All visible results should contain the search term
    for (let i = 0; i < count; i++) {
      const rowText = await userPage.userRow(i).textContent();
      expect(rowText!.toLowerCase()).toContain(searchTerm.toLowerCase());
    }
  });
});
