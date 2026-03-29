import { by, element, expect, waitFor } from 'detox';

class ScanPage {
  // --- Element selectors ---

  screenContainer() {
    return element(by.id('scan-screen'));
  }

  eyeSelectionLeft() {
    return element(by.id('scan-eye-left'));
  }

  eyeSelectionRight() {
    return element(by.id('scan-eye-right'));
  }

  startScanButton() {
    return element(by.id('scan-start-button'));
  }

  cameraViewfinder() {
    return element(by.id('scan-camera-viewfinder'));
  }

  positioningGuide() {
    return element(by.id('scan-positioning-guide'));
  }

  captureButton() {
    return element(by.id('scan-capture-button'));
  }

  processingSpinner() {
    return element(by.id('scan-processing-spinner'));
  }

  processingProgress() {
    return element(by.id('scan-processing-progress'));
  }

  resultsContainer() {
    return element(by.id('scan-results'));
  }

  resultsScore() {
    return element(by.id('scan-results-score'));
  }

  resultsConditionChips() {
    return element(by.id('scan-results-condition-chips'));
  }

  viewDiagnosisButton() {
    return element(by.id('scan-view-diagnosis-button'));
  }

  rescanButton() {
    return element(by.id('scan-rescan-button'));
  }

  shareButton() {
    return element(by.id('scan-share-button'));
  }

  positioningFeedback() {
    return element(by.id('scan-positioning-feedback'));
  }

  closeButton() {
    return element(by.id('scan-close-button'));
  }

  // --- Actions ---

  async selectEye(side: 'left' | 'right'): Promise<void> {
    if (side === 'left') {
      await this.eyeSelectionLeft().tap();
    } else {
      await this.eyeSelectionRight().tap();
    }
  }

  async startScan(): Promise<void> {
    await this.startScanButton().tap();
  }

  async capture(): Promise<void> {
    await waitFor(this.captureButton())
      .toBeVisible()
      .withTimeout(10000);
    await this.captureButton().tap();
  }

  async waitForProcessing(): Promise<void> {
    await waitFor(this.processingSpinner())
      .toBeVisible()
      .withTimeout(5000);
    await waitFor(this.resultsContainer())
      .toBeVisible()
      .withTimeout(60000);
  }

  async tapViewDiagnosis(): Promise<void> {
    await this.viewDiagnosisButton().tap();
  }

  async tapRescan(): Promise<void> {
    await this.rescanButton().tap();
  }

  async tapShare(): Promise<void> {
    await this.shareButton().tap();
  }

  async tapClose(): Promise<void> {
    await this.closeButton().tap();
  }

  // --- Assertions ---

  async isVisible(): Promise<void> {
    await waitFor(this.screenContainer())
      .toBeVisible()
      .withTimeout(10000);
  }

  async showsPositioningFeedback(): Promise<void> {
    await waitFor(this.positioningFeedback())
      .toBeVisible()
      .withTimeout(5000);
  }

  async showsResults(): Promise<void> {
    await waitFor(this.resultsContainer())
      .toBeVisible()
      .withTimeout(10000);
  }

  async hasScore(score: string): Promise<void> {
    await expect(this.resultsScore()).toHaveText(score);
  }

  async cameraIsVisible(): Promise<void> {
    await waitFor(this.cameraViewfinder())
      .toBeVisible()
      .withTimeout(10000);
  }

  async positioningGuideIsVisible(): Promise<void> {
    await expect(this.positioningGuide()).toBeVisible();
  }

  async conditionChipsAreVisible(): Promise<void> {
    await expect(this.resultsConditionChips()).toBeVisible();
  }
}

export default new ScanPage();
