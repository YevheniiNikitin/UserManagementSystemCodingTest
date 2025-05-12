using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.Dtos;

public class AdminUpdateUserDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }
}
