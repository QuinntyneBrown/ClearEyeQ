/**
 * FatigueEstimator - On-device eye fatigue estimation module.
 *
 * Integration approach:
 * 1. Consume blink rate data from BlinkDetector over time.
 * 2. Track blink rate trends: normal is 15-20 blinks/min; fatigue causes
 *    either significantly reduced blinks (<10/min) or compensatory rapid
 *    blinking (>25/min).
 * 3. Factor in session duration: fatigue increases with continuous screen use.
 * 4. Use a weighted scoring model combining:
 *    - Blink rate deviation from baseline (40% weight)
 *    - Session duration (30% weight)
 *    - Blink rate variability / irregularity (20% weight)
 *    - Time of day factor (10% weight)
 * 5. Output a fatigue score from 0 (alert) to 100 (severely fatigued).
 *
 * In production, this could be replaced with a TFLite model trained on
 * labeled fatigue datasets. The model file would be at
 * assets/models/fatigue_estimator.tflite.
 */

interface FatigueResult {
  fatigueScore: number;
  level: 'low' | 'moderate' | 'high' | 'severe';
  recommendation: string;
  factors: {
    blinkRateFactor: number;
    durationFactor: number;
    variabilityFactor: number;
    timeOfDayFactor: number;
  };
}

const NORMAL_BLINK_RATE_LOW = 15;
const NORMAL_BLINK_RATE_HIGH = 20;
const BLINK_RATE_WEIGHT = 0.4;
const DURATION_WEIGHT = 0.3;
const VARIABILITY_WEIGHT = 0.2;
const TIME_OF_DAY_WEIGHT = 0.1;

const recommendations: Record<string, string> = {
  low: 'Your eyes are in good shape. Keep up good habits!',
  moderate: 'Consider taking a short break. Follow the 20-20-20 rule.',
  high: 'Your eyes are showing signs of fatigue. Take a 10-minute break and look at distant objects.',
  severe: 'Significant eye fatigue detected. Stop screen use, apply warm compress, and rest your eyes for at least 15 minutes.',
};

function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max);
}

function calculateBlinkRateFactor(blinkRate: number): number {
  if (blinkRate >= NORMAL_BLINK_RATE_LOW && blinkRate <= NORMAL_BLINK_RATE_HIGH) {
    return 0;
  }

  const midpoint = (NORMAL_BLINK_RATE_LOW + NORMAL_BLINK_RATE_HIGH) / 2;
  const deviation = Math.abs(blinkRate - midpoint);
  const normalizedDeviation = deviation / midpoint;

  return clamp(normalizedDeviation * 100, 0, 100);
}

function calculateDurationFactor(durationMinutes: number): number {
  // Fatigue increases exponentially after 20 minutes of continuous use
  if (durationMinutes <= 20) return (durationMinutes / 20) * 15;
  if (durationMinutes <= 45) return 15 + ((durationMinutes - 20) / 25) * 25;
  if (durationMinutes <= 90) return 40 + ((durationMinutes - 45) / 45) * 30;
  return clamp(70 + ((durationMinutes - 90) / 60) * 30, 0, 100);
}

function calculateTimeOfDayFactor(): number {
  const hour = new Date().getHours();
  // Higher fatigue factor in late evening and early morning
  if (hour >= 22 || hour <= 5) return 80;
  if (hour >= 20 || hour <= 7) return 50;
  if (hour >= 14 && hour <= 16) return 30; // Post-lunch dip
  return 10;
}

function getFatigueLevel(score: number): 'low' | 'moderate' | 'high' | 'severe' {
  if (score < 25) return 'low';
  if (score < 50) return 'moderate';
  if (score < 75) return 'high';
  return 'severe';
}

export function estimateFatigue(
  blinkRate: number,
  durationMinutes: number
): FatigueResult {
  const blinkRateFactor = calculateBlinkRateFactor(blinkRate);
  const durationFactor = calculateDurationFactor(durationMinutes);
  const variabilityFactor = Math.random() * 30; // Simulated; real implementation tracks variance
  const timeOfDayFactor = calculateTimeOfDayFactor();

  const rawScore =
    blinkRateFactor * BLINK_RATE_WEIGHT +
    durationFactor * DURATION_WEIGHT +
    variabilityFactor * VARIABILITY_WEIGHT +
    timeOfDayFactor * TIME_OF_DAY_WEIGHT;

  const fatigueScore = Math.round(clamp(rawScore, 0, 100));
  const level = getFatigueLevel(fatigueScore);

  return {
    fatigueScore,
    level,
    recommendation: recommendations[level],
    factors: {
      blinkRateFactor: Math.round(blinkRateFactor),
      durationFactor: Math.round(durationFactor),
      variabilityFactor: Math.round(variabilityFactor),
      timeOfDayFactor: Math.round(timeOfDayFactor),
    },
  };
}

export type { FatigueResult };
