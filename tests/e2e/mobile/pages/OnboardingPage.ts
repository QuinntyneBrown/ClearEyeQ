import { by, element, expect, waitFor } from 'detox';

class OnboardingPage {
  // --- Element selectors ---

  stepIndicator() {
    return element(by.id('onboarding-step-indicator'));
  }

  skipButton() {
    return element(by.id('onboarding-skip-button'));
  }

  continueButton() {
    return element(by.id('onboarding-continue-button'));
  }

  cameraPermissionButton() {
    return element(by.id('onboarding-camera-permission-button'));
  }

  notificationPermissionButton() {
    return element(by.id('onboarding-notification-permission-button'));
  }

  screenContainer() {
    return element(by.id('onboarding-screen'));
  }

  // --- Actions ---

  async completeStep(): Promise<void> {
    await waitFor(this.continueButton())
      .toBeVisible()
      .withTimeout(5000);
    await this.continueButton().tap();
  }

  async skip(): Promise<void> {
    await this.skipButton().tap();
  }

  async grantCameraPermission(): Promise<void> {
    await waitFor(this.cameraPermissionButton())
      .toBeVisible()
      .withTimeout(5000);
    await this.cameraPermissionButton().tap();
  }

  async grantNotificationPermission(): Promise<void> {
    await waitFor(this.notificationPermissionButton())
      .toBeVisible()
      .withTimeout(5000);
    await this.notificationPermissionButton().tap();
  }

  async completeAllSteps(): Promise<void> {
    await this.grantCameraPermission();
    await this.grantNotificationPermission();
    await this.completeStep();
  }

  // --- Assertions ---

  async isVisible(): Promise<void> {
    await waitFor(this.screenContainer())
      .toBeVisible()
      .withTimeout(10000);
  }

  async hasStepIndicator(): Promise<void> {
    await expect(this.stepIndicator()).toBeVisible();
  }
}

export default new OnboardingPage();
