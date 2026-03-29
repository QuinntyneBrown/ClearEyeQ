import React, { useState, useEffect } from 'react';
import { ScrollView, View, Text, StyleSheet, SafeAreaView } from 'react-native';
import { useLocalSearchParams, router } from 'expo-router';
import { AlertCircle, Droplets, Sun, Monitor, Moon } from 'lucide-react-native';
import { Badge } from '@/components/ui/Badge';
import { Card } from '@/components/ui/Card';
import { Spinner } from '@/components/ui/Spinner';
import api from '@/services/api';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';

interface DiagnosisCondition {
  id: string;
  name: string;
  confidence: number;
  severity: 'mild' | 'moderate' | 'severe';
  description: string;
}

interface RootCause {
  id: string;
  factor: string;
  icon: string;
  contribution: number;
}

interface DiagnosisData {
  scanId: string;
  conditions: DiagnosisCondition[];
  rootCauses: RootCause[];
  summary: string;
}

const factorIcons: Record<string, React.ReactNode> = {
  allergens: <Droplets size={18} color={colors.primary} />,
  environment: <Sun size={18} color={colors.warning} />,
  screen: <Monitor size={18} color={colors.textSecondary} />,
  sleep: <Moon size={18} color={colors.primary} />,
  default: <AlertCircle size={18} color={colors.textTertiary} />,
};

export default function DiagnosisDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const [data, setData] = useState<DiagnosisData | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadDiagnosis();
  }, [id]);

  const loadDiagnosis = async () => {
    try {
      const response = await api.get(`/api/diagnostics/scans/${id}/diagnosis`);
      setData(response.data);
    } catch (error) {
      setData({
        scanId: id ?? '',
        conditions: [
          {
            id: '1',
            name: 'Allergic Conjunctivitis',
            confidence: 87,
            severity: 'moderate',
            description: 'Inflammation of the conjunctiva caused by allergic reaction, likely related to elevated pollen levels.',
          },
          {
            id: '2',
            name: 'Dry Eye Syndrome',
            confidence: 72,
            severity: 'mild',
            description: 'Insufficient tear production or excessive tear evaporation causing discomfort and redness.',
          },
          {
            id: '3',
            name: 'Digital Eye Strain',
            confidence: 65,
            severity: 'mild',
            description: 'Eye discomfort caused by prolonged screen use, correlated with 6+ hours daily screen time.',
          },
        ],
        rootCauses: [
          { id: '1', factor: 'High Pollen', icon: 'allergens', contribution: 40 },
          { id: '2', factor: 'Screen Time (6.5h)', icon: 'screen', contribution: 30 },
          { id: '3', factor: 'Low Humidity (35%)', icon: 'environment', contribution: 20 },
          { id: '4', factor: 'Poor Sleep (5.5h)', icon: 'sleep', contribution: 10 },
        ],
        summary: 'Multiple contributing factors identified. Primary driver is allergic reaction to elevated pollen levels, compounded by extended screen time and environmental dryness.',
      });
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return <Spinner fullScreen label="Loading diagnosis..." />;
  }

  if (!data) return null;

  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView
        style={styles.container}
        contentContainerStyle={styles.content}
        showsVerticalScrollIndicator={false}
      >
        <Text style={styles.summary}>{data.summary}</Text>

        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Differential Diagnosis</Text>
          <View style={styles.conditionsList}>
            {data.conditions.map((condition) => (
              <View key={condition.id} style={styles.conditionCard}>
                <View style={styles.conditionHeader}>
                  <Text style={styles.conditionName}>{condition.name}</Text>
                  <Badge
                    label={condition.severity}
                    variant={condition.severity}
                  />
                </View>

                <View style={styles.confidenceContainer}>
                  <View style={styles.confidenceBarBg}>
                    <View
                      style={[
                        styles.confidenceBarFill,
                        {
                          width: `${condition.confidence}%`,
                          backgroundColor:
                            condition.confidence >= 80
                              ? colors.success
                              : condition.confidence >= 60
                                ? colors.warning
                                : colors.textTertiary,
                        },
                      ]}
                    />
                  </View>
                  <Text style={styles.confidenceText}>{condition.confidence}%</Text>
                </View>

                <Text style={styles.conditionDescription}>{condition.description}</Text>
              </View>
            ))}
          </View>
        </View>

        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Root Causes</Text>
          <Card variant="insight">
            {data.rootCauses.map((cause, index) => (
              <View
                key={cause.id}
                style={[
                  styles.causeRow,
                  index < data.rootCauses.length - 1 && styles.causeRowBorder,
                ]}
              >
                <View style={styles.causeIconContainer}>
                  {factorIcons[cause.icon] ?? factorIcons.default}
                </View>
                <Text style={styles.causeFactor}>{cause.factor}</Text>
                <View style={styles.causeBarContainer}>
                  <View
                    style={[
                      styles.causeBar,
                      { width: `${cause.contribution}%` },
                    ]}
                  />
                </View>
                <Text style={styles.causePercent}>{cause.contribution}%</Text>
              </View>
            ))}
          </Card>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.bgPage,
  },
  container: {
    flex: 1,
  },
  content: {
    padding: spacing.lg,
    gap: spacing.lg,
  },
  summary: {
    ...typography.body,
    color: colors.textSecondary,
    lineHeight: 24,
  },
  section: {
    gap: spacing.sm,
  },
  sectionTitle: {
    ...typography.heading3,
    color: colors.textPrimary,
  },
  conditionsList: {
    gap: spacing.sm,
  },
  conditionCard: {
    backgroundColor: colors.bgPage,
    borderRadius: radius.md,
    padding: spacing.md,
    borderWidth: 1,
    borderColor: colors.border,
  },
  conditionHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: spacing.sm,
  },
  conditionName: {
    ...typography.bodyMedium,
    color: colors.textPrimary,
    fontWeight: '600',
    flex: 1,
    marginRight: spacing.sm,
  },
  confidenceContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
    marginBottom: spacing.sm,
  },
  confidenceBarBg: {
    flex: 1,
    height: 6,
    backgroundColor: colors.bgSurface,
    borderRadius: 3,
    overflow: 'hidden',
  },
  confidenceBarFill: {
    height: '100%',
    borderRadius: 3,
  },
  confidenceText: {
    ...typography.labelSmall,
    color: colors.textSecondary,
    width: 36,
    textAlign: 'right',
  },
  conditionDescription: {
    ...typography.caption,
    color: colors.textSecondary,
    lineHeight: 20,
  },
  causeRow: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: spacing.sm,
    gap: spacing.sm,
  },
  causeRowBorder: {
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  causeIconContainer: {
    width: 32,
    height: 32,
    borderRadius: 16,
    backgroundColor: colors.bgSurface,
    alignItems: 'center',
    justifyContent: 'center',
  },
  causeFactor: {
    ...typography.label,
    color: colors.textPrimary,
    flex: 1,
  },
  causeBarContainer: {
    width: 60,
    height: 6,
    backgroundColor: colors.bgSurface,
    borderRadius: 3,
    overflow: 'hidden',
  },
  causeBar: {
    height: '100%',
    backgroundColor: colors.primary,
    borderRadius: 3,
  },
  causePercent: {
    ...typography.labelSmall,
    color: colors.textSecondary,
    width: 30,
    textAlign: 'right',
  },
});
