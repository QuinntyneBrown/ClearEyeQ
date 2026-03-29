import { by, element, expect, waitFor } from 'detox';

class DiagnosisPage {
  // --- Element selectors ---

  screenContainer() {
    return element(by.id('diagnosis-screen'));
  }

  backButton() {
    return element(by.id('diagnosis-back-button'));
  }

  diagnosisList() {
    return element(by.id('diagnosis-list'));
  }

  diagnosisItem(index: number) {
    return element(by.id(`diagnosis-item-${index}`));
  }

  confidenceBar(index: number) {
    return element(by.id(`diagnosis-confidence-bar-${index}`));
  }

  severityBadge(index: number) {
    return element(by.id(`diagnosis-severity-badge-${index}`));
  }

  rootCausesCard() {
    return element(by.id('diagnosis-root-causes-card'));
  }

  rootCauseFactors() {
    return element(by.id('diagnosis-root-cause-factors'));
  }

  // --- Actions ---

  async tapBack(): Promise<void> {
    await this.backButton().tap();
  }

  async tapDiagnosis(index: number): Promise<void> {
    await this.diagnosisItem(index).tap();
  }

  // --- Assertions ---

  async isVisible(): Promise<void> {
    await waitFor(this.screenContainer())
      .toBeVisible()
      .withTimeout(10000);
  }

  async hasDiagnosisCount(count: number): Promise<void> {
    for (let i = 0; i < count; i++) {
      await expect(this.diagnosisItem(i)).toBeVisible();
    }
  }

  async hasConfidenceBar(index: number): Promise<void> {
    await expect(this.confidenceBar(index)).toBeVisible();
  }

  async hasSeverityBadge(index: number): Promise<void> {
    await expect(this.severityBadge(index)).toBeVisible();
  }

  async hasRootCauses(): Promise<void> {
    await waitFor(this.rootCausesCard())
      .toBeVisible()
      .withTimeout(5000);
    await expect(this.rootCauseFactors()).toBeVisible();
  }

  async diagnosisListIsVisible(): Promise<void> {
    await waitFor(this.diagnosisList())
      .toBeVisible()
      .withTimeout(10000);
  }
}

export default new DiagnosisPage();
