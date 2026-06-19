import { apiClient } from './client'
import type { User } from '@/types'

export const adminApi = {

  updateRole: async (userId: string, role: string): Promise<void> => {
    await apiClient.put('/admin/role', { userId, role })
  },

  updateStatus: async (userId: string, isActive: boolean): Promise<void> => {
    await apiClient.put('/admin/status', { userId, isActive })
  },

  resetPassword: async (userId: string, newPassword: string): Promise<void> => {
    await apiClient.put('/admin/password', { userId, newPassword })
  },

  deleteUser: async (userId: string): Promise<void> => {
    await apiClient.delete(`/admin/${userId}`)
  },
}