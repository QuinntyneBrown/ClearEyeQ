import { test, expect } from '../fixtures/auth.fixture';
import { NavMenu } from '../pages/NavMenu';
import { FeatureFlagsPage } from '../pages/FeatureFlagsPage';

test.describe('Feature Flag Management Flow', () => {
  test('should navigate to Feature Flags and display flag table', async ({ adminPage }) => {
    const nav = new NavMenu(adminPage);
    await nav.navigateTo('feature flags');
    await nav.expectActiveSection('feature flags');

    await expect(adminPage.locator('h2:has-text("Feature Flags")')).toBeVisible();
  });

  test('should show flag table with toggle switches', async ({ adminPage }) => {
    const flagsPage = new FeatureFlagsPage(adminPage);
    await flagsPage.goto();

    await flagsPage.expectFlagTableVisible();

    // There should be at least one toggle checkbox in the table
    const toggles = flagsPage.flagTable.locator('input[type="checkbox"]');
    const toggleCount = await toggles.count();
    expect(toggleCount).toBeGreaterThan(0);
  });

  test('should toggle a global flag and verify state change', async ({ adminPage }) => {
    const flagsPage = new FeatureFlagsPage(adminPage);
    await flagsPage.goto();

    // Get the first flag name from the table
    const firstFlagCell = flagsPage.flagTable.locator('tbody tr').first().locator('td').first();
    const flagName = (await firstFlagCell.textContent())!.trim();

    // Record current state
    const globalToggle = flagsPage.globalToggle(flagName);
    const wasChecked = await globalToggle.isChecked();

    // Toggle the flag
    await flagsPage.toggleGlobalFlag(flagName);

    // Verify it changed
    await flagsPage.expectFlagEnabled(flagName, !wasChecked);

    // Toggle back to restore original state
    await flagsPage.toggleGlobalFlag(flagName);
    await flagsPage.expectFlagEnabled(flagName, wasChecked);
  });

  test('should display tenant override information', async ({ adminPage }) => {
    const flagsPage = new FeatureFlagsPage(adminPage);
    await flagsPage.goto();

    // Get the first flag name
    const firstFlagCell = flagsPage.flagTable.locator('tbody tr').first().locator('td').first();
    const flagName = (await firstFlagCell.textContent())!.trim();

    // The override section should be visible (showing "None" or override entries)
    const overrideSection = flagsPage.tenantOverrideSection(flagName);
    await expect(overrideSection).toBeVisible();
    const text = await overrideSection.textContent();
    expect(text).toBeTruthy();
  });

  test('should show override toggles for flags with tenant overrides', async ({ adminPage }) => {
    const flagsPage = new FeatureFlagsPage(adminPage);
    await flagsPage.goto();

    // Check all flag rows for ones that have tenant overrides
    const rows = flagsPage.flagTable.locator('tbody tr');
    const rowCount = await rows.count();

    let foundOverride = false;
    for (let i = 0; i < rowCount; i++) {
      const overrideCell = rows.nth(i).locator('td').nth(3);
      const cellText = (await overrideCell.textContent())!.trim();

      if (cellText !== 'None') {
        foundOverride = true;
        // Override section should contain a toggle
        const overrideToggles = overrideCell.locator('input[type="checkbox"]');
        const count = await overrideToggles.count();
        expect(count).toBeGreaterThan(0);
        break;
      }
    }

    // If no overrides exist, that is still a valid state; just verify the table loaded
    if (!foundOverride) {
      expect(rowCount).toBeGreaterThan(0);
    }
  });
});
