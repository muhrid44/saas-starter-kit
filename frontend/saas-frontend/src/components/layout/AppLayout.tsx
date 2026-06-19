import { Outlet, NavLink, useNavigate } from 'react-router-dom'
import { LayoutDashboard, Users, ScrollText, LogOut, Building2, Settings } from 'lucide-react'
import { useAuthStore } from '@/stores/authStore'
import { cn } from '@/lib/utils'
import { useMe } from '@/features/auth/hooks/getUserProfile'

const NAV_ITEMS = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/users', label: 'Users', icon: Users },
  { to: '/audit', label: 'Audit Logs', icon: ScrollText },
]

export function AppLayout() {
  useMe()
  const { user, logout } = useAuthStore()
  const navigate = useNavigate()
  const isAdmin = user?.roles?.includes('Admin')

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Sidebar */}
      <aside className="w-60 border-r flex flex-col bg-white">
        {/* Brand */}
        <div className="h-16 flex items-center gap-2 px-5 border-b">
          <Building2 className="w-5 h-5 text-blue-600" />
          <span className="font-semibold text-sm">SaaS Starter Kit</span>
        </div>

        {/* Nav */}
        <nav className="flex-1 px-3 py-4 space-y-1">
          {NAV_ITEMS
          .filter((item) => isAdmin || item.to !== '/audit')
          .map(({ to, label, icon: Icon }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium transition-colors',
                  isActive
                    ? 'bg-blue-600 text-white'
                    : 'text-gray-600 hover:bg-gray-100'
                )
              }
            >
              <Icon className="w-4 h-4" />
              {label}
            </NavLink>
          ))}
        </nav>

        <NavLink
          to="/profile"
          className={({ isActive }) =>
            cn(
              'flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium transition-colors',
              isActive
                ? 'bg-blue-600 text-white'
                : 'text-gray-600 hover:bg-gray-100'
            )
          }
        >
          <Settings className="w-4 h-4" />
          Profile
        </NavLink>

        {/* User + logout */}
        <div className="p-3 border-t">
          <div className="px-3 py-2 mb-1">
            <p className="text-xs font-medium truncate">{user?.email ?? 'User'}</p>
          </div>
          <button
            onClick={handleLogout}
            className="w-full flex items-center gap-3 px-3 py-2 rounded-md text-sm text-gray-600 hover:bg-red-50 hover:text-red-600 transition-colors"
          >
            <LogOut className="w-4 h-4" />
            Sign out
          </button>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 overflow-y-auto">
        <div className="p-8">
          <Outlet />
        </div>
      </main>
    </div>
  )
}