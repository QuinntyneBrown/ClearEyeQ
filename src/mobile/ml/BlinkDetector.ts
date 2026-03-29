/**
 * BlinkDetector - On-device blink rate detection module.
 *
 * Integration approach:
 * 1. Use expo-camera frame processor to capture frames at ~15fps.
 * 2. Load a TFLite face landmark model (e.g., MediaPipe Face Mesh) via
 *    react-native-tflite or a custom native module.
 * 3. Extract eye landmark points (upper/lower eyelid) from each frame.
 * 4. Calculate Eye Aspect Ratio (EAR) = (|p2-p6| + |p3-p5|) / (2 * |p1-p4|)
 *    where p1-p6 are the 6 eye landmark points.
 * 5. A blink is detected when EAR drops below threshold (~0.21) for
 *    consecutive frames and then rises above it.
 * 6. Count blinks over a sliding window to compute blinks per minute.
 *
 * The TFLite model file would be bundled in assets/models/face_landmark.tflite
 * and loaded at runtime. For now, this module provides the interface and a
 * simulation for development/testing purposes.
 */

interface BlinkDetectionResult {
  blinksPerMinute: number;
  totalBlinks: number;
  averageEAR: number;
  timestamp: number;
}

interface FrameData {
  base64?: string;
  width: number;
  height: number;
  timestamp: number;
}

const EAR_THRESHOLD = 0.21;
const MIN_BLINK_DURATION_MS = 50;
const MAX_BLINK_DURATION_MS = 400;
const WINDOW_SIZE_MS = 60000;

class BlinkDetectorEngine {
  private blinkTimestamps: number[] = [];
  private isEyeClosed = false;
  private eyeCloseStartTime = 0;
  private frameCount = 0;
  private earSum = 0;

  reset(): void {
    this.blinkTimestamps = [];
    this.isEyeClosed = false;
    this.eyeCloseStartTime = 0;
    this.frameCount = 0;
    this.earSum = 0;
  }

  detectBlinks(frameData: FrameData): BlinkDetectionResult {
    // In a real implementation, this would:
    // 1. Pass frameData through TFLite face landmark model
    // 2. Extract eye landmarks
    // 3. Calculate EAR
    // For development, simulate with realistic variation
    const simulatedEAR = 0.25 + Math.random() * 0.1;
    const shouldBlink = Math.random() < 0.02; // ~1.2 blinks per second at 60fps

    this.frameCount++;
    this.earSum += simulatedEAR;

    const now = frameData.timestamp;

    if (shouldBlink && !this.isEyeClosed) {
      this.isEyeClosed = true;
      this.eyeCloseStartTime = now;
    } else if (this.isEyeClosed) {
      const blinkDuration = now - this.eyeCloseStartTime;
      if (blinkDuration >= MIN_BLINK_DURATION_MS && blinkDuration <= MAX_BLINK_DURATION_MS) {
        this.blinkTimestamps.push(now);
        this.isEyeClosed = false;
      } else if (blinkDuration > MAX_BLINK_DURATION_MS) {
        this.isEyeClosed = false;
      }
    }

    // Prune old timestamps outside the window
    const windowStart = now - WINDOW_SIZE_MS;
    this.blinkTimestamps = this.blinkTimestamps.filter((t) => t >= windowStart);

    const elapsedMs = this.blinkTimestamps.length > 0
      ? now - this.blinkTimestamps[0]
      : WINDOW_SIZE_MS;
    const elapsedMinutes = Math.max(elapsedMs / 60000, 1 / 60);

    return {
      blinksPerMinute: Math.round(this.blinkTimestamps.length / elapsedMinutes),
      totalBlinks: this.blinkTimestamps.length,
      averageEAR: this.frameCount > 0 ? this.earSum / this.frameCount : 0,
      timestamp: now,
    };
  }
}

const detector = new BlinkDetectorEngine();

export function detectBlinks(frameData: FrameData): BlinkDetectionResult {
  return detector.detectBlinks(frameData);
}

export function resetBlinkDetector(): void {
  detector.reset();
}

export type { BlinkDetectionResult, FrameData };
