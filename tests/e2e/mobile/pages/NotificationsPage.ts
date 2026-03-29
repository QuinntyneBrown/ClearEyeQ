import { by, element, expect, waitFor } from 'detox';

class NotificationsPage {
  // --- Element selectors ---

  screenContainer() {
    return element(by.id('notifications-screen'));
  }

  forecastCards() {
    return element(by.id('notifications-forecast-cards'));
  }

  forecastCard(index: number) {
    return element(by.id(`notifications-forecast-card-${index}`));
  }

  alertsList() {
    return element(by.id('notifications-alerts-list'));
  }

  alertCard(index: number) {
    return element(by.id(`notifications-alert-${index}`));
  }

  urgentAlert() {
    return element(by.id('notifications-urgent-alert'));
  }

  treatmentReminder() {
    return element(by.id('notifications-treatment-reminder'));
  }

  flareUpWarning() {
    return element(by.id('notifications-flare-up-warning'));
  }

  findSpecialistButton() {
    return element(by.id('notifications-find-specialist-button'));
  }

  reminderDoneButton() {
    return element(by.id('notifications-reminder-done-button'));
  }

  // --- Actions ---

  async tapAlert(index: number): Promise<void> {
    await this.alertCard(index).tap();
  }

  async tapFindSpecialist(): Promise<void> {
    await this.findSpecialistButton().tap();
  }

  async tapReminderDone(): Promise<void> {
    await this.reminderDoneButton().tap();
  }

  // --- Assertions ---

  async isVisible(): Promise<void> {
    await waitFor(this.screenContainer())
      .toBeVisible()
      .withTimeout(10000);
  }

  async hasForecastDays(count: number): Promise<void> {
    for (let i = 0; i < count; i++) {
      await expect(this.forecastCard(i)).toBeVisible();
    }
  }

  async hasAlertCount(count: number): Promise<void> {
    for (let i = 0; i < count; i++) {
      await expect(this.alertCard(i)).toBeVisible();
    }
  }

  async urgentAlertIsVisible(): Promise<void> {
    await waitFor(this.urgentAlert())
      .toBeVisible()
      .withTimeout(5000);
  }

  async treatmentReminderIsVisible(): Promise<void> {
    await waitFor(this.treatmentReminder())
      .toBeVisible()
      .withTimeout(5000);
  }

  async flareUpWarningIsVisible(): Promise<void> {
    await waitFor(this.flareUpWarning())
      .toBeVisible()
      .withTimeout(5000);
  }
}

export default new NotificationsPage();
