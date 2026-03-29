import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';
import { Toggle } from '@/components/ui/Toggle';
import { Button } from '@/components/ui/Button';

interface PrivacyControlsProps {
  monitoringEnabled: boolean;
  dataSharingEnabled: boolean;
  onToggleMonitoring: (value: boolean) => void;
  onToggleDataSharing: (value: boolean) => void;
  onExportData: () => void;
  onDeleteData: () => void;
}

export function PrivacyControls({
  monitoringEnabled,
  dataSharingEnabled,
  onToggleMonitoring,
  onToggleDataSharing,
  onExportData,
  onDeleteData,
}: PrivacyControlsProps) {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>Privacy</Text>

      <Toggle
        label="Passive Monitoring"
        description="Allow background health tracking"
        value={monitoringEnabled}
        onValueChange={onToggleMonitoring}
      />

      <View style={styles.divider} />

      <Toggle
        label="Anonymized Data Sharing"
        description="Help improve ClearEyeQ for everyone"
        value={dataSharingEnabled}
        onValueChange={onToggleDataSharing}
      />

      <View style={styles.divider} />

      <View style={styles.actions}>
        <Button
          label="Export My Data"
          onPress={onExportData}
          variant="secondary"
          size="sm"
          style={styles.actionButton}
        />
        <Button
          label="Delete My Data"
          onPress={onDeleteData}
          variant="destructive"
          size="sm"
          style={styles.actionButton}
        />
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
    marginBottom: spacing.sm,
  },
  divider: {
    height: 1,
    backgroundColor: colors.border,
    marginVertical: spacing.xs,
  },
  actions: {
    flexDirection: 'row',
    gap: spacing.sm,
    marginTop: spacing.md,
  },
  actionButton: {
    flex: 1,
  },
});
