import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authService } from '@/services/authService'
import type { User } from 'oidc-client-ts'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const loading = ref(false)

  const isAuthenticated = computed(() => !!user.value && !user.value.expired)
  const userName = computed(() => user.value?.profile?.name ?? user.value?.profile?.preferred_username ?? '')
  const accessToken = computed(() => user.value?.access_token ?? null)

  async function init() {
    loading.value = true
    try {
      user.value = await authService.getUser()
    } finally {
      loading.value = false
    }

    authService.onUserLoaded((loadedUser) => { user.value = loadedUser })
    authService.onUserUnloaded(() => { user.value = null })
  }

  async function login() {
    await authService.login()
  }

  async function logout() {
    await authService.logout()
    user.value = null
  }

  async function handleCallback() {
    loading.value = true
    try {
      user.value = await authService.handleCallback()
    } finally {
      loading.value = false
    }
  }

  return { user, loading, isAuthenticated, userName, accessToken, init, login, logout, handleCallback }
})
