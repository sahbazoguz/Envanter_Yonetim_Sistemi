using TKBYSApp.Web.Models;

namespace TKBYSApp.Web.Services.Interfaces;

public interface ISatinAlmaTalepService
{
    Task<List<SatinAlmaTalep>> GetAllAsync();
    Task<SatinAlmaTalep?> GetByIdAsync(int id);
    Task<SatinAlmaTalep> CreateTalepAsync(SatinAlmaTalep talep, string personelId);
    Task StartPurchaseReviewAsync(int talepId);
    Task RouteToHarcamaYetkilisiAsync(int talepId);
    Task ApproveAsync(int talepId);
    Task RejectAsync(int talepId);
    Task MarkCompletedAsync(int talepId, string belgeYolu);
}
