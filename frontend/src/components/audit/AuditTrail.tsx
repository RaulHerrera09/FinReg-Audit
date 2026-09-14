import { useState } from 'react'
import { ChevronDown, ChevronUp } from 'lucide-react'
import type { AuditEventDto, PaginatedList } from '../../types'
import Pagination from '../ui/Pagination'
import { formatUtcDateTime as utc } from '../../lib/dateFormat'

function local(value: string) { return new Date(value).toLocaleString('en-GB', { dateStyle: 'medium', timeStyle: 'short' }) }

export default function AuditTrail({ list, loading, error, page, onPageChange }: { list?: PaginatedList<AuditEventDto>, loading: boolean, error?: Error | null, page: number, onPageChange: (page: number) => void }) {
  const [open, setOpen] = useState<string | null>(null)
  if (loading) return <div className="loading-state" aria-live="polite">Loading event-sourced audit history…</div>
  if (error) return <div className="error-state" role="alert">Unable to load the audit history. {error.message}</div>
  if (!list?.items.length) return <div className="empty-state">No recorded events are available for this account.</div>
  return <>
    <div className="status-note">Event-sourced audit history. Actor, source, ingestion time and payload are shown only when supplied by the API.</div>
    <div className="evidence-rail" aria-label="Audit event timeline">
      {list.items.map(event => {
        const expanded = open === event.eventId
        return <article className="evidence-row" key={event.eventId}>
          <div className="event-grid"><div><p className="event-title">{event.eventType}</p><p className="event-meta"><span className="mono">v{event.version}</span> · {event.aggregateType} · occurred {utc(event.occurredOn)}</p></div><button className="icon-button" aria-label={`${expanded ? 'Hide' : 'Show'} metadata for ${event.eventType}`} aria-expanded={expanded} onClick={() => setOpen(expanded ? null : event.eventId)}>{expanded ? <ChevronUp aria-hidden="true" /> : <ChevronDown aria-hidden="true" />}</button></div>
          {expanded && <dl className="metadata-list" style={{ marginTop: 16 }}><dt>Event ID</dt><dd className="mono">{event.eventId}</dd><dt>Entity</dt><dd className="mono">{event.aggregateId}</dd><dt>Occurred at (UTC)</dt><dd>{utc(event.occurredOn)}</dd><dt>Local display time</dt><dd>{local(event.occurredOn)}</dd><dt>Actor</dt><dd>{event.actor ?? 'Not available'}</dd><dt>Source / origin</dt><dd>{event.source ?? 'Not available'}</dd><dt>Ingested at</dt><dd>{event.ingestedAt ? utc(event.ingestedAt) : 'Not available'}</dd></dl>}
        </article>
      })}
    </div>
    <Pagination page={page} totalPages={list.totalPages} totalCount={list.totalCount} onPageChange={onPageChange} />
  </>
}
