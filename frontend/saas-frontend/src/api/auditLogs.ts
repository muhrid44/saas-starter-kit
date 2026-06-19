import { apiClient } from './client'
import type { AuditLog, PaginatedResult } from '@/types'

export const auditLogsApi = {
  getAll: async (page = 1, pageSize = 20): Promise<PaginatedResult<AuditLog>> => {
    const res = await apiClient.get('/auditlogs', { params: { page, pageSize } })
    return res.data
  },
}