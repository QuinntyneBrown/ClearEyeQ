import { by, element, expect, waitFor } from 'detox';

class TabBar {
  // --- Element selectors ---

  tabBar() {
    return element(by.id('tab-bar'));
  }

  homeTab() {
    return element(by.id('tab-home'));
  }

  scanTab() {
    return element(by.id('tab-scan'));
  }

  trendsTab() {
    return element(by.id('tab-trends'));
  }

  treatTab() {
    return element(by.id('tab-treat'));
  }

  moreTab() {
    return element(by.id('tab-more'));
  }

  // --- Actions ---

  async tapHome(): Promise<void> {
    await this.homeTab().tap();
  }

  async tapScan(): Promise<void> {
    await this.scanTab().tap();
  }

  async tapTrends(): Promise<void> {
    await this.trendsTab().tap();
  }

  async tapTreat(): Promise<void> {
    await this.treatTab().tap();
  }

  async tapMore(): Promise<void> {
    await this.moreTab().tap();
  }

  // --- Assertions ---

  async isActiveTab(name: string): Promise<void> {
    const tabId = `tab-${name.toLowerCase()}`;
    await waitFor(element(by.id(tabId)))
      .toBeVisible()
      .withTimeout(5000);
  }

  async isVisible(): Promise<void> {
    await waitFor(this.tabBar())
      .toBeVisible()
      .withTimeout(10000);
  }
}

export default new TabBar();
