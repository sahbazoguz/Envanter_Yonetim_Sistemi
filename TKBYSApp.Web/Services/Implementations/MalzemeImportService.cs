using System.Globalization;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TKBYSApp.Web.Data;
using TKBYSApp.Web.Models;
using TKBYSApp.Web.Models.Enums;
using TKBYSApp.Web.Services.Interfaces;

namespace TKBYSApp.Web.Services.Implementations;

public class MalzemeImportService : IMalzemeImportService
{
    private static readonly HashSet<string> RequiredHeaders =
    [
        "bar_kod",
        "aciklama",
        "cinsi",
        "ekozellik",
        "markaadi",
        "modeli",
        "olcuadi",
        "sicil_no",
        "seri_no",
        "fis_son_durum",
        "verildigi_yer_birim",
        "tc_numarasi",
        "fis_no",
        "tarih",
        "ambar_adi",
        "kurum_giris_tarihi"
    ];

    private readonly ApplicationDbContext _context;
    private readonly ILogger<MalzemeImportService> _logger;

    static MalzemeImportService()
    {
        System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public MalzemeImportService(ApplicationDbContext context, ILogger<MalzemeImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MalzemeImportResult> ImportAsync(IFormFile excelFile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(excelFile);

        if (excelFile.Length == 0)
        {
            throw new InvalidOperationException("Yüklenen dosya boş.");
        }

        await using var memoryStream = new MemoryStream();
        await excelFile.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        using var reader = ExcelReaderFactory.CreateReader(memoryStream);

        var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var rows = new List<MalzemeImportRow>();
        var errors = new List<string>();
        var rowIndex = 0;

        do
        {
            while (reader.Read())
            {
                rowIndex++;

                if (rowIndex == 1)
                {
                    headerMap = BuildHeaderMap(reader);
                    var missingHeaders = RequiredHeaders
                        .Where(header => !headerMap.ContainsKey(header))
                        .ToArray();

                    if (missingHeaders.Length > 0)
                    {
                        throw new InvalidOperationException($"Excel dosyasında eksik başlık(lar) var: {string.Join(", ", missingHeaders)}");
                    }

                    continue;
                }

                if (IsRowEmpty(reader))
                {
                    continue;
                }

                try
                {
                    var row = CreateRow(reader, headerMap, rowIndex);
                    rows.Add(row);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Excel satırı işlenirken hata oluştu. Satır: {RowIndex}", rowIndex);
                    errors.Add($"Satır {rowIndex}: {ex.Message}");
                }
            }
        } while (reader.NextResult());

        if (rows.Count == 0)
        {
            return new MalzemeImportResult(0, 0, errors);
        }

        var barKodlar = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.BarKod))
            .Select(r => r.BarKod!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var mevcutMalzemeler = await _context.Malzemeler
            .Where(m => m.BarKod != null && barKodlar.Contains(m.BarKod))
            .ToDictionaryAsync(m => m.BarKod!, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var ambarAdlari = rows
            .Select(r => r.AmbarAdi)
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var mevcutDepolar = await _context.Depolar
            .Where(d => ambarAdlari.Contains(d.Ad))
            .ToDictionaryAsync(d => d.Ad, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var defaultDepo = await EnsureDefaultDepoAsync(cancellationToken);

        var created = 0;
        var updated = 0;

        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.BarKod))
            {
                errors.Add($"Satır {row.RowNumber}: barkod değeri bulunamadı.");
                continue;
            }

            var depo = ResolveDepo(row.AmbarAdi, mevcutDepolar, defaultDepo);

            Malzeme malzeme;
            var hasExisting = mevcutMalzemeler.TryGetValue(row.BarKod!, out malzeme!);

            if (!hasExisting)
            {
                malzeme = new Malzeme
                {
                    BarKod = row.BarKod,
                    Ad = DetermineMalzemeAdi(row),
                    Adet = 1,
                    Durum = DetermineMalzemeDurumu(row.FisSonDurum),
                    KayitTarihi = DateTime.UtcNow
                };
                ApplyRowToEntity(row, malzeme, depo);
                await _context.Malzemeler.AddAsync(malzeme, cancellationToken);
                mevcutMalzemeler[row.BarKod!] = malzeme;
                created++;
            }
            else
            {
                ApplyRowToEntity(row, malzeme, depo);
                malzeme.Durum = DetermineMalzemeDurumu(row.FisSonDurum);
                malzeme.Ad = DetermineMalzemeAdi(row) ?? malzeme.Ad;
                mevcutMalzemeler[row.BarKod!] = malzeme;
                updated++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new MalzemeImportResult(created, updated, errors);
    }

    private static Dictionary<string, int> BuildHeaderMap(IExcelDataReader reader)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < reader.FieldCount; i++)
        {
            var headerValue = reader.GetValue(i)?.ToString();
            if (string.IsNullOrWhiteSpace(headerValue))
            {
                continue;
            }

            map[NormalizeHeader(headerValue)] = i;
        }

        return map;
    }

