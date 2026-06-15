import { apiClient } from './client'
import type { User } from '@/types'

export const usersApi = {
  getAll: async (): Promise<User[]> => {
    const res = await apiClient.get('/users')
    return res.data
  },
}