using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TKBYSApp.Web.Models.Enums;

namespace TKBYSApp.Web.Models;

public class SatinAlmaTalep
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string TalepEdenPersonelId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string MalzemeAdi { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Gerekce { get; set; }

    [Required]
    public SatinAlmaTalepDurum Durum { get; set; } = SatinAlmaTalepDurum.TalepEdildi;

    [Column(TypeName = "datetime2")]
    public DateTime TalepTarihi { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? BelgeYolu { get; set; }

    public Personel? TalepEdenPersonel { get; set; }
}
