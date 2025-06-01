using System;
using System.Collections.Generic;

namespace test1mechanic.Models;

public class VisitDetails
{
    public DateTime Date { get; set; }
    public Client Client { get; set; }
    public Mechanic Mechanic { get; set; }
    public List<VisitService> VisitServices { get; set; }
}

public class VisitService
{
    public string Name { get; set; }
    public decimal ServiceFee { get; set; }
}