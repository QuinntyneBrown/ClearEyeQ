import { Page } from '@playwright/test';

/**
 * Waits for the Blazor Server SignalR circuit to be fully established.
 * Blazor Server renders initial HTML on the server, then establishes
 * an interactive SignalR connection. We must wait for this connection
 * before performing any interactive actions (clicks, inputs, etc.).
 */
export async function waitForBlazor(page: Page): Promise<void> {
  // Wait for the initial page load to settle
  await page.waitForLoadState('domcontentloaded');

  // Wait for Blazor's SignalR connection to be established.
  // Blazor Server sets document._blazorState or starts its circuit
  // via the blazor.server.js script. We poll for the connection marker.
  await page.waitForFunction(
    () => {
      // Blazor Server adds a circuit ID to the DOM when connected.
      // The _blazor object is available on the document once the circuit starts.
      const blazor = (window as Record<string, unknown>)['Blazor'];
      if (blazor) return true;

      // Fallback: check if any Blazor-enhanced elements are interactive
      // by verifying the absence of the loading/reconnect UI
      const reconnectModal = document.getElementById('components-reconnect-modal');
      if (reconnectModal) {
        const display = window.getComputedStyle(reconnectModal).display;
        return display === 'none';
      }

      // If no reconnect modal exists at all, Blazor may already be connected
      return document.querySelector('[b-]') !== null ||
        document.querySelector('.sidebar-nav') !== null;
    },
    { timeout: 30_000 }
  );

  // Additional short wait for any pending Blazor re-renders
  await page.waitForTimeout(250);
}

/**
 * Waits for a Blazor-driven loading spinner to disappear,
 * indicating that async data has been loaded on the page.
 */
export async function waitForDataLoad(page: Page): Promise<void> {
  // Wait for loading spinners to vanish
  const spinner = page.locator('.loading-spinner');
  if (await spinner.count() > 0) {
    await spinner.first().waitFor({ state: 'hidden', timeout: 30_000 });
  }

  // Brief pause for Blazor re-render after data loads
  await page.waitForTimeout(200);
}

/**
 * Navigates to a route and waits for Blazor to be ready and data to load.
 */
export async function blazorNavigate(page: Page, path: string): Promise<void> {
  await page.goto(path);
  await waitForBlazor(page);
  await waitForDataLoad(page);
}
