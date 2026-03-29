import { test, expect } from '../fixtures/auth.fixture';
import { SidebarNav } from '../pages/SidebarNav';
import { TopBar } from '../pages/TopBar';
import { NAV_SECTIONS, NAV_ROUTES } from '../helpers/test-data';

test.describe('Navigation Flow', () => {
  let sidebar: SidebarNav;
  let topBar: TopBar;

  test.beforeEach(async ({ authenticatedPage }) => {
    sidebar = new SidebarNav(authenticatedPage);
    topBar = new TopBar(authenticatedPage);
    await authenticatedPage.goto('/');
    await authenticatedPage.waitForLoadState('networkidle');
  });

  test('sidebar contains all 5 navigation items', async ({
    authenticatedPage,
  }) => {
    await expect(sidebar.sidebar).toBeVisible();
    await expect(sidebar.dashboardLink).toBeVisible();
    await expect(sidebar.patientsLink).toBeVisible();
    await expect(sidebar.treatmentReviewsLink).toBeVisible();
    await expect(sidebar.referralsLink).toBeVisible();
    await expect(sidebar.settingsLink).toBeVisible();
  });

  test('clicking each nav item loads the correct page and marks it active', async ({
    authenticatedPage,
  }) => {
    for (const section of NAV_SECTIONS) {
      await sidebar.navigateTo(section);

      const expectedRoute = NAV_ROUTES[section];

      if (expectedRoute === '/') {
        await expect(authenticatedPage).toHaveURL(/\/$/);
      } else {
        await expect(authenticatedPage).toHaveURL(new RegExp(expectedRoute));
      }

      // Verify the sidebar item has the active class
      await sidebar.expectActiveSection(section);
    }
  });

  test('top bar search input is functional', async ({ authenticatedPage }) => {
    await expect(topBar.searchInput).toBeVisible();
    await expect(topBar.searchInput).toBeEditable();

    await topBar.search('test query');
    await expect(topBar.searchInput).toHaveValue('test query');
  });

  test('notification bell is visible and shows a count', async ({
    authenticatedPage,
  }) => {
    await expect(topBar.notificationBell).toBeVisible();

    // The notification count badge shows "3" in the mock
    await expect(topBar.notificationCount).toBeVisible();
    const countText = await topBar.notificationCount.textContent();
    expect(Number(countText)).toBeGreaterThanOrEqual(0);
  });

  test('user avatar opens a dropdown menu', async ({ authenticatedPage }) => {
    await expect(topBar.userAvatar).toBeVisible();

    await topBar.openUserMenu();
    await expect(topBar.userDropdown).toBeVisible();

    // Dropdown should contain Profile, Settings, and Logout items
    await expect(
      authenticatedPage.getByRole('menuitem', { name: 'Profile' }),
    ).toBeVisible();
    await expect(
      authenticatedPage.getByRole('menuitem', { name: 'Settings' }),
    ).toBeVisible();
    await expect(
      authenticatedPage.getByRole('menuitem', { name: 'Logout' }),
    ).toBeVisible();
  });

  test('page title in top bar updates with navigation', async ({
    authenticatedPage,
  }) => {
    // Dashboard
    await expect(topBar.pageTitle).toHaveText('Dashboard');

    // Patients
    await sidebar.navigateTo('Patients');
    await expect(topBar.pageTitle).toHaveText('Patients');

    // Treatment Reviews
    await sidebar.navigateTo('Treatment Reviews');
    await expect(topBar.pageTitle).toHaveText('Treatment Reviews');

    // Referrals
    await sidebar.navigateTo('Referrals');
    await expect(topBar.pageTitle).toHaveText('Referrals');

    // Settings
    await sidebar.navigateTo('Settings');
    await expect(topBar.pageTitle).toHaveText('Settings');
  });
});
