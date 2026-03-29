import React, { useState } from 'react';
import { View, Text, StyleSheet, SafeAreaView } from 'react-native';
import { router } from 'expo-router';
import { User, Smartphone, Camera, Bell } from 'lucide-react-native';
import { Button } from '@/components/ui/Button';
import { Toggle } from '@/components/ui/Toggle';
import { useHealthKit } from '@/hooks/useHealthKit';
import { registerForPushNotifications } from '@/services/notifications';
import { useCameraPermissions } from 'expo-camera';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';

const TOTAL_STEPS = 4;

export default function OnboardingScreen() {
  const [currentStep, setCurrentStep] = useState(0);
  const [healthKitEnabled, setHealthKitEnabled] = useState(false);
  const { requestPermission: requestHealthKit } = useHealthKit();
  const [, requestCameraPermission] = useCameraPermissions();

  const handleNext = async () => {
    if (currentStep === 1 && healthKitEnabled) {
      await requestHealthKit();
    }
    if (currentStep === 2) {
      await requestCameraPermission();
    }
    if (currentStep === 3) {
      await registerForPushNotifications();
    }

    if (currentStep < TOTAL_STEPS - 1) {
      setCurrentStep(currentStep + 1);
    } else {
      router.replace('/(tabs)');
    }
  };

  const handleSkip = () => {
    if (currentStep < TOTAL_STEPS - 1) {
      setCurrentStep(currentStep + 1);
    } else {
      router.replace('/(tabs)');
    }
  };

  const steps = [
    {
      icon: <User size={40} color={colors.primary} />,
      title: 'Set Up Your Profile',
      description: 'Tell us about yourself so we can personalize your eye health experience.',
      content: (
        <View style={styles.stepContent}>
          <Text style={styles.contentText}>
            Your profile helps us tailor recommendations and track your eye health progress over time.
          </Text>
        </View>
      ),
    },
    {
      icon: <Smartphone size={40} color={colors.primary} />,
      title: 'Health Integrations',
      description: 'Connect health apps to correlate sleep and activity with eye health.',
      content: (
        <View style={styles.stepContent}>
          <Toggle
            label="Apple Health / Google Fit"
            description="Sync sleep, steps, and activity data"
            value={healthKitEnabled}
            onValueChange={setHealthKitEnabled}
          />
        </View>
      ),
    },
    {
      icon: <Camera size={40} color={colors.primary} />,
      title: 'Camera Access',
      description: 'We need your camera to scan and analyze your eye health.',
      content: (
        <View style={styles.stepContent}>
          <Text style={styles.contentText}>
            ClearEyeQ uses your front camera to capture detailed images of your eyes for health analysis. Images are processed securely and never shared without your consent.
          </Text>
        </View>
      ),
    },
    {
      icon: <Bell size={40} color={colors.primary} />,
      title: 'Stay Informed',
      description: 'Get timely reminders and health alerts.',
      content: (
        <View style={styles.stepContent}>
          <Text style={styles.contentText}>
            Receive scan reminders, treatment notifications, and environmental alerts that may affect your eye health.
          </Text>
        </View>
      ),
    },
  ];

  const step = steps[currentStep];

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.stepIndicator}>
        {Array.from({ length: TOTAL_STEPS }).map((_, index) => (
          <View
            key={index}
            style={[
              styles.dot,
              index === currentStep && styles.dotActive,
              index < currentStep && styles.dotCompleted,
            ]}
          />
        ))}
      </View>

      <View style={styles.body}>
        <View style={styles.iconCircle}>{step.icon}</View>
        <Text style={styles.title}>{step.title}</Text>
        <Text style={styles.description}>{step.description}</Text>
        {step.content}
      </View>

      <View style={styles.footer}>
        <Button
          label={currentStep === TOTAL_STEPS - 1 ? 'Get Started' : 'Continue'}
          onPress={handleNext}
          fullWidth
          size="lg"
        />
        {currentStep < TOTAL_STEPS - 1 && (
          <Button
            label="Skip"
            onPress={handleSkip}
            variant="ghost"
            fullWidth
          />
        )}
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.bgPage,
    padding: spacing.lg,
  },
  stepIndicator: {
    flexDirection: 'row',
    justifyContent: 'center',
    gap: spacing.sm,
    marginTop: spacing.lg,
    marginBottom: spacing.xl,
  },
  dot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: colors.border,
  },
  dotActive: {
    width: 24,
    backgroundColor: colors.primary,
    borderRadius: radius.pill,
  },
  dotCompleted: {
    backgroundColor: colors.primary,
  },
  body: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: spacing.md,
  },
  iconCircle: {
    width: 88,
    height: 88,
    borderRadius: 44,
    backgroundColor: colors.primaryLight,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: spacing.lg,
  },
  title: {
    ...typography.heading1,
    color: colors.textPrimary,
    textAlign: 'center',
    marginBottom: spacing.sm,
  },
  description: {
    ...typography.body,
    color: colors.textSecondary,
    textAlign: 'center',
    marginBottom: spacing.lg,
  },
  stepContent: {
    width: '100%',
  },
  contentText: {
    ...typography.body,
    color: colors.textSecondary,
    textAlign: 'center',
    lineHeight: 24,
  },
  footer: {
    gap: spacing.sm,
    paddingBottom: spacing.md,
  },
});
