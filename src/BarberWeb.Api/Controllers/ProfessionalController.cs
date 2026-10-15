using BarberWeb.Api.Middlewares;
using BarberWeb.Application.DTOs.Response;
using BarberWeb.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BarberWeb.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProfessionalController : ControllerBase
{
    private readonly IProfessionalService _service;

    public ProfessionalController(IProfessionalService service)
    {
        _service = service;
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProfessionalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var professional = await _service.GetByIdAsync(id);
        return Ok(professional);
    }
}