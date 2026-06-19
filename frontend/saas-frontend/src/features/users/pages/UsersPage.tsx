import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { adminApi } from '@/api/admin'
import { userApi } from '@/api/user'
import { authApi } from '@/api/auth'
import { useAuthStore } from '@/stores/authStore'
import axios from 'axios'
import type { User } from '@/types'

type ModalType =
  | 'invite'
  | 'role'
  | 'status'
  | 'resetPassword'
  | 'delete'
  | null

export function UsersPage() {
  const queryClient = useQueryClient()
  const currentUser = useAuthStore((s) => s.user)
  const isAdmin = currentUser?.roles?.includes('Admin')

  const [modal, setModal] = useState<ModalType>(null)
  const [selectedUser, setSelectedUser] = useState<User | null>(null)

  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [role, setRole] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [error, setError] = useState('')

  const { data: users, isLoading, isError } = useQuery({
    queryKey: ['users'],
    queryFn: () => userApi.getUsers(),
  })

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['users'] })
    queryClient.invalidateQueries({ queryKey: ['auditlogs'] })
    queryClient.invalidateQueries({ queryKey: ['dashboardInfo'] })
  }

  const closeModal = () => {
    setModal(null)
    setSelectedUser(null)
    setError('')
    setRole('')
    setNewPassword('')
  }

  const invite = useMutation({
    mutationFn: () => authApi.register({ fullName, email, password }),
    onSuccess: () => {
      invalidate()
      closeModal()
      setFullName('')
      setEmail('')
      setPassword('')
    },
    onError: (err) => {
      if (axios.isAxiosError(err))
        setError(err.response?.data?.detail ?? 'Something went wrong.')
    }
  })

  const updateRole = useMutation({
    mutationFn: () => adminApi.updateRole(selectedUser!.id, role),
    onSuccess: () => { invalidate(); closeModal() },
    onError: (err) => {
      if (axios.isAxiosError(err))
        setError(err.response?.data?.detail ?? 'Something went wrong.')
    }
  })

  const updateStatus = useMutation({
    mutationFn: () => adminApi.updateStatus(selectedUser!.id, !selectedUser!.isActive),
    onSuccess: () => { invalidate(); closeModal() },
    onError: (err) => {
      if (axios.isAxiosError(err))
        setError(err.response?.data?.detail ?? 'Something went wrong.')
    }
  })

  const resetPassword = useMutation({
    mutationFn: () => adminApi.resetPassword(selectedUser!.id, newPassword),
    onSuccess: () => { invalidate(); closeModal() },
    onError: (err) => {
      if (axios.isAxiosError(err))
        setError(err.response?.data?.detail ?? 'Something went wrong.')
    }
  })

  const deleteUser = useMutation({
    mutationFn: () => adminApi.deleteUser(selectedUser!.id),
    onSuccess: () => { invalidate(); closeModal() },
    onError: (err) => {
      if (axios.isAxiosError(err))
        setError(err.response?.data?.detail ?? 'Something went wrong.')
    }
  })

  const formatDateTime = (date: string) => {
    const d = new Date(date);

    const pad = (n: number) => n.toString().padStart(2, '0');

    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
  };

  if (isLoading) return <p className="text-sm text-gray-500">Loading users...</p>
  if (isError) return <p className="text-sm text-red-500">Failed to load users.</p>

  return (
    <div className="mx-auto max-w-7xl px-4 py-4 sm:px-6 lg:px-8">
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl sm:text-3xl font-bold">Users</h1>
          <p className="text-gray-500 text-sm mt-1">Manage users in your tenant.</p>
        </div>
        {isAdmin && (
          <button
            onClick={() => setModal('invite')}
            className="w-full sm:w-auto rounded-md bg-blue-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-blue-700"
          >
            + Register User
          </button>
        )}
      </div>

      {/* Table */}
      <div className="overflow-x-auto rounded-xl border bg-white shadow-sm">
       <table className="min-w-[1100px] w-full text-sm">
          <thead>
            <tr className="border-b bg-gray-50">
              <th className="text-left px-4 py-3 font-medium text-gray-500">Email</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Full Name</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Status</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Roles</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Created</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Modified</th>
              {isAdmin && (
                <th className="text-left px-4 py-3 font-medium text-gray-500">Actions</th>
              )}
            </tr>
          </thead>
          <tbody>
            {users?.map((user) => (
              <tr key={user.id} className="border-b last:border-0 transition-colors hover:bg-gray-50">
                <td className="px-4 py-3 break-all">{user.email}</td>
                <td className="px-4 py-3">{user.fullName || '—'}</td>
                <td className="px-4 py-3">
                  <span className={`inline-flex whitespace-nowrap items-center px-2 py-1 rounded-full text-xs font-medium ${user.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'}`}>
                    {user.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td className="px-4 py-3 whitespace-nowrap">{user.roles?.join(', ') || '—'}</td>
                <td className="whitespace-nowrap px-4 py-3 text-gray-400">{formatDateTime(user.createdDate)}</td>
                <td className="whitespace-nowrap px-4 py-3 text-gray-400">{formatDateTime(user.modifiedDate)}</td>
                {isAdmin && (
                  <td className="px-4 py-3">
                    {currentUser?.id !== user.id && (
                      <div className="flex min-w-[320px] flex-wrap gap-2">
                        <button
                          onClick={() => {
                            setSelectedUser(user)
                            setRole(user.roles?.[0] ?? 'User')
                            setModal('role')
                          }}
                          className="rounded-md border border-blue-200 bg-blue-50 px-3 py-1 text-xs font-medium text-blue-700 transition hover:bg-blue-100"
                        >
                          Change Role
                        </button>

                        <button
                          onClick={() => {
                            setSelectedUser(user)
                            setModal('status')
                          }}
                          className={`rounded-md border px-3 py-1 text-xs font-medium transition ${
                            user.isActive
                              ? 'border-red-200 bg-red-50 text-red-700 hover:bg-red-100'
                              : 'border-green-200 bg-green-50 text-green-700 hover:bg-green-100'
                          }`}
                        >
                          {user.isActive ? 'Deactivate' : 'Activate'}
                        </button>

                        <button
                          onClick={() => {
                            setSelectedUser(user)
                            setModal('resetPassword')
                          }}
                          className="rounded-md border border-gray-200 bg-gray-50 px-3 py-1 text-xs font-medium text-gray-700 transition hover:bg-gray-100"
                        >
                          Reset Password
                        </button>

                        <button
                          onClick={() => {
                            setSelectedUser(user)
                            setModal('delete')
                          }}
                          className="rounded-md border border-red-200 bg-red-50 px-3 py-1 text-xs font-medium text-red-700 transition hover:bg-red-100"
                        >
                          Delete
                        </button>
                      </div>
                    )}
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Register Modal */}
      {modal === 'invite' && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="bg-white rounded-lg p-6 w-full max-w-sm shadow-xl">
            <h2 className="text-lg font-semibold mb-1">Register User</h2>
            <p className="text-sm text-gray-500 mb-4">Add a new user to your workspace.</p>
            <div className="space-y-4">
              <div>
                <label className="text-sm font-medium block mb-1.5">Full Name</label>
                <input type="text" placeholder="John Doe" value={fullName}
                  onChange={(e) => setFullName(e.target.value)}
                  className="w-full rounded-md border px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div>
                <label className="text-sm font-medium block mb-1.5">Email</label>
                <input type="email" placeholder="john@company.com" value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="w-full rounded-md border px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div>
                <label className="text-sm font-medium block mb-1.5">Password</label>
                <input type="password" placeholder="••••••••" value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="w-full rounded-md border px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              {error && <p className="text-sm text-red-500">{error}</p>}
              <div className="flex flex-col gap-2 pt-2 sm:flex-row">
                <button onClick={() => invite.mutate()} disabled={invite.isPending}
                  className="flex-1 bg-blue-600 text-white rounded-md py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
                  {invite.isPending ? 'Registering...' : 'Register'}
                </button>
                <button onClick={closeModal}
                  className="w-full sm:flex-1 border rounded-md py-2 text-sm font-medium hover:bg-gray-50">
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Change Role Modal */}
      {modal === 'role' && selectedUser && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="bg-white rounded-lg p-6 w-full max-w-sm shadow-xl">
            <h2 className="text-lg font-semibold mb-1">Change Role</h2>
            <p className="text-sm text-gray-500 mb-4">{selectedUser.email}</p>
            <div className="space-y-4">
              <div>
                <label className="text-sm font-medium block mb-1.5">Role</label>
                <select value={role} onChange={(e) => setRole(e.target.value)}
                  className="w-full rounded-md border px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option value="User">User</option>
                  <option value="Admin">Admin</option>
                </select>
              </div>
              {error && <p className="text-sm text-red-500">{error}</p>}
              <div className="flex flex-col gap-2 pt-2 sm:flex-row">
                <button onClick={() => updateRole.mutate()} disabled={updateRole.isPending}
                  className="flex-1 bg-blue-600 text-white rounded-md py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
                  {updateRole.isPending ? 'Saving...' : 'Save'}
                </button>
                <button onClick={closeModal}
                  className="w-full sm:flex-1 border rounded-md py-2 text-sm font-medium hover:bg-gray-50">
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Activate/Deactivate Modal */}
      {modal === 'status' && selectedUser && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="bg-white rounded-lg p-6 w-full max-w-sm shadow-xl">
            <h2 className="text-lg font-semibold mb-1">
              {selectedUser.isActive ? 'Deactivate User' : 'Activate User'}
            </h2>
            <p className="text-sm text-gray-500 mb-4">
              {selectedUser.isActive
                ? `Are you sure you want to deactivate ${selectedUser.email}?`
                : `Are you sure you want to activate ${selectedUser.email}?`}
            </p>
            {error && <p className="text-sm text-red-500">{error}</p>}
            <div className="flex flex-col gap-2 pt-2 sm:flex-row">
              <button onClick={() => updateStatus.mutate()} disabled={updateStatus.isPending}
                className={`flex-1 text-white rounded-md py-2 text-sm font-medium disabled:opacity-50 ${selectedUser.isActive ? 'bg-red-500 hover:bg-red-600' : 'bg-green-600 hover:bg-green-700'}`}>
                {updateStatus.isPending ? 'Saving...' : selectedUser.isActive ? 'Deactivate' : 'Activate'}
              </button>
              <button onClick={closeModal}
                className="w-full sm:flex-1 border rounded-md py-2 text-sm font-medium hover:bg-gray-50">
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Reset Password Modal */}
      {modal === 'resetPassword' && selectedUser && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="bg-white rounded-lg p-6 w-full max-w-sm shadow-xl">
            <h2 className="text-lg font-semibold mb-1">Reset Password</h2>
            <p className="text-sm text-gray-500 mb-4">{selectedUser.email}</p>
            <div className="space-y-4">
              <div>
                <label className="text-sm font-medium block mb-1.5">New Password</label>
                <input type="password" placeholder="••••••••" value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  className="w-full rounded-md border px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              {error && <p className="text-sm text-red-500">{error}</p>}
              <div className="flex flex-col gap-2 pt-2 sm:flex-row">
                <button onClick={() => resetPassword.mutate()} disabled={resetPassword.isPending}
                  className="flex-1 bg-blue-600 text-white rounded-md py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
                  {resetPassword.isPending ? 'Saving...' : 'Reset'}
                </button>
                <button onClick={closeModal}
                  className="w-full sm:flex-1 border rounded-md py-2 text-sm font-medium hover:bg-gray-50">
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {modal === 'delete' && selectedUser && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="bg-white rounded-lg p-6 w-full max-w-sm shadow-xl">
            <h2 className="text-lg font-semibold mb-1 text-red-600">
              Delete User
            </h2>

            <p className="text-sm text-gray-500 mb-4">
              Are you sure you want to permanently delete
              <span className="font-medium"> {selectedUser.email}</span>?
              <br />
              <br />
              This action cannot be undone.
            </p>

            {error && (
              <p className="text-sm text-red-500 mb-2">
                {error}
              </p>
            )}

            <div className="flex flex-col gap-2 pt-2 sm:flex-row">
              <button
                onClick={() => deleteUser.mutate()}
                disabled={deleteUser.isPending}
                className="flex-1 bg-red-600 text-white rounded-md py-2 text-sm font-medium hover:bg-red-700 disabled:opacity-50"
              >
                {deleteUser.isPending ? 'Deleting...' : 'Delete'}
              </button>

              <button
                onClick={closeModal}
                className="w-full sm:flex-1 border rounded-md py-2 text-sm font-medium hover:bg-gray-50"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}