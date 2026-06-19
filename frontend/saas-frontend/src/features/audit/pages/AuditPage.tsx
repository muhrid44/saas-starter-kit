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
  <div className="mx-auto max-w-7xl px-4 py-4 sm:px-6 lg:px-8">
    <div className="mb-6">
      <h1 className="text-2xl sm:text-3xl font-bold">
        Audit Logs
      </h1>

      <p className="mt-2 text-sm text-gray-500">
        Track important activities across your tenant.
      </p>
    </div>

    <div className="overflow-x-auto rounded-xl border bg-white shadow-sm">
      <table className="min-w-[900px] w-full text-sm">
        <thead>
          <tr className="border-b bg-gray-50">
            <th className="px-4 py-3 text-left font-medium text-gray-500">
              Event
            </th>

            <th className="px-4 py-3 text-left font-medium text-gray-500">
              Description
            </th>

            <th className="px-4 py-3 text-left font-medium text-gray-500">
              Performed By
            </th>

            <th className="px-4 py-3 text-left font-medium text-gray-500">
              Date
            </th>
          </tr>
        </thead>

        <tbody>
          {data?.items.map((log) => (
            <tr
              key={log.id}
              className="border-b last:border-0 transition-colors hover:bg-gray-50"
            >
              <td className="px-4 py-3 whitespace-nowrap">
                <span
                  className={`inline-flex whitespace-nowrap items-center rounded-full px-2 py-1 text-xs font-medium ${
                    EVENT_STYLES[log.eventName] ??
                    'bg-gray-100 text-gray-700'
                  }`}
                >
                  {log.eventName}
                </span>
              </td>

              <td className="px-4 py-3 text-gray-700 break-words">
                {log.description}
              </td>

              <td className="px-4 py-3 text-gray-500 break-all">
                {log.changedBy ?? '—'}
              </td>

              <td className="whitespace-nowrap px-4 py-3 text-gray-400">
                {formatDate(log.changedDate ?? '')}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="flex flex-col gap-4 border-t px-4 py-4 sm:flex-row sm:items-center sm:justify-between">
        <p className="text-center text-sm text-gray-500 sm:text-left">
          Showing {((page - 1) * PAGE_SIZE) + 1}–
          {Math.min(page * PAGE_SIZE, data?.totalCount ?? 0)} of{' '}
          {data?.totalCount ?? 0} results
        </p>

        <div className="flex items-center justify-center gap-2">
          <button
            onClick={() => setPage((p) => p - 1)}
            disabled={page === 1}
            className="rounded border px-3 py-2 text-sm hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
          >
            Previous
          </button>

          <span className="whitespace-nowrap px-2 text-sm text-gray-500">
            Page {page} of {data?.totalPages ?? 1}
          </span>

          <button
            onClick={() => setPage((p) => p + 1)}
            disabled={page === data?.totalPages}
            className="rounded border px-3 py-2 text-sm hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>
    </div>
  </div>
)}