import { Users, ScrollText, ShieldCheck, Activity } from 'lucide-react'
import { useAuthStore } from '@/stores/authStore'
import { useQuery } from '@tanstack/react-query'
import { userApi } from '@/api/user'
import { healthApi } from '@/api/health'

export function DashboardPage() {
  const user = useAuthStore((s) => s.user)
  const isAdmin = user?.roles?.includes('Admin')

  const { data: dashboardInfo, isLoading } = useQuery({
    queryKey: ['dashboardInfo'],
    queryFn: () => userApi.getDashboardInfo(),
  })

  const { data: healthInfo } = useQuery({
  queryKey: ['health'],
  queryFn: () => healthApi.getHealth(),
  refetchInterval: 30000,
  refetchOnWindowFocus: false,
    retry: 1,
})

  const METRIC_CARDS = [
    { label: 'Total Users', value: isLoading ? '...' : dashboardInfo?.totalUser ?? 0, icon: Users, description: 'Total users in your tenant' },
    { label: 'Audit Events', value: isLoading ? '...' : dashboardInfo?.auditLogsEvent ?? 0, icon: ScrollText, description: 'Last 30 days' },
    { label: 'Active Users', value: isLoading ? '...' : dashboardInfo?.activeUser ?? 0, icon: ShieldCheck, description: 'Active users in your tenant' },
    { label: 'API Health', value: healthInfo?.status ?? 'Unknown', icon: Activity, description: 'Backend responding' },
  ]

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-2xl font-semibold">Welcome back, {user?.email ?? 'there'}</h1>
        <p className="text-gray-500 mt-1 text-sm">Here's what's happening in your workspace.</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {METRIC_CARDS.filter((card) => isAdmin || card.label !== 'Audit Events').
        map(({ label, value, icon: Icon, description }) => (
          <div key={label} className="bg-white border rounded-lg p-5">
            <div className="flex items-center justify-between mb-3">
              <span className="text-sm font-medium text-gray-500">{label}</span>
              <Icon className="w-4 h-4 text-gray-400" />
            </div>
              <div
                className={`text-2xl font-semibold ${
                  label === 'API Health'
                    ? value === 'Healthy'
                      ? 'text-green-600'
                      : value === 'Unhealthy'
                      ? 'text-red-600'
                      : 'text-gray-500'
                    : ''
                }`}
              >
                {value}
              </div>            
              <p className="text-xs text-gray-400 mt-1">{description}</p>
          </div>
        ))}
      </div>
    </div>
  )
}