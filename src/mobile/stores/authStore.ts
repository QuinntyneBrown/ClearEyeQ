import { create } from 'zustand';

export interface AuthUser {
  id: string;
  email: string;
  name: string;
  avatarUrl?: string;
  subscription?: 'free' | 'pro' | 'premium';
}

interface AuthState {
  user: AuthUser | null;
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  setAuth: (user: AuthUser, token: string, refreshToken: string) => void;
  clearAuth: () => void;
  setToken: (token: string) => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  token: null,
  refreshToken: null,
  isAuthenticated: false,

  setAuth: (user, token, refreshToken) =>
    set({
      user,
      token,
      refreshToken,
      isAuthenticated: true,
    }),

  clearAuth: () =>
    set({
      user: null,
      token: null,
      refreshToken: null,
      isAuthenticated: false,
    }),

  setToken: (token) =>
    set({ token }),
}));
