namespace dotnet_solid.Models;

public class StockInvestmentCalculator(decimal expectedGrowthRate) : InvestmentCalculator
{
    public override decimal CalculateReturn(Investment investment)
    {
        return investment.Amount * expectedGrowthRate;
    }
}
