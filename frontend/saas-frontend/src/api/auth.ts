import { apiClient } from './client'
import type { AuthTokens, LoginRequest, RegisterRequest } from '@/types'

export const authApi = {
  login: async (data: LoginRequest): Promise<{ token: AuthTokens }> => {
    const res = await apiClient.post('/auth/login', data)
    return res.data
  },
  register: async (data: RegisterRequest): Promise<{ message: string }> => {
    const res = await apiClient.post('/auth/register', data)
    return res.data
  },
}