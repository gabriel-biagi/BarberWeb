using System.ComponentModel.DataAnnotations;

namespace BarberWeb.Application.DTOs.Request;

public class ProfessionalRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 60 characters.")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is not a valid email address.")]
    [StringLength(60, MinimumLength = 5, ErrorMessage = "Email must be between 5 and 60 characters.")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Phone number is required.")]
    [StringLength(15, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 15 characters.")]
    public string PhoneNumber { get; set; } = string.Empty;
    
}