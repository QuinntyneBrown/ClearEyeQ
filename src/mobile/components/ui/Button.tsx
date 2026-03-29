import React from 'react';
import {
  TouchableOpacity,
  Text,
  ActivityIndicator,
  StyleSheet,
  ViewStyle,
  TextStyle,
} from 'react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';

type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'destructive';
type ButtonSize = 'sm' | 'md' | 'lg';

interface ButtonProps {
  label: string;
  onPress: () => void;
  variant?: ButtonVariant;
  size?: ButtonSize;
  icon?: React.ReactNode;
  iconPosition?: 'left' | 'right';
  disabled?: boolean;
  loading?: boolean;
  fullWidth?: boolean;
  style?: ViewStyle;
}

const variantStyles: Record<ButtonVariant, { container: ViewStyle; text: TextStyle }> = {
  primary: {
    container: { backgroundColor: colors.primary },
    text: { color: colors.textOnAccent },
  },
  secondary: {
    container: { backgroundColor: colors.bgPage, borderWidth: 1, borderColor: colors.border },
    text: { color: colors.textPrimary },
  },
  ghost: {
    container: { backgroundColor: colors.transparent },
    text: { color: colors.primary },
  },
  destructive: {
    container: { backgroundColor: colors.error },
    text: { color: colors.textOnAccent },
  },
};

const sizeStyles: Record<ButtonSize, { container: ViewStyle; text: TextStyle }> = {
  sm: {
    container: { paddingVertical: spacing.sm, paddingHorizontal: spacing.md },
    text: { fontSize: 14 },
  },
  md: {
    container: { paddingVertical: spacing.sm + 4, paddingHorizontal: spacing.lg },
    text: { fontSize: 16 },
  },
  lg: {
    container: { paddingVertical: spacing.md, paddingHorizontal: spacing.xl },
    text: { fontSize: 18 },
  },
};

export function Button({
  label,
  onPress,
  variant = 'primary',
  size = 'md',
  icon,
  iconPosition = 'left',
  disabled = false,
  loading = false,
  fullWidth = false,
  style,
}: ButtonProps) {
  const isDisabled = disabled || loading;
  const variantStyle = variantStyles[variant];
  const sizeStyle = sizeStyles[size];

  return (
    <TouchableOpacity
      onPress={onPress}
      disabled={isDisabled}
      activeOpacity={0.7}
      style={[
        styles.container,
        variantStyle.container,
        sizeStyle.container,
        fullWidth && styles.fullWidth,
        isDisabled && styles.disabled,
        style,
      ]}
    >
      {loading ? (
        <ActivityIndicator
          size="small"
          color={variantStyle.text.color}
        />
      ) : (
        <>
          {icon && iconPosition === 'left' && (
            <>{icon}</>
          )}
          <Text
            style={[
              styles.label,
              variantStyle.text,
              sizeStyle.text,
              icon ? (iconPosition === 'left' ? styles.labelWithIconLeft : styles.labelWithIconRight) : undefined,
            ]}
          >
            {label}
          </Text>
          {icon && iconPosition === 'right' && (
            <>{icon}</>
          )}
        </>
      )}
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: radius.md,
  },
  fullWidth: {
    width: '100%',
  },
  disabled: {
    opacity: 0.5,
  },
  label: {
    ...typography.bodyMedium,
    fontWeight: '600',
  },
  labelWithIconLeft: {
    marginLeft: spacing.sm,
  },
  labelWithIconRight: {
    marginRight: spacing.sm,
  },
});
