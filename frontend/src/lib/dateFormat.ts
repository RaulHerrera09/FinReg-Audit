// Intl rejects dateStyle/timeStyle combined with timeZoneName, so zoned formats list their fields explicitly.
export function formatDateTimeWithZone(value: string) {
  return new Date(value).toLocaleString('en-GB', { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit', timeZoneName: 'short' })
}

export function formatUtcDateTime(value: string) {
  return new Date(value).toLocaleString('en-GB', { timeZone: 'UTC', day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit', second: '2-digit', timeZoneName: 'short' })
}
