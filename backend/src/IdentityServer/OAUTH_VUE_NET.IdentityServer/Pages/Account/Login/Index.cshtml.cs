using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace OAUTH_VUE_NET.IdentityServer.Pages.Account.Login;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IIdentityServerInteractionService _interaction;

    public IndexModel(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IIdentityServerInteractionService interaction)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _interaction = interaction;
    }

    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        Input = new LoginInputModel { ReturnUrl = returnUrl ?? string.Empty };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please fill in all required fields.";
            return Page();
        }

        // Allow login by username or email
        var user = await _userManager.FindByNameAsync(Input.Username)
            ?? await _userManager.FindByEmailAsync(Input.Username);
        if (user == null)
        {
            ErrorMessage = "Invalid username or password.";
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!, Input.Password, Input.RememberLogin, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (context != null || Url.IsLocalUrl(Input.ReturnUrl))
                return Redirect(Input.ReturnUrl ?? "/");

            return RedirectToPage("/Index");
        }

        if (result.IsLockedOut)
            ErrorMessage = "Account locked out. Try again later.";
        else
            ErrorMessage = "Invalid username or password.";

        return Page();
    }
}

public class LoginInputModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberLogin { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
}
