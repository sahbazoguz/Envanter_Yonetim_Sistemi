using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TKBYSApp.Web.Models;

namespace TKBYSApp.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Personel>>();

        string[] roles =
        [
            RoleConstants.Admin,
            RoleConstants.Personel,
            RoleConstants.TasinirKayitYetkilisi,
            RoleConstants.TasinirKontrolYetkilisi,
            RoleConstants.HarcamaYetkilisi,
            RoleConstants.SatinAlmaMemuru
        ];

        // Roller ekle
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Admin kullanýcý ekle
        var adminEmail = "admin@tkbys.local";
        var adminPassword = "Admin123!"; // Ýlk giriþ için varsayýlan þifre

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new Personel
            {
                UserName = adminEmail,
                Email = adminEmail,
                AdSoyad = "Sistem Admini",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, RoleConstants.Admin);
            }
        }
    }
}
