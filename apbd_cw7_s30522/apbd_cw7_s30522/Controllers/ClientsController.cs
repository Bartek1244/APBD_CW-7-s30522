﻿using apbd_cw7_s30522.Exceptions;
using apbd_cw7_s30522.Models.DTOs;
using apbd_cw7_s30522.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd_cw7_s30522.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ClientsController(IDbService dbService) : ControllerBase
{

    //Ten endpoint będzie pobierał wszystkie wycieczki powiązane z konkretnym klientem.
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

    //Ten endpoint utworzy nowy rekord klienta.
    [HttpPost]
    public async Task<IActionResult> CreateClientAsync([FromBody] ClientCreateDTO body)
    {
        var client = await dbService.CreateClientAsync(body);
        return Created($"/api/clients/{client.IdClient}", client.IdClient);
    }

    //Ten endpoint zarejestruje klienta na konkretną wycieczkę.
    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClientToTripAsync([FromRoute] int id, [FromRoute] int tripId)
    {
        try
        {
            return Ok(await dbService.RegisterClientToTripAsync(id, tripId));
        }
        catch (NotFoundException eNotFound)
        {
            return NotFound(eNotFound.Message);
        }
        catch (ConflictException eConflict)
        {
            return Conflict(eConflict.Message);
        }
    }

    //Ten endpoint usunie rejestrację klienta z wycieczki.
    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> DeleteClientTripRegistrationAsync([FromRoute] int id, [FromRoute] int tripId)
    {
        try
        {
            await dbService.DeleteClientTripRegistrationAsync(id, tripId);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
}