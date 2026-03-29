import { by, element, expect, waitFor } from 'detox';

class LoginPage {
  // --- Element selectors ---

  emailInput() {
    return element(by.id('login-email-input'));
  }

  passwordInput() {
    return element(by.id('login-password-input'));
  }

  loginButton() {
    return element(by.id('login-button'));
  }

  socialAppleButton() {
    return element(by.id('login-apple-button'));
  }

  socialGoogleButton() {
    return element(by.id('login-google-button'));
  }

  createAccountLink() {
    return element(by.id('login-create-account-link'));
  }

  forgotPasswordLink() {
    return element(by.id('login-forgot-password-link'));
  }

  errorMessage() {
    return element(by.id('login-error-message'));
  }

  screenContainer() {
    return element(by.id('login-screen'));
  }

  // --- Actions ---

  async login(email: string, password: string): Promise<void> {
    await this.emailInput().clearText();
    await this.emailInput().typeText(email);
    await this.passwordInput().clearText();
    await this.passwordInput().typeText(password);
    await this.loginButton().tap();
  }

  async tapCreateAccount(): Promise<void> {
    await this.createAccountLink().tap();
  }

  async tapForgotPassword(): Promise<void> {
    await this.forgotPasswordLink().tap();
  }

  async tapAppleSignIn(): Promise<void> {
    await this.socialAppleButton().tap();
  }

  async tapGoogleSignIn(): Promise<void> {
    await this.socialGoogleButton().tap();
  }

  // --- Assertions ---

  async isVisible(): Promise<void> {
    await waitFor(this.screenContainer())
      .toBeVisible()
      .withTimeout(10000);
  }

  async hasError(message: string): Promise<void> {
    await waitFor(this.errorMessage())
      .toBeVisible()
      .withTimeout(5000);
    await expect(this.errorMessage()).toHaveText(message);
  }
}

export default new LoginPage();
