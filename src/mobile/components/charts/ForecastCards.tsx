import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';

interface ForecastDay {
  day: string;
  score: number;
  risk: string;
}

interface ForecastCardsProps {
  forecasts: ForecastDay[];
}

function getScoreColor(score: number): string {
  if (score >= 70) return colors.scoreGreen;
  if (score >= 40) return colors.scoreYellow;
  return colors.scoreRed;
}

function getRiskColor(score: number): string {
  if (score >= 70) return colors.success;
  if (score >= 40) return colors.warning;
  return colors.error;
}

export function ForecastCards({ forecasts }: ForecastCardsProps) {
  return (
    <View style={styles.container}>
      {forecasts.map((forecast, index) => {
        const scoreColor = getScoreColor(forecast.score);
        const riskColor = getRiskColor(forecast.score);

        return (
          <View key={index} style={styles.card}>
            <Text style={styles.day}>{forecast.day}</Text>
            <Text style={[styles.score, { color: scoreColor }]}>{forecast.score}</Text>
            <Text style={[styles.risk, { color: riskColor }]}>{forecast.risk}</Text>
          </View>
        );
      })}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    gap: spacing.sm,
  },
  card: {
    flex: 1,
    backgroundColor: colors.bgPage,
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.md,
    alignItems: 'center',
  },
  day: {
    ...typography.labelSmall,
    color: colors.textSecondary,
    marginBottom: spacing.sm,
  },
  score: {
    fontSize: 28,
    fontWeight: '700',
    marginBottom: spacing.xs,
  },
  risk: {
    ...typography.labelSmall,
    fontWeight: '600',
  },
});
