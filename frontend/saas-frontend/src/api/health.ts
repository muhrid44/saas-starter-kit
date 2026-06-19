import { rootClient } from './client'

export const healthApi = {

  getHealth: async (): Promise<{ status: string }> => {
    const res = await rootClient.get('/health')
    return res.data
  }
}