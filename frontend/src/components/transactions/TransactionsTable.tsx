import { useState } from 'react'
import { Flag } from 'lucide-react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import type { TransactionDto } from '../../types'
import { flagTransaction } from '../../api/transactions'

function badge(value: string) { return `badge ${value === 'Critical' || value === 'Failed' ? 'badge-critical' : value === 'High' || value === 'Flagged' ? 'badge-review' : value === 'Completed' || value === 'Low' ? 'badge-positive' : 'badge-neutral'}` }

export default function TransactionsTable({ accountId, data, isLoading, canReview }: { accountId: string; data: TransactionDto[]; isLoading: boolean; canReview: boolean }) {
  const [selected, setSelected] = useState<string | null>(null)
  const [reason, setReason] = useState('')
  const [error, setError] = useState('')
  const client = useQueryClient()
  const mutation = useMutation({ mutationFn: () => flagTransaction(accountId, selected!, 'High', 'High', reason.trim()), onSuccess: (res) => { if (res.success) { client.invalidateQueries({ queryKey: ['transactions', accountId] }); client.invalidateQueries({ queryKey: ['alerts'] }); setSelected(null); setReason('') } else setError(res.error ?? 'The review could not be recorded.') }, onError: (e) => setError(e.message) })
  if (isLoading) return <div className="loading-state">Loading transactions…</div>
  if (!data.length) return <div className="empty-state">No transactions are available for this account.</div>
  return <><div className="data-scroll"><table className="data-table"><thead><tr><th>Amount</th><th>Type</th><th>Status</th><th>Risk</th><th>Description</th><th>Occurred</th>{canReview && <th>Review</th>}</tr></thead><tbody>{data.map(tx => <tr key={tx.id}><td className="mono">{tx.currency} {tx.amount.toLocaleString('en-GB', { minimumFractionDigits: 2 })}</td><td>{tx.type}</td><td><span className={badge(tx.status)}>{tx.status}</span></td><td><span className={badge(tx.riskLevel)}>{tx.riskLevel}</span></td><td>{tx.description}</td><td>{new Date(tx.initiatedAt).toLocaleString('en-GB', { timeZoneName: 'short', dateStyle: 'medium', timeStyle: 'short' })}</td>{canReview && <td>{(tx.status === 'Pending' || tx.status === 'Completed') && <button className="secondary-button" onClick={() => { setSelected(tx.id); setError('') }}><Flag size={16} aria-hidden="true" /> Record exception</button>}</td>}</tr>)}</tbody></table></div>{selected && <div className="modal-backdrop"><section className="modal" role="dialog" aria-modal="true" aria-labelledby="exception-title" onKeyDown={e => { if (e.key === 'Escape') setSelected(null) }}><h2 id="exception-title">Record risk exception</h2><p className="page-summary">This records a risk flag. It does not resolve or approve an alert.</p><div className="form-field"><label htmlFor="exception-reason">Review reason</label><input id="exception-reason" autoFocus value={reason} onChange={e => setReason(e.target.value)} onBlur={() => !reason.trim() && setError('Provide a reason for the risk exception.')} aria-describedby={error ? 'exception-error' : undefined} />{error && <p id="exception-error" className="field-error">{error}</p>}</div><div className="modal-actions"><button className="secondary-button" onClick={() => setSelected(null)}>Cancel</button><button className="action-button" disabled={mutation.isPending || !reason.trim()} onClick={() => mutation.mutate()}>{mutation.isPending ? 'Recording…' : 'Record exception'}</button></div></section></div>}</>
}
