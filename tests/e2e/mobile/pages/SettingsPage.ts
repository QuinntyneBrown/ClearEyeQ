import { by, element, expect, waitFor } from 'detox';

class SettingsPage {
  // --- Element selectors ---

  screenContainer() {
    return element(by.id('settings-screen'));
  }

  profileCard() {
    return element(by.id('settings-profile-card'));
  }

  profileName() {
    return element(by.id('settings-profile-name'));
  }

  profileEmail() {
    return element(by.id('settings-profile-email'));
  }

  connectedAppsList() {
    return element(by.id('settings-connected-apps-list'));
  }

  connectedApp(name: string) {
    return element(by.id(`settings-connected-app-${name.toLowerCase().replace(/\s+/g, '-')}`));
  }

  connectedAppStatus(name: string) {
    return element(by.id(`settings-connected-app-status-${name.toLowerCase().replace(/\s+/g, '-')}`));
  }

  privacyToggles() {
    return element(by.id('settings-privacy-toggles'));
  }

  passiveMonitoringToggle() {
    return element(by.id('settings-passive-monitoring-toggle'));
  }

  dataShareToggle() {
    return element(by.id('settings-data-share-toggle'));
  }

  exportDataButton() {
    return element(by.id('settings-export-data-button'));
  }

  subscriptionCards() {
    return element(by.id('settings-subscription-cards'));
  }

  subscriptionCard(tier: string) {
    return element(by.id(`settings-subscription-${tier.toLowerCase()}`));
  }

  currentPlanBadge() {
    return element(by.id('settings-current-plan-badge'));
  }

  deleteAccountButton() {
    return element(by.id('settings-delete-account-button'));
  }

  logoutButton() {
    return element(by.id('settings-logout-button'));
  }

  // --- Actions ---

  async tapProfile(): Promise<void> {
    await this.profileCard().tap();
  }

  async togglePassiveMonitoring(): Promise<void> {
    await this.passiveMonitoringToggle().tap();
  }

  async toggleDataSharing(): Promise<void> {
    await this.dataShareToggle().tap();
  }

  async tapExportData(): Promise<void> {
    await this.exportDataButton().tap();
  }

  async tapUpgradePlan(tier: string): Promise<void> {
    await this.subscriptionCard(tier).tap();
  }

  async tapDeleteAccount(): Promise<void> {
    await this.deleteAccountButton().tap();
  }

  async tapLogout(): Promise<void> {
    await this.logoutButton().tap();
  }

  // --- Assertions ---

  async isVisible(): Promise<void> {
    await waitFor(this.screenContainer())
      .toBeVisible()
      .withTimeout(15000);
  }

  async hasConnectedApp(name: string, connected: boolean): Promise<void> {
    await waitFor(this.connectedApp(name))
      .toBeVisible()
      .withTimeout(5000);
  }

  async hasCurrentPlan(tier: string): Promise<void> {
    await waitFor(this.currentPlanBadge())
      .toBeVisible()
      .withTimeout(5000);
  }

  async profileIsVisible(): Promise<void> {
    await waitFor(this.profileCard())
      .toBeVisible()
      .withTimeout(10000);
  }

  async deleteAccountButtonIsVisible(): Promise<void> {
    await expect(this.deleteAccountButton()).toBeVisible();
  }
}

export default new SettingsPage();
