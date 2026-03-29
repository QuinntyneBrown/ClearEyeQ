import { device } from 'detox';
import { loginAsTestUser } from '../helpers/auth';
import ScanPage from '../pages/ScanPage';
import DiagnosisPage from '../pages/DiagnosisPage';
import TabBar from '../pages/TabBar';

describe('Eye Scan Flow', () => {
  beforeEach(async () => {
    await device.reloadReactNative();
    await loginAsTestUser();
    await TabBar.tapScan();
  });

  it('should display the scan screen with eye selection', async () => {
    await ScanPage.isVisible();
  });

  it('should select the right eye and start a scan', async () => {
    await ScanPage.isVisible();
    await ScanPage.selectEye('right');
    await ScanPage.startScan();
    await ScanPage.cameraIsVisible();
  });

  it('should show the camera viewfinder with positioning guide', async () => {
    await ScanPage.isVisible();
    await ScanPage.selectEye('right');
    await ScanPage.startScan();
    await ScanPage.cameraIsVisible();
    await ScanPage.positioningGuideIsVisible();
  });

  it('should capture and show processing then results', async () => {
    await ScanPage.isVisible();
    await ScanPage.selectEye('right');
    await ScanPage.startScan();
    await ScanPage.cameraIsVisible();
    await ScanPage.capture();
    await ScanPage.waitForProcessing();
    await ScanPage.showsResults();
    await ScanPage.conditionChipsAreVisible();
  });

  it('should navigate to diagnosis detail from results', async () => {
    await ScanPage.isVisible();
    await ScanPage.selectEye('right');
    await ScanPage.startScan();
    await ScanPage.cameraIsVisible();
    await ScanPage.capture();
    await ScanPage.waitForProcessing();
    await ScanPage.showsResults();

    await ScanPage.tapViewDiagnosis();
    await DiagnosisPage.isVisible();
  });

  it('should navigate back from diagnosis to results', async () => {
    await ScanPage.isVisible();
    await ScanPage.selectEye('right');
    await ScanPage.startScan();
    await ScanPage.cameraIsVisible();
    await ScanPage.capture();
    await ScanPage.waitForProcessing();
    await ScanPage.showsResults();

    await ScanPage.tapViewDiagnosis();
    await DiagnosisPage.isVisible();
    await DiagnosisPage.tapBack();
    await ScanPage.showsResults();
  });

  it('should return to camera when tapping rescan', async () => {
    await ScanPage.isVisible();
    await ScanPage.selectEye('right');
    await ScanPage.startScan();
    await ScanPage.cameraIsVisible();
    await ScanPage.capture();
    await ScanPage.waitForProcessing();
    await ScanPage.showsResults();

    await ScanPage.tapRescan();
    await ScanPage.isVisible();
  });
});
