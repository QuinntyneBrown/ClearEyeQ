import React, { useState, useCallback } from 'react';
import { View, Text, TouchableOpacity, StyleSheet, SafeAreaView } from 'react-native';
import { router } from 'expo-router';
import { Eye } from 'lucide-react-native';
import { CameraViewfinder } from '@/components/scan/CameraViewfinder';
import { ScanProgress } from '@/components/scan/ScanProgress';
import { ScanResults } from '@/components/scan/ScanResults';
import { Button } from '@/components/ui/Button';
import { useToast } from '@/components/ui/Toast';
import { useScan } from '@/hooks/useScan';
import { colors } from '@/theme/colors';
import { typography } from '@/theme/typography';
import { spacing, radius } from '@/theme/spacing';

type ScanPhase = 'select' | 'camera' | 'processing' | 'results';

export default function ScanScreen() {
  const [phase, setPhase] = useState<ScanPhase>('select');
  const [eyeSide, setEyeSide] = useState<'left' | 'right'>('left');
  const [scanId, setScanId] = useState<string | null>(null);
  const { showToast } = useToast();

  const {
    currentScan,
    status,
    progress,
    positioningFeedback,
    initiateScan,
    captureScan,
    endScanSession,
    reset,
  } = useScan();

  const handleStartScan = useCallback(async (side: 'left' | 'right') => {
    setEyeSide(side);
    try {
      const id = await initiateScan(side);
      setScanId(id);
      setPhase('camera');
    } catch (error) {
      showToast('error', 'Scan Error', 'Failed to start scan session. Please try again.');
    }
  }, [initiateScan, showToast]);

  const handleCapture = useCallback(async (base64: string) => {
    if (!scanId) return;
    setPhase('processing');
    try {
      await captureScan(scanId, base64);
    } catch (error) {
      showToast('error', 'Upload Error', 'Failed to upload scan image.');
      setPhase('camera');
    }
  }, [scanId, captureScan, showToast]);

  // Transition to results when scan completes
  React.useEffect(() => {
    if (status === 'complete' && currentScan) {
      setPhase('results');
    }
  }, [status, currentScan]);

  const handleRescan = useCallback(() => {
    reset();
    setPhase('select');
    setScanId(null);
  }, [reset]);

  const handleClose = useCallback(() => {
    endScanSession();
    setPhase('select');
    setScanId(null);
  }, [endScanSession]);

  if (phase === 'camera') {
    return (
      <CameraViewfinder
        positioningFeedback={positioningFeedback}
        isAligned={status === 'positioning'}
        onCapture={handleCapture}
        onClose={handleClose}
      />
    );
  }

  if (phase === 'processing') {
    return (
      <ScanProgress
        progress={progress}
        statusText={
          status === 'uploading' ? 'Uploading image...'
            : status === 'processing' ? 'Analyzing your eye...'
            : 'Preparing...'
        }
      />
    );
  }

  if (phase === 'results' && currentScan) {
    return (
      <ScanResults
        result={currentScan}
        onViewDiagnosis={() => router.push(`/diagnosis/${currentScan.id}`)}
        onRescan={handleRescan}
        onShare={() => showToast('info', 'Share', 'Sharing will be available soon.')}
      />
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.content}>
        <View style={styles.header}>
          <Text style={styles.title}>Eye Scan</Text>
          <Text style={styles.subtitle}>Choose which eye to scan</Text>
        </View>

        <View style={styles.eyeSelection}>
          <TouchableOpacity
            style={[styles.eyeCard, eyeSide === 'left' && styles.eyeCardSelected]}
            onPress={() => setEyeSide('left')}
            activeOpacity={0.7}
          >
            <View style={styles.eyeIconContainer}>
              <Eye size={32} color={eyeSide === 'left' ? colors.primary : colors.textTertiary} />
            </View>
            <Text style={[styles.eyeLabel, eyeSide === 'left' && styles.eyeLabelSelected]}>
              Left Eye
            </Text>
          </TouchableOpacity>

          <TouchableOpacity
            style={[styles.eyeCard, eyeSide === 'right' && styles.eyeCardSelected]}
            onPress={() => setEyeSide('right')}
            activeOpacity={0.7}
          >
            <View style={styles.eyeIconContainer}>
              <Eye size={32} color={eyeSide === 'right' ? colors.primary : colors.textTertiary} />
            </View>
            <Text style={[styles.eyeLabel, eyeSide === 'right' && styles.eyeLabelSelected]}>
              Right Eye
            </Text>
          </TouchableOpacity>
        </View>

        <View style={styles.instructions}>
          <Text style={styles.instructionTitle}>Scan Tips</Text>
          <Text style={styles.instructionText}>1. Find a well-lit environment</Text>
          <Text style={styles.instructionText}>2. Hold your phone at arm's length</Text>
          <Text style={styles.instructionText}>3. Position your eye within the guide</Text>
          <Text style={styles.instructionText}>4. Keep still until the capture completes</Text>
        </View>

        <Button
          label="Start Scan"
          onPress={() => handleStartScan(eyeSide)}
          size="lg"
          fullWidth
        />
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.bgPage,
  },
  content: {
    flex: 1,
    padding: spacing.lg,
    justifyContent: 'center',
  },
  header: {
    alignItems: 'center',
    marginBottom: spacing.xl,
  },
  title: {
    ...typography.heading1,
    color: colors.textPrimary,
  },
  subtitle: {
    ...typography.body,
    color: colors.textSecondary,
    marginTop: spacing.xs,
  },
  eyeSelection: {
    flexDirection: 'row',
    gap: spacing.md,
    marginBottom: spacing.xl,
  },
  eyeCard: {
    flex: 1,
    alignItems: 'center',
    paddingVertical: spacing.lg,
    borderRadius: radius.md,
    borderWidth: 2,
    borderColor: colors.border,
    backgroundColor: colors.bgPage,
  },
  eyeCardSelected: {
    borderColor: colors.primary,
    backgroundColor: colors.primaryLight,
  },
  eyeIconContainer: {
    marginBottom: spacing.sm,
  },
  eyeLabel: {
    ...typography.label,
    color: colors.textSecondary,
  },
  eyeLabelSelected: {
    color: colors.primary,
    fontWeight: '600',
  },
  instructions: {
    backgroundColor: colors.bgSurface,
    borderRadius: radius.md,
    padding: spacing.md,
    marginBottom: spacing.xl,
  },
  instructionTitle: {
    ...typography.label,
    color: colors.textPrimary,
    marginBottom: spacing.sm,
  },
  instructionText: {
    ...typography.caption,
    color: colors.textSecondary,
    marginBottom: spacing.xs,
    lineHeight: 22,
  },
});
