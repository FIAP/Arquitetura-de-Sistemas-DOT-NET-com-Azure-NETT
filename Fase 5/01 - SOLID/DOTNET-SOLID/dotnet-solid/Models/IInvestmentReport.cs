namespace dotnet_solid.Models;

public interface IInvestmentReport
{
    void GenerateReport(List<Investment> investments);
}
