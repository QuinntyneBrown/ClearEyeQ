import { type Page, type Locator, expect } from '@playwright/test';

export class ReferralInboxPage {
  readonly page: Page;
  readonly urgencyTabs: Locator;
  readonly allTab: Locator;
  readonly urgentTab: Locator;
  readonly standardTab: Locator;
  readonly referralCards: Locator;
  readonly confirmDialog: Locator;
  readonly confirmButton: Locator;

  constructor(page: Page) {
    this.page = page;

    // Urgency filter tabs
    this.urgencyTabs = page.locator('[role="tablist"]');
    this.allTab = page.getByRole('tab', { name: /^All/ });
    this.urgentTab = page.getByRole('tab', { name: /^Urgent/ });
    this.standardTab = page.getByRole('tab', { name: /^Standard/ });

    // Referral cards are the bordered divs in the list
    this.referralCards = page.locator('.rounded-lg.border.border-border.bg-white');

    // Confirm dialog (if the app presents one)
    this.confirmDialog = page.locator('[role="alertdialog"], [role="dialog"]');
    this.confirmButton = this.confirmDialog.getByRole('button', { name: /confirm/i });
  }

  referralCard(index: number): Locator {
    return this.referralCards.nth(index);
  }

  patientName(index: number): Locator {
    return this.referralCard(index).locator('h4').first();
  }

  conditionName(index: number): Locator {
    return this.referralCard(index).locator('p.text-xs').first();
  }

  urgencyBadge(index: number): Locator {
    return this.referralCard(index).locator('[class*="badge"], [class*="Badge"]').first();
  }

  expandButton(index: number): Locator {
    return this.referralCard(index).locator('button').first();
  }

  acceptButton(index: number): Locator {
    return this.referralCard(index).getByRole('button', {
      name: /Accept Referral/i,
    });
  }

  declineButton(index: number): Locator {
    return this.referralCard(index).getByRole('button', {
      name: /Decline/i,
    });
  }

  async goto() {
    await this.page.goto('/referrals');
    await this.page.waitForLoadState('networkidle');
  }

  async filterByUrgency(level: string) {
    switch (level) {
      case 'All':
        await this.allTab.click();
        break;
      case 'Urgent':
        await this.urgentTab.click();
        break;
      case 'Standard':
        await this.standardTab.click();
        break;
    }
    await this.page.waitForTimeout(300);
  }

  async expandReferral(index: number) {
    await this.expandButton(index).click();
    await this.page.waitForTimeout(200);
  }

  async acceptReferral(index: number) {
    await this.acceptButton(index).click();
  }

  async declineReferral(index: number) {
    await this.declineButton(index).click();
  }

  async confirmAction() {
    await this.confirmButton.click();
  }

  async expectReferralCount(count: number) {
    await expect(this.referralCards).toHaveCount(count);
  }

  async expectUrgencyBadge(index: number, level: string) {
    await expect(this.urgencyBadge(index)).toHaveText(level);
  }
}
