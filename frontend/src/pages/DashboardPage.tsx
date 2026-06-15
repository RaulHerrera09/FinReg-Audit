import { useQuery } from '@tanstack/react-query'
import { AlertTriangle, Building2, Flag, ShieldCheck } from 'lucide-react'
import { getAccounts } from '../api/accounts'
import { getAlerts } from '../api/alerts'
import StatCard from '../components/ui/StatCard'
import AlertsTable from '../components/alerts/AlertsTable'

export default function DashboardPage() {
  const { data: accountsData } = useQuery({
    queryKey: ['accounts', 1, 1],
    queryFn: () => getAccounts(1, 1),
  })

  const { data: alertsData, isLoading: alertsLoading } = useQuery({
    queryKey: ['alerts', 1, 10],
    queryFn: () => getAlerts(1, 10),
  })

  const totalAccounts = accountsData?.data?.totalCount ?? 0
  const totalAlerts = alertsData?.data?.totalCount ?? 0
  const criticalAlerts =
    alertsData?.data?.items.filter((a) => a.severity === 'Critical').length ?? 0
  const flaggedTx =
    alertsData?.data?.items.filter(
      (a) => a.riskLevel === 'High' || a.riskLevel === 'Critical',
    ).length ?? 0

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Compliance Dashboard</h1>
        <p className="text-slate-500 text-sm mt-1">
          Real-time FCA transaction monitoring and risk assessment
        </p>
      </div>

      <div className="grid grid-cols-2 xl:grid-cols-4 gap-4">
        <StatCard title="Total Accounts" value={totalAccounts} icon={Building2} accent="indigo" />
        <StatCard title="Active Alerts" value={totalAlerts} icon={AlertTriangle} accent="amber" />
        <StatCard title="High-Risk Flags" value={flaggedTx} icon={Flag} accent="red" />
        <StatCard title="Critical Alerts" value={criticalAlerts} icon={ShieldCheck} accent="red" />
      </div>

      <div className="bg-white rounded-xl border border-slate-200">
        <div className="px-5 py-4 border-b border-slate-200">
          <h2 className="text-sm font-semibold text-slate-900">Recent Alerts</h2>
          <p className="text-xs text-slate-500 mt-0.5">Latest FCA-threshold breach notifications</p>
        </div>
        <AlertsTable data={alertsData?.data?.items ?? []} isLoading={alertsLoading} />
      </div>
    </div>
  )
}
