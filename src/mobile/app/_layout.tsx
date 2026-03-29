import React, { useEffect } from 'react';
import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import * as SplashScreen from 'expo-splash-screen';
import { ToastProvider } from '@/components/ui/Toast';
import { useAuth } from '@/hooks/useAuth';
import { Redirect } from 'expo-router';
import { colors } from '@/theme/colors';

SplashScreen.preventAutoHideAsync();

export default function RootLayout() {
  const { isAuthenticated, isLoading } = useAuth();

  useEffect(() => {
    if (!isLoading) {
      SplashScreen.hideAsync();
    }
  }, [isLoading]);

  if (isLoading) {
    return null;
  }

  return (
    <ToastProvider>
      <StatusBar style="dark" />
      <Stack
        screenOptions={{
          headerShown: false,
          contentStyle: { backgroundColor: colors.bgPage },
          animation: 'slide_from_right',
        }}
      >
        {isAuthenticated ? (
          <>
            <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
            <Stack.Screen
              name="notifications"
              options={{
                headerShown: true,
                title: 'Notifications',
                headerBackTitle: 'Back',
                headerTintColor: colors.textPrimary,
              }}
            />
            <Stack.Screen
              name="diagnosis/[id]"
              options={{
                headerShown: true,
                title: 'Diagnosis',
                headerBackTitle: 'Back',
                headerTintColor: colors.textPrimary,
              }}
            />
          </>
        ) : (
          <Stack.Screen name="(auth)" options={{ headerShown: false }} />
        )}
      </Stack>
      {!isAuthenticated && <Redirect href="/(auth)/login" />}
    </ToastProvider>
  );
}
