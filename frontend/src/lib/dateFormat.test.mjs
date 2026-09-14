// Regression tests for zoned timestamps. Runs on Node's built-in test runner: pnpm test
import assert from 'node:assert/strict'
import test from 'node:test'
import { formatDateTimeWithZone, formatUtcDateTime } from './dateFormat.ts'

const sample = '2026-06-16T11:16:05Z'

test('Intl rejects dateStyle/timeStyle with timeZoneName, the cause of the account detail crash', () => {
  assert.throws(
    () => new Date(sample).toLocaleString('en-GB', { timeZoneName: 'short', dateStyle: 'medium', timeStyle: 'short' }),
    TypeError,
  )
})

test('transaction timestamps keep date, time and time zone', () => {
  const text = formatDateTimeWithZone(sample)
  assert.match(text, /\d{1,2} [A-Z][a-z]{2} 2026/)
  assert.match(text, /\d{2}:\d{2}/)
  assert.match(text, /(GMT|UTC|[A-Z]{2,5})([+-]\d{1,2}(:\d{2})?)?$/)
})

test('audit timestamps are shown in UTC with seconds', () => {
  assert.equal(formatUtcDateTime(sample), '16 Jun 2026, 11:16:05 UTC')
})
