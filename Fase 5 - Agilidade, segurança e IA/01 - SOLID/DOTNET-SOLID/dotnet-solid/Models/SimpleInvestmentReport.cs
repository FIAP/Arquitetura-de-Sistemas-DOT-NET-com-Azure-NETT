namespace dotnet_solid.Models;

public class SimpleInvestmentReport : IInvestmentReport
{
    public void GenerateReport(List<Investment> investments)
    {
        foreach (var investment in investments)
        {
            Console.WriteLine($"Investment: {investment.Name}, Amount: {investment.Amount:C}");
        }
    }
}
