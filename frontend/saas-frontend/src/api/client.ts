import axios from 'axios'
import { useAuthStore } from '@/stores/authStore'

const BASE_URL = import.meta.env.VITE_BASE_URL ?? 'http://localhost:5000'
const API_VERSION = import.meta.env.VITE_API_VERSION ?? '/api/v1'

export const apiClient = axios.create({
  baseURL: `${BASE_URL}${API_VERSION}`,
  headers: { 'Content-Type': 'application/json' },
})

export const rootClient = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
})

// 1. Attach JWT to every request
apiClient.interceptors.request.use((config) => {
  const tokens = useAuthStore.getState().tokens
  if (tokens?.accessToken) {
    config.headers.Authorization = `Bearer ${tokens.accessToken}`
  }
  return config
})

// 2. Handle expired tokens
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status !== 401 || originalRequest._retry) {
      return Promise.reject(error)
    }

    originalRequest._retry = true

    const { tokens, setTokens, logout } = useAuthStore.getState()

    if (!tokens?.refreshToken) {
      logout()
      return Promise.reject(error)
    }

    try {
      const { data } = await axios.post(`${BASE_URL}/auth/refresh`, {
        refreshToken: tokens.refreshToken,
      })

      setTokens({
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
      })

      originalRequest.headers.Authorization = `Bearer ${data.accessToken}`
      return apiClient(originalRequest)
    } catch {
      logout()
      return Promise.reject(error)
    }
  }
)