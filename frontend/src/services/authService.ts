import { UserManager, WebStorageStateStore, type User } from 'oidc-client-ts'

const userManager = new UserManager({
  authority: 'https://localhost:5001',
  client_id: 'vue-client',
  redirect_uri: `${window.location.origin}/callback`,
  post_logout_redirect_uri: `${window.location.origin}`,
  response_type: 'code',
  scope: 'openid profile email api1 offline_access',
  userStore: new WebStorageStateStore({ store: localStorage }),
  automaticSilentRenew: true,
  silent_redirect_uri: `${window.location.origin}/silent-renew`,
  loadUserInfo: true,
})

export const authService = {
  getUser(): Promise<User | null> {
    return userManager.getUser()
  },

  login(): Promise<void> {
    return userManager.signinRedirect()
  },

  async handleCallback(): Promise<User> {
    return userManager.signinRedirectCallback()
  },

  logout(): Promise<void> {
    return userManager.signoutRedirect()
  },

  silentRenew(): Promise<void> {
    return userManager.signinSilentCallback()
  },

  async getAccessToken(): Promise<string | null> {
    const user = await userManager.getUser()
    return user?.access_token ?? null
  },

  onUserLoaded(callback: (user: User) => void): void {
    userManager.events.addUserLoaded(callback)
  },

  onUserUnloaded(callback: () => void): void {
    userManager.events.addUserUnloaded(callback)
  },
}
