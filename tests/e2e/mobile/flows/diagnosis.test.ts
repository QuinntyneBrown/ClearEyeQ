import { device } from 'detox';
import { loginAsTestUser } from '../helpers/auth';
import ScanPage from '../pages/ScanPage';
import DiagnosisPage from '../pages/DiagnosisPage';
import TabBar from '../pages/TabBar';

describe('Diagnosis Detail Flow', () => {
  beforeEach(async () => {
    await device.reloadReactNative();
    await loginAsTestUser();

    // Navigate through scan to reach diagnosis
    await TabBar.tapScan();
    await ScanPage.isVisible();
    await ScanPage.selectEye('right');
    await ScanPage.startScan();
    await ScanPage.cameraIsVisible();
    await ScanPage.capture();
    await ScanPage.waitForProcessing();
    await ScanPage.showsResults();
    await ScanPage.tapViewDiagnosis();
  });

  it('should display the diagnosis screen', async () => {
    await DiagnosisPage.isVisible();
  });

  it('should show a differential diagnosis list with conditions', async () => {
    await DiagnosisPage.isVisible();
    await DiagnosisPage.diagnosisListIsVisible();
    await DiagnosisPage.hasDiagnosisCount(1);
  });

  it('should show confidence bar and severity badge for each condition', async () => {
    await DiagnosisPage.isVisible();
    await DiagnosisPage.hasConfidenceBar(0);
    await DiagnosisPage.hasSeverityBadge(0);
  });

  it('should display root causes card with contributing factors', async () => {
    await DiagnosisPage.isVisible();
    await DiagnosisPage.hasRootCauses();
  });

  it('should navigate back to scan results when tapping back', async () => {
    await DiagnosisPage.isVisible();
    await DiagnosisPage.tapBack();
    await ScanPage.showsResults();
  });
});
