using System.Data.Common;
using apbd_cw7_s30522.Exceptions;
using apbd_cw7_s30522.Models;
using apbd_cw7_s30522.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace apbd_cw7_s30522.Services;

public interface IDbService
{
    public Task<IEnumerable<TripCountriesGetDTO>> GetAllTripsWithCountriesAsync();
    public Task<IEnumerable<ClientTripDetailsGetDTO>> GetClientTripsAsync(int id);
    public Task<Client> CreateClientAsync(ClientCreateDTO client);
    public Task<ClientTripRegistrationDTO> RegisterClientToTripAsync(int idClient, int idTrip);
    public Task<bool> DeleteClientTripRegistrationAsync(int idClient, int idTrip);
}

public class DbService(IConfiguration config) : IDbService
{
    private readonly string _connectionString = config.GetConnectionString("Default");

    public async Task<IEnumerable<TripCountriesGetDTO>> GetAllTripsWithCountriesAsync()
    {
        var tripsWithCountriesDict = new Dictionary<int, TripCountriesGetDTO>();
        var countriesDict = new Dictionary<int, CountryGetDTO>();
        
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var queryCountries = "SELECT IdCountry, Name FROM Country";
        await using var commandCountries = new SqlCommand(queryCountries, connection);
        await using (var readerCountries = await commandCountries.ExecuteReaderAsync())
        {
            while (await readerCountries.ReadAsync())
            {
                var country = new CountryGetDTO
                {
                    IdCountry = readerCountries.GetInt32(0),
                    Name = readerCountries.GetString(1)
                };
                
                countriesDict.Add(country.IdCountry, country);
            }
        }
        
        var queryTrips = "SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople FROM Trip";
        await using var commandTrips = new SqlCommand(queryTrips, connection);
        await using (var readerTrips = await commandTrips.ExecuteReaderAsync())
        {
            while (await readerTrips.ReadAsync())
            {
                var tripWithCountries = new TripCountriesGetDTO
                {
                    IdTrip = readerTrips.GetInt32(0),
                    Name = readerTrips.GetString(1),
                    Description = readerTrips.GetString(2),
                    DateFrom = readerTrips.GetDateTime(3),
                    DateTo = readerTrips.GetDateTime(4),
                    MaxPeople = readerTrips.GetInt32(5),
                    Countries = new List<CountryGetDTO>()
                };
                
                tripsWithCountriesDict.Add(tripWithCountries.IdTrip, tripWithCountries);
            }
        }
        
        var queryCountriesTrips = "SELECT IdCountry, IdTrip FROM Country_Trip";
        await using var commandCountriesTrips = new SqlCommand(queryCountriesTrips, connection);
        await using (var readerCountriesTrips = await commandCountriesTrips.ExecuteReaderAsync())
        {

            while (await readerCountriesTrips.ReadAsync())
            {
                int idCountry = readerCountriesTrips.GetInt32(0);
                int idTrip = readerCountriesTrips.GetInt32(1);

                tripsWithCountriesDict[idTrip].Countries.Add(countriesDict[idCountry]);
            }
        }
        
        return tripsWithCountriesDict.Values;
    }

    public async Task<IEnumerable<ClientTripDetailsGetDTO>> GetClientTripsAsync(int id)
    {
        var clientTripsDict = new Dictionary<int, List<ClientTripDetailsGetDTO>>();
        
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var queryClient = "SELECT * FROM Client WHERE IdClient = @idClient";
        await using var commandClient = new SqlCommand(queryClient, connection);
        commandClient.Parameters.AddWithValue("@idClient", id);
        await using (var readerClient = await commandClient.ExecuteReaderAsync())
        {
            if (!await readerClient.ReadAsync())
            {
                throw new NotFoundException($"Client of id {id} does not exist.");
            }
        }

        var queryClientTrips = "SELECT IdTrip, RegisteredAt, PaymentDate FROM Client_Trip WHERE IdClient = @idClient";
        await using var commandClientTrips = new SqlCommand(queryClientTrips, connection);
        commandClientTrips.Parameters.AddWithValue("@idClient", id);
        await using (var readerClientTrips = await commandClientTrips.ExecuteReaderAsync())
        {
            while (await readerClientTrips.ReadAsync())
            {
                var clientTrip = new ClientTripDetailsGetDTO
                {
                    IdTrip = readerClientTrips.GetInt32(0),
                    RegisteredAt = readerClientTrips.GetInt32(1),
                    PaymentDate = readerClientTrips.IsDBNull(readerClientTrips.GetOrdinal("PaymentDate")) ? null : readerClientTrips.GetInt32(2)
                };

                if (!clientTripsDict.ContainsKey(clientTrip.IdTrip))
                {
                    clientTripsDict.Add(clientTrip.IdTrip, new List<ClientTripDetailsGetDTO>());
                }

                clientTripsDict[clientTrip.IdTrip].Add(clientTrip);
            }
        }

        if (clientTripsDict.Count == 0)
        {
            throw new NotFoundException($"Client of id {id} has no trips registered");
        }

        foreach (var idTrip in clientTripsDict.Keys)
        {
            var queryTrip = "SELECT Name, Description, DateFrom, DateTo, MaxPeople FROM Trip WHERE IdTrip = @idTrip";
            await using var commandTrip = new SqlCommand(queryTrip, connection);
            commandTrip.Parameters.AddWithValue("@idTrip", idTrip);
            await using (var readerTrip = await commandTrip.ExecuteReaderAsync())
            {
                while (await readerTrip.ReadAsync())
                {
                    foreach (var clientTrip in clientTripsDict[idTrip])
                    {
                        clientTrip.Name = readerTrip.GetString(0);
                        clientTrip.Description = readerTrip.GetString(1);
                        clientTrip.DateFrom = readerTrip.GetDateTime(2);
                        clientTrip.DateTo = readerTrip.GetDateTime(3);
                        clientTrip.MaxPeople = readerTrip.GetInt32(4);
                    }
                }
            }
        }
        
        return clientTripsDict.Values.SelectMany(i => i);
    }

