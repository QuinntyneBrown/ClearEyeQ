import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import Slider from '@react-native-community/slider';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';
import { Toggle } from '@/components/ui/Toggle';
import { Button } from '@/components/ui/Button';

interface AccessibilitySettingsProps {
  textSize: 'small' | 'medium' | 'large';
  highContrast: boolean;
  voiceGuided: boolean;
  language: string;
  onTextSizeChange: (size: 'small' | 'medium' | 'large') => void;
  onHighContrastChange: (enabled: boolean) => void;
  onVoiceGuidedChange: (enabled: boolean) => void;
  onLanguagePress: () => void;
}

const textSizeValues = ['small', 'medium', 'large'] as const;
const textSizeLabels: Record<string, string> = {
  small: 'Small',
  medium: 'Medium',
  large: 'Large',
};

export function AccessibilitySettings({
  textSize,
  highContrast,
  voiceGuided,
  language,
  onTextSizeChange,
  onHighContrastChange,
  onVoiceGuidedChange,
  onLanguagePress,
}: AccessibilitySettingsProps) {
  const textSizeIndex = textSizeValues.indexOf(textSize);

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Accessibility</Text>

      <View style={styles.section}>
        <View style={styles.sliderHeader}>
          <Text style={styles.label}>Text Size</Text>
          <Text style={styles.value}>{textSizeLabels[textSize]}</Text>
        </View>
        <View style={styles.sliderRow}>
          <Text style={styles.sliderLabel}>A</Text>
          <View style={styles.sliderContainer}>
            <Slider
              minimumValue={0}
              maximumValue={2}
              step={1}
              value={textSizeIndex}
              onValueChange={(val: number) => onTextSizeChange(textSizeValues[Math.round(val)])}
              minimumTrackTintColor={colors.primary}
              maximumTrackTintColor={colors.border}
              thumbTintColor={colors.primary}
            />
          </View>
          <Text style={styles.sliderLabelLarge}>A</Text>
        </View>
      </View>

      <View style={styles.divider} />

      <Toggle
        label="High Contrast"
        description="Increase color contrast for better visibility"
        value={highContrast}
        onValueChange={onHighContrastChange}
      />

      <View style={styles.divider} />

      <Toggle
        label="Voice Guided"
        description="Enable voice narration during scans"
        value={voiceGuided}
        onValueChange={onVoiceGuidedChange}
      />

      <View style={styles.divider} />

      <View style={styles.languageRow}>
        <Text style={styles.label}>Language</Text>
        <Button
          label={language === 'en' ? 'English' : language}
          onPress={onLanguagePress}
          variant="secondary"
          size="sm"
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
    marginBottom: spacing.md,
  },
  section: {
    marginBottom: spacing.xs,
  },
  sliderHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: spacing.xs,
  },
  label: {
    ...typography.body,
    color: colors.textPrimary,
  },
  value: {
    ...typography.caption,
    color: colors.textSecondary,
  },
  sliderRow: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  sliderContainer: {
    flex: 1,
    marginHorizontal: spacing.sm,
  },
  sliderLabel: {
    fontSize: 12,
    color: colors.textSecondary,
  },
  sliderLabelLarge: {
    fontSize: 20,
    color: colors.textSecondary,
    fontWeight: '600',
  },
  divider: {
    height: 1,
    backgroundColor: colors.border,
    marginVertical: spacing.sm,
  },
  languageRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: spacing.sm,
  },
});
