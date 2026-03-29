import { device } from 'detox';
import LoginPage from '../pages/LoginPage';
import SignupPage from '../pages/SignupPage';
import OnboardingPage from '../pages/OnboardingPage';
import DashboardPage from '../pages/DashboardPage';
import SettingsPage from '../pages/SettingsPage';
import TabBar from '../pages/TabBar';

describe('Authentication Flow', () => {
  beforeEach(async () => {
    await device.reloadReactNative();
  });

  it('should show the login screen on app launch', async () => {
    await LoginPage.isVisible();
  });

  it('should show an error when logging in with invalid credentials', async () => {
    await LoginPage.isVisible();
    await LoginPage.login('testuser@cleareyeq.com', 'WrongPassword1!');
    await LoginPage.hasError('Invalid email or password. Please try again.');
  });

  it('should navigate to signup and complete registration with onboarding', async () => {
    await LoginPage.isVisible();
    await LoginPage.tapCreateAccount();

    await SignupPage.isVisible();
    await SignupPage.acceptTerms();
    await SignupPage.signup('Test User', 'newuser@cleareyeq.com', 'SecurePass123!');

    await OnboardingPage.isVisible();
    await OnboardingPage.grantCameraPermission();
    await OnboardingPage.grantNotificationPermission();
    await OnboardingPage.completeStep();

    await DashboardPage.isVisible();
  });

  it('should login successfully and navigate to dashboard', async () => {
    await LoginPage.isVisible();
    await LoginPage.login('testuser@cleareyeq.com', 'TestPass123!');
    await DashboardPage.isVisible();
  });

  it('should logout from settings and return to login screen', async () => {
    await LoginPage.isVisible();
    await LoginPage.login('testuser@cleareyeq.com', 'TestPass123!');
    await DashboardPage.isVisible();

    await TabBar.tapMore();
    await SettingsPage.isVisible();
    await SettingsPage.tapLogout();

    await LoginPage.isVisible();
  });
});
