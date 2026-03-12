import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { CurrentUser } from '../api/client';

type AuthState = {
  user: CurrentUser | null;
  accessToken: string | null;
  setAuth: (user: CurrentUser, accessToken: string) => void;
  logout: () => void;
  hydrated: boolean;
  setHydrated: (v: boolean) => void;
};

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      hydrated: false,
      setHydrated: (v) => set({ hydrated: v }),
      setAuth: (user, accessToken) => {
        localStorage.setItem('accessToken', accessToken);
        set({ user, accessToken });
      },
      logout: () => {
        localStorage.removeItem('accessToken');
        set({ user: null, accessToken: null });
      },
    }),
    { name: 'livecommerce-auth', partialize: (s) => ({ user: s.user, accessToken: s.accessToken }) }
  )
);
