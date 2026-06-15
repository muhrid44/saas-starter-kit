import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { User, AuthTokens } from '@/types'

interface AuthState {
  user: User | null
  tokens: AuthTokens | null
  tenantSlug: string | null

  setAuth: (user: User, tokens: AuthTokens, tenantSlug: string) => void
  setTokens: (tokens: AuthTokens) => void
  logout: () => void
  isAuthenticated: () => boolean
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      tokens: null,
      tenantSlug: null,

      setAuth: (user, tokens, tenantSlug) => set({ user, tokens, tenantSlug }),

      setTokens: (tokens) => set({ tokens }),

      logout: () => set({ user: null, tokens: null, tenantSlug: null }),

      isAuthenticated: () => !!get().tokens?.accessToken,
    }),
    {
      name: 'auth-storage',
    }
  )
)