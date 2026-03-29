import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import AsyncStorage from '@react-native-async-storage/async-storage';

interface SettingsState {
  textSize: 'small' | 'medium' | 'large';
  highContrast: boolean;
  voiceGuided: boolean;
  language: string;
  monitoringEnabled: boolean;
  dataSharingEnabled: boolean;
  setTextSize: (size: 'small' | 'medium' | 'large') => void;
  setHighContrast: (enabled: boolean) => void;
  setVoiceGuided: (enabled: boolean) => void;
  setLanguage: (language: string) => void;
  setMonitoringEnabled: (enabled: boolean) => void;
  setDataSharingEnabled: (enabled: boolean) => void;
}

export const useSettingsStore = create<SettingsState>()(
  persist(
    (set) => ({
      textSize: 'medium',
      highContrast: false,
      voiceGuided: false,
      language: 'en',
      monitoringEnabled: true,
      dataSharingEnabled: false,

      setTextSize: (textSize) => set({ textSize }),
      setHighContrast: (highContrast) => set({ highContrast }),
      setVoiceGuided: (voiceGuided) => set({ voiceGuided }),
      setLanguage: (language) => set({ language }),
      setMonitoringEnabled: (monitoringEnabled) => set({ monitoringEnabled }),
      setDataSharingEnabled: (dataSharingEnabled) => set({ dataSharingEnabled }),
    }),
    {
      name: 'cleareyeq-settings',
      storage: createJSONStorage(() => AsyncStorage),
    }
  )
);
