using System.ComponentModel.DataAnnotations;
using BarberWeb.Api.Extensions;
using BarberWeb.Application.DTOs.Request;
using BarberWeb.Application.DTOs.Response;
using BarberWeb.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BarberWeb.Api.Controllers;

/// <summary>
/// Catálogo de serviços. Não há exclusão por enquanto: a política de exclusão de Service ainda está pendente.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ServiceController : ControllerBase
{
    private readonly ICatalogServiceService _service;

    public ServiceController(ICatalogServiceService service)
    {
        _service = service;
    }

    [HttpGet("{id:int}", Name = "GetServiceByIdAsync")]
    [ProducesResponseType(typeof(CatalogServiceResponse), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(List<CatalogServiceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var response = await _service.GetAllAsync();

        if (response.IsFailure)
            return this.ToProblem(response);

        return Ok(response.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CatalogServiceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PostAsync([FromBody] CatalogServiceRequest request)
    {
        var response = await _service.CreateAsync(request);

        if (response.IsFailure)
            return this.ToProblem(response);

        return CreatedAtRoute("GetServiceByIdAsync", new { id = response.Value.Id }, response.Value);
    }

    /// <summary>Substitui nome e descrição do serviço (ambos obrigatórios).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CatalogServiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutAsync(
        [Range(1, int.MaxValue, ErrorMessage = "Id must be greater than zero.")] int id,
        [FromBody] CatalogServiceRequest request)
    {
        var response = await _service.UpdateAsync(id, request);

        if (response.IsFailure)
            return this.ToProblem(response);

        return Ok(response.Value);
    }
}
