import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { TrendingUp, TrendingDown } from 'lucide-react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';

interface ProgressTrackerProps {
  beforeScore: number;
  currentScore: number;
}

export function ProgressTracker({ beforeScore, currentScore }: ProgressTrackerProps) {
  const delta = currentScore - beforeScore;
  const deltaPercent = beforeScore > 0 ? Math.round((delta / beforeScore) * 100) : 0;
  const isImprovement = delta >= 0;

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Treatment Progress</Text>

      <View style={styles.comparisonRow}>
        <View style={styles.scoreBox}>
          <Text style={styles.scoreLabel}>Before</Text>
          <Text style={styles.scoreValue}>{beforeScore}</Text>
        </View>

        <View style={styles.arrowContainer}>
          {isImprovement ? (
            <TrendingUp size={24} color={colors.success} />
          ) : (
            <TrendingDown size={24} color={colors.error} />
          )}
        </View>

        <View style={styles.scoreBox}>
          <Text style={styles.scoreLabel}>Current</Text>
          <Text style={[styles.scoreValue, { color: isImprovement ? colors.success : colors.error }]}>
            {currentScore}
          </Text>
        </View>
      </View>

      <View style={[styles.deltaBadge, { backgroundColor: isImprovement ? colors.successLight : colors.errorLight }]}>
        <Text style={[styles.deltaText, { color: isImprovement ? colors.success : colors.error }]}>
          {isImprovement ? '+' : ''}{deltaPercent}% {isImprovement ? 'improvement' : 'decline'}
        </Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: colors.bgPage,
    borderRadius: radius.md,
    padding: spacing.md,
    borderWidth: 1,
    borderColor: colors.border,
  },
  title: {
    ...typography.label,
    color: colors.textPrimary,
    marginBottom: spacing.md,
  },
  comparisonRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: spacing.md,
  },
  scoreBox: {
    flex: 1,
    alignItems: 'center',
    backgroundColor: colors.bgSurface,
    borderRadius: radius.sm,
    padding: spacing.md,
  },
  scoreLabel: {
    ...typography.caption,
    color: colors.textSecondary,
    marginBottom: spacing.xs,
  },
  scoreValue: {
    fontSize: 32,
    fontWeight: '700',
    color: colors.textPrimary,
  },
  arrowContainer: {
    paddingHorizontal: spacing.md,
  },
  deltaBadge: {
    alignSelf: 'center',
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.xs,
    borderRadius: radius.pill,
  },
  deltaText: {
    ...typography.label,
    fontWeight: '600',
  },
});
