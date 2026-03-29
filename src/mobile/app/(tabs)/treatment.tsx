import React, { useState, useEffect, useCallback } from 'react';
import { ScrollView, View, Text, StyleSheet, SafeAreaView } from 'react-native';
import { Calendar } from 'lucide-react-native';
import { TreatmentOverview } from '@/components/treatment/TreatmentOverview';
import { InterventionCards } from '@/components/treatment/InterventionCards';
import { ProgressTracker } from '@/components/treatment/ProgressTracker';
import { SpecialistReferralCard } from '@/components/treatment/SpecialistReferralCard';
import { Card } from '@/components/ui/Card';
import { Spinner } from '@/components/ui/Spinner';
import { useToast } from '@/components/ui/Toast';
import api from '@/services/api';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing } from '@/theme/spacing';

interface TreatmentData {
  phases: Array<{ id: string; label: string; status: 'completed' | 'active' | 'pending' }>;
  nextAction: { title: string; description: string; dueTime: string };
  interventions: Array<{
    id: string;
    type: 'medication' | 'behavioral' | 'environmental';
    title: string;
    description: string;
    enabled: boolean;
  }>;
  beforeScore: number;
  currentScore: number;
  referralNeeded: boolean;
  referralReason: string;
}

export default function TreatmentScreen() {
  const [data, setData] = useState<TreatmentData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const { showToast } = useToast();

  useEffect(() => {
    loadTreatment();
  }, []);

  const loadTreatment = async () => {
    try {
      const response = await api.get('/api/treatments/current');
      setData(response.data);
    } catch (error) {
      setData({
        phases: [
          { id: '1', label: 'Week 1', status: 'completed' },
          { id: '2', label: 'Week 2', status: 'active' },
          { id: '3', label: 'Week 3-4', status: 'pending' },
        ],
        nextAction: {
          title: 'Apply eye drops',
          description: 'Preservative-free artificial tears, 1 drop each eye',
          dueTime: 'Due in 2 hours',
        },
        interventions: [
          {
            id: '1',
            type: 'medication',
            title: 'Artificial Tears',
            description: 'Apply 4x daily, preservative-free',
            enabled: true,
          },
          {
            id: '2',
            type: 'behavioral',
            title: '20-20-20 Rule',
            description: 'Every 20 min, look 20 ft away for 20 sec',
            enabled: true,
          },
          {
            id: '3',
            type: 'environmental',
            title: 'Humidifier',
            description: 'Run humidifier when humidity < 40%',
            enabled: false,
          },
        ],
        beforeScore: 52,
        currentScore: 71,
        referralNeeded: false,
        referralReason: '',
      });
    } finally {
      setIsLoading(false);
    }
  };

  const handleToggleIntervention = useCallback(async (id: string, enabled: boolean) => {
    if (!data) return;

    const updated = data.interventions.map((i) =>
      i.id === id ? { ...i, enabled } : i
    );
    setData({ ...data, interventions: updated });

    try {
      await api.patch(`/api/treatments/interventions/${id}`, { enabled });
    } catch (error) {
      showToast('error', 'Update Failed', 'Could not update intervention setting.');
      const reverted = data.interventions.map((i) =>
        i.id === id ? { ...i, enabled: !enabled } : i
      );
      setData({ ...data, interventions: reverted });
    }
  }, [data, showToast]);

  if (isLoading) {
    return <Spinner fullScreen label="Loading treatment plan..." />;
  }

  if (!data) return null;

  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView
        style={styles.container}
        contentContainerStyle={styles.content}
        showsVerticalScrollIndicator={false}
      >
        <Text style={styles.title}>Treatment Plan</Text>

        <TreatmentOverview phases={data.phases} />

        <Card variant="action" style={styles.nextActionCard}>
          <View style={styles.nextActionHeader}>
            <Calendar size={18} color={colors.primary} />
            <Text style={styles.nextActionTitle}>{data.nextAction.title}</Text>
          </View>
          <Text style={styles.nextActionDescription}>{data.nextAction.description}</Text>
          <Text style={styles.nextActionDue}>{data.nextAction.dueTime}</Text>
        </Card>

        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Interventions</Text>
          <InterventionCards
            interventions={data.interventions}
            onToggle={handleToggleIntervention}
          />
        </View>

        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Progress</Text>
          <ProgressTracker
            beforeScore={data.beforeScore}
            currentScore={data.currentScore}
          />
        </View>

        {data.referralNeeded && (
          <View style={styles.section}>
            <SpecialistReferralCard
              reason={data.referralReason}
              onBookAppointment={() =>
                showToast('info', 'Booking', 'Specialist booking coming soon.')
              }
            />
          </View>
        )}
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
    paddingBottom: 120,
    gap: spacing.lg,
  },
  title: {
    ...typography.heading1,
    color: colors.textPrimary,
  },
  nextActionCard: {
    marginTop: spacing.sm,
  },
  nextActionHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
    marginBottom: spacing.xs,
  },
  nextActionTitle: {
    ...typography.bodyMedium,
    color: colors.textPrimary,
    fontWeight: '600',
  },
  nextActionDescription: {
    ...typography.caption,
    color: colors.textSecondary,
    marginBottom: spacing.xs,
  },
  nextActionDue: {
    ...typography.labelSmall,
    color: colors.primary,
    fontWeight: '600',
  },
  section: {
    gap: spacing.sm,
  },
  sectionTitle: {
    ...typography.label,
    color: colors.textPrimary,
  },
});
