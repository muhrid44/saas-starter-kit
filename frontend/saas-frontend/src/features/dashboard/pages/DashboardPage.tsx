import { Users, ScrollText, ShieldCheck, Activity } from 'lucide-react'
import { useAuthStore } from '@/stores/authStore'
import { useQuery } from '@tanstack/react-query'
import { usersApi } from '@/api/users'

export function DashboardPage() {
  const user = useAuthStore((s) => s.user)

  const { data: users, isLoading } = useQuery({
    queryKey: ['users'],
    queryFn: () => usersApi.getAll(),
  })

  const METRIC_CARDS = [
    { label: 'Total Users', value: isLoading ? '...' : users?.length ?? 0, icon: Users, description: 'Active in your tenant' },
    { label: 'Audit Events', value: '—', icon: ScrollText, description: 'Last 30 days' },
    { label: 'Active Roles', value: '—', icon: ShieldCheck, description: 'Assigned across users' },
    { label: 'API Health', value: 'OK', icon: Activity, description: 'Backend responding' },
  ]

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-2xl font-semibold">Welcome back, {user?.email ?? 'there'}</h1>
        <p className="text-gray-500 mt-1 text-sm">Here's what's happening in your workspace.</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {METRIC_CARDS.map(({ label, value, icon: Icon, description }) => (
          <div key={label} className="bg-white border rounded-lg p-5">
            <div className="flex items-center justify-between mb-3">
              <span className="text-sm font-medium text-gray-500">{label}</span>
              <Icon className="w-4 h-4 text-gray-400" />
            </div>
            <div className="text-2xl font-semibold">{value}</div>
            <p className="text-xs text-gray-400 mt-1">{description}</p>
          </div>
        ))}
      </div>
    </div>
  )
}