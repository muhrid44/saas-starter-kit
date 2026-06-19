import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuthStore } from '@/stores/authStore'
import { userApi } from '@/api/user'
import axios from 'axios'

type ConfirmModal = 'profile' | 'password' | null

export function ProfilePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { user, logout } = useAuthStore()

  // profile form
  const [fullName, setFullName] = useState(user?.fullName ?? '')
  const [profileError, setProfileError] = useState('')
  const [profileSuccess, setProfileSuccess] = useState('')

  // password form
  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [passwordError, setPasswordError] = useState('')

  // confirmation modal
  const [confirmModal, setConfirmModal] = useState<ConfirmModal>(null)

  const updateProfile = useMutation({
    mutationFn: () => userApi.updateProfile({ fullName, email: user?.email ?? '' }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['me'] })
      setProfileSuccess('Profile updated successfully.')
      setProfileError('')
      setConfirmModal(null)
    },
    onError: (err) => {
      if (axios.isAxiosError(err))
        setProfileError(err.response?.data?.detail ?? 'Something went wrong.')
      setProfileSuccess('')
      setConfirmModal(null)
    }
  })

  const changePassword = useMutation({
    mutationFn: () => userApi.changePassword({ currentPassword, newPassword }),
    onSuccess: () => {
      queryClient.clear()
      logout()
      navigate('/login')
    },
    onError: (err) => {
      if (axios.isAxiosError(err))
        setPasswordError(err.response?.data?.detail ?? 'Something went wrong.')
      setConfirmModal(null)
    }
  })

  return (
    <div className="max-w-lg">
      <div className="mb-8">
        <h1 className="text-2xl font-semibold">Profile</h1>
        <p className="text-gray-500 text-sm mt-1">Manage your account settings.</p>
      </div>

      {/* Profile Section */}
      <div className="bg-white border rounded-lg p-6 mb-4">
        <h2 className="text-base font-semibold mb-4">Personal Information</h2>
        <div className="space-y-4">
          <div>
            <label className="text-sm font-medium block mb-1.5">Full Name</label>
            <input
              type="text"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              className="w-full border rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="text-sm font-medium block mb-1.5">Email</label>
            <input
              type="email"
              value={user?.email ?? ''}
              disabled
              className="w-full border rounded-md px-3 py-2 text-sm bg-gray-50 text-gray-400 cursor-not-allowed"
            />
            <p className="text-xs text-gray-400 mt-1">Email cannot be changed.</p>
          </div>

          {profileError && <p className="text-sm text-red-500">{profileError}</p>}
          {profileSuccess && <p className="text-sm text-green-600">{profileSuccess}</p>}

          <button
            onClick={() => setConfirmModal('profile')}
            disabled={updateProfile.isPending}
            className="bg-blue-600 text-white px-4 py-2 rounded-md text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
          >
            Save Changes
          </button>
        </div>
      </div>

      {/* Password Section */}
      <div className="bg-white border rounded-lg p-6">
        <h2 className="text-base font-semibold mb-4">Change Password</h2>
        <div className="space-y-4">
          <div>
            <label className="text-sm font-medium block mb-1.5">Current Password</label>
            <input
              type="password"
              placeholder="••••••••"
              value={currentPassword}
              onChange={(e) => setCurrentPassword(e.target.value)}
              className="w-full border rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="text-sm font-medium block mb-1.5">New Password</label>
            <input
              type="password"
              placeholder="••••••••"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              className="w-full border rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {passwordError && <p className="text-sm text-red-500">{passwordError}</p>}

          <button
            onClick={() => setConfirmModal('password')}
            disabled={changePassword.isPending}
            className="bg-blue-600 text-white px-4 py-2 rounded-md text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
          >
            Update Password
          </button>
          <p className="text-xs text-gray-400">You will be logged out after changing your password.</p>
        </div>
      </div>

      {/* Confirm Update Profile Modal */}
      {confirmModal === 'profile' && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-sm shadow-xl">
            <h2 className="text-lg font-semibold mb-1">Confirm Changes</h2>
            <p className="text-sm text-gray-500 mb-6">
              Are you sure you want to update your profile?
            </p>
            <div className="flex gap-2">
              <button
                onClick={() => updateProfile.mutate()}
                disabled={updateProfile.isPending}
                className="flex-1 bg-blue-600 text-white rounded-md py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
              >
                {updateProfile.isPending ? 'Saving...' : 'Confirm'}
              </button>
              <button
                onClick={() => setConfirmModal(null)}
                className="flex-1 border rounded-md py-2 text-sm font-medium hover:bg-gray-50"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Confirm Change Password Modal */}
      {confirmModal === 'password' && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-sm shadow-xl">
            <h2 className="text-lg font-semibold mb-1">Confirm Password Change</h2>
            <p className="text-sm text-gray-500 mb-6">
              You will be logged out after changing your password. Are you sure?
            </p>
            <div className="flex gap-2">
              <button
                onClick={() => changePassword.mutate()}
                disabled={changePassword.isPending}
                className="flex-1 bg-red-500 text-white rounded-md py-2 text-sm font-medium hover:bg-red-600 disabled:opacity-50"
              >
                {changePassword.isPending ? 'Updating...' : 'Yes, Change Password'}
              </button>
              <button
                onClick={() => setConfirmModal(null)}
                className="flex-1 border rounded-md py-2 text-sm font-medium hover:bg-gray-50"
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