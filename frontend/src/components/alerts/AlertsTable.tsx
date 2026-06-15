import {
  createColumnHelper,
  flexRender,
  getCoreRowModel,
  useReactTable,
} from '@tanstack/react-table'
import type { AlertDto } from '../../types'
import Badge from '../ui/Badge'

const col = createColumnHelper<AlertDto>()

const columns = [
  col.accessor('severity', { header: 'Severity', cell: (i) => <Badge value={i.getValue()} /> }),
  col.accessor('riskLevel', { header: 'Risk', cell: (i) => <Badge value={i.getValue()} /> }),
  col.accessor('reason', { header: 'Reason', cell: (i) => (
    <span className="text-slate-600 max-w-sm block truncate">{i.getValue()}</span>
  )}),
  col.accessor('accountId', { header: 'Account ID', cell: (i) => (
    <span className="font-mono text-xs text-slate-400">{i.getValue().slice(0, 8)}…</span>
  )}),
  col.accessor('occurredOn', {
    header: 'Date',
    cell: (i) => new Date(i.getValue()).toLocaleString('en-GB', { dateStyle: 'medium', timeStyle: 'short' }),
  }),
]

interface Props {
  data: AlertDto[]
  isLoading?: boolean
}

export default function AlertsTable({ data, isLoading = false }: Props) {
  const table = useReactTable({ data, columns, getCoreRowModel: getCoreRowModel() })

  if (isLoading)
    return <div className="py-12 text-center text-slate-400 text-sm">Loading alerts…</div>

  return (
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
                No alerts
              </td>
            </tr>
          ) : (
            table.getRowModel().rows.map((row) => (
              <tr key={row.id} className="border-b border-slate-100 hover:bg-slate-50 transition-colors">
                {row.getVisibleCells().map((cell) => (
                  <td key={cell.id} className="px-4 py-3">
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
