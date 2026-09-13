import { create } from 'zustand'

interface AuthState {
  token: string | null
  userId: string | null
  role: string | null
  setAuth: (token: string, userId: string, role: string) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>((set) => ({
  token: null,
  userId: null,
  role: null,
  setAuth: (token, userId, role) => set({ token, userId, role }),
  logout: () => set({ token: null, userId: null, role: null }),
}))
