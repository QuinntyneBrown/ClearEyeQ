import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { Check } from 'lucide-react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';
import { Button } from '@/components/ui/Button';

interface Plan {
  id: string;
  name: string;
  price: string;
  features: string[];
  isCurrent: boolean;
}

interface SubscriptionCardsProps {
  plans: Plan[];
  onUpgrade: (planId: string) => void;
}

export function SubscriptionCards({ plans, onUpgrade }: SubscriptionCardsProps) {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>Subscription</Text>
      {plans.map((plan) => (
        <View
          key={plan.id}
          style={[
            styles.card,
            plan.isCurrent && styles.cardCurrent,
          ]}
        >
          <View style={styles.cardHeader}>
            <Text style={styles.planName}>{plan.name}</Text>
            <Text style={styles.planPrice}>{plan.price}</Text>
          </View>
          <View style={styles.features}>
            {plan.features.map((feature, index) => (
              <View key={index} style={styles.featureRow}>
                <Check size={14} color={plan.isCurrent ? colors.primary : colors.textTertiary} />
                <Text style={styles.featureText}>{feature}</Text>
              </View>
            ))}
          </View>
          {plan.isCurrent ? (
            <View style={styles.currentBadge}>
              <Text style={styles.currentBadgeText}>Current Plan</Text>
            </View>
          ) : (
            <Button
              label="Upgrade"
              onPress={() => onUpgrade(plan.id)}
              variant="primary"
              size="sm"
              fullWidth
            />
          )}
        </View>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    gap: spacing.sm,
  },
  title: {
    ...typography.label,
    color: colors.textPrimary,
  },
  card: {
    backgroundColor: colors.bgPage,
    borderRadius: radius.md,
    padding: spacing.md,
    borderWidth: 1,
    borderColor: colors.border,
  },
  cardCurrent: {
    borderColor: colors.primary,
    borderWidth: 2,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: spacing.sm,
  },
  planName: {
    ...typography.bodyMedium,
    color: colors.textPrimary,
    fontWeight: '600',
  },
  planPrice: {
    ...typography.heading3,
    color: colors.textPrimary,
  },
  features: {
    marginBottom: spacing.md,
    gap: spacing.xs,
  },
  featureRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
  },
  featureText: {
    ...typography.caption,
    color: colors.textSecondary,
  },
  currentBadge: {
    backgroundColor: colors.primaryLight,
    borderRadius: radius.sm,
    paddingVertical: spacing.xs,
    alignItems: 'center',
  },
  currentBadgeText: {
    ...typography.labelSmall,
    color: colors.primary,
    fontWeight: '600',
  },
});
