import { apiClient } from './client'
import type { DashboardInfo, User } from '@/types'

export const userApi = {
  getUsers: async (): Promise<User[]> => {
    const res = await apiClient.get('/user/all')
    return res.data
  },

  getUser: async (): Promise<User> => {
    const res = await apiClient.get('/user')
    return res.data
  },

  getDashboardInfo: async (): Promise<DashboardInfo> => {
    const res = await apiClient.get('/user/dashboard/info')
    return res.data
  },

  updateProfile: async (data: { fullName: string; email: string }): Promise<void> => {
    await apiClient.put('/user/profile', data)
  },

  changePassword: async (data: { currentPassword: string; newPassword: string }): Promise<void> => {
    await apiClient.put('/user/password', data)
  },

}