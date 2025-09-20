using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TKBYSApp.Web.Models;
using TKBYSApp.Web.Models.Enums;
using TKBYSApp.Web.Models.ViewModels;
using TKBYSApp.Web.Services.Interfaces;

namespace TKBYSApp.Web.Controllers;

[Authorize]
public class SatinAlmaTalepleriController : Controller
{
    private readonly ISatinAlmaTalepService _service;
    private readonly UserManager<Personel> _userManager;

    public SatinAlmaTalepleriController(ISatinAlmaTalepService service, UserManager<Personel> userManager)
    {
        _service = service;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var talepler = await _service.GetAllAsync();
        return View(talepler);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var talep = await _service.GetByIdAsync(id.Value);
        if (talep == null)
        {
            return NotFound();
        }

        return View(talep);
    }

    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Personel}")]
    public IActionResult Create()
    {
        return View(new SatinAlmaTalep());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Personel}")]
    public async Task<IActionResult> Create([Bind("MalzemeAdi,Gerekce")] SatinAlmaTalep talep)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Challenge();
        }

        if (ModelState.IsValid)
        {
            await _service.CreateTalepAsync(talep, userId);
            return RedirectToAction(nameof(Index));
        }

        return View(talep);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SatinAlmaMemuru}")]
    public async Task<IActionResult> Baslat(int id)
    {
        try
        {
            await _service.StartPurchaseReviewAsync(id);
            TempData["StatusMessage"] = "Talep satın alma incelemesine alındı.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SatinAlmaMemuru}")]
    public async Task<IActionResult> HarcamaYetkilisineGonder(int id)
    {
        try
        {
            await _service.RouteToHarcamaYetkilisiAsync(id);
            TempData["StatusMessage"] = "Talep harcama yetkilisi onayına gönderildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.HarcamaYetkilisi}")]
    public async Task<IActionResult> Onayla(int id)
    {
        try
        {
            await _service.ApproveAsync(id);
            TempData["StatusMessage"] = "Talep onaylandı.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.HarcamaYetkilisi}")]
    public async Task<IActionResult> Reddet(int id)
    {
        try
        {
            await _service.RejectAsync(id);
            TempData["StatusMessage"] = "Talep reddedildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SatinAlmaMemuru}")]
    public async Task<IActionResult> Tamamla(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var talep = await _service.GetByIdAsync(id.Value);
        if (talep == null)
        {
            return NotFound();
        }

        var model = new SatinAlmaTamamlamaViewModel
        {
            Id = talep.Id,
            BelgeYolu = talep.BelgeYolu
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SatinAlmaMemuru}")]
    public async Task<IActionResult> Tamamla(SatinAlmaTamamlamaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _service.MarkCompletedAsync(model.Id, model.BelgeYolu ?? string.Empty);
            TempData["StatusMessage"] = "Satın alma tamamlandı.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return View(model);
    }
}
