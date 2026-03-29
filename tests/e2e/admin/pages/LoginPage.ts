import { type Page, type Locator, expect } from '@playwright/test';
import { waitForBlazor } from '../helpers/blazor';

export class LoginPage {
  readonly page: Page;
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly loginButton: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page) {
    this.page = page;
    this.emailInput = page.locator('input[type="email"], input[name="email"], input[placeholder*="email" i], [data-testid="email-input"]');
    this.passwordInput = page.locator('input[type="password"], input[name="password"], [data-testid="password-input"]');
    this.loginButton = page.locator('button[type="submit"], button:has-text("Log in"), button:has-text("Sign in"), [data-testid="login-button"]');
    this.errorMessage = page.locator('.text-error, .alert-error, .validation-message, [data-testid="error-message"], .text-danger');
  }

  async goto(): Promise<void> {
    await this.page.goto('/');
    await this.page.waitForLoadState('domcontentloaded');
    // If already authenticated, we may land on the dashboard
    // Otherwise, we'll be redirected to login
    await this.page.waitForTimeout(500);
  }

  async login(email: string, password: string): Promise<void> {
    await this.emailInput.waitFor({ state: 'visible', timeout: 15_000 });
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.loginButton.click();
    await this.page.waitForLoadState('domcontentloaded');
    await this.page.waitForTimeout(1000);
  }

  async expectError(message: string): Promise<void> {
    await expect(this.errorMessage).toBeVisible({ timeout: 10_000 });
    await expect(this.errorMessage).toContainText(message);
  }

  async expectLoggedIn(): Promise<void> {
    // After successful login, we should be on the dashboard with the sidebar visible
    await this.page.waitForSelector('.sidebar', { timeout: 15_000 });
    await waitForBlazor(this.page);
    await expect(this.page.locator('.sidebar-logo')).toBeVisible();
  }
}
