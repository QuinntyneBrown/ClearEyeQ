import { test, expect } from '../fixtures/auth.fixture';
import { TreatmentReviewPage } from '../pages/TreatmentReviewPage';
import { SidebarNav } from '../pages/SidebarNav';

test.describe('Treatment Review Flow', () => {
  let reviews: TreatmentReviewPage;
  let sidebar: SidebarNav;

  test.beforeEach(async ({ authenticatedPage }) => {
    reviews = new TreatmentReviewPage(authenticatedPage);
    sidebar = new SidebarNav(authenticatedPage);
  });

  test('navigates to Treatment Reviews via sidebar and shows the review queue', async ({
    authenticatedPage,
  }) => {
    await sidebar.navigateTo('Treatment Reviews');
    await expect(authenticatedPage).toHaveURL(/\/treatment-reviews/);

    // Mock data has 4 reviews
    const count = await reviews.reviewQueue.count();
    expect(count).toBeGreaterThanOrEqual(1);
  });

  test('each review card shows patient name, diagnosis, and proposed plan', async ({
    authenticatedPage,
  }) => {
    await reviews.goto();

    const count = await reviews.reviewQueue.count();
    expect(count).toBeGreaterThanOrEqual(1);

    // Check the first review card
    const patientNameText = await reviews.patientName(0).textContent();
    expect(patientNameText).toBeTruthy();

    const diagnosisText = await reviews.diagnosisSummary(0).textContent();
    expect(diagnosisText).toBeTruthy();

    const planText = await reviews.proposedPlan(0).textContent();
    expect(planText).toBeTruthy();
    expect(planText).toContain('Proposed Treatment');
  });

  test('approving a treatment plan shows confirmation dialog and completes', async ({
    authenticatedPage,
  }) => {
    await reviews.goto();

    // Click Approve on the first review
    await reviews.approveReview(0);

    // Confirm dialog should be visible
    await expect(reviews.confirmDialog).toBeVisible();
    await expect(
      authenticatedPage.locator('text=Approve Treatment Plan'),
    ).toBeVisible();

    // Confirm the approval
    await reviews.confirmApproval();

    // Dialog should close
    await authenticatedPage.waitForTimeout(500);
    await expect(authenticatedPage.locator('body')).toBeVisible();
  });

  test('rejecting a treatment plan requires a reason and shows confirmation', async ({
    authenticatedPage,
  }) => {
    await reviews.goto();

    const count = await reviews.reviewQueue.count();
    const lastIndex = count - 1;

    // Click Reject on the last review
    await reviews.rejectReview(lastIndex, 'Insufficient evidence for this treatment approach. Requesting additional diagnostic data.');

    // Confirm dialog should be visible
    await expect(reviews.confirmDialog).toBeVisible();
    await expect(
      authenticatedPage.locator('text=Reject Treatment Plan'),
    ).toBeVisible();

    // The reject reason should be filled
    await expect(reviews.rejectReasonTextarea).toHaveValue(
      'Insufficient evidence for this treatment approach. Requesting additional diagnostic data.',
    );

    // Confirm the rejection
    await reviews.confirmRejection();

    // Dialog should close
    await authenticatedPage.waitForTimeout(500);
    await expect(authenticatedPage.locator('body')).toBeVisible();
  });
});
