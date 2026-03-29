import React, { useRef, useCallback } from 'react';
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { CameraView, useCameraPermissions } from 'expo-camera';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing } from '@/theme/spacing';
import { PositioningGuide } from './PositioningGuide';

interface CameraViewfinderProps {
  positioningFeedback: string;
  isAligned: boolean;
  onCapture: (base64: string) => void;
  onClose: () => void;
}

export function CameraViewfinder({
  positioningFeedback,
  isAligned,
  onCapture,
  onClose,
}: CameraViewfinderProps) {
  const cameraRef = useRef<CameraView>(null);
  const [permission, requestPermission] = useCameraPermissions();

  const handleCapture = useCallback(async () => {
    if (!cameraRef.current) return;

    try {
      const photo = await cameraRef.current.takePictureAsync({
        base64: true,
        quality: 0.8,
      });
      if (photo?.base64) {
        onCapture(photo.base64);
      }
    } catch (error) {
      console.error('Failed to capture photo:', error);
    }
  }, [onCapture]);

  if (!permission) {
    return <View style={styles.container} />;
  }

  if (!permission.granted) {
    return (
      <View style={styles.permissionContainer}>
        <Text style={styles.permissionTitle}>Camera Access Required</Text>
        <Text style={styles.permissionText}>
          ClearEyeQ needs camera access to scan your eyes and assess eye health.
        </Text>
        <TouchableOpacity style={styles.permissionButton} onPress={requestPermission}>
          <Text style={styles.permissionButtonText}>Grant Access</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <CameraView
        ref={cameraRef}
        style={styles.camera}
        facing="front"
      >
        <View style={styles.overlay}>
          <TouchableOpacity style={styles.closeButton} onPress={onClose}>
            <Text style={styles.closeText}>Cancel</Text>
          </TouchableOpacity>

          <PositioningGuide isAligned={isAligned} />

          <Text style={styles.feedback}>{positioningFeedback || 'Position your eye within the guide'}</Text>

          <View style={styles.captureRow}>
            <TouchableOpacity
              style={[styles.captureButton, isAligned && styles.captureButtonActive]}
              onPress={handleCapture}
              activeOpacity={0.7}
            >
              <View style={[styles.captureInner, isAligned && styles.captureInnerActive]} />
            </TouchableOpacity>
          </View>
        </View>
      </CameraView>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.black,
  },
  camera: {
    flex: 1,
  },
  overlay: {
    flex: 1,
    justifyContent: 'space-between',
    paddingVertical: spacing.xl + spacing.xl,
    paddingHorizontal: spacing.lg,
  },
  closeButton: {
    alignSelf: 'flex-start',
    padding: spacing.sm,
  },
  closeText: {
    ...typography.body,
    color: colors.white,
    fontWeight: '600',
  },
  feedback: {
    ...typography.body,
    color: colors.white,
    textAlign: 'center',
    fontWeight: '500',
  },
  captureRow: {
    alignItems: 'center',
    paddingBottom: spacing.lg,
  },
  captureButton: {
    width: 76,
    height: 76,
    borderRadius: 38,
    borderWidth: 4,
    borderColor: colors.white,
    alignItems: 'center',
    justifyContent: 'center',
  },
  captureButtonActive: {
    borderColor: colors.primary,
  },
  captureInner: {
    width: 60,
    height: 60,
    borderRadius: 30,
    backgroundColor: colors.white,
  },
  captureInnerActive: {
    backgroundColor: colors.primary,
  },
  permissionContainer: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: colors.black,
    paddingHorizontal: spacing.xl,
  },
  permissionTitle: {
    ...typography.heading2,
    color: colors.white,
    marginBottom: spacing.sm,
  },
  permissionText: {
    ...typography.body,
    color: colors.textTertiary,
    textAlign: 'center',
    marginBottom: spacing.lg,
  },
  permissionButton: {
    backgroundColor: colors.primary,
    paddingHorizontal: spacing.xl,
    paddingVertical: spacing.sm + 4,
    borderRadius: 12,
  },
  permissionButtonText: {
    ...typography.bodyMedium,
    color: colors.white,
    fontWeight: '600',
  },
});
