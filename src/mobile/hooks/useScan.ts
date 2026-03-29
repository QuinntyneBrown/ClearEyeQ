import { useCallback, useEffect } from 'react';
import api from '@/services/api';
import { useScanStore, ScanResult, ScanStatus } from '@/stores/scanStore';
import { useSignalR } from './useSignalR';

interface InitiateScanResponse {
  scanId: string;
  uploadUrl: string;
}

interface ScanResultResponse {
  scan: ScanResult;
}

export function useScan() {
  const {
    currentScan,
    status,
    progress,
    positioningFeedback,
    setCurrentScan,
    addToHistory,
    setStatus,
    setProgress,
    setPositioningFeedback,
    reset,
  } = useScanStore();

  const { on, invoke, connect, disconnect } = useSignalR({
    hub: 'scan',
    autoConnect: false,
  });

  useEffect(() => {
    const unsubProgress = on('ScanProgress', (...args: unknown[]) => {
      const data = args[0] as { progress: number; status: ScanStatus };
      setProgress(data.progress);
      setStatus(data.status);
    });

    const unsubPositioning = on('PositioningFeedback', (...args: unknown[]) => {
      const data = args[0] as { message: string; aligned: boolean };
      setPositioningFeedback(data.message);
    });

    const unsubComplete = on('ScanComplete', (...args: unknown[]) => {
      const data = args[0] as { scan: ScanResult };
      setCurrentScan(data.scan);
      addToHistory(data.scan);
      setStatus('complete');
    });

    const unsubError = on('ScanError', (...args: unknown[]) => {
      const data = args[0] as { message: string };
      console.error('[Scan] Error:', data.message);
      setStatus('error');
    });

    return () => {
      unsubProgress();
      unsubPositioning();
      unsubComplete();
      unsubError();
    };
  }, [on, setProgress, setStatus, setPositioningFeedback, setCurrentScan, addToHistory]);

  const initiateScan = useCallback(async (eyeSide: 'left' | 'right') => {
    reset();
    setStatus('positioning');

    await connect();

    try {
      const response = await api.post<InitiateScanResponse>('/api/scans/initiate', { eyeSide });
      const { scanId } = response.data;

      await invoke('JoinScanSession', scanId);

      return scanId;
    } catch (error) {
      setStatus('error');
      throw error;
    }
  }, [connect, invoke, reset, setStatus]);

  const captureScan = useCallback(async (scanId: string, imageBase64: string) => {
    setStatus('uploading');
    try {
      await api.post(`/api/scans/${scanId}/capture`, {
        imageData: imageBase64,
      });
      setStatus('processing');
    } catch (error) {
      setStatus('error');
      throw error;
    }
  }, [setStatus]);

  const getScanResult = useCallback(async (scanId: string) => {
    try {
      const response = await api.get<ScanResultResponse>(`/api/scans/${scanId}/result`);
      const { scan } = response.data;
      setCurrentScan(scan);
      return scan;
    } catch (error) {
      console.error('[Scan] Failed to get result:', error);
      throw error;
    }
  }, [setCurrentScan]);

  const endScanSession = useCallback(async () => {
    await disconnect();
    reset();
  }, [disconnect, reset]);

  return {
    currentScan,
    status,
    progress,
    positioningFeedback,
    initiateScan,
    captureScan,
    getScanResult,
    endScanSession,
    reset,
  };
}
