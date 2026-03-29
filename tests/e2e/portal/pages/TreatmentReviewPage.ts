import { type Page, type Locator, expect } from '@playwright/test';

export class TreatmentReviewPage {
  readonly page: Page;
  readonly reviewQueue: Locator;
  readonly confirmDialog: Locator;
  readonly confirmApproveButton: Locator;
  readonly confirmRejectButton: Locator;
  readonly rejectReasonTextarea: Locator;
  readonly approveNotesTextarea: Locator;

  constructor(page: Page) {
    this.page = page;

    // Review cards are bordered containers in the review queue
    this.reviewQueue = page.locator('.rounded-lg.border.border-border.bg-white');

    // Dialog elements
    this.confirmDialog = page.locator('[role="dialog"]');
    this.confirmApproveButton = this.confirmDialog.getByRole('button', {
      name: /Confirm Approval/i,
    });
    this.confirmRejectButton = this.confirmDialog.getByRole('button', {
      name: /Confirm Rejection/i,
    });
    this.rejectReasonTextarea = this.confirmDialog.locator(
      'textarea[placeholder*="Reason for rejection"]',
    );
    this.approveNotesTextarea = this.confirmDialog.locator(
      'textarea[placeholder*="optional notes"]',
    );
  }

  reviewCard(index: number): Locator {
    return this.reviewQueue.nth(index);
  }

  patientName(index: number): Locator {
    return this.reviewCard(index).locator('h4').first();
  }

  diagnosisSummary(index: number): Locator {
    return this.reviewCard(index).locator('p.text-xs').first();
  }

  proposedPlan(index: number): Locator {
    return this.reviewCard(index).locator('h5').first();
  }

  approveButton(index: number): Locator {
    return this.reviewCard(index).getByRole('button', { name: 'Approve' });
  }

  rejectButton(index: number): Locator {
    return this.reviewCard(index).getByRole('button', { name: 'Reject' });
  }

  async goto() {
    await this.page.goto('/treatment-reviews');
    await this.page.waitForLoadState('networkidle');
  }

  async approveReview(index: number) {
    await this.approveButton(index).click();
    await this.confirmDialog.waitFor({ state: 'visible' });
  }

  async rejectReview(index: number, reason: string) {
    await this.rejectButton(index).click();
    await this.confirmDialog.waitFor({ state: 'visible' });
    await this.rejectReasonTextarea.fill(reason);
  }

  async confirmApproval() {
    await this.confirmApproveButton.click();
  }

  async confirmRejection() {
    await this.confirmRejectButton.click();
  }

  async expectReviewCount(count: number) {
    await expect(this.reviewQueue).toHaveCount(count);
  }

  async expectReviewPatient(index: number, name: string) {
    await expect(this.patientName(index)).toHaveText(name);
  }
}
