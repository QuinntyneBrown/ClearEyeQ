import React, { useState } from 'react';
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { Lightbulb, X } from 'lucide-react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';

interface DailyTipBannerProps {
  tip: string;
}

export function DailyTipBanner({ tip }: DailyTipBannerProps) {
  const [dismissed, setDismissed] = useState(false);

  if (dismissed) return null;

  return (
    <View style={styles.container}>
      <View style={styles.iconContainer}>
        <Lightbulb size={18} color={colors.warning} />
      </View>
      <Text style={styles.tip} numberOfLines={2}>
        {tip}
      </Text>
      <TouchableOpacity
        onPress={() => setDismissed(true)}
        hitSlop={{ top: 10, bottom: 10, left: 10, right: 10 }}
      >
        <X size={16} color={colors.textTertiary} />
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.warningLight,
    borderRadius: radius.md,
    padding: spacing.md,
  },
  iconContainer: {
    marginRight: spacing.sm,
  },
  tip: {
    ...typography.caption,
    color: colors.textPrimary,
    flex: 1,
    marginRight: spacing.sm,
  },
});
