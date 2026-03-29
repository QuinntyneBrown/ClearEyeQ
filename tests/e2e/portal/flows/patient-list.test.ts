import { test, expect } from '../fixtures/auth.fixture';
import { PatientListPage } from '../pages/PatientListPage';
import { SidebarNav } from '../pages/SidebarNav';

test.describe('Patient List Flow', () => {
  let patientList: PatientListPage;
  let sidebar: SidebarNav;

  test.beforeEach(async ({ authenticatedPage }) => {
    patientList = new PatientListPage(authenticatedPage);
    sidebar = new SidebarNav(authenticatedPage);
  });

  test('navigates to Patients via sidebar and shows a table with rows', async ({
    authenticatedPage,
  }) => {
    await sidebar.navigateTo('Patients');
    await expect(authenticatedPage).toHaveURL(/\/patients/);

    await expect(patientList.patientTable).toBeVisible();
    const rowCount = await patientList.getPatientCount();
    expect(rowCount).toBeGreaterThan(0);
  });

  test('searching for a patient filters the table', async ({
    authenticatedPage,
  }) => {
    await patientList.goto();
    const initialCount = await patientList.getPatientCount();
    expect(initialCount).toBeGreaterThan(1);

    // Search for "Sarah" -- should filter to Sarah Chen
    await patientList.search('Sarah');
    const filteredCount = await patientList.getPatientCount();
    expect(filteredCount).toBeLessThan(initialCount);
    expect(filteredCount).toBeGreaterThanOrEqual(1);

    await patientList.expectPatientName(0, 'Sarah Chen');
  });

  test('clearing search restores the full list', async ({
    authenticatedPage,
  }) => {
    await patientList.goto();
    const initialCount = await patientList.getPatientCount();

    await patientList.search('Sarah');
    const filteredCount = await patientList.getPatientCount();
    expect(filteredCount).toBeLessThan(initialCount);

    // Clear the search
    await patientList.search('');
    const restoredCount = await patientList.getPatientCount();
    expect(restoredCount).toBe(initialCount);
  });

  test('filtering by Flagged status shows only flagged patients', async ({
    authenticatedPage,
  }) => {
    await patientList.goto();
    const initialCount = await patientList.getPatientCount();

    await patientList.filterByStatus('Flagged');
    const flaggedCount = await patientList.getPatientCount();
    expect(flaggedCount).toBeLessThan(initialCount);
    expect(flaggedCount).toBeGreaterThanOrEqual(1);

    // Verify the first patient has Flagged status
    await patientList.expectPatientStatus(0, 'Flagged');
  });

  test('clicking All filter restores the full list', async ({
    authenticatedPage,
  }) => {
    await patientList.goto();
    const initialCount = await patientList.getPatientCount();

    await patientList.filterByStatus('Flagged');
    const flaggedCount = await patientList.getPatientCount();
    expect(flaggedCount).toBeLessThan(initialCount);

    await patientList.filterByStatus('All');
    const restoredCount = await patientList.getPatientCount();
    expect(restoredCount).toBe(initialCount);
  });

  test('clicking View on a patient navigates to patient detail', async ({
    authenticatedPage,
  }) => {
    await patientList.goto();
    await patientList.clickPatient(0);

    await expect(authenticatedPage).toHaveURL(/\/patients\/p\d+/);
    // Patient detail page should show the patient name heading
    const heading = authenticatedPage.locator('h2.text-2xl');
    await expect(heading).toBeVisible();
  });
});
