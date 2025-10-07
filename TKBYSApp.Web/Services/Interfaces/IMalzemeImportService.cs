using Microsoft.AspNetCore.Http;

namespace TKBYSApp.Web.Services.Interfaces;

public interface IMalzemeImportService
{
    Task<MalzemeImportResult> ImportAsync(IFormFile excelFile, CancellationToken cancellationToken = default);
}

public sealed record MalzemeImportResult(int CreatedCount, int UpdatedCount, IReadOnlyCollection<string> Errors);
