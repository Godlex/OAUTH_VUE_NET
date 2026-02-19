using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OAUTH_VUE_NET.IdentityServer.Pages.Account.Logout;

public class IndexModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;

    public IndexModel(SignInManager<IdentityUser> signInManager, IIdentityServerInteractionService interaction)
    {
        _signInManager = signInManager;
        _interaction = interaction;
    }

    public string? PostLogoutRedirectUri { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? logoutId = null)
    {
        var logout = await _interaction.GetLogoutContextAsync(logoutId);

        await _signInManager.SignOutAsync();

        PostLogoutRedirectUri = logout?.PostLogoutRedirectUri;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? logoutId = null)
    {
        var logout = await _interaction.GetLogoutContextAsync(logoutId);
        await _signInManager.SignOutAsync();
        await _interaction.RevokeUserConsentAsync(logoutId ?? string.Empty);

        PostLogoutRedirectUri = logout?.PostLogoutRedirectUri;
        return Page();
    }
}
