using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MunicipalElections.Web.Models;

public class Municipality
{
    public int MunicipalityId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Province { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Election Date")]
    public DateTime ElectionDate { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    // Name of the uploaded logo file (see ImageUrls for where it is served from).
    [StringLength(255)]
    [Display(Name = "Logo")]
    public string? LogoFileName { get; set; }

    // Name of the Identity role ("group") whose members administer this municipality, e.g. "Pitt Meadows Admin".
    // Set by the application when the municipality is created, never from a form.
    [BindNever, ValidateNever, StringLength(150)]
    public string AdminRoleName { get; set; } = string.Empty;

    public ICollection<Position> Positions { get; set; } = new List<Position>();
}
