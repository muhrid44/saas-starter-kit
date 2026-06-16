import { useQuery } from '@tanstack/react-query'
import { usersApi } from '@/api/users'

export function UsersPage() {
  const { data: users, isLoading, isError } = useQuery({
    queryKey: ['users'],
    queryFn: () => usersApi.getAll(),
  })

  if (isLoading) return <p className="text-sm text-gray-500">Loading users...</p>
  if (isError) return <p className="text-sm text-red-500">Failed to load users.</p>

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl font-semibold">Users</h1>
        <p className="text-gray-500 text-sm mt-1">Manage users in your tenant.</p>
      </div>

      <div className="bg-white border rounded-lg overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b bg-gray-50">
              <th className="text-left px-4 py-3 font-medium text-gray-500">Email</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Full Name</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Status</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Roles</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Created</th>
            </tr>
          </thead>
          <tbody>
            {users?.map((user) => (
              <tr key={user.id} className="border-b last:border-0 hover:bg-gray-50">
                <td className="px-4 py-3">{user.email}</td>
                <td className="px-4 py-3">{user.fullName || '—'}</td>
                <td className="px-4 py-3">
                  <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${user.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'}`}>
                    {user.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td className="px-4 py-3">{user.roles?.join(', ') || '—'}</td>
                <td className="px-4 py-3 text-gray-400">{new Date(user.createAt).toLocaleDateString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}