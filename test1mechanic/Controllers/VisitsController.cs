using Microsoft.AspNetCore.Mvc;
using test1mechanic.Models;
using VisitService = test1mechanic.Services.VisitService;

namespace test1mechanic.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisitsController : ControllerBase
{
    private readonly VisitService _visitService;

    public VisitsController(VisitService visitService)
    {
        _visitService = visitService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVisit(int id)
    {
        var visit = await _visitService.GetVisitDetailsAsync(id);
        return visit != null ? Ok(visit) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> AddVisit([FromBody] VisitRequest request)
    {
        try
        {
            await _visitService.AddVisitAsync(request);
            return CreatedAtAction(nameof(GetVisit), new { id = request.VisitId }, null);
        }
        catch (Exception ex)
        {
            return ex switch
            {
                InvalidOperationException => Conflict(ex.Message),
                KeyNotFoundException => NotFound(ex.Message),
                _ => StatusCode(500, ex.Message)
            };
        }
    }
}