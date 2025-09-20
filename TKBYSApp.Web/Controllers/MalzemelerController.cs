using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TKBYSApp.Web.Data;
using TKBYSApp.Web.Models;
using TKBYSApp.Web.Models.Enums;

namespace TKBYSApp.Web.Controllers;

[Authorize]
public class MalzemelerController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index()
    {
        var items = await _context.Malzemeler.Include(m => m.Depo).ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var malzeme = await _context.Malzemeler
            .Include(m => m.Depo)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (malzeme == null)
        {
            return NotFound();
        }

        return View(malzeme);
    }

    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> Create([Bind("Id,TKBYSNo,Ad,Ozellikler,Adet,Aciklama,DepoId,Durum")] Malzeme malzeme)
    {
        if (ModelState.IsValid)
        {
            malzeme.KayitTarihi = DateTime.UtcNow;
            _context.Add(malzeme);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync(malzeme.DepoId);
        return View(malzeme);
    }

    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var malzeme = await _context.Malzemeler.FindAsync(id);
        if (malzeme == null)
        {
            return NotFound();
        }

        await PopulateLookupsAsync(malzeme.DepoId);
        return View(malzeme);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,TKBYSNo,Ad,Ozellikler,Adet,Aciklama,DepoId,Durum,KayitTarihi")] Malzeme malzeme)
    {
        if (id != malzeme.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(malzeme);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await MalzemeExists(malzeme.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync(malzeme.DepoId);
        return View(malzeme);
    }

    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var malzeme = await _context.Malzemeler
            .Include(m => m.Depo)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (malzeme == null)
        {
            return NotFound();
        }

        return View(malzeme);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var malzeme = await _context.Malzemeler.FindAsync(id);
        if (malzeme != null)
        {
            _context.Malzemeler.Remove(malzeme);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(int? selectedDepoId = null)
    {
        var depolar = await _context.Depolar
            .OrderBy(d => d.Ad)
            .ToListAsync();
        ViewData["DepoId"] = new SelectList(depolar, "Id", "Ad", selectedDepoId);
    }

    private async Task<bool> MalzemeExists(int id)
    {
        return await _context.Malzemeler.AnyAsync(e => e.Id == id);
    }
}
