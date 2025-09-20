using System.ComponentModel.DataAnnotations;

namespace TKBYSApp.Web.Models;

public class Depo
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Ad { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Aciklama { get; set; }

    public ICollection<Malzeme> Malzemeler { get; set; } = new List<Malzeme>();
}
