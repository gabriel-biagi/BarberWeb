using BarberWeb.Api.Middlewares;
using BarberWeb.Application.DTOs.Request;
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
    
    [HttpGet("{id:int}", Name = "GetByIdAsync")]
    [ProducesResponseType(typeof(ProfessionalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var professional = await _service.GetByIdAsync(id);
        return Ok(professional);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ProfessionalResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var professionals = await _service.GetAllAsync();
        return Ok(professionals);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProfessionalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PostAsync([FromBody] ProfessionalRequest professionalRequest)
    {
        var response = await _service.CreateAsync(professionalRequest);
        return CreatedAtRoute("GetByIdAsync", new { id = response.Id }, response);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}