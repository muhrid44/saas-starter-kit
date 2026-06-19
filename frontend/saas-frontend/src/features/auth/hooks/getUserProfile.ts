import { useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { userApi } from '@/api/user'
import { useAuthStore } from '@/stores/authStore'

export function useMe() {
  const setAuth = useAuthStore((s) => s.setAuth)
  const tokens = useAuthStore((s) => s.tokens)

  const { data: me, dataUpdatedAt } = useQuery({
    queryKey: ['me'],
    queryFn: () => userApi.getUser(),
    enabled: !!tokens?.accessToken,
  })

  useEffect(() => {
    if (me && tokens) {
      setAuth(me, tokens)
    }
  }, [dataUpdatedAt])

  return me
}