using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TKBYSApp.Web.Data;
using TKBYSApp.Web.Models;

namespace TKBYSApp.Web.Controllers;

[Authorize]
public class DepolarController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index()
    {
        return View(await _context.Depolar.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var depo = await _context.Depolar.FirstOrDefaultAsync(m => m.Id == id);
        if (depo == null)
        {
            return NotFound();
        }

        return View(depo);
    }

    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> Create([Bind("Id,Ad,Aciklama")] Depo depo)
    {
        if (ModelState.IsValid)
        {
            _context.Add(depo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(depo);
    }

    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var depo = await _context.Depolar.FindAsync(id);
        if (depo == null)
        {
            return NotFound();
        }
        return View(depo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Aciklama")] Depo depo)
    {
        if (id != depo.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(depo);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await DepoExists(depo.Id))
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
        return View(depo);
    }

    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var depo = await _context.Depolar.FirstOrDefaultAsync(m => m.Id == id);
        if (depo == null)
        {
            return NotFound();
        }

        return View(depo);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var depo = await _context.Depolar.FindAsync(id);
        if (depo != null)
        {
            _context.Depolar.Remove(depo);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> DepoExists(int id)
    {
        return await _context.Depolar.AnyAsync(e => e.Id == id);
    }
}
