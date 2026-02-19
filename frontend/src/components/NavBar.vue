<template>
  <nav class="navbar">
    <div class="navbar-brand">
      <RouterLink to="/" class="brand-link">
        📦 InventoryApp
      </RouterLink>
    </div>

    <div class="navbar-menu">
      <RouterLink to="/" class="nav-link">Home</RouterLink>
      <RouterLink v-if="isAuthenticated" to="/products" class="nav-link">Products</RouterLink>
    </div>

    <div class="navbar-auth">
      <template v-if="isAuthenticated">
        <span class="user-greeting">👋 {{ userName }}</span>
        <button class="btn btn-outline" @click="logout">Sign Out</button>
      </template>
      <template v-else>
        <button class="btn btn-primary" @click="login">Sign In</button>
      </template>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { storeToRefs } from 'pinia'

const authStore = useAuthStore()
const { isAuthenticated, userName } = storeToRefs(authStore)

function login() { authStore.login() }
function logout() { authStore.logout() }
</script>

<style scoped>
.navbar {
  background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
  padding: 0.9rem 2rem;
  display: flex;
  align-items: center;
  gap: 1.5rem;
  box-shadow: 0 2px 8px rgba(0,0,0,0.15);
}

.brand-link {
  color: #f8fafc;
  text-decoration: none;
  font-size: 1.2rem;
  font-weight: 700;
  letter-spacing: -0.5px;
}

.navbar-menu {
  display: flex;
  gap: 0.5rem;
  flex: 1;
}

.nav-link {
  color: #94a3b8;
  text-decoration: none;
  padding: 0.4rem 0.8rem;
  border-radius: 6px;
  font-size: 0.95rem;
  transition: color 0.2s, background 0.2s;
}

.nav-link:hover,
.nav-link.router-link-exact-active {
  color: #f8fafc;
  background: rgba(255,255,255,0.08);
}

.navbar-auth {
  display: flex;
  align-items: center;
  gap: 0.9rem;
}

.user-greeting {
  color: #cbd5e1;
  font-size: 0.9rem;
}

.btn {
  padding: 0.45rem 1rem;
  border-radius: 7px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  border: none;
  transition: all 0.2s;
}

.btn-primary {
  background: linear-gradient(135deg, #6366f1, #8b5cf6);
  color: #fff;
}

.btn-primary:hover { background: linear-gradient(135deg, #4f46e5, #7c3aed); }

.btn-outline {
  background: transparent;
  color: #94a3b8;
  border: 1.5px solid #334155;
}

.btn-outline:hover {
  color: #f8fafc;
  border-color: #6366f1;
}
</style>
