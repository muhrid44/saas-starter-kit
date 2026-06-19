export interface User {
  id: string
  email: string
  fullName: string
  isActive: boolean
  tenantId: string
  createdDate: string
  modifiedDate: string
  roles: string[]
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
  eventName: string
  description: 'Created' | 'Updated' | 'Deleted'
  changedBy: string | null
  changedDate: string | null
  tenantId: string
}

export interface RegisterRequest {
  email: string
  password: string
  fullName: string
}

export interface SignupRequest {
  fullName: string
  email: string
  password: string
  tenantName: string
  tenantSlug: string
}

export interface SignupTokens {
  accessToken: string
  refreshToken: string
}

export interface DashboardInfo {
  totalUser: number
  auditLogsEvent: number
  activeUser: number
}