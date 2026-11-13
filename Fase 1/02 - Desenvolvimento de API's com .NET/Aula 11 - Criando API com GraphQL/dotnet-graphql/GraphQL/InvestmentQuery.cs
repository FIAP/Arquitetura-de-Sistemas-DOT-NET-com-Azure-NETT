using dotnet_graphql.Models;
using dotnet_graphql.Services;

namespace dotnet_graphql.GraphQL;

public class InvestmentQuery
{
    public List<Investment> GetInvestments([Service] InvestmentService service) =>
        service.GetInvestments();

    public Investment GetInvestmentById(int id, [Service] InvestmentService service) =>
        service.GetInvestmentById(id);
}
