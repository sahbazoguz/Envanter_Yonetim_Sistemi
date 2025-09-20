using TKBYSApp.Web.Models;

namespace TKBYSApp.Web.Services.Interfaces;

public interface IZimmetService
{
    Task<List<Zimmet>> GetAllAsync();
    Task<Zimmet?> GetByIdAsync(int id);
    Task<Zimmet> CreateZimmetAsync(int malzemeId, string personelId);
    Task ApproveAsync(int zimmetId, string onaylayanYetkiliId);
    Task ReturnAsync(int zimmetId);
    Task DeleteAsync(int zimmetId);
}
