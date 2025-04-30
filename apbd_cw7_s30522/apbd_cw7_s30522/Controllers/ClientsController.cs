using apbd_cw7_s30522.Exceptions;
using apbd_cw7_s30522.Models.DTOs;
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

    [HttpPost]
    public async Task<IActionResult> CreateClientAsync([FromBody] ClientCreateDTO body)
    {
        var client = await dbService.CreateClientAsync(body);
        return Created($"/api/clients/{client.IdClient}", client.IdClient);
    }


}