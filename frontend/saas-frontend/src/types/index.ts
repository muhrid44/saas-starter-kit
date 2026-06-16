export interface User {
  id: string
  email: string
  fullName: string
  tenantId: string
  isActive: boolean
  roles: string[]
  createAt: string
}

export interface AuthTokens {
  accessToken: string
  refreshToken: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface AuditLog {
  id: string
  entityName: string
  action: 'Created' | 'Updated' | 'Deleted'
  oldValues: string | null
  newValues: string | null
  changedBy: string
  changedAt: string
  tenantId: string
}

export interface RegisterRequest {
  email: string
  password: string
  fullName: string
  tenantSlug: string
}