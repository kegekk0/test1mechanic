using System;
using System.Threading.Tasks;
using test1mechanic.Models;
using test1mechanic.Repositories;

namespace test1mechanic.Controllers;

using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class VisitsController : ControllerBase
{
    private readonly IVisitRepository _visitRepository;

    public VisitsController(IVisitRepository visitRepository)
    {
        _visitRepository = visitRepository;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVisit(int id)
    {
        var visitDetails = await _visitRepository.GetVisitDetailsAsync(id);
        
        if (visitDetails == null)
            return NotFound();

        return Ok(visitDetails);
    }

    [HttpPost]
    public async Task<IActionResult> AddVisit([FromBody] VisitRequest visitRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _visitRepository.VisitExistsAsync(visitRequest.VisitId))
            return Conflict("A visit with this ID already exists");

        if (!await _visitRepository.ClientExistsAsync(visitRequest.ClientId))
            return NotFound("Client not found");

        var mechanicId = await _visitRepository.GetMechanicIdByLicenceAsync(visitRequest.MechanicLicenceNumber);
        if (mechanicId == null)
            return NotFound("Mechanic not found");

        foreach (var service in visitRequest.Services)
        {
            var serviceId = await _visitRepository.GetServiceIdByNameAsync(service.ServiceName);
            if (serviceId == null)
                return NotFound("Service not found");
        }

        try
        {
            var result = await _visitRepository.AddVisitAsync(visitRequest);
            if (result)
                return CreatedAtAction(nameof(GetVisit), new { id = visitRequest.VisitId }, null);
            
            return BadRequest("Failed to add visit");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}