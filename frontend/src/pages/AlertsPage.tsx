import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getAlerts } from '../api/alerts'
import AlertsTable from '../components/alerts/AlertsTable'
import Pagination from '../components/ui/Pagination'

export default function AlertsPage() {
  const [page, setPage] = useState(1)

  const { data, isLoading, error } = useQuery({
    queryKey: ['alerts', page],
    queryFn: () => getAlerts(page, 20),
  })

  const list = data?.data

  return (
    <div className="space-y-6">
      <div>
        <h1 className="page-title">Risk alerts</h1>
        <p className="text-slate-500 text-sm mt-1">
          Recorded risk exceptions and threshold-based alert records.
        </p>
      </div>

      <div className="bg-white rounded-xl border border-slate-200">
        {error ? (
          <div className="px-5 py-12 text-center text-red-600 text-sm">
            Failed to load alerts.
          </div>
        ) : (
          <>
            <AlertsTable data={list?.items ?? []} isLoading={isLoading} />
            <Pagination
              page={page}
              totalPages={list?.totalPages ?? 1}
              totalCount={list?.totalCount ?? 0}
              onPageChange={setPage}
            />
          </>
        )}
      </div>
    </div>
  )
}
