import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getAccounts } from '../api/accounts'
import AccountsTable from '../components/accounts/AccountsTable'
import Pagination from '../components/ui/Pagination'

export default function AccountsPage() {
  const [page, setPage] = useState(1)

  const { data, isLoading, error } = useQuery({
    queryKey: ['accounts', page],
    queryFn: () => getAccounts(page, 20),
  })

  const list = data?.data

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Accounts</h1>
          <p className="text-slate-500 text-sm mt-1">Manage and monitor regulated accounts</p>
        </div>
        <p className="user-context"><strong>Read access</strong>Account opening requires an operational role, which is not implemented.</p>
      </div>

      <div className="bg-white rounded-xl border border-slate-200">
        {error ? (
          <div className="px-5 py-12 text-center text-red-600 text-sm">
            Failed to load accounts. Is the API running?
          </div>
        ) : (
          <>
            <AccountsTable data={list?.items ?? []} isLoading={isLoading} />
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
