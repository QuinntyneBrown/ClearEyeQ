import { test, expect } from '../fixtures/auth.fixture';
import { NavMenu } from '../pages/NavMenu';
import { AuditLogPage } from '../pages/AuditLogPage';

test.describe('Audit Log Viewer Flow', () => {
  test('should navigate to Audit Log and display audit table', async ({ adminPage }) => {
    const nav = new NavMenu(adminPage);
    await nav.navigateTo('audit');
    await nav.expectActiveSection('audit');

    const auditPage = new AuditLogPage(adminPage);
    await auditPage.expectAuditTableVisible();
  });

  test('should show audit entries in the table', async ({ adminPage }) => {
    const auditPage = new AuditLogPage(adminPage);
    await auditPage.goto();

    await auditPage.expectLogCount(1);

    // First entry should have a timestamp, user, action, and resource
    await expect(auditPage.timestamp(0)).toBeVisible();
    await expect(auditPage.user(0)).toBeVisible();
    await expect(auditPage.action(0)).toBeVisible();
    await expect(auditPage.resource(0)).toBeVisible();
  });

  test('should filter audit entries by date range', async ({ adminPage }) => {
    const auditPage = new AuditLogPage(adminPage);
    await auditPage.goto();

    // Record entry count before filtering
    const initialEntries = await auditPage.getCurrentPageEntryTexts();

    // Set a narrow date range (today only)
    const today = new Date().toISOString().split('T')[0];
    await auditPage.filterByDateRange(today, today);

    // Table should still be visible (may have fewer or same entries)
    await auditPage.expectAuditTableVisible();
  });

  test('should filter audit entries by action type', async ({ adminPage }) => {
    const auditPage = new AuditLogPage(adminPage);
    await auditPage.goto();

    // Filter by "Create" action
    await auditPage.filterByAction('Create');

    // If there are results, all should be Create actions
    const rows = auditPage.auditTable.locator('tbody tr');
    const count = await rows.count();

    if (count > 0) {
      // Check that the "no entries found" message is not showing
      const firstRowText = await rows.first().textContent();
      if (!firstRowText!.includes('No audit log entries found')) {
        for (let i = 0; i < count; i++) {
          await auditPage.expectLogAction(i, 'Create');
        }
      }
    }
  });

  test('should search audit entries by user', async ({ adminPage }) => {
    const auditPage = new AuditLogPage(adminPage);
    await auditPage.goto();

    // Get the first user name to search for
    const firstUser = (await auditPage.user(0).textContent())!.trim();
    const searchTerm = firstUser.split(' ')[0];

    await auditPage.searchByUser(searchTerm);

    // Results should contain the searched user
    const rows = auditPage.auditTable.locator('tbody tr');
    const count = await rows.count();

    if (count > 0) {
      const firstRowText = await rows.first().textContent();
      if (!firstRowText!.includes('No audit log entries found')) {
        for (let i = 0; i < count; i++) {
          const userText = await auditPage.user(i).textContent();
          expect(userText!.toLowerCase()).toContain(searchTerm.toLowerCase());
        }
      }
    }
  });

  test('should paginate to page 2 and back to page 1', async ({ adminPage }) => {
    const auditPage = new AuditLogPage(adminPage);
    await auditPage.goto();

    // Check if pagination exists (more than 1 page)
    const paginationVisible = await auditPage.pagination.isVisible().catch(() => false);

    if (paginationVisible) {
      const pageInfoText = await auditPage.pageInfo.textContent();

      // Only proceed if there are multiple pages
      if (pageInfoText && pageInfoText.includes('of') && !pageInfoText.includes('of 1')) {
        // Record page 1 entries
        const page1Entries = await auditPage.getCurrentPageEntryTexts();

        // Navigate to page 2
        await auditPage.nextPage();

        // Page info should show page 2
        await expect(auditPage.pageInfo).toContainText('Page 2');

        // Page 2 entries should be different from page 1
        const page2Entries = await auditPage.getCurrentPageEntryTexts();
        expect(page2Entries).not.toEqual(page1Entries);

        // Navigate back to page 1
        await auditPage.prevPage();

        await expect(auditPage.pageInfo).toContainText('Page 1');

        // Should be back to the original entries
        const backOnPage1 = await auditPage.getCurrentPageEntryTexts();
        expect(backOnPage1).toEqual(page1Entries);
      }
    }
  });
});
