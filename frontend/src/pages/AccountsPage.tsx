import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Plus } from 'lucide-react'
import { getAccounts } from '../api/accounts'
import AccountsTable from '../components/accounts/AccountsTable'
import OpenAccountModal from '../components/accounts/OpenAccountModal'
import Pagination from '../components/ui/Pagination'

export default function AccountsPage() {
  const [page, setPage] = useState(1)
  const [showModal, setShowModal] = useState(false)

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
        <button
          onClick={() => setShowModal(true)}
          className="flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 transition-colors"
        >
          <Plus size={16} />
          Open Account
        </button>
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

      {showModal && <OpenAccountModal onClose={() => setShowModal(false)} />}
    </div>
  )
}
