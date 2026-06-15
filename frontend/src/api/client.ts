import type { ApiResponse } from '../types'
import { useAuthStore } from '../store/authStore'

const BASE = `${import.meta.env.VITE_API_BASE_URL ?? ''}/api`

async function request<T>(path: string, options: RequestInit = {}): Promise<ApiResponse<T>> {
  const token = useAuthStore.getState().token

  const res = await fetch(`${BASE}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
  })

  if (res.status === 401) {
    useAuthStore.getState().logout()
    window.location.href = '/login'
    throw new Error('Unauthorized')
  }

  return res.json()
}

export const apiGet = <T>(path: string) => request<T>(path)

export const apiPost = <T>(path: string, body: unknown) =>
  request<T>(path, { method: 'POST', body: JSON.stringify(body) })
