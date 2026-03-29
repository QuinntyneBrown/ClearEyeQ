import React from 'react';
import { View, Text, ScrollView, StyleSheet } from 'react-native';
import { Wind, Flower2, Droplets, Monitor } from 'lucide-react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';

interface EnvironmentData {
  aqi?: number;
  pollen?: string;
  humidity?: number;
  screenTime?: string;
}

interface EnvironmentCardsProps {
  data: EnvironmentData;
}

interface EnvCardItem {
  icon: React.ReactNode;
  value: string;
  label: string;
}

export function EnvironmentCards({ data }: EnvironmentCardsProps) {
  const cards: EnvCardItem[] = [
    {
      icon: <Wind size={20} color={colors.primary} />,
      value: data.aqi !== undefined ? `${data.aqi}` : '--',
      label: 'AQI',
    },
    {
      icon: <Flower2 size={20} color={colors.warning} />,
      value: data.pollen || '--',
      label: 'Pollen',
    },
    {
      icon: <Droplets size={20} color={colors.primary} />,
      value: data.humidity !== undefined ? `${data.humidity}%` : '--',
      label: 'Humidity',
    },
    {
      icon: <Monitor size={20} color={colors.textSecondary} />,
      value: data.screenTime || '--',
      label: 'Screen Time',
    },
  ];

  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      contentContainerStyle={styles.scrollContent}
    >
      {cards.map((card, index) => (
        <View key={index} style={styles.card}>
          {card.icon}
          <Text style={styles.value}>{card.value}</Text>
          <Text style={styles.label}>{card.label}</Text>
        </View>
      ))}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  scrollContent: {
    paddingRight: spacing.md,
    gap: spacing.sm,
  },
  card: {
    backgroundColor: colors.bgPage,
    borderRadius: radius.md,
    padding: spacing.md,
    borderWidth: 1,
    borderColor: colors.border,
    width: 100,
    alignItems: 'center',
  },
  value: {
    ...typography.heading3,
    color: colors.textPrimary,
    marginTop: spacing.sm,
  },
  label: {
    ...typography.caption,
    color: colors.textSecondary,
    marginTop: 2,
  },
});
