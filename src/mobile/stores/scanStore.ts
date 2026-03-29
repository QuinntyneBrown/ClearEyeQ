import { create } from 'zustand';

export type ScanStatus = 'idle' | 'positioning' | 'capturing' | 'uploading' | 'processing' | 'complete' | 'error';

export interface ScanCondition {
  id: string;
  name: string;
  confidence: number;
  severity: 'mild' | 'moderate' | 'severe';
}

export interface ScanResult {
  id: string;
  eyeSide: 'left' | 'right';
  rednessScore: number;
  tearFilmScore: number;
  overallScore: number;
  conditions: ScanCondition[];
  imageUrl: string;
  createdAt: string;
}

interface ScanState {
  currentScan: ScanResult | null;
  scanHistory: ScanResult[];
  status: ScanStatus;
  progress: number;
  positioningFeedback: string;
  setCurrentScan: (scan: ScanResult | null) => void;
  addToHistory: (scan: ScanResult) => void;
  setStatus: (status: ScanStatus) => void;
  setProgress: (progress: number) => void;
  setPositioningFeedback: (feedback: string) => void;
  reset: () => void;
}

export const useScanStore = create<ScanState>((set) => ({
  currentScan: null,
  scanHistory: [],
  status: 'idle',
  progress: 0,
  positioningFeedback: '',

  setCurrentScan: (scan) => set({ currentScan: scan }),

  addToHistory: (scan) =>
    set((state) => ({
      scanHistory: [scan, ...state.scanHistory],
    })),

  setStatus: (status) => set({ status }),

  setProgress: (progress) => set({ progress }),

  setPositioningFeedback: (feedback) => set({ positioningFeedback: feedback }),

  reset: () =>
    set({
      currentScan: null,
      status: 'idle',
      progress: 0,
      positioningFeedback: '',
    }),
}));
