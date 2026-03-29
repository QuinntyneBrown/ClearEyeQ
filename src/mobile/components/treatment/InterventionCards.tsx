import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { Pill, Timer, Droplets } from 'lucide-react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';
import { Toggle } from '@/components/ui/Toggle';

type InterventionType = 'medication' | 'behavioral' | 'environmental';

interface Intervention {
  id: string;
  type: InterventionType;
  title: string;
  description: string;
  enabled: boolean;
}

interface InterventionCardsProps {
  interventions: Intervention[];
  onToggle: (id: string, enabled: boolean) => void;
}

const typeIcons: Record<InterventionType, React.ReactNode> = {
  medication: <Pill size={20} color={colors.primary} />,
  behavioral: <Timer size={20} color={colors.warning} />,
  environmental: <Droplets size={20} color={colors.success} />,
};

export function InterventionCards({ interventions, onToggle }: InterventionCardsProps) {
  return (
    <View style={styles.container}>
      {interventions.map((intervention) => (
        <View key={intervention.id} style={styles.card}>
          <View style={styles.header}>
            <View style={styles.iconContainer}>
              {typeIcons[intervention.type]}
            </View>
            <View style={styles.content}>
              <Text style={styles.title}>{intervention.title}</Text>
              <Text style={styles.description}>{intervention.description}</Text>
            </View>
          </View>
          <Toggle
            value={intervention.enabled}
            onValueChange={(value) => onToggle(intervention.id, value)}
          />
        </View>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    gap: spacing.sm,
  },
  card: {
    backgroundColor: colors.bgPage,
    borderRadius: radius.md,
    padding: spacing.md,
    borderWidth: 1,
    borderColor: colors.border,
  },
  header: {
    flexDirection: 'row',
    marginBottom: spacing.sm,
  },
  iconContainer: {
    width: 40,
    height: 40,
    borderRadius: 20,
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
  description: {
    ...typography.caption,
    color: colors.textSecondary,
    marginTop: 2,
  },
});
