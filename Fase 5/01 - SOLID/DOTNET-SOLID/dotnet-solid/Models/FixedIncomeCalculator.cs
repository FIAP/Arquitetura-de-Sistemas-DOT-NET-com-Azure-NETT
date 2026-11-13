namespace dotnet_solid.Models;

public class FixedIncomeCalculator(decimal interestRate) : InvestmentCalculator
{
    private readonly decimal _interestRate = interestRate;

    public override decimal CalculateReturn(Investment investment)
    {
        return investment.Amount * _interestRate;
    }
}
