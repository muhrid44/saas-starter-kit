import { apiClient } from './client'
import type { AuditLog } from '@/types'

export const auditLogsApi = {
  getAll: async (): Promise<AuditLog[]> => {
    const res = await apiClient.get('/auditlogs')
    return res.data
  },
}