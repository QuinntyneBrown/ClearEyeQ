import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { Lightbulb } from 'lucide-react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';
import { Badge } from '@/components/ui/Badge';

interface Insight {
  id: string;
  text: string;
  confidence: number;
}

interface PatternInsightCardsProps {
  insights: Insight[];
}

function getConfidenceLabel(confidence: number): string {
  if (confidence >= 80) return 'High';
  if (confidence >= 50) return 'Medium';
  return 'Low';
}

function getConfidenceVariant(confidence: number): 'success' | 'warning' | 'neutral' {
  if (confidence >= 80) return 'success';
  if (confidence >= 50) return 'warning';
  return 'neutral';
}

export function PatternInsightCards({ insights }: PatternInsightCardsProps) {
  return (
    <View style={styles.container}>
      {insights.map((insight) => (
        <View key={insight.id} style={styles.card}>
          <View style={styles.iconContainer}>
            <Lightbulb size={18} color={colors.warning} />
          </View>
          <View style={styles.content}>
            <Text style={styles.text}>{insight.text}</Text>
            <Badge
              label={`${getConfidenceLabel(insight.confidence)} confidence`}
              variant={getConfidenceVariant(insight.confidence)}
              style={styles.badge}
            />
          </View>
        </View>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    gap: spacing.sm,
  },
  card: {
    flexDirection: 'row',
    backgroundColor: colors.bgSurface,
    borderRadius: radius.md,
    padding: spacing.md,
    borderWidth: 1,
    borderColor: colors.border,
  },
  iconContainer: {
    marginRight: spacing.sm,
    marginTop: 2,
  },
  content: {
    flex: 1,
  },
  text: {
    ...typography.body,
    color: colors.textPrimary,
    marginBottom: spacing.sm,
  },
  badge: {
    alignSelf: 'flex-start',
  },
});
