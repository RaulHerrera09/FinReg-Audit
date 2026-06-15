import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, Plus } from 'lucide-react'
import { closeAccount, getAccount, getComplianceReport, suspendAccount } from '../api/accounts'
import { getTransactionsByAccount } from '../api/transactions'
import { getAlertsByAccount } from '../api/alerts'
import Badge from '../components/ui/Badge'
import TransactionsTable from '../components/transactions/TransactionsTable'
import InitiateTransactionModal from '../components/transactions/InitiateTransactionModal'
import AlertsTable from '../components/alerts/AlertsTable'
import Pagination from '../components/ui/Pagination'

type Tab = 'transactions' | 'alerts' | 'compliance'

export default function AccountDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [tab, setTab] = useState<Tab>('transactions')
  const [txPage, setTxPage] = useState(1)
  const [showTxModal, setShowTxModal] = useState(false)

  const { data: accountData, isLoading } = useQuery({
    queryKey: ['account', id],
    queryFn: () => getAccount(id!),
    enabled: !!id,
  })

  const { data: txData, isLoading: txLoading } = useQuery({
    queryKey: ['transactions', id, txPage],
    queryFn: () => getTransactionsByAccount(id!, txPage),
    enabled: !!id && tab === 'transactions',
  })

  const { data: alertData, isLoading: alertLoading } = useQuery({
    queryKey: ['account-alerts', id],
    queryFn: () => getAlertsByAccount(id!),
    enabled: !!id && tab === 'alerts',
  })

  const { data: reportData } = useQuery({
    queryKey: ['compliance', id],
    queryFn: () => getComplianceReport(id!),
    enabled: !!id && tab === 'compliance',
  })

  const suspendMutation = useMutation({
    mutationFn: () => suspendAccount(id!, 'Suspended via compliance dashboard'),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['account', id] }),
  })

  const closeMutation = useMutation({
    mutationFn: () => closeAccount(id!, 'Closed via compliance dashboard'),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['account', id] }),
  })

  if (isLoading)
    return <div className="py-20 text-center text-slate-400">Loading account…</div>

  const account = accountData?.data
  if (!account)
    return <div className="py-20 text-center text-red-500">Account not found.</div>

  const report = reportData?.data

  const tabs: { key: Tab; label: string }[] = [
    { key: 'transactions', label: 'Transactions' },
    { key: 'alerts', label: 'Alerts' },
    { key: 'compliance', label: 'Compliance Report' },
  ]

  return (
    <div className="space-y-6">
      <button
        onClick={() => navigate('/accounts')}
        className="flex items-center gap-1.5 text-sm text-slate-500 hover:text-slate-900 transition-colors"
      >
        <ArrowLeft size={15} />
        Back to Accounts
      </button>

      {/* Account header */}
      <div className="bg-white rounded-xl border border-slate-200 p-6">
        <div className="flex items-start justify-between">
          <div className="space-y-1">
            <div className="flex items-center gap-3">
              <h1 className="text-xl font-bold text-slate-900 font-mono">{account.accountNumber}</h1>
              <Badge value={account.status} />
              <Badge value={account.riskLevel} />
            </div>
            <p className="text-slate-600">{account.holderName}</p>
            <p className="text-2xl font-bold text-slate-900 font-mono mt-2">
              {account.currency}{' '}
              {account.balance.toLocaleString('en-GB', { minimumFractionDigits: 2 })}
            </p>
          </div>

          {account.status === 'Active' && (
            <div className="flex gap-2">
              <button
                onClick={() => suspendMutation.mutate()}
                disabled={suspendMutation.isPending}
                className="px-3 py-1.5 text-xs font-medium text-amber-700 border border-amber-300 rounded-lg hover:bg-amber-50 disabled:opacity-50"
              >
                Suspend
              </button>
              <button
                onClick={() => closeMutation.mutate()}
                disabled={closeMutation.isPending}
                className="px-3 py-1.5 text-xs font-medium text-red-700 border border-red-300 rounded-lg hover:bg-red-50 disabled:opacity-50"
              >
                Close
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Tabs */}
      <div className="bg-white rounded-xl border border-slate-200 overflow-hidden">
        <div className="flex border-b border-slate-200 px-1">
          {tabs.map(({ key, label }) => (
            <button
              key={key}
              onClick={() => setTab(key)}
              className={`px-4 py-3 text-sm font-medium border-b-2 -mb-px transition-colors ${
                tab === key
                  ? 'border-indigo-600 text-indigo-600'
                  : 'border-transparent text-slate-500 hover:text-slate-900'
              }`}
            >
              {label}
            </button>
          ))}
        </div>

        {tab === 'transactions' && (
          <>
            <div className="flex justify-end px-4 py-3 border-b border-slate-100">
              <button
                onClick={() => setShowTxModal(true)}
                className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700"
              >
                <Plus size={13} />
                Initiate Transaction
              </button>
            </div>
            <TransactionsTable
              accountId={id!}
              data={txData?.data?.items ?? []}
              isLoading={txLoading}
            />
            <Pagination
              page={txPage}
              totalPages={txData?.data?.totalPages ?? 1}
              totalCount={txData?.data?.totalCount ?? 0}
              onPageChange={setTxPage}
            />
          </>
        )}

        {tab === 'alerts' && (
          <AlertsTable data={alertData?.data?.items ?? []} isLoading={alertLoading} />
        )}

        {tab === 'compliance' && (
          <div className="p-6">
            {!report ? (
              <div className="text-center text-slate-400 py-8 text-sm">Loading report…</div>
            ) : (
              <div className="space-y-6">
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  {[
                    { label: 'Total Transactions', value: report.totalTransactions },
                    { label: 'Flagged Transactions', value: report.flaggedTransactions },
                    {
                      label: 'Total Value (GBP)',
                      value: `£${report.totalValueGbp.toLocaleString('en-GB', { minimumFractionDigits: 2 })}`,
                    },
                    { label: 'Risk Level', value: report.riskLevel },
                  ].map(({ label, value }) => (
                    <div key={label} className="bg-slate-50 rounded-lg p-4">
                      <p className="text-xs font-medium text-slate-500 uppercase tracking-wide">{label}</p>
                      <p className="text-xl font-bold text-slate-900 mt-1">{value}</p>
                    </div>
                  ))}
                </div>
                <p className="text-xs text-slate-400">
                  Generated {new Date(report.generatedAt).toLocaleString('en-GB')}
                </p>
              </div>
            )}
          </div>
        )}
      </div>

      {showTxModal && (
        <InitiateTransactionModal accountId={id!} onClose={() => setShowTxModal(false)} />
      )}
    </div>
  )
}
