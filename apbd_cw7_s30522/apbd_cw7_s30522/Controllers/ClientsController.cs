using apbd_cw7_s30522.Exceptions;
using apbd_cw7_s30522.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd_cw7_s30522.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ClientsController(IDbService dbService) : ControllerBase
{

    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientWithTripsAsync([FromRoute] int id)
    {
        try
        {
            return Ok(await dbService.GetClientTripsAsync(id));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
}