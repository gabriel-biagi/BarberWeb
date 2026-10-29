using System.ComponentModel.DataAnnotations;
using BarberWeb.Api.Extensions;
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
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync([Range(1, int.MaxValue, ErrorMessage = "Id must be greater than zero.")] int id)
    {
        var response = await _service.GetByIdAsync(id);

        if (response.IsFailure)
            return this.ToProblem(response);

        return Ok(response.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ProfessionalResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var response = await _service.GetAllAsync();

        if (response.IsFailure)
            return this.ToProblem(response);

        return Ok(response.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProfessionalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PostAsync([FromBody] ProfessionalRequest professionalRequest)
    {
        var response = await _service.CreateAsync(professionalRequest);

        if (response.IsFailure)
            return this.ToProblem(response);

        return CreatedAtRoute("GetByIdAsync", new { id = response.Value.Id }, response.Value);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync([Range(1, int.MaxValue, ErrorMessage = "Id must be greater than zero.")] int id)
    {
        var response = await _service.DeleteAsync(id);

        if (response.IsFailure)
            return this.ToProblem(response);

        return NoContent();
    }
}
