import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { auditLogsApi } from '@/api/auditLogs'
import { formatDate } from '@/lib/utils'

const EVENT_STYLES: Record<string, string> = {
  'User Registration': 'bg-green-100 text-green-700',
  'Update Role': 'bg-blue-100 text-blue-700',
  'Change Password': 'bg-yellow-100 text-yellow-700',
  'Password Reset': 'bg-yellow-100 text-yellow-700',
  'Update Status': 'bg-green-100 text-green-700',
  'Update Profile': 'bg-green-100 text-green-700',
  'User Deleted': 'bg-red-100 text-red-700',
}

const PAGE_SIZE = 20

export function AuditPage() {
  const [page, setPage] = useState(1)

  const { data, isLoading, isError } = useQuery({
    queryKey: ['auditlogs', page],
    queryFn: () => auditLogsApi.getAll(page, PAGE_SIZE),
  })

  if (isLoading)
    return <p className="text-sm text-gray-500">Loading audit logs...</p>

  if (isError)
    return <p className="text-sm text-red-500">Failed to load audit logs.</p>

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl font-semibold">Audit Logs</h1>
        <p className="text-gray-500 text-sm mt-1">
          Track important activities across your tenant.
        </p>
      </div>

      <div className="bg-white border rounded-lg overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b bg-gray-50">
              <th className="text-left px-4 py-3 font-medium text-gray-500">Event</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Description</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Performed By</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Date</th>
            </tr>
          </thead>

          <tbody>
            {data?.items.map((log) => (
              <tr key={log.id} className="border-b last:border-0 hover:bg-gray-50">
                <td className="px-4 py-3">
                  <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${EVENT_STYLES[log.eventName] ?? 'bg-gray-100 text-gray-700'}`}>
                    {log.eventName}
                  </span>
                </td>
                <td className="px-4 py-3 text-gray-700">{log.description}</td>
                <td className="px-4 py-3 text-gray-500">{log.changedBy ?? '—'}</td>
                <td className="px-4 py-3 text-gray-400 whitespace-nowrap">
                  {formatDate(log.changedDate ?? '')}
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {/* Pagination */}
        <div className="flex items-center justify-between px-4 py-3 border-t">
          <p className="text-sm text-gray-500">
            Showing {((page - 1) * PAGE_SIZE) + 1}–{Math.min(page * PAGE_SIZE, data?.totalCount ?? 0)} of {data?.totalCount ?? 0} results
          </p>
          <div className="flex gap-2">
            <button
              onClick={() => setPage((p) => p - 1)}
              disabled={page === 1}
              className="px-3 py-1 border rounded text-sm hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Previous
            </button>
            <span className="px-3 py-1 text-sm text-gray-500">
              Page {page} of {data?.totalPages ?? 1}
            </span>
            <button
              onClick={() => setPage((p) => p + 1)}
              disabled={page === data?.totalPages}
              className="px-3 py-1 border rounded text-sm hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Next
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}