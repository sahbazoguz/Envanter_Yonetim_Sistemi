using Microsoft.EntityFrameworkCore;
using TKBYSApp.Web.Data;
using TKBYSApp.Web.Models;
using TKBYSApp.Web.Models.Enums;
using TKBYSApp.Web.Services.Interfaces;

namespace TKBYSApp.Web.Services.Implementations;

public class ZimmetService(ApplicationDbContext context) : IZimmetService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Zimmet> CreateZimmetAsync(int malzemeId, string personelId)
    {
        var malzeme = await _context.Malzemeler.FindAsync(malzemeId)
            ?? throw new InvalidOperationException("Malzeme bulunamadı.");

        if (malzeme.Durum != MalzemeDurum.Depoda)
        {
            throw new InvalidOperationException("Malzeme zimmete uygun durumda değil.");
        }

        var personel = await _context.Users.FindAsync(personelId)
            ?? throw new InvalidOperationException("Personel bulunamadı.");
        _ = personel;

        var zimmet = new Zimmet
        {
            MalzemeId = malzemeId,
            PersonelId = personelId,
            ZimmetTarihi = DateTime.UtcNow,
            Durum = ZimmetDurum.OnayBekliyor
        };

        _context.Zimmetler.Add(zimmet);
        await _context.SaveChangesAsync();
        return zimmet;
    }

    public async Task ApproveAsync(int zimmetId, string onaylayanYetkiliId)
    {
        var zimmet = await _context.Zimmetler
            .Include(z => z.Malzeme)
            .FirstOrDefaultAsync(z => z.Id == zimmetId)
            ?? throw new InvalidOperationException("Zimmet kaydı bulunamadı.");

        if (zimmet.Durum != ZimmetDurum.OnayBekliyor)
        {
            throw new InvalidOperationException("Zimmet kaydı zaten işlem görmüş.");
        }

        zimmet.Durum = ZimmetDurum.Aktif;
        zimmet.OnaylayanYetkiliId = onaylayanYetkiliId;
        zimmet.Malzeme!.Durum = MalzemeDurum.Zimmetli;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int zimmetId)
    {
        var zimmet = await _context.Zimmetler.FindAsync(zimmetId)
            ?? throw new InvalidOperationException("Zimmet kaydı bulunamadı.");

        if (zimmet.Durum != ZimmetDurum.OnayBekliyor)
        {
            throw new InvalidOperationException("Sadece onay bekleyen zimmetler silinebilir.");
        }

        _context.Zimmetler.Remove(zimmet);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Zimmet>> GetAllAsync()
    {
        return await _context.Zimmetler
            .Include(z => z.Malzeme)
            .Include(z => z.Personel)
            .Include(z => z.OnaylayanYetkili)
            .OrderByDescending(z => z.ZimmetTarihi)
            .ToListAsync();
    }

    public async Task<Zimmet?> GetByIdAsync(int id)
    {
        return await _context.Zimmetler
            .Include(z => z.Malzeme)
            .Include(z => z.Personel)
            .Include(z => z.OnaylayanYetkili)
            .FirstOrDefaultAsync(z => z.Id == id);
    }

    public async Task ReturnAsync(int zimmetId)
    {
        var zimmet = await _context.Zimmetler
            .Include(z => z.Malzeme)
            .FirstOrDefaultAsync(z => z.Id == zimmetId)
            ?? throw new InvalidOperationException("Zimmet kaydı bulunamadı.");

        if (zimmet.Durum != ZimmetDurum.Aktif)
        {
            throw new InvalidOperationException("Sadece aktif zimmetler iade edilebilir.");
        }

        zimmet.Durum = ZimmetDurum.IadeEdildi;
        zimmet.IadeTarihi = DateTime.UtcNow;
        if (zimmet.Malzeme is not null)
        {
            zimmet.Malzeme.Durum = MalzemeDurum.Depoda;
        }

        await _context.SaveChangesAsync();
    }
}
