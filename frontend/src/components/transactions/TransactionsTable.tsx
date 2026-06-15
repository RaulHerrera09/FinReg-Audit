import { useState } from 'react'
import { CheckCircle2, Flag, X } from 'lucide-react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import {
  createColumnHelper,
  flexRender,
  getCoreRowModel,
  useReactTable,
} from '@tanstack/react-table'
import type { RiskLevel, TransactionDto } from '../../types'
import Badge from '../ui/Badge'
import { completeTransaction, flagTransaction } from '../../api/transactions'

const col = createColumnHelper<TransactionDto>()

interface Props {
  accountId: string
  data: TransactionDto[]
  isLoading: boolean
}

const RISK_LEVELS: RiskLevel[] = ['Low', 'Medium', 'High', 'Critical']
const SEVERITIES = ['Info', 'Warning', 'High', 'Critical']

function FlagModal({
  accountId,
  transactionId,
  onClose,
}: {
  accountId: string
  transactionId: string
  onClose: () => void
}) {
  const [riskLevel, setRiskLevel] = useState<RiskLevel>('High')
  const [severity, setSeverity] = useState('High')
  const [reason, setReason] = useState('')
  const [error, setError] = useState('')
  const queryClient = useQueryClient()

  const mutation = useMutation({
    mutationFn: () => flagTransaction(accountId, transactionId, riskLevel, severity, reason.trim()),
    onSuccess: (res) => {
      if (res.success) {
        queryClient.invalidateQueries({ queryKey: ['transactions', accountId] })
        queryClient.invalidateQueries({ queryKey: ['alerts'] })
        onClose()
      } else {
        setError(res.error ?? 'Failed to flag transaction')
      }
    },
  })

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-md p-6">
        <div className="flex items-center justify-between mb-5">
          <h2 className="text-base font-semibold text-slate-900">Flag Transaction</h2>
          <button onClick={onClose} className="text-slate-400 hover:text-slate-600"><X size={18} /></button>
        </div>
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Risk Level</label>
              <select
                value={riskLevel}
                onChange={(e) => setRiskLevel(e.target.value as RiskLevel)}
                className="w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
              >
                {RISK_LEVELS.map((r) => <option key={r}>{r}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Severity</label>
              <select
                value={severity}
                onChange={(e) => setSeverity(e.target.value)}
                className="w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
              >
                {SEVERITIES.map((s) => <option key={s}>{s}</option>)}
              </select>
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Reason</label>
            <input
              type="text"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              required
              className="w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
              placeholder="Unusual transaction pattern"
            />
          </div>
          {error && <p className="text-red-600 text-sm">{error}</p>}
          <div className="flex gap-3">
            <button onClick={onClose} className="flex-1 px-4 py-2 text-sm border border-slate-300 rounded-lg hover:bg-slate-50">
              Cancel
            </button>
            <button
              onClick={() => { if (reason.trim()) mutation.mutate() }}
              disabled={mutation.isPending || !reason.trim()}
              className="flex-1 px-4 py-2 text-sm text-white bg-amber-600 rounded-lg hover:bg-amber-700 disabled:opacity-50"
            >
              {mutation.isPending ? 'Flagging…' : 'Flag'}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}

export default function TransactionsTable({ accountId, data, isLoading }: Props) {
  const [flaggingId, setFlaggingId] = useState<string | null>(null)
  const queryClient = useQueryClient()

  const completeMutation = useMutation({
    mutationFn: (transactionId: string) => completeTransaction(accountId, transactionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions', accountId] })
      queryClient.invalidateQueries({ queryKey: ['account', accountId] })
      queryClient.invalidateQueries({ queryKey: ['alerts'] })
    },
  })

  const columns = [
    col.accessor('amount', {
      header: 'Amount',
      cell: (i) => (
        <span className="font-mono">
          {i.row.original.currency}{' '}
          {i.getValue().toLocaleString('en-GB', { minimumFractionDigits: 2 })}
        </span>
      ),
    }),
    col.accessor('type', { header: 'Type' }),
    col.accessor('status', { header: 'Status', cell: (i) => <Badge value={i.getValue()} /> }),
    col.accessor('riskLevel', { header: 'Risk', cell: (i) => <Badge value={i.getValue()} /> }),
    col.accessor('description', { header: 'Description', cell: (i) => (
      <span className="max-w-48 truncate block text-slate-500">{i.getValue()}</span>
    )}),
    col.accessor('initiatedAt', {
      header: 'Date',
      cell: (i) => new Date(i.getValue()).toLocaleDateString('en-GB'),
    }),
    col.display({
      id: 'actions',
      header: '',
      cell: (i) => {
        const tx = i.row.original
        return (
          <div className="flex gap-1 justify-end">
            {tx.status === 'Pending' && (
              <button
                onClick={(e) => { e.stopPropagation(); completeMutation.mutate(tx.id) }}
                disabled={completeMutation.isPending}
                title="Complete"
                className="p-1.5 text-green-600 hover:bg-green-50 rounded disabled:opacity-40"
              >
                <CheckCircle2 size={15} />
              </button>
            )}
            {(tx.status === 'Pending' || tx.status === 'Completed') && (
              <button
                onClick={(e) => { e.stopPropagation(); setFlaggingId(tx.id) }}
                title="Flag"
                className="p-1.5 text-amber-600 hover:bg-amber-50 rounded"
              >
                <Flag size={15} />
              </button>
            )}
          </div>
        )
      },
    }),
  ]

  const table = useReactTable({ data, columns, getCoreRowModel: getCoreRowModel() })

  if (isLoading)
    return <div className="py-12 text-center text-slate-400 text-sm">Loading transactions…</div>

  return (
    <>
      <div className="overflow-x-auto">
        <table className="w-full text-sm text-left">
          <thead>
            {table.getHeaderGroups().map((hg) => (
              <tr key={hg.id} className="border-b border-slate-200">
                {hg.headers.map((h) => (
                  <th key={h.id} className="px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wide">
                    {flexRender(h.column.columnDef.header, h.getContext())}
                  </th>
                ))}
              </tr>
            ))}
          </thead>
          <tbody>
            {table.getRowModel().rows.length === 0 ? (
              <tr>
                <td colSpan={columns.length} className="px-4 py-12 text-center text-slate-400">
                  No transactions yet
                </td>
              </tr>
            ) : (
              table.getRowModel().rows.map((row) => (
                <tr key={row.id} className="border-b border-slate-100 hover:bg-slate-50 transition-colors">
                  {row.getVisibleCells().map((cell) => (
                    <td key={cell.id} className="px-4 py-3 text-slate-700">
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {flaggingId && (
        <FlagModal
          accountId={accountId}
          transactionId={flaggingId}
          onClose={() => setFlaggingId(null)}
        />
      )}
    </>
  )
}
