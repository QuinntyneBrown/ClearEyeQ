import { by, element, expect, waitFor } from 'detox';

class SignupPage {
  // --- Element selectors ---

  nameInput() {
    return element(by.id('signup-name-input'));
  }

  emailInput() {
    return element(by.id('signup-email-input'));
  }

  passwordInput() {
    return element(by.id('signup-password-input'));
  }

  termsCheckbox() {
    return element(by.id('signup-terms-checkbox'));
  }

  createAccountButton() {
    return element(by.id('signup-create-account-button'));
  }

  socialAppleButton() {
    return element(by.id('signup-apple-button'));
  }

  socialGoogleButton() {
    return element(by.id('signup-google-button'));
  }

  loginLink() {
    return element(by.id('signup-login-link'));
  }

  screenContainer() {
    return element(by.id('signup-screen'));
  }

  fieldError(field: string) {
    return element(by.id(`signup-${field}-error`));
  }

  // --- Actions ---

  async signup(name: string, email: string, password: string): Promise<void> {
    await this.nameInput().clearText();
    await this.nameInput().typeText(name);
    await this.emailInput().clearText();
    await this.emailInput().typeText(email);
    await this.passwordInput().clearText();
    await this.passwordInput().typeText(password);
    await this.createAccountButton().tap();
  }

  async acceptTerms(): Promise<void> {
    await this.termsCheckbox().tap();
  }

  async tapLogin(): Promise<void> {
    await this.loginLink().tap();
  }

  // --- Assertions ---

  async isVisible(): Promise<void> {
    await waitFor(this.screenContainer())
      .toBeVisible()
      .withTimeout(10000);
  }

  async hasFieldError(field: string, message: string): Promise<void> {
    await waitFor(this.fieldError(field))
      .toBeVisible()
      .withTimeout(5000);
    await expect(this.fieldError(field)).toHaveText(message);
  }
}

export default new SignupPage();
