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
        var tripsWithCountries = new List<TripCountriesGetDTO>();
        var countries = new List<CountryGetDTO>();
        
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        string queryCountries = "SELECT IdCountry, Name FROM Country";
        await using var commandCountries = new SqlCommand(queryCountries, connection);
        await using (var readerCountries = await commandCountries.ExecuteReaderAsync())
        {
            while (await readerCountries.ReadAsync())
            {
                countries.Add(new CountryGetDTO
                {
                    IdCountry = readerCountries.GetInt32(0),
                    Name = readerCountries.GetString(1)
                });
            }
        }
        
        string queryTrips = "SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople FROM Trip";
        await using var commandTrips = new SqlCommand(queryTrips, connection);
        await using (var readerTrips = await commandTrips.ExecuteReaderAsync())
        {
            while (await readerTrips.ReadAsync())
            {
                tripsWithCountries.Add(new TripCountriesGetDTO
                {
                    IdTrip = readerTrips.GetInt32(0),
                    Name = readerTrips.GetString(1),
                    Description = readerTrips.GetString(2),
                    DateFrom = readerTrips.GetDateTime(3),
                    DateTo = readerTrips.GetDateTime(4),
                    MaxPeople = readerTrips.GetInt32(5),
                    Countries = new List<CountryGetDTO>()
                });
            }
        }
        
        string queryCountriesTrips = "SELECT IdCountry, IdTrip FROM Country_Trip";
        await using var commandCountriesTrips = new SqlCommand(queryCountriesTrips, connection);
        await using (var readerCountriesTrips = await commandCountriesTrips.ExecuteReaderAsync())
        {

            while (await readerCountriesTrips.ReadAsync())
            {
                int idCountry = readerCountriesTrips.GetInt32(0);
                int idTrip = readerCountriesTrips.GetInt32(1);

                tripsWithCountries.Find(t => t.IdTrip == idTrip)
                    .Countries.Add(countries.Find(c => c.IdCountry == idCountry));
            }
        }
        
        return tripsWithCountries;
    }
     
}