using System.ComponentModel.DataAnnotations;

namespace expense_tracker.Dtos;

public class RegistrationRequestDto
{
    [Required] public string Username { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    [Required] public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}