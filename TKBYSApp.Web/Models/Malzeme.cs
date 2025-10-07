using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TKBYSApp.Web.Models.Enums;

namespace TKBYSApp.Web.Models;

public class Malzeme
{
    [Key]
    public int Id { get; set; }

    [MaxLength(100)]
    public string? TKBYSNo { get; set; }

    [Required]
    [MaxLength(200)]
    public string Ad { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Ozellikler { get; set; }

    [Range(0, int.MaxValue)]
    public int Adet { get; set; }

    [MaxLength(500)]
    public string? Aciklama { get; set; }

    [MaxLength(200)]
    public string? BarKod { get; set; }

    [MaxLength(200)]
    public string? Cinsi { get; set; }

    [MaxLength(500)]
    public string? EkOzellik { get; set; }

    [MaxLength(200)]
    public string? MarkaAdi { get; set; }

    [MaxLength(200)]
    public string? Modeli { get; set; }

    [MaxLength(200)]
    public string? OlcuAdi { get; set; }

    [MaxLength(100)]
    public string? SicilNo { get; set; }

    [MaxLength(200)]
    public string? SeriNo { get; set; }

    [MaxLength(200)]
    public string? FisSonDurum { get; set; }

    [MaxLength(200)]
    public string? VerildigiYerBirim { get; set; }

    [MaxLength(11)]
    public string? TcNumarasi { get; set; }

    [MaxLength(100)]
    public string? FisNo { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? Tarih { get; set; }

    [MaxLength(200)]
    public string? AmbarAdi { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? KurumGirisTarihi { get; set; }

    [Required]
    public int DepoId { get; set; }

    [Required]
    public MalzemeDurum Durum { get; set; } = MalzemeDurum.Depoda;

    [Column(TypeName = "datetime2")]
    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;

    public Depo? Depo { get; set; }

    public ICollection<Zimmet> Zimmetler { get; set; } = new List<Zimmet>();
}
