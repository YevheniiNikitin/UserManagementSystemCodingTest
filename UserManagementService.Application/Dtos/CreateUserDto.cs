using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.Dtos;

public class CreateUserDto
{
    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }
}
