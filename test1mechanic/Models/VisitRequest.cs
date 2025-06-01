namespace test1mechanic.Models;

public class VisitRequest
{
    public int VisitId { get; set; }
    public int ClientId { get; set; }
    public string MechanicLicenceNumber { get; set; }
    public List<ServiceRequest> Services { get; set; }
}