import { useNavigate } from 'react-router-dom'
import {
  createColumnHelper,
  flexRender,
  getCoreRowModel,
  useReactTable,
} from '@tanstack/react-table'
import type { AccountDto } from '../../types'
import Badge from '../ui/Badge'

const col = createColumnHelper<AccountDto>()

const columns = [
  col.accessor('accountNumber', { header: 'Account Number' }),
  col.accessor('holderName', { header: 'Holder Name' }),
  col.accessor('status', {
    header: 'Status',
    cell: (i) => <Badge value={i.getValue()} />,
  }),
  col.accessor('balance', {
    header: 'Balance',
    cell: (i) => (
      <span className="font-mono">
        {i.row.original.currency} {i.getValue().toLocaleString('en-GB', { minimumFractionDigits: 2 })}
      </span>
    ),
  }),
  col.accessor('riskLevel', {
    header: 'Risk',
    cell: (i) => <Badge value={i.getValue()} />,
  }),
]

interface Props {
  data: AccountDto[]
  isLoading: boolean
}

export default function AccountsTable({ data, isLoading }: Props) {
  const navigate = useNavigate()

  const table = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
  })

  if (isLoading)
    return <div className="py-12 text-center text-slate-400 text-sm">Loading accounts…</div>

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm text-left">
        <thead>
          {table.getHeaderGroups().map((hg) => (
            <tr key={hg.id} className="border-b border-slate-200">
              {hg.headers.map((h) => (
                <th
                  key={h.id}
                  className="px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wide"
                >
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
                No accounts found
              </td>
            </tr>
          ) : (
            table.getRowModel().rows.map((row) => (
              <tr
                key={row.id}
                onClick={() => navigate(`/accounts/${row.original.id}`)}
                className="border-b border-slate-100 hover:bg-slate-50 cursor-pointer transition-colors"
              >
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
  )
}
