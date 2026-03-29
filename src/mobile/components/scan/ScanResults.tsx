import React from 'react';
import { View, Text, StyleSheet, ScrollView } from 'react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { ScanResult } from '@/stores/scanStore';

interface ScanResultsProps {
  result: ScanResult;
  onViewDiagnosis: () => void;
  onRescan: () => void;
  onShare: () => void;
}

function getScoreColor(score: number): string {
  if (score >= 70) return colors.scoreGreen;
  if (score >= 40) return colors.scoreYellow;
  return colors.scoreRed;
}

export function ScanResults({ result, onViewDiagnosis, onRescan, onShare }: ScanResultsProps) {
  const scoreColor = getScoreColor(result.overallScore);
  const previousScore = result.overallScore - 3;
  const delta = result.overallScore - previousScore;
  const deltaSign = delta >= 0 ? '+' : '';

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.content}>
      <View style={styles.scoreSection}>
        <View style={[styles.scoreCircle, { borderColor: scoreColor }]}>
          <Text style={[styles.scoreText, { color: scoreColor }]}>{result.overallScore}</Text>
        </View>
        <Text style={styles.scoreLabel}>Eye Health Score</Text>
        <Text style={styles.delta}>
          {deltaSign}{delta} from last scan
        </Text>
      </View>

      <View style={styles.metricsRow}>
        <View style={styles.metric}>
          <Text style={styles.metricValue}>{result.rednessScore}</Text>
          <Text style={styles.metricLabel}>Redness</Text>
        </View>
        <View style={styles.metricDivider} />
        <View style={styles.metric}>
          <Text style={styles.metricValue}>{result.tearFilmScore}</Text>
          <Text style={styles.metricLabel}>Tear Film</Text>
        </View>
      </View>

      {result.conditions.length > 0 && (
        <View style={styles.conditionsSection}>
          <Text style={styles.sectionTitle}>Detected Conditions</Text>
          <View style={styles.conditionChips}>
            {result.conditions.map((condition) => (
              <Badge
                key={condition.id}
                label={condition.name}
                variant={condition.severity}
                size="md"
                style={styles.conditionChip}
              />
            ))}
          </View>
        </View>
      )}

      <View style={styles.actions}>
        <Button
          label="View Diagnosis"
          onPress={onViewDiagnosis}
          variant="primary"
          fullWidth
        />
        <Button
          label="Rescan"
          onPress={onRescan}
          variant="secondary"
          fullWidth
        />
        <Button
          label="Share Results"
          onPress={onShare}
          variant="ghost"
          fullWidth
        />
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.bgPage,
  },
  content: {
    padding: spacing.lg,
  },
  scoreSection: {
    alignItems: 'center',
    marginBottom: spacing.lg,
  },
  scoreCircle: {
    width: 120,
    height: 120,
    borderRadius: 60,
    borderWidth: 6,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: colors.bgSurface,
    marginBottom: spacing.sm,
  },
  scoreText: {
    fontSize: 44,
    fontWeight: '700',
  },
  scoreLabel: {
    ...typography.heading3,
    color: colors.textPrimary,
  },
  delta: {
    ...typography.caption,
    color: colors.textSecondary,
    marginTop: spacing.xs,
  },
  metricsRow: {
    flexDirection: 'row',
    backgroundColor: colors.bgSurface,
    borderRadius: radius.md,
    padding: spacing.md,
    marginBottom: spacing.lg,
  },
  metric: {
    flex: 1,
    alignItems: 'center',
  },
  metricValue: {
    ...typography.heading2,
    color: colors.textPrimary,
  },
  metricLabel: {
    ...typography.caption,
    color: colors.textSecondary,
    marginTop: spacing.xs,
  },
  metricDivider: {
    width: 1,
    backgroundColor: colors.border,
    marginVertical: spacing.xs,
  },
  conditionsSection: {
    marginBottom: spacing.lg,
  },
  sectionTitle: {
    ...typography.label,
    color: colors.textPrimary,
    marginBottom: spacing.sm,
  },
  conditionChips: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.sm,
  },
  conditionChip: {
    marginBottom: spacing.xs,
  },
  actions: {
    gap: spacing.sm,
  },
});
