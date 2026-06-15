import { apiGet, apiPost } from './client'
import type { PaginatedList, TransactionDto, TransactionType } from '../types'

export const getTransactionsByAccount = (accountId: string, page = 1, pageSize = 20) =>
  apiGet<PaginatedList<TransactionDto>>(
    `/accounts/${accountId}/transactions?page=${page}&pageSize=${pageSize}`,
  )

export const getTransaction = (id: string) =>
  apiGet<TransactionDto>(`/transactions/${id}`)

export const initiateTransaction = (
  accountId: string,
  amount: number,
  currency: string,
  type: TransactionType,
  description: string,
) => apiPost<string>(`/accounts/${accountId}/transactions`, { amount, currency, type, description })

export const completeTransaction = (accountId: string, transactionId: string) =>
  apiPost<boolean>(`/accounts/${accountId}/transactions/${transactionId}/complete`, {})

export const flagTransaction = (
  accountId: string,
  transactionId: string,
  riskLevel: string,
  severity: string,
  reason: string,
) =>
  apiPost<boolean>(`/accounts/${accountId}/transactions/${transactionId}/flag`, {
    riskLevel,
    severity,
    reason,
  })
