import React, { useState } from 'react';
import { ScrollView, View, Text, StyleSheet, SafeAreaView, Alert } from 'react-native';
import { ProfileSection } from '@/components/settings/ProfileSection';
import { ConnectedApps } from '@/components/settings/ConnectedApps';
import { PrivacyControls } from '@/components/settings/PrivacyControls';
import { SubscriptionCards } from '@/components/settings/SubscriptionCards';
import { AccessibilitySettings } from '@/components/settings/AccessibilitySettings';
import { Button } from '@/components/ui/Button';
import { Dialog } from '@/components/ui/Dialog';
import { useAuth } from '@/hooks/useAuth';
import { useAuthStore } from '@/stores/authStore';
import { useSettingsStore } from '@/stores/settingsStore';
import { useToast } from '@/components/ui/Toast';
import api from '@/services/api';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing } from '@/theme/spacing';

export default function SettingsScreen() {
  const user = useAuthStore((s) => s.user);
  const { logout } = useAuth();
  const { showToast } = useToast();
  const [deleteDialogVisible, setDeleteDialogVisible] = useState(false);

  const {
    textSize,
    highContrast,
    voiceGuided,
    language,
    monitoringEnabled,
    dataSharingEnabled,
    setTextSize,
    setHighContrast,
    setVoiceGuided,
    setMonitoringEnabled,
    setDataSharingEnabled,
  } = useSettingsStore();

  const [connectedApps, setConnectedApps] = useState([
    { id: 'apple-health', name: 'Apple Health', connected: true },
    { id: 'google-fit', name: 'Google Fit', connected: false },
    { id: 'fitbit', name: 'Fitbit', connected: false },
  ]);

  const plans = [
    {
      id: 'free',
      name: 'Free',
      price: '$0/mo',
      features: ['3 scans/month', 'Basic trends', 'Environmental alerts'],
      isCurrent: user?.subscription === 'free' || !user?.subscription,
    },
    {
      id: 'pro',
      name: 'Pro',
      price: '$9.99/mo',
      features: ['Unlimited scans', 'Full trends & insights', 'Treatment plans', 'Priority support'],
      isCurrent: user?.subscription === 'pro',
    },
    {
      id: 'premium',
      name: 'Premium',
      price: '$19.99/mo',
      features: ['Everything in Pro', 'Specialist referrals', 'Family accounts', 'FHIR export'],
      isCurrent: user?.subscription === 'premium',
    },
  ];

  const handleConnectApp = async (appId: string) => {
    try {
      await api.post(`/api/monitoring/integrations/${appId}/connect`);
      setConnectedApps((prev) =>
        prev.map((app) => (app.id === appId ? { ...app, connected: true } : app))
      );
      showToast('success', 'Connected', 'App connected successfully.');
    } catch (error) {
      showToast('error', 'Connection Failed', 'Could not connect the app.');
    }
  };

  const handleDisconnectApp = async (appId: string) => {
    try {
      await api.post(`/api/monitoring/integrations/${appId}/disconnect`);
      setConnectedApps((prev) =>
        prev.map((app) => (app.id === appId ? { ...app, connected: false } : app))
      );
      showToast('info', 'Disconnected', 'App disconnected.');
    } catch (error) {
      showToast('error', 'Error', 'Could not disconnect the app.');
    }
  };

  const handleExportData = async () => {
    try {
      await api.post('/api/identity/data/export');
      showToast('success', 'Export Started', 'You will receive an email with your data.');
    } catch (error) {
      showToast('error', 'Export Failed', 'Could not export your data.');
    }
  };

  const handleDeleteAccount = async () => {
    setDeleteDialogVisible(false);
    try {
      await api.delete('/api/identity/account');
      await logout();
      showToast('info', 'Account Deleted', 'Your account has been permanently deleted.');
    } catch (error) {
      showToast('error', 'Deletion Failed', 'Could not delete your account. Please contact support.');
    }
  };

  const handleUpgrade = async (planId: string) => {
    try {
      await api.post(`/api/billing/subscribe`, { planId });
      showToast('success', 'Subscribed', `Upgraded to ${planId} plan.`);
    } catch (error) {
      showToast('error', 'Upgrade Failed', 'Could not process subscription.');
    }
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView
        style={styles.container}
        contentContainerStyle={styles.content}
        showsVerticalScrollIndicator={false}
      >
        <Text style={styles.title}>Settings</Text>

        <ProfileSection
          name={user?.name ?? 'User'}
          email={user?.email ?? 'user@example.com'}
          subscription={user?.subscription ?? 'Free'}
          onPress={() => {}}
        />

        <ConnectedApps
          apps={connectedApps}
          onConnect={handleConnectApp}
          onDisconnect={handleDisconnectApp}
        />

        <PrivacyControls
          monitoringEnabled={monitoringEnabled}
          dataSharingEnabled={dataSharingEnabled}
          onToggleMonitoring={setMonitoringEnabled}
          onToggleDataSharing={setDataSharingEnabled}
          onExportData={handleExportData}
          onDeleteData={() => setDeleteDialogVisible(true)}
        />

        <SubscriptionCards plans={plans} onUpgrade={handleUpgrade} />

        <AccessibilitySettings
          textSize={textSize}
          highContrast={highContrast}
          voiceGuided={voiceGuided}
          language={language}
          onTextSizeChange={setTextSize}
          onHighContrastChange={setHighContrast}
          onVoiceGuidedChange={setVoiceGuided}
          onLanguagePress={() => showToast('info', 'Language', 'Language selection coming soon.')}
        />

        <Button
          label="Delete Account"
          onPress={() => setDeleteDialogVisible(true)}
          variant="destructive"
          fullWidth
        />

        <Button
          label="Log Out"
          onPress={logout}
          variant="secondary"
          fullWidth
        />
      </ScrollView>

      <Dialog
        visible={deleteDialogVisible}
        onClose={() => setDeleteDialogVisible(false)}
        title="Delete Account"
        body="This action is permanent and cannot be undone. All your data, scans, and treatment history will be permanently deleted."
        confirmLabel="Delete Forever"
        cancelLabel="Keep Account"
        onConfirm={handleDeleteAccount}
        variant="destructive"
      />
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
    gap: spacing.md,
  },
  title: {
    ...typography.heading1,
    color: colors.textPrimary,
  },
});
