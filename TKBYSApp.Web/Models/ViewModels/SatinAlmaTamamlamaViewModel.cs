using System.ComponentModel.DataAnnotations;

namespace TKBYSApp.Web.Models.ViewModels;

public class SatinAlmaTamamlamaViewModel
{
    public int Id { get; set; }

    [Display(Name = "Belge Yolu")]
    [MaxLength(500)]
    public string? BelgeYolu { get; set; }
}
