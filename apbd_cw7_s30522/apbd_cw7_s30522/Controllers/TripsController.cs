using apbd_cw7_s30522.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd_cw7_s30522.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class TripsController(IDbService dbService) : ControllerBase
{
    //Ten endpoint będzie pobierał wszystkie dostępne wycieczki wraz z ich podstawowymi informacjami.
    [HttpGet]
    public async Task<IActionResult> GetAllTripsWithCountriesAsync()
    {
        return Ok(await dbService.GetAllTripsWithCountriesAsync());
    }
}