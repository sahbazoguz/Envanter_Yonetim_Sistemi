using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TKBYSApp.Web.Models.ViewModels;

public class PersonelFormViewModel
{
    public string? Id { get; set; }

    [Required]
    [Display(Name = "Ad Soyad")]
    [MaxLength(200)]
    public string AdSoyad { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Sicil No")]
    [MaxLength(50)]
    public string SicilNo { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Parola")]
    public string? Password { get; set; }

    public IList<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();

    [Display(Name = "Roller")]
    public IList<string> SelectedRoles { get; set; } = new List<string>();

    public bool RequirePassword => string.IsNullOrEmpty(Id);
}
