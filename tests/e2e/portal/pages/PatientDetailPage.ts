import { type Page, type Locator, expect } from '@playwright/test';

export class PatientDetailPage {
  readonly page: Page;
  readonly patientHeader: Locator;
  readonly patientName: Locator;
  readonly statusBadge: Locator;
  readonly tabsList: Locator;
  readonly overviewTab: Locator;
  readonly scansTab: Locator;
  readonly diagnosisTab: Locator;
  readonly treatmentTab: Locator;
  readonly notesTab: Locator;
  readonly activeTabContent: Locator;
  readonly scanCards: Locator;
  readonly diagnosisList: Locator;
  readonly treatmentTimeline: Locator;
  readonly notesList: Locator;
  readonly newNoteTextarea: Locator;
  readonly submitNoteButton: Locator;
  readonly backLink: Locator;

  constructor(page: Page) {
    this.page = page;

    // Patient header contains name and badge
    this.patientHeader = page.locator('h2.text-2xl');
    this.patientName = this.patientHeader;
    this.statusBadge = page
      .locator('div')
      .filter({ has: this.patientHeader })
      .locator('[class*="badge"], [class*="Badge"]')
      .first();

    // Tabs
    this.tabsList = page.locator('[role="tablist"]');
    this.overviewTab = page.getByRole('tab', { name: 'Overview' });
    this.scansTab = page.getByRole('tab', { name: 'Scans' });
    this.diagnosisTab = page.getByRole('tab', { name: 'Diagnosis' });
    this.treatmentTab = page.getByRole('tab', { name: 'Treatment' });
    this.notesTab = page.getByRole('tab', { name: 'Notes' });

    // Tab content areas
    this.activeTabContent = page.locator('[role="tabpanel"]');

    // Scan gallery items
    this.scanCards = page.locator('[role="tabpanel"]').locator('[class*="rounded"]');

    // Diagnosis list items
    this.diagnosisList = page.locator('[role="tabpanel"]');

    // Treatment timeline
    this.treatmentTimeline = page.locator('[role="tabpanel"]');

    // Notes
    this.notesList = page.locator('[role="tabpanel"]').locator('.rounded-lg.border');
    this.newNoteTextarea = page.locator(
      'textarea[placeholder="Write a clinical note..."]',
    );
    this.submitNoteButton = page.getByRole('button', { name: 'Add Note' });

    // Back link
    this.backLink = page.getByRole('link', { name: 'Back to Patients' });
  }

  async goto(id: string) {
    await this.page.goto(`/patients/${id}`);
    await this.page.waitForLoadState('networkidle');
  }

  async selectTab(name: string) {
    const tab = this.page.getByRole('tab', { name });
    await tab.click();
    // Wait for tab panel content to appear
    await this.page.waitForTimeout(300);
  }

  async addNote(text: string) {
    await this.newNoteTextarea.fill(text);
  }

  async submitNote() {
    await this.submitNoteButton.click();
  }

  async expectPatientNameText(name: string) {
    await expect(this.patientName).toContainText(name);
  }

  async expectActiveTab(name: string) {
    const tab = this.page.getByRole('tab', { name });
    await expect(tab).toHaveAttribute('data-state', 'active');
  }

  async expectScanCount(count: number) {
    await this.selectTab('Scans');
    // Scan gallery shows cards for each scan
    const scanItems = this.activeTabContent.locator('[class*="rounded-lg"]');
    const actualCount = await scanItems.count();
    expect(actualCount).toBeGreaterThanOrEqual(count);
  }

  async expectNoteAdded(text: string) {
    const noteContent = this.page.locator(`text=${text}`);
    await expect(noteContent).toBeVisible();
  }
}
