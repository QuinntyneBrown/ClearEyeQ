import { device } from 'detox';
import { loginAsTestUser } from '../helpers/auth';
import TreatmentPage from '../pages/TreatmentPage';
import TabBar from '../pages/TabBar';

describe('Treatment Plan Flow', () => {
  beforeEach(async () => {
    await device.reloadReactNative();
    await loginAsTestUser();
    await TabBar.tapTreat();
  });

  it('should display the treatment screen', async () => {
    await TreatmentPage.isVisible();
  });

  it('should show the phase stepper with at least one phase', async () => {
    await TreatmentPage.isVisible();
    await TreatmentPage.phaseStepperIsVisible();
    await TreatmentPage.hasPhase(1, 'completed');
  });

  it('should show the next action card with upcoming intervention', async () => {
    await TreatmentPage.isVisible();
    await TreatmentPage.nextActionCardIsVisible();
  });

  it('should display intervention cards list', async () => {
    await TreatmentPage.isVisible();
    await TreatmentPage.interventionCardsAreVisible();
  });

  it('should toggle an intervention off and back on', async () => {
    await TreatmentPage.isVisible();
    await TreatmentPage.interventionCardsAreVisible();

    // Toggle humidifier intervention off
    await TreatmentPage.toggleIntervention('humidifier');

    // Toggle humidifier intervention back on
    await TreatmentPage.toggleIntervention('humidifier');
  });
});
