using System.Linq;
using Microsoft.EntityFrameworkCore;
using TKBYSApp.Web.Data;
using TKBYSApp.Web.Models;
using TKBYSApp.Web.Models.Enums;
using TKBYSApp.Web.Services.Interfaces;

namespace TKBYSApp.Web.Services.Implementations;

public class SatinAlmaTalepService(ApplicationDbContext context) : ISatinAlmaTalepService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<SatinAlmaTalep> CreateTalepAsync(SatinAlmaTalep talep, string personelId)
    {
        talep.TalepEdenPersonelId = personelId;
        talep.TalepTarihi = DateTime.UtcNow;
        talep.Durum = SatinAlmaTalepDurum.TalepEdildi;

        _context.SatinAlmaTalepleri.Add(talep);
        await _context.SaveChangesAsync();
        return talep;
    }

    public async Task<List<SatinAlmaTalep>> GetAllAsync()
    {
        return await _context.SatinAlmaTalepleri
            .Include(t => t.TalepEdenPersonel)
            .OrderByDescending(t => t.TalepTarihi)
            .ToListAsync();
    }

    public async Task<SatinAlmaTalep?> GetByIdAsync(int id)
    {
        return await _context.SatinAlmaTalepleri
            .Include(t => t.TalepEdenPersonel)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task MarkCompletedAsync(int talepId, string belgeYolu)
    {
        var talep = await _context.SatinAlmaTalepleri.FindAsync(talepId);
        if (talep is null)
        {
            throw new InvalidOperationException("Talep bulunamadı.");
        }

        talep.BelgeYolu = belgeYolu;
        talep.Durum = SatinAlmaTalepDurum.Tamamlandi;
        await _context.SaveChangesAsync();
    }

    public async Task ApproveAsync(int talepId)
    {
        var talep = await _context.SatinAlmaTalepleri.FindAsync(talepId);
        if (talep is null)
        {
            throw new InvalidOperationException("Talep bulunamadı.");
        }

        talep.Durum = SatinAlmaTalepDurum.Onaylandi;
        await _context.SaveChangesAsync();
    }

    public async Task RejectAsync(int talepId)
    {
        var talep = await _context.SatinAlmaTalepleri.FindAsync(talepId);
        if (talep is null)
        {
            throw new InvalidOperationException("Talep bulunamadı.");
        }

        talep.Durum = SatinAlmaTalepDurum.Reddedildi;
        await _context.SaveChangesAsync();
    }

    public async Task RouteToHarcamaYetkilisiAsync(int talepId)
    {
        var talep = await _context.SatinAlmaTalepleri.FindAsync(talepId);
        if (talep is null)
        {
            throw new InvalidOperationException("Talep bulunamadı.");
        }

        talep.Durum = SatinAlmaTalepDurum.HarcamaYetkilisiOnayinda;
        await _context.SaveChangesAsync();
    }

    public async Task StartPurchaseReviewAsync(int talepId)
    {
        var talep = await _context.SatinAlmaTalepleri.FindAsync(talepId);
        if (talep is null)
        {
            throw new InvalidOperationException("Talep bulunamadı.");
        }

        talep.Durum = SatinAlmaTalepDurum.SatinAlmaOnayinda;
        await _context.SaveChangesAsync();
    }
}
