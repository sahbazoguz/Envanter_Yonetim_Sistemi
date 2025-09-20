using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TKBYSApp.Web.Models;
using TKBYSApp.Web.Models.ViewModels;

namespace TKBYSApp.Web.Controllers;

[Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
public class PersonellerController : Controller
{
    private readonly UserManager<Personel> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public PersonellerController(UserManager<Personel> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderBy(u => u.AdSoyad).ToListAsync();
        return View(users);
    }

    public async Task<IActionResult> Details(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        ViewData["Roller"] = await _userManager.GetRolesAsync(user);
        return View(user);
    }

    public async Task<IActionResult> Create()
    {
        var model = new PersonelFormViewModel();
        await PopulateRolesAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PersonelFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Parola zorunludur.");
        }

        if (ModelState.IsValid)
        {
            var personel = new Personel
            {
                UserName = model.Email,
                Email = model.Email,
                AdSoyad = model.AdSoyad,
                SicilNo = model.SicilNo
            };

            var result = await _userManager.CreateAsync(personel, model.Password!);
            if (result.Succeeded)
            {
                if (model.SelectedRoles.Any())
                {
                    await _userManager.AddToRolesAsync(personel, model.SelectedRoles);
                }
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        await PopulateRolesAsync(model);
        return View(model);
    }

    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var personel = await _userManager.FindByIdAsync(id);
        if (personel == null)
        {
            return NotFound();
        }

        var model = new PersonelFormViewModel
        {
            Id = personel.Id,
            AdSoyad = personel.AdSoyad,
            Email = personel.Email ?? string.Empty,
            SicilNo = personel.SicilNo,
            SelectedRoles = (await _userManager.GetRolesAsync(personel)).ToList()
        };

        await PopulateRolesAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, PersonelFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var personel = await _userManager.FindByIdAsync(id);
            if (personel == null)
            {
                return NotFound();
            }

            personel.AdSoyad = model.AdSoyad;
            personel.Email = model.Email;
            personel.UserName = model.Email;
            personel.SicilNo = model.SicilNo;

            var result = await _userManager.UpdateAsync(personel);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await PopulateRolesAsync(model);
                return View(model);
            }

            var currentRoles = await _userManager.GetRolesAsync(personel);
            var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();

            if (rolesToAdd.Any())
            {
                await _userManager.AddToRolesAsync(personel, rolesToAdd);
            }

            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(personel, rolesToRemove);
            }

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(personel);
                var passwordResult = await _userManager.ResetPasswordAsync(personel, token, model.Password);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await PopulateRolesAsync(model);
                    return View(model);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        await PopulateRolesAsync(model);
        return View(model);
    }

    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var personel = await _userManager.FindByIdAsync(id);
        if (personel == null)
        {
            return NotFound();
        }

        ViewData["Roller"] = await _userManager.GetRolesAsync(personel);
        return View(personel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var personel = await _userManager.FindByIdAsync(id);
        if (personel != null)
        {
            await _userManager.DeleteAsync(personel);
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateRolesAsync(PersonelFormViewModel model)
    {
        var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
        model.AvailableRoles = roles
            .Select(r => new SelectListItem(r.Name, r.Name, model.SelectedRoles.Contains(r.Name)))
            .ToList();
    }
}
