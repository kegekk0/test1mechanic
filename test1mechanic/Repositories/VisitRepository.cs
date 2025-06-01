using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using test1mechanic.Models;

namespace test1mechanic.Repositories;

public class VisitRepository : IVisitRepository
{
    private readonly string _connectionString;

    public VisitRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("WorkshopDb") 
            ?? throw new ArgumentNullException("Connection string 'WorkshopDb' not found.");
    }

    public async Task<VisitDetails> GetVisitDetailsAsync(int visitId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string visitQuery = @"
            SELECT v.date, 
                   c.client_id, c.first_name, c.last_name, c.date_of_birth,
                m.mechanic_id, m.licence_number
            FROM Visit v
            JOIN Client c ON v.client_id = c.client_id
            JOIN Mechanic m ON v.mechanic_id = m.mechanic_id
            WHERE v.visit_id = @VisitId";

        using var visitCommand = new SqlCommand(visitQuery, connection);
        visitCommand.Parameters.AddWithValue("@VisitId", visitId);

        using var visitReader = await visitCommand.ExecuteReaderAsync();
    
        if (!visitReader.HasRows) return null;

        await visitReader.ReadAsync();

        var visitDetails = new VisitDetails
        {
            Date = visitReader.GetDateTime(0),
            Client = new Client
            {
                ClientId = visitReader.GetInt32(1),
                FirstName = visitReader.GetString(2),
                LastName = visitReader.GetString(3),
                DateOfBirth = visitReader.GetDateTime(4)
            },
            Mechanic = new Mechanic
            {
                MechanicId = visitReader.GetInt32(5),
                LicenceNumber = visitReader.GetString(6)
            },
            Services = new List<ServiceDetail>()
        };

        await visitReader.CloseAsync();

        const string servicesQuery = @"
            SELECT s.name, vs.service_fee
            FROM Visit_Service vs
            JOIN Service s ON vs.service_id = s.service_id
            WHERE vs.visit_id = @VisitId";

        using var servicesCommand = new SqlCommand(servicesQuery, connection);
        servicesCommand.Parameters.AddWithValue("@VisitId", visitId);

        using var servicesReader = await servicesCommand.ExecuteReaderAsync();
    
        while (await servicesReader.ReadAsync())
        {
            visitDetails.Services.Add(new ServiceDetail
            {
                Name = servicesReader.GetString(0),
                ServiceFee = servicesReader.GetDecimal(1)
            });
        }

        return visitDetails;
    }

    public async Task<bool> AddVisitAsync(Visit visit, List<VisitServiceRequest> services)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Insert Visit record
            const string visitQuery = @"
            INSERT INTO Visit (visit_id, client_id, mechanic_id, date)
            VALUES (@VisitId, @ClientId, @MechanicId, @Date)";

            using (var visitCommand = new SqlCommand(visitQuery, connection, transaction))
            {
                visitCommand.Parameters.AddWithValue("@VisitId", visit.VisitId);
                visitCommand.Parameters.AddWithValue("@ClientId", visit.ClientId);
                visitCommand.Parameters.AddWithValue("@MechanicId", visit.MechanicId);
                visitCommand.Parameters.AddWithValue("@Date", visit.Date);
                await visitCommand.ExecuteNonQueryAsync();
            }

            foreach (var service in services)
            {
                const string serviceQuery = @"
                INSERT INTO Visit_Service (visit_id, service_id, service_fee)
                VALUES (@VisitId, @ServiceId, @ServiceFee)";

                using (var serviceCommand = new SqlCommand(serviceQuery, connection, transaction))
                {
                    serviceCommand.Parameters.AddWithValue("@VisitId", visit.VisitId);
                    serviceCommand.Parameters.AddWithValue("@ServiceId", service.ServiceId);
                    serviceCommand.Parameters.AddWithValue("@ServiceFee", service.ServiceFee);
                    await serviceCommand.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> VisitExistsAsync(int visitId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = "SELECT 1 FROM Visit WHERE visit_id = @VisitId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@VisitId", visitId);

        return (await command.ExecuteScalarAsync()) != null;
    }

    public async Task<bool> ClientExistsAsync(int clientId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = "SELECT 1 FROM Client WHERE client_id = @ClientId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ClientId", clientId);

        return (await command.ExecuteScalarAsync()) != null;
    }

    public async Task<int?> GetMechanicIdByLicenceAsync(string licenceNumber)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = "SELECT mechanic_id FROM Mechanic WHERE licence_number = @LicenceNumber";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@LicenceNumber", licenceNumber);

        return (int?)await command.ExecuteScalarAsync();
    }

    public async Task<int?> GetServiceIdByNameAsync(string serviceName)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = "SELECT service_id FROM Service WHERE name = @ServiceName";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ServiceName", serviceName);

        return (int?)await command.ExecuteScalarAsync();
    }
}