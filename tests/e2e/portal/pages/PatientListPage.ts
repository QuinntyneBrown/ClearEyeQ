import { type Page, type Locator, expect } from '@playwright/test';

export class PatientListPage {
  readonly page: Page;
  readonly searchBar: Locator;
  readonly filterChips: Locator;
  readonly patientTable: Locator;
  readonly pagination: Locator;
  readonly emptyState: Locator;

  constructor(page: Page) {
    this.page = page;
    this.searchBar = page.locator(
      'input[placeholder="Search patients by name or email..."]',
    );
    this.filterChips = page.locator('button.rounded-full');
    this.patientTable = page.locator('table');
    this.pagination = page.getByTestId('pagination');
    this.emptyState = page.locator('text=No patients found');
  }

  filterChip(status: string): Locator {
    return this.filterChips.filter({ hasText: status });
  }

  patientRow(index: number): Locator {
    return this.patientTable.locator('tbody tr').nth(index);
  }

  patientName(index: number): Locator {
    return this.patientRow(index).locator('p.font-medium');
  }

  patientScore(index: number): Locator {
    return this.patientRow(index).locator('span.font-semibold');
  }

  patientStatus(index: number): Locator {
    return this.patientRow(index).locator('[class*="badge"], [class*="Badge"]');
  }

  viewButton(index: number): Locator {
    return this.patientRow(index).getByRole('link', { name: 'View' });
  }

  async goto() {
    await this.page.goto('/patients');
    await this.page.waitForLoadState('networkidle');
  }

  async search(query: string) {
    await this.searchBar.fill(query);
    // Allow React state update to filter
    await this.page.waitForTimeout(300);
  }

  async filterByStatus(status: string) {
    await this.filterChip(status).click();
    await this.page.waitForTimeout(300);
  }

  async clickPatient(index: number) {
    await this.viewButton(index).click();
    await this.page.waitForLoadState('networkidle');
  }

  async getPatientCount(): Promise<number> {
    return this.patientTable.locator('tbody tr').count();
  }

  async expectPatientCount(count: number) {
    await expect(this.patientTable.locator('tbody tr')).toHaveCount(count);
  }

  async expectPatientName(index: number, name: string) {
    await expect(this.patientName(index)).toHaveText(name);
  }

  async expectPatientStatus(index: number, status: string) {
    await expect(this.patientStatus(index)).toHaveText(status);
  }
}
