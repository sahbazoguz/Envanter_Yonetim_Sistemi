using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TKBYSApp.Web.Models;

public class Personel : IdentityUser
{
    [Required]
    [MaxLength(200)]
    public string AdSoyad { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string SicilNo { get; set; } = string.Empty;

    public ICollection<Zimmet> Zimmetler { get; set; } = new List<Zimmet>();

    public ICollection<SatinAlmaTalep> SatinAlmaTalepleri { get; set; } = new List<SatinAlmaTalep>();
}
