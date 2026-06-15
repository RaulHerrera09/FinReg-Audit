import { apiGet, apiPost } from './client'
import type { AccountDto, AuditEventDto, ComplianceReportDto, PaginatedList } from '../types'

export const getAccounts = (page = 1, pageSize = 20) =>
  apiGet<PaginatedList<AccountDto>>(`/accounts?page=${page}&pageSize=${pageSize}`)

export const getAccount = (id: string) =>
  apiGet<AccountDto>(`/accounts/${id}`)

export const openAccount = (holderName: string, currency: string) =>
  apiPost<string>('/accounts', { holderName, currency })

export const suspendAccount = (id: string, reason: string) =>
  apiPost<boolean>(`/accounts/${id}/suspend`, { reason })

export const closeAccount = (id: string, reason: string) =>
  apiPost<boolean>(`/accounts/${id}/close`, { reason })

export const getComplianceReport = (id: string) =>
  apiGet<ComplianceReportDto>(`/accounts/${id}/compliance-report`)

export const getAuditTrail = (id: string, page = 1, pageSize = 20) =>
  apiGet<PaginatedList<AuditEventDto>>(
    `/accounts/${id}/audit-trail?page=${page}&pageSize=${pageSize}`,
  )
