import { by, element, expect, waitFor } from 'detox';

class DashboardPage {
  // --- Element selectors ---

  screenContainer() {
    return element(by.id('dashboard-screen'));
  }

  healthScoreCard() {
    return element(by.id('health-score-card'));
  }

  healthScoreValue() {
    return element(by.id('health-score-value'));
  }

  recentScanCard() {
    return element(by.id('recent-scan-card'));
  }

  scanNowButton() {
    return element(by.id('scan-now-button'));
  }

  environmentCards() {
    return element(by.id('environment-cards'));
  }

  environmentCard(type: string) {
    return element(by.id(`environment-card-${type.toLowerCase()}`));
  }

  dailyTipBanner() {
    return element(by.id('daily-tip-banner'));
  }

  tipDismissButton() {
    return element(by.id('daily-tip-dismiss-button'));
  }

  quickActionBar() {
    return element(by.id('quick-action-bar'));
  }

  quickAction(name: string) {
    return element(by.id(`quick-action-${name.toLowerCase()}`));
  }

  notificationButton() {
    return element(by.id('notification-button'));
  }

  greetingText() {
    return element(by.id('dashboard-greeting'));
  }

  // --- Actions ---

  async tapScanNow(): Promise<void> {
    await this.scanNowButton().tap();
  }

  async dismissTip(): Promise<void> {
    await this.tipDismissButton().tap();
  }

  async tapQuickAction(name: string): Promise<void> {
    await this.quickAction(name).tap();
  }

  async tapNotifications(): Promise<void> {
    await this.notificationButton().tap();
  }

  // --- Assertions ---

  async isVisible(): Promise<void> {
    await waitFor(this.screenContainer())
      .toBeVisible()
      .withTimeout(15000);
  }

  async hasHealthScore(score: string): Promise<void> {
    await waitFor(this.healthScoreCard())
      .toBeVisible()
      .withTimeout(10000);
    await expect(this.healthScoreValue()).toHaveText(score);
  }

  async hasEnvironmentCard(type: string): Promise<void> {
    await waitFor(this.environmentCard(type))
      .toBeVisible()
      .withTimeout(5000);
  }

  async dailyTipIsVisible(): Promise<void> {
    await expect(this.dailyTipBanner()).toBeVisible();
  }

  async dailyTipIsNotVisible(): Promise<void> {
    await expect(this.dailyTipBanner()).not.toBeVisible();
  }
}

export default new DashboardPage();
