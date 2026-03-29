import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { AlertTriangle } from 'lucide-react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';
import { Button } from '@/components/ui/Button';

interface SpecialistReferralCardProps {
  reason: string;
  onBookAppointment: () => void;
}

export function SpecialistReferralCard({ reason, onBookAppointment }: SpecialistReferralCardProps) {
  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <View style={styles.iconContainer}>
          <AlertTriangle size={20} color={colors.error} />
        </View>
        <Text style={styles.title}>Specialist Referral Recommended</Text>
      </View>
      <Text style={styles.reason}>{reason}</Text>
      <Button
        label="Book Appointment"
        onPress={onBookAppointment}
        variant="destructive"
        fullWidth
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: colors.errorLight,
    borderRadius: radius.md,
    padding: spacing.md,
    borderWidth: 1,
    borderColor: colors.error + '30',
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: spacing.sm,
  },
  iconContainer: {
    marginRight: spacing.sm,
  },
  title: {
    ...typography.label,
    color: colors.error,
    fontWeight: '600',
  },
  reason: {
    ...typography.body,
    color: colors.textPrimary,
    marginBottom: spacing.md,
  },
});
