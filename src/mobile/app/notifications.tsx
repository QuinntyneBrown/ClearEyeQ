import React, { useState, useEffect } from 'react';
import { ScrollView, View, Text, StyleSheet, SafeAreaView } from 'react-native';
import { CloudSun, AlertTriangle, Bell, Clock } from 'lucide-react-native';
import { ForecastCards } from '@/components/charts/ForecastCards';
import { Card } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Spinner } from '@/components/ui/Spinner';
import api from '@/services/api';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';

interface NotificationItem {
  id: string;
  type: 'urgent' | 'warning' | 'reminder';
  title: string;
  message: string;
  time: string;
  read: boolean;
}

export default function NotificationsScreen() {
  const [forecasts, setForecasts] = useState<Array<{ day: string; score: number; risk: string }>>([]);
  const [notifications, setNotifications] = useState<NotificationItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadNotifications();
  }, []);

  const loadNotifications = async () => {
    try {
      const [forecastRes, notifsRes] = await Promise.allSettled([
        api.get('/api/predictions/forecast?days=3'),
        api.get('/api/notifications/recent'),
      ]);

      setForecasts(
        forecastRes.status === 'fulfilled'
          ? forecastRes.value.data.forecasts
          : [
              { day: 'Tomorrow', score: 68, risk: 'Moderate' },
              { day: 'Wed', score: 72, risk: 'Low' },
              { day: 'Thu', score: 55, risk: 'High' },
            ]
      );

      setNotifications(
        notifsRes.status === 'fulfilled'
          ? notifsRes.value.data.notifications
          : [
              {
                id: '1',
                type: 'urgent',
                title: 'High Pollen Alert',
                message: 'Pollen levels are extremely high today. Consider wearing protective eyewear outdoors.',
                time: '2h ago',
                read: false,
              },
              {
                id: '2',
                type: 'warning',
                title: 'Screen Time Warning',
                message: 'You have been using screens for 4 hours. Take a break using the 20-20-20 rule.',
                time: '3h ago',
                read: false,
              },
              {
                id: '3',
                type: 'reminder',
                title: 'Eye Drops Reminder',
                message: 'Time to apply your artificial tears as prescribed.',
                time: '5h ago',
                read: true,
              },
              {
                id: '4',
                type: 'reminder',
                title: 'Weekly Scan Due',
                message: 'It has been 7 days since your last eye scan. Schedule one today.',
                time: '1d ago',
                read: true,
              },
            ]
      );
    } catch (error) {
      // Fallback data already set
    } finally {
      setIsLoading(false);
    }
  };

  const getNotificationIcon = (type: NotificationItem['type']) => {
    switch (type) {
      case 'urgent':
        return <AlertTriangle size={18} color={colors.error} />;
      case 'warning':
        return <CloudSun size={18} color={colors.warning} />;
      case 'reminder':
        return <Clock size={18} color={colors.primary} />;
    }
  };

  const getNotificationVariant = (type: NotificationItem['type']): 'alert' | 'insight' | 'action' => {
    switch (type) {
      case 'urgent': return 'alert';
      case 'warning': return 'insight';
      case 'reminder': return 'action';
    }
  };

  if (isLoading) {
    return <Spinner fullScreen label="Loading notifications..." />;
  }

  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView
        style={styles.container}
        contentContainerStyle={styles.content}
        showsVerticalScrollIndicator={false}
      >
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>72-Hour Forecast</Text>
          <ForecastCards forecasts={forecasts} />
        </View>

        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Recent Alerts</Text>
          <View style={styles.notificationList}>
            {notifications.map((notification) => (
              <Card
                key={notification.id}
                variant={getNotificationVariant(notification.type)}
                style={[styles.notificationCard, !notification.read && styles.unread]}
              >
                <View style={styles.notificationHeader}>
                  {getNotificationIcon(notification.type)}
                  <Text style={styles.notificationTitle}>{notification.title}</Text>
                  <Text style={styles.notificationTime}>{notification.time}</Text>
                </View>
                <Text style={styles.notificationMessage}>{notification.message}</Text>
                {!notification.read && (
                  <Badge label="New" variant="info" style={styles.newBadge} />
                )}
              </Card>
            ))}
          </View>
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
  section: {
    gap: spacing.sm,
  },
  sectionTitle: {
    ...typography.label,
    color: colors.textPrimary,
  },
  notificationList: {
    gap: spacing.sm,
  },
  notificationCard: {
    position: 'relative',
  },
  unread: {
    borderLeftWidth: 3,
    borderLeftColor: colors.primary,
  },
  notificationHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
    marginBottom: spacing.xs,
  },
  notificationTitle: {
    ...typography.label,
    color: colors.textPrimary,
    flex: 1,
  },
  notificationTime: {
    ...typography.caption,
    color: colors.textTertiary,
  },
  notificationMessage: {
    ...typography.caption,
    color: colors.textSecondary,
    lineHeight: 20,
  },
  newBadge: {
    position: 'absolute',
    top: spacing.sm,
    right: spacing.sm,
  },
});
