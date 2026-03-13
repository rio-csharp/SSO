using System.ComponentModel.DataAnnotations;

namespace MySso.Web.ViewModels.Admin;

public sealed class CreateLocalUserViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string GivenName { get; set; } = string.Empty;

    [Required]
    public string FamilyName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(12)]
    public string Password { get; set; } = string.Empty;

    public bool AssignAdministratorRole { get; set; }
}