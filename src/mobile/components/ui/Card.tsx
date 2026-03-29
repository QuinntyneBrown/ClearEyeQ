import React from 'react';
import { View, StyleSheet, ViewStyle } from 'react-native';
import { colors } from '@/theme/colors';
import { spacing, radius } from '@/theme/spacing';

type CardVariant = 'stat' | 'action' | 'insight' | 'alert';

interface CardProps {
  children: React.ReactNode;
  variant?: CardVariant;
  style?: ViewStyle;
  onPress?: () => void;
}

const variantStyles: Record<CardVariant, ViewStyle> = {
  stat: {
    backgroundColor: colors.bgPage,
    borderWidth: 1,
    borderColor: colors.border,
  },
  action: {
    backgroundColor: colors.primaryLight,
    borderWidth: 1,
    borderColor: colors.primary + '20',
  },
  insight: {
    backgroundColor: colors.bgSurface,
    borderWidth: 1,
    borderColor: colors.border,
  },
  alert: {
    backgroundColor: colors.errorLight,
    borderWidth: 1,
    borderColor: colors.error + '30',
  },
};

export function Card({ children, variant = 'stat', style }: CardProps) {
  return (
    <View style={[styles.container, variantStyles[variant], style]}>
      {children}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    borderRadius: radius.md,
    padding: spacing.md,
  },
});
