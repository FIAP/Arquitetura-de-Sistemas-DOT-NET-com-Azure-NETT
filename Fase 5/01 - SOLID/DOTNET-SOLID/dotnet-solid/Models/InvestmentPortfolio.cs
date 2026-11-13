namespace dotnet_solid.Models;

public class InvestmentPortfolio(IInvestmentReport report)
{
    private readonly List<Investment> _investments = new();
    private readonly IInvestmentReport _report = report;

    public void AddInvestment(Investment investment)
    {
        _investments.Add(investment);
    }

    public decimal CalculateTotalReturn(InvestmentCalculator calculator)
    {
        decimal totalReturn = 0;
        foreach (var investment in _investments)
        {
            totalReturn += calculator.CalculateReturn(investment);
        }
        return totalReturn;
    }

    public void GenerateReport()
    {
        _report.GenerateReport(_investments);
    }
}
