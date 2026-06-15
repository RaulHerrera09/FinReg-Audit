import { apiPost } from './client'
import type { LoginResponseDto } from '../types'

export const login = (email: string, password: string) =>
  apiPost<LoginResponseDto>('/auth/login', { email, password })

export const refreshToken = (userId: string, token: string) =>
  apiPost<LoginResponseDto>('/auth/refresh', { userId, refreshToken: token })
