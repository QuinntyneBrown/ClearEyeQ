import { useCallback, useState } from 'react';
import { Platform } from 'react-native';
import api from '@/services/api';

interface SleepData {
  date: string;
  totalMinutes: number;
  quality: 'poor' | 'fair' | 'good' | 'excellent';
}

interface StepData {
  date: string;
  count: number;
}

export function useHealthKit() {
  const [isConnected, setIsConnected] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const requestPermission = useCallback(async (): Promise<boolean> => {
    if (Platform.OS !== 'ios') {
      console.warn('HealthKit is only available on iOS');
      return false;
    }

    setIsLoading(true);
    try {
      // HealthKit permission request would use expo-health or react-native-health
      // For now we simulate the permission request
      setIsConnected(true);
      return true;
    } catch (error) {
      console.error('Failed to request HealthKit permissions:', error);
      return false;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const getSleepData = useCallback(async (days: number = 7): Promise<SleepData[]> => {
    if (!isConnected) {
      console.warn('HealthKit not connected');
      return [];
    }

    try {
      // In a real implementation, this would read from HealthKit
      // and sync to the backend monitoring API
      const response = await api.get<{ sleepData: SleepData[] }>(
        `/api/monitoring/health/sleep?days=${days}`
      );
      return response.data.sleepData;
    } catch (error) {
      console.error('Failed to get sleep data:', error);
      return [];
    }
  }, [isConnected]);

  const getSteps = useCallback(async (days: number = 7): Promise<StepData[]> => {
    if (!isConnected) {
      console.warn('HealthKit not connected');
      return [];
    }

    try {
      const response = await api.get<{ stepData: StepData[] }>(
        `/api/monitoring/health/steps?days=${days}`
      );
      return response.data.stepData;
    } catch (error) {
      console.error('Failed to get step data:', error);
      return [];
    }
  }, [isConnected]);

  const syncHealthData = useCallback(async () => {
    if (!isConnected) return;

    setIsLoading(true);
    try {
      const [sleepData, stepData] = await Promise.all([
        getSleepData(1),
        getSteps(1),
      ]);

      await api.post('/api/monitoring/health/sync', {
        sleepData,
        stepData,
        syncedAt: new Date().toISOString(),
      });
    } catch (error) {
      console.error('Failed to sync health data:', error);
    } finally {
      setIsLoading(false);
    }
  }, [isConnected, getSleepData, getSteps]);

  return {
    isConnected,
    isLoading,
    requestPermission,
    getSleepData,
    getSteps,
    syncHealthData,
  };
}
