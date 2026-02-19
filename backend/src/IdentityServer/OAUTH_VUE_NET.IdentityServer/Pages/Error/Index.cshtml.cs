using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OAUTH_VUE_NET.IdentityServer.Pages.Error;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly IIdentityServerInteractionService _interaction;

    public IndexModel(IIdentityServerInteractionService interaction)
    {
        _interaction = interaction;
    }

    public ErrorMessage? Error { get; private set; }

    public async Task OnGetAsync(string? errorId = null)
    {
        if (errorId != null)
            Error = await _interaction.GetErrorContextAsync(errorId);
    }
}
