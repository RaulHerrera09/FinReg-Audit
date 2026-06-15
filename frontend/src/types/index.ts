export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: string
  validationErrors?: string[]
}

export interface PaginatedList<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type AccountStatus = 'Active' | 'Suspended' | 'UnderReview' | 'Closed'
export type RiskLevel = 'Low' | 'Medium' | 'High' | 'Critical'
export type TransactionType = 'Credit' | 'Debit' | 'Transfer'
export type TransactionStatus = 'Pending' | 'Completed' | 'Failed' | 'Flagged'
export type AlertSeverity = 'Info' | 'Warning' | 'High' | 'Critical'

export interface AccountDto {
  id: string
  accountNumber: string
  holderName: string
  status: AccountStatus
  balance: number
  currency: string
  riskLevel: RiskLevel
  version: number
}

export interface TransactionDto {
  id: string
  accountId: string
  amount: number
  currency: string
  type: TransactionType
  status: TransactionStatus
  description: string
  riskLevel: RiskLevel
  initiatedAt: string
  completedAt?: string
  failureReason?: string
}

export interface AlertDto {
  transactionId: string
  accountId: string
  severity: AlertSeverity
  riskLevel: RiskLevel
  reason: string
  occurredOn: string
}

export interface AuditEventDto {
  eventId: string
  aggregateId: string
  aggregateType: string
  eventType: string
  occurredOn: string
  version: number
}

export interface ComplianceReportDto {
  accountId: string
  accountNumber: string
  holderName: string
  riskLevel: string
  totalTransactions: number
  totalValueGbp: number
  flaggedTransactions: number
  generatedAt: string
}

export interface LoginResponseDto {
  accessToken: string
  refreshToken: string
  expiresAt: string
  userId: string
  role: string
}
