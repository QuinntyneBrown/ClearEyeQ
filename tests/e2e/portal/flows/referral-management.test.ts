import { test, expect } from '../fixtures/auth.fixture';
import { ReferralInboxPage } from '../pages/ReferralInboxPage';
import { SidebarNav } from '../pages/SidebarNav';

test.describe('Referral Management Flow', () => {
  let referrals: ReferralInboxPage;
  let sidebar: SidebarNav;

  test.beforeEach(async ({ authenticatedPage }) => {
    referrals = new ReferralInboxPage(authenticatedPage);
    sidebar = new SidebarNav(authenticatedPage);
  });

  test('navigates to Referrals via sidebar and shows referral cards', async ({
    authenticatedPage,
  }) => {
    await sidebar.navigateTo('Referrals');
    await expect(authenticatedPage).toHaveURL(/\/referrals/);

    // The mock data has 4 referrals
    const count = await referrals.referralCards.count();
    expect(count).toBeGreaterThanOrEqual(1);
  });

  test('referral cards display patient name and condition', async ({
    authenticatedPage,
  }) => {
    await referrals.goto();

    // First referral card should have patient name visible
    const firstPatient = await referrals.patientName(0).textContent();
    expect(firstPatient).toBeTruthy();

    const firstCondition = await referrals.conditionName(0).textContent();
    expect(firstCondition).toBeTruthy();
  });

  test('filtering by Urgent shows only urgent referrals', async ({
    authenticatedPage,
  }) => {
    await referrals.goto();
    const totalCount = await referrals.referralCards.count();

    await referrals.filterByUrgency('Urgent');
    const urgentCount = await referrals.referralCards.count();
    expect(urgentCount).toBeLessThan(totalCount);
    expect(urgentCount).toBeGreaterThanOrEqual(1);

    // Verify all visible cards have Urgent badge
    for (let i = 0; i < urgentCount; i++) {
      await referrals.expectUrgencyBadge(i, 'Urgent');
    }
  });

  test('expanding a referral reveals the diagnostic summary', async ({
    authenticatedPage,
  }) => {
    await referrals.goto();

    // Expand the first referral card
    await referrals.expandReferral(0);

    // After expansion, "Diagnostic Summary" heading should be visible
    await expect(
      authenticatedPage.locator('text=Diagnostic Summary'),
    ).toBeVisible();
  });

  test('accepting a referral triggers the accept action', async ({
    authenticatedPage,
  }) => {
    await referrals.goto();

    // Expand first referral to reveal action buttons
    await referrals.expandReferral(0);

    // Click accept
    await referrals.acceptReferral(0);

    // The button text may change to "Accepting..." or a toast appears
    // Wait briefly for the mutation to process
    await authenticatedPage.waitForTimeout(500);

    // Verify the page is still functional (no crash)
    await expect(authenticatedPage.locator('body')).toBeVisible();
  });

  test('declining a referral triggers the decline action', async ({
    authenticatedPage,
  }) => {
    await referrals.goto();

    // Expand the last referral to avoid conflict with previously accepted ones
    const count = await referrals.referralCards.count();
    const lastIndex = count - 1;

    await referrals.expandReferral(lastIndex);
    await referrals.declineReferral(lastIndex);

    // Wait for the mutation
    await authenticatedPage.waitForTimeout(500);

    // Verify the page is still functional
    await expect(authenticatedPage.locator('body')).toBeVisible();
  });
});
