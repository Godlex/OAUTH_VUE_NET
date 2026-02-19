using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace OAUTH_VUE_NET.IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResources.Email()
    ];

    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new ApiScope("api1", "Inventory API")
    ];

    public static IEnumerable<ApiResource> ApiResources =>
    [
        new ApiResource("api1", "Inventory API")
        {
            Scopes = { "api1" }
        }
    ];

    public static IEnumerable<Client> Clients =>
    [
        // Vue.js SPA — Authorization Code + PKCE (no client secret, public client)
        new Client
        {
            ClientId = "vue-client",
            ClientName = "Vue.js Inventory App",
            ClientUri = "http://localhost:5173",

            AllowedGrantTypes = GrantTypes.Code,
            RequireClientSecret = false,
            RequirePkce = true,
            AllowPlainTextPkce = false,

            RedirectUris           = { "http://localhost:5173/callback" },
            PostLogoutRedirectUris = { "http://localhost:5173" },
            AllowedCorsOrigins     = { "http://localhost:5173" },

            AllowedScopes =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.Email,
                "api1"
            },

            AllowOfflineAccess = true,
            RequireConsent = false,
            AccessTokenLifetime = 3600,
            RefreshTokenUsage = TokenUsage.ReUse,
            RefreshTokenExpiration = TokenExpiration.Sliding
        }
    ];
}
