using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TKBYSApp.Web.Models.ViewModels;

public class ZimmetCreateViewModel
{
    [Required]
    [Display(Name = "Malzeme")]
    public int MalzemeId { get; set; }

    [Required]
    [Display(Name = "Personel")]
    public string PersonelId { get; set; } = string.Empty;

    public IEnumerable<SelectListItem> Malzemeler { get; set; } = Enumerable.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> Personeller { get; set; } = Enumerable.Empty<SelectListItem>();
}
