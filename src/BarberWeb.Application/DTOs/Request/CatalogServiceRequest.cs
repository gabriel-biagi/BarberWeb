using System.ComponentModel.DataAnnotations;

namespace BarberWeb.Application.DTOs.Request;

/// <summary>Dados para criar ou substituir (PUT) um serviço do catálogo.</summary>
public class CatalogServiceRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 60 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 100 characters.")]
    public string Description { get; set; } = string.Empty;
}
