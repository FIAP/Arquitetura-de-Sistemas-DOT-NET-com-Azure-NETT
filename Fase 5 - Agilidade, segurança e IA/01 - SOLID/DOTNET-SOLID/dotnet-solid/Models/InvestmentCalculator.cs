namespace dotnet_solid.Models;

public abstract class InvestmentCalculator
{
    public abstract decimal CalculateReturn(Investment investment);
}
