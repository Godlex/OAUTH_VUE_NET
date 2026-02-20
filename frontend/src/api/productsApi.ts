import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

const apiBase = import.meta.env.VITE_API_URL ?? 'https://localhost:5002'
const apiClient = axios.create({
  baseURL: `${apiBase}/api`,
  headers: { 'Content-Type': 'application/json' },
})

apiClient.interceptors.request.use(async (config) => {
  const authStore = useAuthStore()
  if (authStore.accessToken) {
    config.headers.Authorization = `Bearer ${authStore.accessToken}`
  }
  return config
})

export interface Product {
  id: number
  name: string
  description: string
  price: number
  quantity: number
  category: string
  createdAt: string
  updatedAt?: string
}

export interface CreateProductPayload {
  name: string
  description: string
  price: number
  quantity: number
  category: string
}

export const productsApi = {
  getAll(): Promise<Product[]> {
    return apiClient.get<Product[]>('/products').then((r) => r.data)
  },
  getById(id: number): Promise<Product> {
    return apiClient.get<Product>(`/products/${id}`).then((r) => r.data)
  },
  create(payload: CreateProductPayload): Promise<Product> {
    return apiClient.post<Product>('/products', payload).then((r) => r.data)
  },
  update(id: number, payload: CreateProductPayload): Promise<Product> {
    return apiClient.put<Product>(`/products/${id}`, payload).then((r) => r.data)
  },
  remove(id: number): Promise<void> {
    return apiClient.delete(`/products/${id}`).then(() => undefined)
  },
}
