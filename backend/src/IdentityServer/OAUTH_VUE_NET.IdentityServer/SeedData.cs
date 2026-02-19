using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OAUTH_VUE_NET.IdentityServer.Data;

namespace OAUTH_VUE_NET.IdentityServer;

public static class SeedData
{
    public static async Task EnsureSeedDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<IdentityDbContext>();
        await context.Database.MigrateAsync();

        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await EnsureUserAsync(userManager, "alice", "alice@example.com", "Alice123!");
        await EnsureUserAsync(userManager, "bob", "bob@example.com", "Bob123!");
    }

    private static async Task EnsureUserAsync(UserManager<IdentityUser> userManager, string userName, string email, string password)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user is not null) return;

        user = new IdentityUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new Exception($"Failed to create user '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }
}
