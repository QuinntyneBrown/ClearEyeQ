import React, { useEffect, useState } from 'react';
import { ScrollView, View, Text, StyleSheet, SafeAreaView } from 'react-native';
import { router } from 'expo-router';
import { Bell } from 'lucide-react-native';
import { TouchableOpacity } from 'react-native';
import { HealthScoreCard } from '@/components/dashboard/HealthScoreCard';
import { DailyTipBanner } from '@/components/dashboard/DailyTipBanner';
import { RecentScanCard } from '@/components/dashboard/RecentScanCard';
import { EnvironmentCards } from '@/components/dashboard/EnvironmentCards';
import { QuickActionBar } from '@/components/dashboard/QuickActionBar';
import { Spinner } from '@/components/ui/Spinner';
import { useAuthStore } from '@/stores/authStore';
import api from '@/services/api';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing } from '@/theme/spacing';

interface DashboardData {
  healthScore: number;
  scoreDelta: number;
  lastScanTime: string | null;
  rednessLevel: string | null;
  environment: {
    aqi?: number;
    pollen?: string;
    humidity?: number;
    screenTime?: string;
  };
  dailyTip: string;
}

export default function HomeScreen() {
  const user = useAuthStore((s) => s.user);
  const [data, setData] = useState<DashboardData | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadDashboard();
  }, []);

  const loadDashboard = async () => {
    try {
      const [scanRes, envRes] = await Promise.allSettled([
        api.get('/api/scans/latest'),
        api.get('/api/environmental/current'),
      ]);

      const scanData = scanRes.status === 'fulfilled' ? scanRes.value.data : null;
      const envData = envRes.status === 'fulfilled' ? envRes.value.data : {};

      setData({
        healthScore: scanData?.overallScore ?? 78,
        scoreDelta: scanData?.delta ?? 3,
        lastScanTime: scanData?.createdAt ?? null,
        rednessLevel: scanData?.rednessLevel ?? null,
        environment: {
          aqi: envData?.aqi,
          pollen: envData?.pollenLevel,
          humidity: envData?.humidity,
          screenTime: envData?.screenTime,
        },
        dailyTip: 'Try the 20-20-20 rule: every 20 minutes, look at something 20 feet away for 20 seconds.',
      });
    } catch (error) {
      setData({
        healthScore: 78,
        scoreDelta: 3,
        lastScanTime: null,
        rednessLevel: null,
        environment: { aqi: 42, pollen: 'Low', humidity: 55, screenTime: '4.2h' },
        dailyTip: 'Try the 20-20-20 rule: every 20 minutes, look at something 20 feet away for 20 seconds.',
      });
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return <Spinner fullScreen label="Loading dashboard..." />;
  }

  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView
        style={styles.container}
        contentContainerStyle={styles.content}
        showsVerticalScrollIndicator={false}
      >
        <View style={styles.header}>
          <View>
            <Text style={styles.greeting}>
              Hello, {user?.name?.split(' ')[0] ?? 'there'}
            </Text>
            <Text style={styles.date}>
              {new Date().toLocaleDateString('en-US', {
                weekday: 'long',
                month: 'long',
                day: 'numeric',
              })}
            </Text>
          </View>
          <TouchableOpacity
            style={styles.notificationButton}
            onPress={() => router.push('/notifications')}
          >
            <Bell size={22} color={colors.textPrimary} />
          </TouchableOpacity>
        </View>

        {data && (
          <View style={styles.sections}>
            <HealthScoreCard
              score={data.healthScore}
              delta={data.scoreDelta}
            />

            <DailyTipBanner tip={data.dailyTip} />

            <RecentScanCard
              scanTime={data.lastScanTime ? formatTimeAgo(data.lastScanTime) : undefined}
              rednessLevel={data.rednessLevel ?? undefined}
              onScan={() => router.push('/(tabs)/scan')}
            />

            <View>
              <Text style={styles.sectionTitle}>Environment</Text>
              <EnvironmentCards data={data.environment} />
            </View>

            <View>
              <Text style={styles.sectionTitle}>Quick Actions</Text>
              <QuickActionBar
                onScan={() => router.push('/(tabs)/scan')}
                onLog={() => {}}
                onTrends={() => router.push('/(tabs)/timeline')}
                onForecast={() => router.push('/notifications')}
              />
            </View>
          </View>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

function formatTimeAgo(dateString: string): string {
  const date = new Date(dateString);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
  const diffDays = Math.floor(diffHours / 24);

  if (diffDays > 0) return `${diffDays}d ago`;
  if (diffHours > 0) return `${diffHours}h ago`;
  return 'Just now';
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
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: spacing.lg,
  },
  greeting: {
    ...typography.heading2,
    color: colors.textPrimary,
  },
  date: {
    ...typography.caption,
    color: colors.textSecondary,
    marginTop: 2,
  },
  notificationButton: {
    width: 44,
    height: 44,
    borderRadius: 22,
    backgroundColor: colors.bgSurface,
    alignItems: 'center',
    justifyContent: 'center',
  },
  sections: {
    gap: 20,
  },
  sectionTitle: {
    ...typography.label,
    color: colors.textPrimary,
    marginBottom: spacing.sm,
  },
});
