import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { Smartphone } from 'lucide-react-native';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';

interface ConnectedApp {
  id: string;
  name: string;
  connected: boolean;
}

interface ConnectedAppsProps {
  apps: ConnectedApp[];
  onConnect: (appId: string) => void;
  onDisconnect: (appId: string) => void;
}

export function ConnectedApps({ apps, onConnect, onDisconnect }: ConnectedAppsProps) {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>Connected Apps</Text>
      {apps.map((app) => (
        <View key={app.id} style={styles.row}>
          <View style={styles.iconContainer}>
            <Smartphone size={18} color={colors.textSecondary} />
          </View>
          <Text style={styles.appName}>{app.name}</Text>
          {app.connected ? (
            <Badge label="Connected" variant="success" />
          ) : (
            <Button
              label="Connect"
              onPress={() => onConnect(app.id)}
              variant="secondary"
              size="sm"
            />
          )}
        </View>
      ))}
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
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: spacing.sm,
    borderTopWidth: 1,
    borderTopColor: colors.border,
  },
  iconContainer: {
    width: 36,
    height: 36,
    borderRadius: 18,
    backgroundColor: colors.bgSurface,
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: spacing.sm,
  },
  appName: {
    ...typography.body,
    color: colors.textPrimary,
    flex: 1,
  },
});
