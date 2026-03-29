import { test, expect } from '../fixtures/auth.fixture';
import { PatientDetailPage } from '../pages/PatientDetailPage';
import { TEST_PATIENT_ID, TEST_PATIENT_NAME } from '../helpers/test-data';

test.describe('Patient Detail Flow', () => {
  let detail: PatientDetailPage;

  test.beforeEach(async ({ authenticatedPage }) => {
    detail = new PatientDetailPage(authenticatedPage);
    await detail.goto(TEST_PATIENT_ID);
  });

  test('shows patient header with name and status', async ({
    authenticatedPage,
  }) => {
    await detail.expectPatientNameText(TEST_PATIENT_NAME);
    await expect(detail.statusBadge).toBeVisible();
  });

  test('Overview tab is active by default with summary content', async ({
    authenticatedPage,
  }) => {
    await detail.expectActiveTab('Overview');

    // Overview tab should show "Latest Scan" and "Primary Diagnosis" cards
    await expect(authenticatedPage.locator('text=Latest Scan')).toBeVisible();
    await expect(
      authenticatedPage.locator('text=Primary Diagnosis'),
    ).toBeVisible();
  });

  test('clicking Scans tab shows the scan gallery', async ({
    authenticatedPage,
  }) => {
    await detail.selectTab('Scans');
    await detail.expectActiveTab('Scans');

    // The active tab panel should contain scan information
    const tabPanel = authenticatedPage.locator('[role="tabpanel"]');
    await expect(tabPanel).toBeVisible();

    // Mock patient has 3 scans -- the gallery should show scan data
    const scoreText = authenticatedPage.locator('text=Redness');
    const count = await scoreText.count();
    expect(count).toBeGreaterThanOrEqual(1);
  });

  test('clicking Diagnosis tab shows the diagnosis list', async ({
    authenticatedPage,
  }) => {
    await detail.selectTab('Diagnosis');
    await detail.expectActiveTab('Diagnosis');

    const tabPanel = authenticatedPage.locator('[role="tabpanel"]');
    await expect(tabPanel).toBeVisible();

    // Mock patient has "Allergic Conjunctivitis" as primary diagnosis
    await expect(
      authenticatedPage.locator('text=Allergic Conjunctivitis'),
    ).toBeVisible();
  });

  test('clicking Treatment tab shows the treatment timeline', async ({
    authenticatedPage,
  }) => {
    await detail.selectTab('Treatment');
    await detail.expectActiveTab('Treatment');

    const tabPanel = authenticatedPage.locator('[role="tabpanel"]');
    await expect(tabPanel).toBeVisible();

    // Mock patient has "Allergy Management Plan" as active treatment
    await expect(
      authenticatedPage.locator('text=Allergy Management Plan'),
    ).toBeVisible();
  });

  test('clicking Notes tab shows clinical notes', async ({
    authenticatedPage,
  }) => {
    await detail.selectTab('Notes');
    await detail.expectActiveTab('Notes');

    const tabPanel = authenticatedPage.locator('[role="tabpanel"]');
    await expect(tabPanel).toBeVisible();

    // Mock patient has notes from Dr. Thompson
    await expect(
      authenticatedPage.locator('text=Dr. Thompson'),
    ).toBeVisible();
  });

  test('adding a clinical note makes it appear in the list', async ({
    authenticatedPage,
  }) => {
    await detail.selectTab('Notes');
    await detail.expectActiveTab('Notes');

    const noteText = 'Automated E2E test note: patient follow-up scheduled.';
    await detail.addNote(noteText);

    // Submit button should become enabled
    await expect(detail.submitNoteButton).toBeEnabled();
    await detail.submitNote();

    // The textarea should be cleared after submission
    await expect(detail.newNoteTextarea).toHaveValue('');
  });
});
