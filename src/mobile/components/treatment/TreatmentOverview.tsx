import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { Check } from 'lucide-react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing } from '@/theme/spacing';

interface Phase {
  id: string;
  label: string;
  status: 'completed' | 'active' | 'pending';
}

interface TreatmentOverviewProps {
  phases: Phase[];
}

export function TreatmentOverview({ phases }: TreatmentOverviewProps) {
  return (
    <View style={styles.container}>
      {phases.map((phase, index) => {
        const isLast = index === phases.length - 1;

        return (
          <View key={phase.id} style={styles.step}>
            <View style={styles.indicator}>
              <View
                style={[
                  styles.circle,
                  phase.status === 'completed' && styles.circleCompleted,
                  phase.status === 'active' && styles.circleActive,
                  phase.status === 'pending' && styles.circlePending,
                ]}
              >
                {phase.status === 'completed' && (
                  <Check size={14} color={colors.white} />
                )}
                {phase.status === 'active' && <View style={styles.activeDot} />}
              </View>
              {!isLast && (
                <View
                  style={[
                    styles.line,
                    (phase.status === 'completed') && styles.lineCompleted,
                  ]}
                />
              )}
            </View>
            <Text
              style={[
                styles.label,
                phase.status === 'active' && styles.labelActive,
                phase.status === 'pending' && styles.labelPending,
              ]}
            >
              {phase.label}
            </Text>
          </View>
        );
      })}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    paddingHorizontal: spacing.md,
  },
  step: {
    flex: 1,
    alignItems: 'center',
  },
  indicator: {
    alignItems: 'center',
    flexDirection: 'row',
    width: '100%',
    justifyContent: 'center',
  },
  circle: {
    width: 28,
    height: 28,
    borderRadius: 14,
    alignItems: 'center',
    justifyContent: 'center',
    zIndex: 1,
  },
  circleCompleted: {
    backgroundColor: colors.primary,
  },
  circleActive: {
    backgroundColor: colors.primary,
  },
  circlePending: {
    backgroundColor: colors.bgPage,
    borderWidth: 2,
    borderColor: colors.border,
  },
  activeDot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: colors.white,
  },
  line: {
    position: 'absolute',
    height: 2,
    backgroundColor: colors.border,
    left: '50%',
    right: '-50%',
    top: 13,
  },
  lineCompleted: {
    backgroundColor: colors.primary,
  },
  label: {
    ...typography.labelSmall,
    color: colors.textPrimary,
    marginTop: spacing.sm,
    textAlign: 'center',
  },
  labelActive: {
    fontWeight: '700',
    color: colors.primary,
  },
  labelPending: {
    color: colors.textTertiary,
  },
});
