using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OAUTH_VUE_NET.IdentityServer.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    public void OnGet() { }
}
