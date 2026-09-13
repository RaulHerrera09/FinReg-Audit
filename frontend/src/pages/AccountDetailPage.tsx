import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ArrowLeft } from 'lucide-react'
import { getAccount, getAuditTrail, getComplianceReport } from '../api/accounts'
import { getTransactionsByAccount } from '../api/transactions'
import { getAlertsByAccount } from '../api/alerts'
import TransactionsTable from '../components/transactions/TransactionsTable'
import AlertsTable from '../components/alerts/AlertsTable'
import Pagination from '../components/ui/Pagination'
import AuditTrail from '../components/audit/AuditTrail'
import { useAuthStore } from '../store/authStore'

type Tab = 'transactions' | 'alerts' | 'compliance' | 'evidence'
export default function AccountDetailPage() {
  const { id } = useParams<{ id: string }>(); const navigate = useNavigate(); const [tab, setTab] = useState<Tab>('transactions'); const [txPage, setTxPage] = useState(1); const [auditPage, setAuditPage] = useState(1); const canReview = useAuthStore(s => s.role) === 'ComplianceOfficer'
  const account = useQuery({ queryKey: ['account', id], queryFn: () => getAccount(id!), enabled: !!id })
  const tx = useQuery({ queryKey: ['transactions', id, txPage], queryFn: () => getTransactionsByAccount(id!, txPage), enabled: !!id && tab === 'transactions' })
  const alerts = useQuery({ queryKey: ['account-alerts', id], queryFn: () => getAlertsByAccount(id!), enabled: !!id && tab === 'alerts' })
  const report = useQuery({ queryKey: ['compliance', id], queryFn: () => getComplianceReport(id!), enabled: !!id && tab === 'compliance' })
  const audit = useQuery({ queryKey: ['audit', id, auditPage], queryFn: () => getAuditTrail(id!, auditPage), enabled: !!id && tab === 'evidence' })
  if (account.isLoading) return <div className="loading-state">Loading account…</div>
  const item = account.data?.data; if (!item) return <div className="error-state">Account not found or unavailable.</div>
  const tabs: Tab[] = ['transactions', 'alerts', 'compliance', 'evidence']
  return <div><button className="secondary-button" onClick={() => navigate('/accounts')}><ArrowLeft size={16} aria-hidden="true" /> Back to accounts</button><header className="workspace-header" style={{ marginTop: 24 }}><div><span className="page-eyebrow">Account record</span><h1 className="page-title mono">{item.accountNumber}</h1><p className="page-summary">{item.holderName} · {item.currency} {item.balance.toLocaleString('en-GB', { minimumFractionDigits: 2 })}</p></div><div className="user-context"><strong>{item.status} / {item.riskLevel}</strong>Operational account changes are unavailable for the supported roles.</div></header><section className="surface"><div className="detail-tabs" role="tablist" aria-label="Account sections">{tabs.map(key => <button key={key} role="tab" aria-selected={tab === key} onClick={() => setTab(key)}>{key === 'compliance' ? 'Compliance report' : key}</button>)}</div>{tab === 'transactions' && <><TransactionsTable accountId={id!} data={tx.data?.data?.items ?? []} isLoading={tx.isLoading} canReview={canReview} /><Pagination page={txPage} totalPages={tx.data?.data?.totalPages ?? 1} totalCount={tx.data?.data?.totalCount ?? 0} onPageChange={setTxPage} /></>}{tab === 'alerts' && <AlertsTable data={alerts.data?.data?.items ?? []} isLoading={alerts.isLoading} />}{tab === 'compliance' && <div className="detail-body">{report.data?.data ? <dl className="metadata-list"><dt>Total transactions</dt><dd>{report.data.data.totalTransactions}</dd><dt>Flagged transactions</dt><dd>{report.data.data.flaggedTransactions}</dd><dt>Completed value (GBP)</dt><dd className="mono">£{report.data.data.totalValueGbp.toLocaleString('en-GB', { minimumFractionDigits: 2 })}</dd><dt>Risk level</dt><dd>{report.data.data.riskLevel}</dd><dt>Generated at</dt><dd>{new Date(report.data.data.generatedAt).toLocaleString('en-GB', { timeZoneName: 'short' })}</dd></dl> : <div className="loading-state">Loading report…</div>}</div>}{tab === 'evidence' && <AuditTrail list={audit.data?.data} loading={audit.isLoading} error={audit.error} page={auditPage} onPageChange={setAuditPage} />}</section></div>
}
