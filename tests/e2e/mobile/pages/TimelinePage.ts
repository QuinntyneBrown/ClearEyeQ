import { by, element, expect, waitFor } from 'detox';

class TimelinePage {
  // --- Element selectors ---

  screenContainer() {
    return element(by.id('timeline-screen'));
  }

  timeRangeTabs() {
    return element(by.id('timeline-time-range-tabs'));
  }

  timeRangeTab(range: string) {
    return element(by.id(`timeline-tab-${range}`));
  }

  chartArea() {
    return element(by.id('timeline-chart-area'));
  }

  correlationChips() {
    return element(by.id('timeline-correlation-chips'));
  }

  correlationChip(factor: string) {
    return element(by.id(`timeline-chip-${factor.toLowerCase()}`));
  }

  insightCards() {
    return element(by.id('timeline-insight-cards'));
  }

  insightCard(index: number) {
    return element(by.id(`timeline-insight-card-${index}`));
  }

  forecastSection() {
    return element(by.id('timeline-forecast-section'));
  }

  // --- Actions ---

  async selectTimeRange(range: string): Promise<void> {
    await this.timeRangeTab(range).tap();
  }

  async toggleCorrelation(factor: string): Promise<void> {
    await this.correlationChip(factor).tap();
  }

  // --- Assertions ---

  async isVisible(): Promise<void> {
    await waitFor(this.screenContainer())
      .toBeVisible()
      .withTimeout(15000);
  }

  async hasTimeRangeSelected(range: string): Promise<void> {
    await expect(this.timeRangeTab(range)).toBeVisible();
  }

  async hasInsightCards(count: number): Promise<void> {
    for (let i = 0; i < count; i++) {
      await expect(this.insightCard(i)).toBeVisible();
    }
  }

  async chartIsVisible(): Promise<void> {
    await waitFor(this.chartArea())
      .toBeVisible()
      .withTimeout(10000);
  }

  async correlationChipIsActive(factor: string): Promise<void> {
    await expect(this.correlationChip(factor)).toBeVisible();
  }

  async insightCardsAreVisible(): Promise<void> {
    await waitFor(this.insightCards())
      .toBeVisible()
      .withTimeout(10000);
  }
}

export default new TimelinePage();
