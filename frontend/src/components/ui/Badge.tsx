const styles: Record<string, string> = {
  // Risk
  Low: 'bg-green-100 text-green-800',
  Medium: 'bg-amber-100 text-amber-800',
  High: 'bg-orange-100 text-orange-800',
  Critical: 'bg-red-100 text-red-800',
  // Account status
  Active: 'bg-green-100 text-green-800',
  Suspended: 'bg-amber-100 text-amber-800',
  UnderReview: 'bg-blue-100 text-blue-800',
  Closed: 'bg-slate-100 text-slate-600',
  // Transaction status
  Pending: 'bg-slate-100 text-slate-600',
  Completed: 'bg-green-100 text-green-800',
  Failed: 'bg-red-100 text-red-800',
  Flagged: 'bg-amber-100 text-amber-800',
  // Alert severity
  Info: 'bg-blue-100 text-blue-800',
  Warning: 'bg-amber-100 text-amber-800',
}

interface BadgeProps {
  value: string
  label?: string
}

export default function Badge({ value, label }: BadgeProps) {
  const cls = styles[value] ?? 'bg-slate-100 text-slate-600'
  return (
    <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${cls}`}>
      {label ?? value}
    </span>
  )
}
