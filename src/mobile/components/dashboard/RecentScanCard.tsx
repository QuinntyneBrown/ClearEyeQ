import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { Eye } from 'lucide-react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';
import { Button } from '@/components/ui/Button';

interface RecentScanCardProps {
  scanTime?: string;
  rednessLevel?: string;
  onScan: () => void;
}

export function RecentScanCard({ scanTime, rednessLevel, onScan }: RecentScanCardProps) {
  return (
    <View style={styles.container}>
      <View style={styles.iconContainer}>
        <Eye size={24} color={colors.primary} />
      </View>
      <View style={styles.content}>
        <Text style={styles.title}>
          {scanTime ? 'Last Scan' : 'No Scans Yet'}
        </Text>
        {scanTime && (
          <Text style={styles.time}>{scanTime}</Text>
        )}
        {rednessLevel && (
          <Text style={styles.level}>Redness: {rednessLevel}</Text>
        )}
      </View>
      <Button
        label="Scan"
        onPress={onScan}
        variant="primary"
        size="sm"
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.bgPage,
    borderRadius: radius.md,
    padding: spacing.md,
    borderWidth: 1,
    borderColor: colors.border,
  },
  iconContainer: {
    width: 44,
    height: 44,
    borderRadius: 22,
    backgroundColor: colors.primaryLight,
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: spacing.sm,
  },
  content: {
    flex: 1,
  },
  title: {
    ...typography.label,
    color: colors.textPrimary,
  },
  time: {
    ...typography.caption,
    color: colors.textSecondary,
    marginTop: 1,
  },
  level: {
    ...typography.caption,
    color: colors.textSecondary,
  },
});
