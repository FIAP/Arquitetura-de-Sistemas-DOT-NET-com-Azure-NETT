using dotnet_graphql.Models;

namespace dotnet_graphql.Services;

public class InvestmentService
{
    private readonly List<Investment> _investments = new List<Investment>
    {
        new Investment { Id = 1, Name = "Stock A", Amount = 1000m, Date = DateTime.Now.AddDays(-10) },
        new Investment { Id = 2, Name = "Bond B", Amount = 500m, Date = DateTime.Now.AddDays(-5) }
    };

    public List<Investment> GetInvestments() => _investments;

    public Investment GetInvestmentById(int id) => _investments.FirstOrDefault(x => x.Id == id);

    public Investment AddInvestment(string name, decimal amount)
    {
        var newInvestment = new Investment
        {
            Id = _investments.Max(x => x.Id) + 1,
            Name = name,
            Amount = amount,
            Date = DateTime.UtcNow
        };

        _investments.Add(newInvestment);
        return newInvestment;
    }

    public Investment UpdateInvestment(int id, string name, decimal amount)
    {
        var investment = _investments.FirstOrDefault(x => x.Id == id);
        if (investment == null)
        {
            return null;
        }

        investment.Name = name;
        investment.Amount = amount;
        investment.Date = DateTime.UtcNow;

        return investment;
    }

    public bool DeleteInvestment(int id)
    {
        var investment = _investments.FirstOrDefault(x => x.Id == id);
        if (investment == null)
        {
            return false;
        }

        _investments.Remove(investment);
        return true;
    }
}


