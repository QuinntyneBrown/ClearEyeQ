import { by, element, expect, waitFor } from 'detox';

class TreatmentPage {
  // --- Element selectors ---

  screenContainer() {
    return element(by.id('treatment-screen'));
  }

  phaseStepper() {
    return element(by.id('treatment-phase-stepper'));
  }

  phaseStep(number: number) {
    return element(by.id(`treatment-phase-${number}`));
  }

  nextActionCard() {
    return element(by.id('treatment-next-action-card'));
  }

  interventionCards() {
    return element(by.id('treatment-intervention-cards'));
  }

  interventionCard(name: string) {
    return element(by.id(`treatment-intervention-${name.toLowerCase().replace(/\s+/g, '-')}`));
  }

  interventionToggle(name: string) {
    return element(by.id(`treatment-intervention-toggle-${name.toLowerCase().replace(/\s+/g, '-')}`));
  }

  progressSection() {
    return element(by.id('treatment-progress-section'));
  }

  referralCard() {
    return element(by.id('treatment-referral-card'));
  }

  // --- Actions ---

  async tapIntervention(name: string): Promise<void> {
    await this.interventionCard(name).tap();
  }

  async toggleIntervention(name: string): Promise<void> {
    await this.interventionToggle(name).tap();
  }

  // --- Assertions ---

  async isVisible(): Promise<void> {
    await waitFor(this.screenContainer())
      .toBeVisible()
      .withTimeout(15000);
  }

  async hasPhase(number: number, status: string): Promise<void> {
    await waitFor(this.phaseStep(number))
      .toBeVisible()
      .withTimeout(5000);
  }

  async hasInterventionCount(count: number): Promise<void> {
    await waitFor(this.interventionCards())
      .toBeVisible()
      .withTimeout(10000);
  }

  async phaseStepperIsVisible(): Promise<void> {
    await waitFor(this.phaseStepper())
      .toBeVisible()
      .withTimeout(10000);
  }

  async nextActionCardIsVisible(): Promise<void> {
    await waitFor(this.nextActionCard())
      .toBeVisible()
      .withTimeout(10000);
  }

  async interventionCardsAreVisible(): Promise<void> {
    await waitFor(this.interventionCards())
      .toBeVisible()
      .withTimeout(10000);
  }
}

export default new TreatmentPage();
