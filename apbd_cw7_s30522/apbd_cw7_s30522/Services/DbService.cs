using apbd_cw7_s30522.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace apbd_cw7_s30522.Services;

public interface IDbService
{
    public Task<IEnumerable<TripCountriesGetDTO>> GetAllTripsWithCountriesAsync();
}

public class DbService(IConfiguration config) : IDbService
{
    private readonly string _connectionString = config.GetConnectionString("Default");

    public async Task<IEnumerable<TripCountriesGetDTO>> GetAllTripsWithCountriesAsync()
    {
        var result = new List<TripCountriesGetDTO>();
        
        await using var connection = new SqlConnection(_connectionString);

        string queryIdTrip = "SELECT IdTrip FROM Country_Trip";
        await using var commandIdTrip = new SqlCommand(queryIdTrip, connection);

        await connection.OpenAsync();
        await using var readerIdTrip = await commandIdTrip.ExecuteReaderAsync();

        while (await readerIdTrip.ReadAsync())
        {
            int idTrip = readerIdTrip.GetInt32(0);

            string queryTrip = "SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople " +
                               "FROM Trip " +
                               $"WHERE IdTrip = {idTrip} ";

            await using var commandTrip = new SqlCommand(queryTrip, connection);
            await using var readerTrip = await commandTrip.ExecuteReaderAsync();
            await readerTrip.ReadAsync();

            var tripCountriesDTO = new TripCountriesGetDTO
            {
                IdTrip = readerTrip.GetInt32(0),
                Name = readerTrip.GetString(1),
                Description = readerTrip.GetString(2),
                DateFrom = readerTrip.GetDateTime(3),
                DateTo = readerTrip.GetDateTime(4),
                MaxPeople = readerTrip.GetInt32(5),
                Countries = new List<CountryGetDTO>()
            };

            string queryCountry = "SELECT c.IdCountry, c.Name " +
                                  "FROM Country c " +
                                  "JOIN Country_Trip c_t ON c.IdCountry = c_t.IdCountry " +
                                  $"WHERE c_t.IdTrip = {idTrip}";

            await using var commandCountry = new SqlCommand(queryCountry, connection);
            await using var readerCountry = await commandCountry.ExecuteReaderAsync();

            while (await readerCountry.ReadAsync())
            {
                tripCountriesDTO.Countries.Add(new CountryGetDTO
                {
                    IdCountry = readerCountry.GetInt32(0),
                    Name = readerCountry.GetString(1)
                });
            }
            
            result.Add(tripCountriesDTO);
        }

        return result;
    }
}