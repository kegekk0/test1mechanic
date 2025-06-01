using test1mechanic.Models;
using test1mechanic.Repositories;

namespace test1mechanic.Services;

public class VisitService
{
    private readonly IVisitRepository _repository;

    public VisitService(IVisitRepository repository)
    {
        _repository = repository;
    }

    public async Task<VisitDetails> GetVisitDetailsAsync(int visitId)
    {
        return await _repository.GetVisitDetailsAsync(visitId);
    }

    public async Task AddVisitAsync(VisitRequest request)
    {
        if (await _repository.VisitExistsAsync(request.VisitId))
            throw new InvalidOperationException("Visit ID already exists");

        if (!await _repository.ClientExistsAsync(request.ClientId))
            throw new KeyNotFoundException("Client not found");

        var mechanicId = await _repository.GetMechanicIdByLicenceAsync(request.MechanicLicenceNumber);
        if (!mechanicId.HasValue)
            throw new KeyNotFoundException("Mechanic not found");

        var visitServices = new List<VisitServiceRequest>();
        foreach (var service in request.Services)
        {
            var serviceId = await _repository.GetServiceIdByNameAsync(service.ServiceName);
        
            if (!serviceId.HasValue)
            {
                throw new KeyNotFoundException($"Service '{service.ServiceName}' not found");
            }

            visitServices.Add(new VisitServiceRequest 
            {
                ServiceId = serviceId.Value,
                ServiceFee = service.ServiceFee
            });
        }

        var visit = new Visit
        {
            VisitId = request.VisitId,
            ClientId = request.ClientId,
            MechanicId = mechanicId.Value,
            Date = DateTime.UtcNow
        };

        await _repository.AddVisitAsync(visit, visitServices);
    }
}