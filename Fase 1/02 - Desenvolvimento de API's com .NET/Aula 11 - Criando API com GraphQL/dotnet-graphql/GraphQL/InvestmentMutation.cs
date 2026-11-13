using dotnet_graphql.Models;
using dotnet_graphql.Services;

namespace dotnet_graphql.GraphQL;

public class InvestmentMutation
{
    public Investment AddInvestment(string name, decimal amount, [Service] InvestmentService service)
    {
        return service.AddInvestment(name, amount);
    }

    public Investment UpdateInvestment(int id, string name, decimal amount, [Service] InvestmentService service)
    {
        return service.UpdateInvestment(id, name, amount);
    }

    public bool DeleteInvestment(int id, [Service] InvestmentService service)
    {
        return service.DeleteInvestment(id);
    }
}
