import { by, element, waitFor } from 'detox';

const DEFAULT_TIMEOUT = 15000;

/**
 * Waits for an element identified by testID to become visible.
 * @param id - The testID of the element to wait for
 * @param timeout - Maximum time to wait in milliseconds (default: 15000)
 */
export async function waitForElement(
  id: string,
  timeout: number = DEFAULT_TIMEOUT
): Promise<void> {
  await waitFor(element(by.id(id)))
    .toBeVisible()
    .withTimeout(timeout);
}

/**
 * Waits for an element identified by testID to not be visible.
 * @param id - The testID of the element to wait for
 * @param timeout - Maximum time to wait in milliseconds (default: 15000)
 */
export async function waitForElementToDisappear(
  id: string,
  timeout: number = DEFAULT_TIMEOUT
): Promise<void> {
  await waitFor(element(by.id(id)))
    .not.toBeVisible()
    .withTimeout(timeout);
}

/**
 * Waits for an element to exist in the view hierarchy (may not be visible).
 * @param id - The testID of the element
 * @param timeout - Maximum time to wait in milliseconds (default: 15000)
 */
export async function waitForElementToExist(
  id: string,
  timeout: number = DEFAULT_TIMEOUT
): Promise<void> {
  await waitFor(element(by.id(id)))
    .toExist()
    .withTimeout(timeout);
}
