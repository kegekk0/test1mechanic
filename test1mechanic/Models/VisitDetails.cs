namespace test1mechanic.Models;

public class VisitDetails
{
    public DateTime Date { get; set; }
    public Client Client { get; set; }
    public Mechanic Mechanic { get; set; }
    public List<ServiceDetail> Services { get; set; }
}