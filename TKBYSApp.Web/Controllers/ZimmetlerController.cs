using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TKBYSApp.Web.Data;
using TKBYSApp.Web.Models;
using TKBYSApp.Web.Models.Enums;
using TKBYSApp.Web.Models.ViewModels;
using TKBYSApp.Web.Services.Interfaces;

namespace TKBYSApp.Web.Controllers;

[Authorize]
public class ZimmetlerController : Controller
{
    private readonly IZimmetService _zimmetService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Personel> _userManager;

    public ZimmetlerController(IZimmetService zimmetService, ApplicationDbContext context, UserManager<Personel> userManager)
    {
        _zimmetService = zimmetService;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var zimmetler = await _zimmetService.GetAllAsync();
        return View(zimmetler);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var zimmet = await _zimmetService.GetByIdAsync(id.Value);
        if (zimmet == null)
        {
            return NotFound();
        }

        return View(zimmet);
    }

    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> Create()
    {
        var model = await BuildCreateViewModelAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> Create(ZimmetCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _zimmetService.CreateZimmetAsync(model.MalzemeId, model.PersonelId);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        var refreshModel = await BuildCreateViewModelAsync(model.MalzemeId, model.PersonelId);
        ModelState.Remove(nameof(ZimmetCreateViewModel.Malzemeler));
        ModelState.Remove(nameof(ZimmetCreateViewModel.Personeller));
        return View(refreshModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.HarcamaYetkilisi}")]
    public async Task<IActionResult> Approve(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Challenge();
        }

        try
        {
            await _zimmetService.ApproveAsync(id, userId);
            TempData["StatusMessage"] = "Zimmet onaylandı.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> Return(int id)
    {
        try
        {
            await _zimmetService.ReturnAsync(id);
            TempData["StatusMessage"] = "Zimmet iade edildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var zimmet = await _zimmetService.GetByIdAsync(id.Value);
        if (zimmet == null)
        {
            return NotFound();
        }

        if (zimmet.Durum != ZimmetDurum.OnayBekliyor)
        {
            TempData["ErrorMessage"] = "Sadece onay bekleyen zimmetler silinebilir.";
            return RedirectToAction(nameof(Index));
        }

        return View(zimmet);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _zimmetService.DeleteAsync(id);
            TempData["StatusMessage"] = "Zimmet kaydı silindi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<ZimmetCreateViewModel> BuildCreateViewModelAsync(int? selectedMalzemeId = null, string? selectedPersonelId = null)
    {
        var malzemeler = await _context.Malzemeler
            .Where(m => m.Durum == MalzemeDurum.Depoda)
            .OrderBy(m => m.Ad)
            .Select(m => new SelectListItem($"{m.Ad} ({m.TKBYSNo})", m.Id.ToString(), selectedMalzemeId == m.Id))
            .ToListAsync();

        var personeller = await _context.Users
            .OrderBy(p => p.AdSoyad)
            .Select(p => new SelectListItem(p.AdSoyad, p.Id, selectedPersonelId == p.Id))
            .ToListAsync();

        return new ZimmetCreateViewModel
        {
            MalzemeId = selectedMalzemeId ?? 0,
            PersonelId = selectedPersonelId ?? string.Empty,
            Malzemeler = malzemeler,
            Personeller = personeller
        };
    }
}
