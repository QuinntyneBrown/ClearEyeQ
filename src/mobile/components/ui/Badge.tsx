import React from 'react';
import { View, Text, StyleSheet, ViewStyle, TextStyle } from 'react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';

type BadgeVariant = 'mild' | 'moderate' | 'severe' | 'success' | 'warning' | 'info' | 'neutral';

interface BadgeProps {
  label: string;
  variant?: BadgeVariant;
  size?: 'sm' | 'md';
  style?: ViewStyle;
}

const variantStyles: Record<BadgeVariant, { bg: string; text: string }> = {
  mild: { bg: colors.warningLight, text: colors.warning },
  moderate: { bg: '#FFF7ED', text: '#EA580C' },
  severe: { bg: colors.errorLight, text: colors.error },
  success: { bg: colors.successLight, text: colors.success },
  warning: { bg: colors.warningLight, text: colors.warning },
  info: { bg: colors.primaryLight, text: colors.primary },
  neutral: { bg: colors.bgSurface, text: colors.textSecondary },
};

export function Badge({ label, variant = 'neutral', size = 'sm', style }: BadgeProps) {
  const variantStyle = variantStyles[variant];

  return (
    <View
      style={[
        styles.container,
        size === 'md' && styles.containerMd,
        { backgroundColor: variantStyle.bg },
        style,
      ]}
    >
      <Text
        style={[
          styles.text,
          size === 'md' && styles.textMd,
          { color: variantStyle.text },
        ]}
      >
        {label}
      </Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    paddingHorizontal: spacing.sm,
    paddingVertical: 2,
    borderRadius: radius.pill,
    alignSelf: 'flex-start',
  },
  containerMd: {
    paddingHorizontal: spacing.sm + 4,
    paddingVertical: spacing.xs,
  },
  text: {
    ...typography.labelSmall,
    fontWeight: '600',
  } as TextStyle,
  textMd: {
    ...typography.label,
  } as TextStyle,
});
