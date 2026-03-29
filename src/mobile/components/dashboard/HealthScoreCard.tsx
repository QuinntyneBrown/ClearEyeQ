import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';

interface HealthScoreCardProps {
  score: number;
  delta: number;
  label?: string;
}

function getScoreColor(score: number): string {
  if (score >= 70) return colors.scoreGreen;
  if (score >= 40) return colors.scoreYellow;
  return colors.scoreRed;
}

export function HealthScoreCard({ score, delta, label = 'Eye Health Score' }: HealthScoreCardProps) {
  const scoreColor = getScoreColor(score);
  const deltaSign = delta >= 0 ? '+' : '';

  return (
    <View style={styles.container}>
      <View style={styles.content}>
        <Text style={styles.label}>{label}</Text>
        <Text style={styles.subtitle}>Overall wellness</Text>
        {delta !== 0 && (
          <View style={[styles.deltaBadge, { backgroundColor: delta >= 0 ? colors.successLight : colors.errorLight }]}>
            <Text style={[styles.deltaText, { color: delta >= 0 ? colors.success : colors.error }]}>
              {deltaSign}{delta} this week
            </Text>
          </View>
        )}
      </View>
      <View style={[styles.scoreCircle, { borderColor: scoreColor }]}>
        <Text style={[styles.scoreText, { color: scoreColor }]}>{score}</Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    backgroundColor: colors.primaryLight,
    borderRadius: radius.md,
    padding: spacing.md,
    borderWidth: 1,
    borderColor: colors.primary + '20',
  },
  content: {
    flex: 1,
    marginRight: spacing.md,
  },
  label: {
    ...typography.heading3,
    color: colors.textPrimary,
  },
  subtitle: {
    ...typography.caption,
    color: colors.textSecondary,
    marginTop: 2,
  },
  deltaBadge: {
    alignSelf: 'flex-start',
    paddingHorizontal: spacing.sm,
    paddingVertical: 2,
    borderRadius: radius.pill,
    marginTop: spacing.sm,
  },
  deltaText: {
    ...typography.labelSmall,
    fontWeight: '600',
  },
  scoreCircle: {
    width: 72,
    height: 72,
    borderRadius: 36,
    borderWidth: 4,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: colors.bgPage,
  },
  scoreText: {
    fontSize: 28,
    fontWeight: '700',
  },
});
