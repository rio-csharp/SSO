using System.ComponentModel.DataAnnotations;

namespace MySso.Web.ViewModels.Admin;

public sealed class CreateRoleViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;
}