import { apiClient } from './client'
import type { AuthTokens, LoginRequest, RegisterRequest, SignupRequest, SignupTokens } from '@/types'

export const authApi = {
  login: async (data: LoginRequest): Promise<{ token: AuthTokens }> => {
    const res = await apiClient.post('/auth/login', data)
    return res.data
  },
  register: async (data: RegisterRequest): Promise<{ message: string }> => {
    const res = await apiClient.post('/auth/register', data)
    return res.data
  },
    signup: async (data: SignupRequest): Promise<{ token: SignupTokens }> => {
    const res = await apiClient.post('/auth/signup', data)
    return res.data
  },
}