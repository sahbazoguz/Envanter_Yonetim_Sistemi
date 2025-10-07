using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TKBYSApp.Web.Data;
using TKBYSApp.Web.Models;
using TKBYSApp.Web.Models.Enums;
using TKBYSApp.Web.Services.Interfaces;

namespace TKBYSApp.Web.Controllers;

[Authorize]
public class MalzemelerController(ApplicationDbContext context, IMalzemeImportService importService) : Controller
{
    private readonly ApplicationDbContext _context = context;
    private readonly IMalzemeImportService _importService = importService;

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
    public async Task<IActionResult> Create([Bind("Id,TKBYSNo,Ad,Ozellikler,Adet,Aciklama,BarKod,Cinsi,EkOzellik,MarkaAdi,Modeli,OlcuAdi,SicilNo,SeriNo,FisSonDurum,VerildigiYerBirim,TcNumarasi,FisNo,Tarih,AmbarAdi,KurumGirisTarihi,DepoId,Durum")] Malzeme malzeme)
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
    public async Task<IActionResult> Edit(int id, [Bind("Id,TKBYSNo,Ad,Ozellikler,Adet,Aciklama,BarKod,Cinsi,EkOzellik,MarkaAdi,Modeli,OlcuAdi,SicilNo,SeriNo,FisSonDurum,VerildigiYerBirim,TcNumarasi,FisNo,Tarih,AmbarAdi,KurumGirisTarihi,DepoId,Durum,KayitTarihi")] Malzeme malzeme)
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

    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public IActionResult Import()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TasinirKayitYetkilisi}")]
    public async Task<IActionResult> Import(IFormFile? excelFile, CancellationToken cancellationToken)
    {
        if (excelFile == null)
        {
            ModelState.AddModelError(nameof(excelFile), "Lütfen yüklemek için bir Excel dosyası seçin.");
            return View();
        }

        try
        {
            var result = await _importService.ImportAsync(excelFile, cancellationToken);

            TempData["StatusMessage"] = $"Excel importu tamamlandı. Yeni kayıt: {result.CreatedCount}, güncellenen kayıt: {result.UpdatedCount}.";

            if (result.Errors.Count > 0)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Errors);
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
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
