using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TKBYSApp.Web.Models.Enums;

namespace TKBYSApp.Web.Models;

public class Zimmet
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int MalzemeId { get; set; }

    [Required]
    public string PersonelId { get; set; } = string.Empty;

    [Column(TypeName = "datetime2")]
    public DateTime ZimmetTarihi { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? IadeTarihi { get; set; }

    public string? OnaylayanYetkiliId { get; set; }

    [Required]
    public ZimmetDurum Durum { get; set; } = ZimmetDurum.OnayBekliyor;

    public Malzeme? Malzeme { get; set; }

    public Personel? Personel { get; set; }

    public Personel? OnaylayanYetkili { get; set; }
}
