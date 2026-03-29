"use client";

import { useState, useCallback, useEffect } from "react";
import { apiPost } from "@/services/api";
import {
  getToken,
  setToken,
  clearToken,
  isAuthenticated as checkAuth,
  setRefreshToken,
  getRefreshToken,
} from "@/lib/auth";

interface User {
  id: string;
  email: string;
  name: string;
  role: string;
  tenantId: string;
}

interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

interface UseAuthReturn {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  refresh: () => Promise<void>;
}

export function useAuth(): UseAuthReturn {
  const [user, setUser] = useState<User | null>(null);
  const [token, setTokenState] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const storedToken = getToken();
    if (storedToken && checkAuth()) {
      setTokenState(storedToken);
      try {
        const payload = JSON.parse(atob(storedToken.split(".")[1]));
        setUser({
          id: payload.sub,
          email: payload.email,
          name: payload.name,
          role: payload.role,
          tenantId: payload.tenant_id,
        });
      } catch {
        clearToken();
      }
    }
    setIsLoading(false);
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    setIsLoading(true);
    try {
      const response = await apiPost<AuthResponse>(
        "/api/identity/auth/login",
        { email, password }
      );
      setToken(response.accessToken);
      setRefreshToken(response.refreshToken);
      setTokenState(response.accessToken);
      setUser(response.user);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const logout = useCallback(() => {
    clearToken();
    setTokenState(null);
    setUser(null);
  }, []);

  const refresh = useCallback(async () => {
    const refreshToken = getRefreshToken();
    if (!refreshToken) {
      logout();
      return;
    }
    try {
      const response = await apiPost<AuthResponse>(
        "/api/identity/auth/refresh",
        { refreshToken }
      );
      setToken(response.accessToken);
      setRefreshToken(response.refreshToken);
      setTokenState(response.accessToken);
      setUser(response.user);
    } catch {
      logout();
    }
  }, [logout]);

  return {
    user,
    token,
    isAuthenticated: checkAuth(),
    isLoading,
    login,
    logout,
    refresh,
  };
}
