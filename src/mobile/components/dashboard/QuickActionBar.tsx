import React from 'react';
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { ScanEye, ClipboardList, TrendingUp, CloudSun } from 'lucide-react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';

interface QuickActionBarProps {
  onScan: () => void;
  onLog: () => void;
  onTrends: () => void;
  onForecast: () => void;
}

export function QuickActionBar({ onScan, onLog, onTrends, onForecast }: QuickActionBarProps) {
  const actions = [
    { icon: <ScanEye size={22} color={colors.primary} />, label: 'Scan', onPress: onScan },
    { icon: <ClipboardList size={22} color={colors.primary} />, label: 'Log', onPress: onLog },
    { icon: <TrendingUp size={22} color={colors.primary} />, label: 'Trends', onPress: onTrends },
    { icon: <CloudSun size={22} color={colors.primary} />, label: 'Forecast', onPress: onForecast },
  ];

  return (
    <View style={styles.container}>
      {actions.map((action, index) => (
        <TouchableOpacity
          key={index}
          style={styles.action}
          activeOpacity={0.7}
          onPress={action.onPress}
        >
          <View style={styles.iconCircle}>{action.icon}</View>
          <Text style={styles.label}>{action.label}</Text>
        </TouchableOpacity>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  action: {
    alignItems: 'center',
    flex: 1,
  },
  iconCircle: {
    width: 52,
    height: 52,
    borderRadius: 26,
    backgroundColor: colors.primaryLight,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: spacing.xs,
  },
  label: {
    ...typography.labelSmall,
    color: colors.textPrimary,
  },
});