    public async Task<Client> CreateClientAsync(ClientCreateDTO client)
    {
        await using var connection = new SqlConnection(_connectionString);
        
        var insert = "INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) " +
                        "VALUES (@firstName, @lastName, @email, @telephone, @pesel) " +
                        "; Select scope_identity()";
        await using var command = new SqlCommand(insert, connection);
        command.Parameters.AddWithValue("@firstName", client.FirstName);
        command.Parameters.AddWithValue("@lastName", client.LastName);
        command.Parameters.AddWithValue("@email", client.Email);
        command.Parameters.AddWithValue("@telephone", client.Telephone);
        command.Parameters.AddWithValue("pesel", client.Pesel);
        
        await connection.OpenAsync();
        var id = Convert.ToInt32(await command.ExecuteScalarAsync());
        
        return new Client
        {
            IdClient = id,
            Email = client.Email,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Telephone = client.Telephone,
            Pesel = client.Pesel
        };
    }

    public async Task<ClientTripRegistrationDTO> RegisterClientToTripAsync(int idClient, int idTrip)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var queryClient = "SELECT * FROM Client WHERE IdClient = @idClient";
        await using var commandClient = new SqlCommand(queryClient, connection);
        commandClient.Parameters.AddWithValue("@idClient", idClient);
        await using (var readerClient = await commandClient.ExecuteReaderAsync())
        {
            if (!await readerClient.ReadAsync())
            {
                throw new NotFoundException($"Client of id {idClient} does not exist");
            }
        }

        var tripMaxPeople = 0;
        var queryTrip = "SELECT MaxPeople FROM Trip WHERE IdTrip = @idTrip";
        await using var commandTrip = new SqlCommand(queryTrip, connection);
        commandTrip.Parameters.AddWithValue("@idTrip", idTrip);
        await using (var readerTrip = await commandTrip.ExecuteReaderAsync())
        {
            if (!await readerTrip.ReadAsync())
            {
                throw new NotFoundException($"Trip of id {idTrip} does not exist");
            }

            tripMaxPeople = readerTrip.GetInt32(0);
        }
        
        var queryExists = "SELECT * FROM Client_Trip WHERE IdTrip = @idTrip AND IdClient = @IdClient";
        await using var commandExists = new SqlCommand(queryExists, connection);
        commandExists.Parameters.AddWithValue("@idTrip", idTrip);
        commandExists.Parameters.AddWithValue("@idClient", idClient);
        await using (var readerExists = await commandExists.ExecuteReaderAsync())
        {
            if (await readerExists.ReadAsync())
            {
                throw new ConflictException($"Client of id {idClient} already booked trip of id {idTrip}");
            }
        }
        
        var queryCountPeople = "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @idTrip";
        await using var commandCountPeople = new SqlCommand(queryCountPeople, connection);
        commandCountPeople.Parameters.AddWithValue("@IdTrip", idTrip);
        var tripPeopleRegistered = (int) (await commandCountPeople.ExecuteScalarAsync() ?? 0);

        if (tripPeopleRegistered >= tripMaxPeople)
        {
            throw new ConflictException($"Trip of id {idTrip} is fully booked");
        }
        
        var clientTripRegistration = new ClientTripRegistrationDTO
        {
            IdClient = idClient,
            IdTrip = idTrip,
            RegisteredAt = int.Parse(DateTime.Now.ToString("yyyyMMdd")),
            PaymentDate = null
        };

        var insertRegistration = "INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate) " +
                                 "VALUES (@idClient, @idTrip, @registeredAt, @paymentDate)";
        await using var commandRegistration = new SqlCommand(insertRegistration, connection);
        commandRegistration.Parameters.AddWithValue("@idClient", clientTripRegistration.IdClient);
        commandRegistration.Parameters.AddWithValue("@idTrip", clientTripRegistration.IdTrip);
        commandRegistration.Parameters.AddWithValue("@registeredAt", clientTripRegistration.RegisteredAt);
        commandRegistration.Parameters.AddWithValue("@paymentDate", (object?) clientTripRegistration.PaymentDate ?? DBNull.Value);
        await commandRegistration.ExecuteScalarAsync();
        
        return clientTripRegistration;
    }

    public async Task<bool> DeleteClientTripRegistrationAsync(int idClient, int idTrip)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var queryExists = "SELECT * FROM Client_Trip WHERE IdClient = @idClient AND IdTrip = @idTrip";
        await using var commandExists = new SqlCommand(queryExists, connection);
        commandExists.Parameters.AddWithValue("@idClient", idClient);
        commandExists.Parameters.AddWithValue("@idTrip", idTrip);
        await using (var readerExists = await commandExists.ExecuteReaderAsync())
        {
            if (!await readerExists.ReadAsync())
            {
                throw new NotFoundException(
                    $"Client of id {idClient} does not have reservation on trip of id {idTrip}");
            }
        }

        var delete = "DELETE FROM Client_Trip WHERE IdClient = @idClient AND IdTrip = @idTrip";
        await using var commandDelete = new SqlCommand(delete, connection);
        commandDelete.Parameters.AddWithValue("@idClient", idClient);
        commandDelete.Parameters.AddWithValue("@idTrip", idTrip);
        await commandDelete.ExecuteScalarAsync();

        return true;
    }
    
}