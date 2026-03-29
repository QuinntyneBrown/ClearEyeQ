import { useState, useCallback, useEffect } from 'react';
import * as SecureStore from 'expo-secure-store';
import api from '@/services/api';
import { useAuthStore, AuthUser } from '@/stores/authStore';
import { disconnectAll } from '@/services/signalr';

const TOKEN_KEY = 'cleareyeq_token';
const REFRESH_TOKEN_KEY = 'cleareyeq_refresh_token';
const USER_KEY = 'cleareyeq_user';

interface LoginResponse {
  user: AuthUser;
  accessToken: string;
  refreshToken: string;
}

export function useAuth() {
  const { user, isAuthenticated, setAuth, clearAuth } = useAuthStore();
  const [isLoading, setIsLoading] = useState(true);

  const persistAuth = useCallback(async (authUser: AuthUser, token: string, refreshToken: string) => {
    await SecureStore.setItemAsync(TOKEN_KEY, token);
    await SecureStore.setItemAsync(REFRESH_TOKEN_KEY, refreshToken);
    await SecureStore.setItemAsync(USER_KEY, JSON.stringify(authUser));
    setAuth(authUser, token, refreshToken);
  }, [setAuth]);

  const login = useCallback(async (email: string, password: string) => {
    setIsLoading(true);
    try {
      const response = await api.post<LoginResponse>('/api/identity/auth/login', {
        email,
        password,
      });
      const { user: authUser, accessToken, refreshToken } = response.data;
      await persistAuth(authUser, accessToken, refreshToken);
      return authUser;
    } finally {
      setIsLoading(false);
    }
  }, [persistAuth]);

  const signup = useCallback(async (email: string, password: string, name: string) => {
    setIsLoading(true);
    try {
      const response = await api.post<LoginResponse>('/api/identity/auth/register', {
        email,
        password,
        name,
      });
      const { user: authUser, accessToken, refreshToken } = response.data;
      await persistAuth(authUser, accessToken, refreshToken);
      return authUser;
    } finally {
      setIsLoading(false);
    }
  }, [persistAuth]);

  const logout = useCallback(async () => {
    setIsLoading(true);
    try {
      await api.post('/api/identity/auth/logout').catch(() => {});
      await disconnectAll();
      await SecureStore.deleteItemAsync(TOKEN_KEY);
      await SecureStore.deleteItemAsync(REFRESH_TOKEN_KEY);
      await SecureStore.deleteItemAsync(USER_KEY);
      clearAuth();
    } finally {
      setIsLoading(false);
    }
  }, [clearAuth]);

  const restoreSession = useCallback(async () => {
    try {
      const [token, refreshToken, userJson] = await Promise.all([
        SecureStore.getItemAsync(TOKEN_KEY),
        SecureStore.getItemAsync(REFRESH_TOKEN_KEY),
        SecureStore.getItemAsync(USER_KEY),
      ]);

      if (token && refreshToken && userJson) {
        const storedUser = JSON.parse(userJson) as AuthUser;
        setAuth(storedUser, token, refreshToken);
      }
    } catch (error) {
      console.error('Failed to restore session:', error);
      clearAuth();
    } finally {
      setIsLoading(false);
    }
  }, [setAuth, clearAuth]);

  useEffect(() => {
    restoreSession();
  }, [restoreSession]);

  return {
    user,
    isAuthenticated,
    isLoading,
    login,
    signup,
    logout,
  };
}
