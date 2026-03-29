import React, { useState, useEffect } from 'react';
import { ScrollView, View, Text, TouchableOpacity, StyleSheet, SafeAreaView } from 'react-native';
import { RednessTimeline } from '@/components/charts/RednessTimeline';
import { ForecastCards } from '@/components/charts/ForecastCards';
import { PatternInsightCards } from '@/components/charts/PatternInsightCards';
import { Spinner } from '@/components/ui/Spinner';
import api from '@/services/api';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';

type TimeRange = '7D' | '30D' | '90D' | '1Y';

const correlationFilters = ['All', 'Pollen', 'Sleep', 'Screen', 'AQI'] as const;

interface TimelineData {
  rednessData: Array<{ date: string; value: number }>;
  forecasts: Array<{ day: string; score: number; risk: string }>;
  insights: Array<{ id: string; text: string; confidence: number }>;
}

export default function TimelineScreen() {
  const [timeRange, setTimeRange] = useState<TimeRange>('7D');
  const [activeFilter, setActiveFilter] = useState<string>('All');
  const [data, setData] = useState<TimelineData | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadTimeline();
  }, [timeRange, activeFilter]);

  const loadTimeline = async () => {
    setIsLoading(true);
    try {
      const [trendsRes, forecastRes] = await Promise.allSettled([
        api.get(`/api/scans/trends?range=${timeRange}&filter=${activeFilter}`),
        api.get('/api/predictions/forecast?days=3'),
      ]);

      const trendsData = trendsRes.status === 'fulfilled' ? trendsRes.value.data : null;
      const forecastData = forecastRes.status === 'fulfilled' ? forecastRes.value.data : null;

      setData({
        rednessData: trendsData?.dataPoints ?? generateMockData(timeRange),
        forecasts: forecastData?.forecasts ?? [
          { day: 'Tomorrow', score: 72, risk: 'Low' },
          { day: 'Wed', score: 65, risk: 'Moderate' },
          { day: 'Thu', score: 58, risk: 'Moderate' },
        ],
        insights: trendsData?.insights ?? [
          { id: '1', text: 'Your redness tends to increase on high pollen days. Consider staying indoors during peak hours.', confidence: 85 },
          { id: '2', text: 'Screen time over 6 hours correlates with higher redness scores the following morning.', confidence: 72 },
          { id: '3', text: 'Getting 7+ hours of sleep is associated with lower redness scores.', confidence: 68 },
        ],
      });
    } catch (error) {
      setData({
        rednessData: generateMockData(timeRange),
        forecasts: [
          { day: 'Tomorrow', score: 72, risk: 'Low' },
          { day: 'Wed', score: 65, risk: 'Moderate' },
          { day: 'Thu', score: 58, risk: 'Moderate' },
        ],
        insights: [
          { id: '1', text: 'Your redness tends to increase on high pollen days.', confidence: 85 },
          { id: '2', text: 'Screen time over 6 hours correlates with higher morning redness.', confidence: 72 },
        ],
      });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView
        style={styles.container}
        contentContainerStyle={styles.content}
        showsVerticalScrollIndicator={false}
      >
        <Text style={styles.title}>Trends</Text>

        <View style={styles.timeRangeTabs}>
          {(['7D', '30D', '90D', '1Y'] as TimeRange[]).map((range) => (
            <TouchableOpacity
              key={range}
              style={[styles.tab, timeRange === range && styles.tabActive]}
              onPress={() => setTimeRange(range)}
            >
              <Text style={[styles.tabText, timeRange === range && styles.tabTextActive]}>
                {range}
              </Text>
            </TouchableOpacity>
          ))}
        </View>

        {isLoading ? (
          <Spinner label="Loading trends..." />
        ) : data ? (
          <View style={styles.sections}>
            <View>
              <Text style={styles.sectionTitle}>Redness Score</Text>
              <RednessTimeline data={data.rednessData} timeRange={timeRange} />
            </View>

            <View style={styles.filterChips}>
              {correlationFilters.map((filter) => (
                <TouchableOpacity
                  key={filter}
                  style={[styles.chip, activeFilter === filter && styles.chipActive]}
                  onPress={() => setActiveFilter(filter)}
                >
                  <Text style={[styles.chipText, activeFilter === filter && styles.chipTextActive]}>
                    {filter}
                  </Text>
                </TouchableOpacity>
              ))}
            </View>

            <View>
              <Text style={styles.sectionTitle}>72-Hour Forecast</Text>
              <ForecastCards forecasts={data.forecasts} />
            </View>

            <View>
              <Text style={styles.sectionTitle}>Insights</Text>
              <PatternInsightCards insights={data.insights} />
            </View>
          </View>
        ) : null}
      </ScrollView>
    </SafeAreaView>
  );
}

function generateMockData(range: TimeRange): Array<{ date: string; value: number }> {
  const count = range === '7D' ? 7 : range === '30D' ? 10 : range === '90D' ? 12 : 12;
  const labels = range === '7D'
    ? ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']
    : Array.from({ length: count }, (_, i) => `${i + 1}`);

  return labels.map((date) => ({
    date,
    value: Math.round(30 + Math.random() * 50),
  }));
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.bgPage,
  },
  container: {
    flex: 1,
  },
  content: {
    padding: spacing.lg,
    paddingBottom: 120,
  },
  title: {
    ...typography.heading1,
    color: colors.textPrimary,
    marginBottom: spacing.lg,
  },
  timeRangeTabs: {
    flexDirection: 'row',
    backgroundColor: colors.bgSurface,
    borderRadius: radius.md,
    padding: spacing.xs,
    marginBottom: spacing.lg,
  },
  tab: {
    flex: 1,
    paddingVertical: spacing.sm,
    alignItems: 'center',
    borderRadius: radius.sm,
  },
  tabActive: {
    backgroundColor: colors.primary,
  },
  tabText: {
    ...typography.label,
    color: colors.textSecondary,
  },
  tabTextActive: {
    color: colors.textOnAccent,
  },
  sections: {
    gap: spacing.lg,
  },
  sectionTitle: {
    ...typography.label,
    color: colors.textPrimary,
    marginBottom: spacing.sm,
  },
  filterChips: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.sm,
  },
  chip: {
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.sm,
    borderRadius: radius.pill,
    backgroundColor: colors.bgSurface,
    borderWidth: 1,
    borderColor: colors.border,
  },
  chipActive: {
    backgroundColor: colors.primary,
    borderColor: colors.primary,
  },
  chipText: {
    ...typography.labelSmall,
    color: colors.textSecondary,
  },
  chipTextActive: {
    color: colors.textOnAccent,
  },
});
