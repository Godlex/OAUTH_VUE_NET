<template>
  <div class="home">
    <div class="hero">
      <div class="hero-badge">🔐 OAuth 2.0 + PKCE</div>
      <h1>Inventory Management</h1>
      <p class="hero-subtitle">
        A full-stack application built with <strong>.NET 10</strong>, <strong>Vue.js 3</strong>,
        <strong>Duende IdentityServer</strong>, and <strong>CQRS</strong>.
      </p>

      <div class="hero-actions">
        <template v-if="isAuthenticated">
          <RouterLink to="/products" class="btn btn-primary btn-lg">
            View Inventory →
          </RouterLink>
          <span class="hero-user">Signed in as <strong>{{ userName }}</strong></span>
        </template>
        <template v-else>
          <button class="btn btn-primary btn-lg" @click="login">
            Sign In to Get Started
          </button>
          <p class="hint">Use <code>alice / Alice123!</code> or <code>bob / Bob123!</code></p>
        </template>
      </div>
    </div>

    <div class="features">
      <div class="feature-card">
        <div class="feature-icon">🏗️</div>
        <h3>Clean Architecture</h3>
        <p>Separated into <strong>Data</strong>, <strong>BLL</strong>, and <strong>WebAPI</strong> projects with clear boundaries.</p>
      </div>
      <div class="feature-card">
        <div class="feature-icon">⚡</div>
        <h3>CQRS with MediatR</h3>
        <p>Commands and Queries are handled separately via MediatR pipeline with FluentValidation behaviors.</p>
      </div>
      <div class="feature-card">
        <div class="feature-icon">🔒</div>
        <h3>OAuth 2.0 / OIDC</h3>
        <p>Duende IdentityServer issues JWTs. Vue uses Authorization Code + PKCE flow via oidc-client-ts.</p>
      </div>
      <div class="feature-card">
        <div class="feature-icon">💾</div>
        <h3>Entity Framework Core</h3>
        <p>SQLite database with repository pattern. Auto-migrates on startup with seed data.</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { storeToRefs } from 'pinia'

const authStore = useAuthStore()
const { isAuthenticated, userName } = storeToRefs(authStore)

function login() { authStore.login() }
</script>

<style scoped>
.home { padding: 1rem 0 3rem; }

.hero {
  text-align: center;
  padding: 4rem 2rem;
  background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
  border-radius: 16px;
  margin-bottom: 3rem;
  color: #f8fafc;
}

.hero-badge {
  display: inline-block;
  background: rgba(99,102,241,0.25);
  color: #a5b4fc;
  border: 1px solid rgba(99,102,241,0.4);
  padding: 0.35rem 0.9rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 600;
  letter-spacing: 0.5px;
  margin-bottom: 1.2rem;
}

.hero h1 {
  font-size: clamp(2rem, 4vw, 3rem);
  font-weight: 800;
  margin-bottom: 1rem;
  letter-spacing: -1px;
}

.hero-subtitle {
  color: #94a3b8;
  font-size: 1.05rem;
  max-width: 560px;
  margin: 0 auto 2rem;
  line-height: 1.6;
}

.hero-actions {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.hero-user {
  color: #94a3b8;
  font-size: 0.9rem;
}

.hint {
  color: #64748b;
  font-size: 0.85rem;
}

.hint code {
  background: rgba(255,255,255,0.08);
  padding: 0.1rem 0.4rem;
  border-radius: 4px;
  font-family: 'Fira Code', monospace;
}

.btn {
  display: inline-block;
  padding: 0.7rem 1.5rem;
  border-radius: 9px;
  font-weight: 600;
  cursor: pointer;
  border: none;
  text-decoration: none;
  transition: all 0.2s;
}

.btn-primary {
  background: linear-gradient(135deg, #6366f1, #8b5cf6);
  color: #fff;
}

.btn-primary:hover { background: linear-gradient(135deg, #4f46e5, #7c3aed); transform: translateY(-1px); }

.btn-lg { padding: 0.85rem 2rem; font-size: 1rem; }

.features {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 1.5rem;
}

.feature-card {
  background: #fff;
  border-radius: 12px;
  padding: 1.8rem;
  box-shadow: 0 1px 6px rgba(0,0,0,0.06);
  border: 1px solid #e2e8f0;
  transition: box-shadow 0.2s, transform 0.2s;
}

.feature-card:hover {
  box-shadow: 0 4px 20px rgba(0,0,0,0.08);
  transform: translateY(-2px);
}

.feature-icon { font-size: 2rem; margin-bottom: 0.8rem; }

.feature-card h3 {
  font-size: 1rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
  color: #1e293b;
}

.feature-card p {
  font-size: 0.9rem;
  color: #64748b;
  line-height: 1.6;
}
</style>
