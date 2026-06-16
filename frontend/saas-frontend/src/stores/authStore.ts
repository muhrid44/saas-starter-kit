import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { User, AuthTokens } from '@/types'

interface AuthState {
  user: User | null
  tokens: AuthTokens | null

  setAuth: (user: User, tokens: AuthTokens) => void
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

      setAuth: (user, tokens) => set({ user, tokens }),

      setTokens: (tokens) => set({ tokens }),

      logout: () => set({ user: null, tokens: null}),

      isAuthenticated: () => !!get().tokens?.accessToken,
    }),
    {
      name: 'auth-storage',
    }
  )
)