    private static bool IsRowEmpty(IExcelDataReader reader)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetValue(i) is { } value && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                return false;
            }
        }

        return true;
    }

    private static MalzemeImportRow CreateRow(IExcelDataReader reader, IReadOnlyDictionary<string, int> headerMap, int rowNumber)
    {
        string? GetString(string header)
        {
            var raw = TryGetRawValue(headerMap, reader, header);
            return raw?.ToString()?.Trim();
        }

        DateTime? GetDate(string header)
        {
            var rawValue = TryGetRawValue(headerMap, reader, header);
            if (rawValue is null)
            {
                return null;
            }

            if (rawValue is DateTime dt)
            {
                return dt;
            }

            if (rawValue is double oa)
            {
                return DateTime.FromOADate(oa);
            }

            var value = rawValue.ToString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var parsed))
            {
                return DateTime.SpecifyKind(parsed, DateTimeKind.Local);
            }

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var oaDate))
            {
                return DateTime.FromOADate(oaDate);
            }

            throw new FormatException($"'{header}' değeri tarih formatına çevrilemedi: {value}");
        }

        return new MalzemeImportRow
        {
            RowNumber = rowNumber,
            BarKod = GetString("bar_kod"),
            Aciklama = GetString("aciklama"),
            Cinsi = GetString("cinsi"),
            EkOzellik = GetString("ekozellik"),
            MarkaAdi = GetString("markaadi"),
            Modeli = GetString("modeli"),
            OlcuAdi = GetString("olcuadi"),
            SicilNo = GetString("sicil_no"),
            SeriNo = GetString("seri_no"),
            FisSonDurum = GetString("fis_son_durum"),
            VerildigiYerBirim = GetString("verildigi_yer_birim"),
            TcNumarasi = GetString("tc_numarasi"),
            FisNo = GetString("fis_no"),
            Tarih = GetDate("tarih"),
            AmbarAdi = GetString("ambar_adi"),
            KurumGirisTarihi = GetDate("kurum_giris_tarihi")
        };
    }

    private async Task<Depo> EnsureDefaultDepoAsync(CancellationToken cancellationToken)
    {
        var depo = await _context.Depolar
            .OrderBy(d => d.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (depo != null)
        {
            return depo;
        }

        depo = new Depo
        {
            Ad = "Genel Ambar",
            Aciklama = "Excel importu için varsayılan depo"
        };

        await _context.Depolar.AddAsync(depo, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return depo;
    }

    private Depo ResolveDepo(string? ambarAdi, IDictionary<string, Depo> mevcutDepolar, Depo defaultDepo)
    {
        if (string.IsNullOrWhiteSpace(ambarAdi))
        {
            return defaultDepo;
        }

        if (mevcutDepolar.TryGetValue(ambarAdi, out var depo))
        {
            return depo;
        }

        depo = new Depo
        {
            Ad = ambarAdi,
            Aciklama = "Excel importu ile oluşturuldu"
        };

        _context.Depolar.Add(depo);
        mevcutDepolar[ambarAdi] = depo;

        return depo;
    }

    private static string? DetermineMalzemeAdi(MalzemeImportRow row)
    {
        if (!string.IsNullOrWhiteSpace(row.Cinsi))
        {
            return row.Cinsi;
        }

        if (!string.IsNullOrWhiteSpace(row.Aciklama))
        {
            return row.Aciklama;
        }

        return row.BarKod;
    }

    private static void ApplyRowToEntity(MalzemeImportRow row, Malzeme malzeme, Depo depo)
    {
        malzeme.BarKod = row.BarKod;
        malzeme.Aciklama = row.Aciklama;
        malzeme.Cinsi = row.Cinsi;
        malzeme.EkOzellik = row.EkOzellik;
        malzeme.MarkaAdi = row.MarkaAdi;
        malzeme.Modeli = row.Modeli;
        malzeme.OlcuAdi = row.OlcuAdi;
        malzeme.SicilNo = row.SicilNo;
        malzeme.SeriNo = row.SeriNo;
        malzeme.FisSonDurum = row.FisSonDurum;
        malzeme.VerildigiYerBirim = row.VerildigiYerBirim;
        malzeme.TcNumarasi = row.TcNumarasi;
        malzeme.FisNo = row.FisNo;
        malzeme.Tarih = row.Tarih;
        malzeme.AmbarAdi = row.AmbarAdi;
        malzeme.KurumGirisTarihi = row.KurumGirisTarihi;
        malzeme.Depo = depo;
        if (depo.Id != 0)
        {
            malzeme.DepoId = depo.Id;
        }
    }

    private static MalzemeDurum DetermineMalzemeDurumu(string? fisSonDurum)
    {
        if (string.IsNullOrWhiteSpace(fisSonDurum))
        {
            return MalzemeDurum.Depoda;
        }

        var normalized = fisSonDurum.Trim().ToLowerInvariant();

        if (normalized.Contains("zimmet"))
        {
            return MalzemeDurum.Zimmetli;
        }

        if (normalized.Contains("hurda"))
        {
            return MalzemeDurum.Hurda;
        }

        if (normalized.Contains("birim dışı") || normalized.Contains("birim disi"))
        {
            return MalzemeDurum.BirimDisi;
        }

        return MalzemeDurum.Depoda;
    }

    private static object? TryGetRawValue(IReadOnlyDictionary<string, int> headerMap, IExcelDataReader reader, string header)
    {
        if (!headerMap.TryGetValue(NormalizeHeader(header), out var index))
        {
            return null;
        }

        return reader.GetValue(index);
    }

    private static string NormalizeHeader(string header)
    {
        return header
            .Trim()
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .ToLowerInvariant();
    }

    private sealed class MalzemeImportRow
    {
        public int RowNumber { get; init; }
        public string? BarKod { get; init; }
        public string? Aciklama { get; init; }
        public string? Cinsi { get; init; }
        public string? EkOzellik { get; init; }
        public string? MarkaAdi { get; init; }
        public string? Modeli { get; init; }
        public string? OlcuAdi { get; init; }
        public string? SicilNo { get; init; }
        public string? SeriNo { get; init; }
        public string? FisSonDurum { get; init; }
        public string? VerildigiYerBirim { get; init; }
        public string? TcNumarasi { get; init; }
        public string? FisNo { get; init; }
        public DateTime? Tarih { get; init; }
        public string? AmbarAdi { get; init; }
        public DateTime? KurumGirisTarihi { get; init; }
    }
}
