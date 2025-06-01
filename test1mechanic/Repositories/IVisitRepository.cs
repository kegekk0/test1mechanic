using System.Threading.Tasks;
using test1mechanic.Models;

namespace test1mechanic.Repositories;

public interface IVisitRepository
{
    Task<VisitDetails> GetVisitDetailsAsync(int visitId);
    Task<bool> AddVisitAsync(VisitRequest visitRequest);
    Task<bool> VisitExistsAsync(int visitId);
    Task<bool> ClientExistsAsync(int clientId);
    Task<int?> GetMechanicIdByLicenceAsync(string licenceNumber);
    Task<int?> GetServiceIdByNameAsync(string serviceName);
}