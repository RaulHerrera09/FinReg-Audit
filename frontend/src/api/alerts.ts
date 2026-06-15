import { apiGet } from './client'
import type { AlertDto, PaginatedList } from '../types'

export const getAlerts = (page = 1, pageSize = 20) =>
  apiGet<PaginatedList<AlertDto>>(`/alerts?page=${page}&pageSize=${pageSize}`)

export const getAlertsByAccount = (accountId: string, page = 1, pageSize = 20) =>
  apiGet<PaginatedList<AlertDto>>(
    `/accounts/${accountId}/alerts?page=${page}&pageSize=${pageSize}`,
  )
