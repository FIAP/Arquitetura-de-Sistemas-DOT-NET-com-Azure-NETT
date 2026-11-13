using Grpc.Core;

namespace dotnet_grpc.Services;

public class InvestmentService : Investment.InvestmentBase
{
    private readonly List<InvestmentResponse> _investments = new List<InvestmentResponse>
        {
            new InvestmentResponse { Id = 1, Name = "Stock A", Amount = 1000, Date = "2024-08-20" },
            new InvestmentResponse { Id = 2, Name = "Bond B", Amount = 500, Date = "2024-08-21" }
        };

    public override Task<InvestmentResponse> CreateInvestment(CreateInvestmentRequest request, ServerCallContext context)
    {
        var newInvestment = new InvestmentResponse
        {
            Id = _investments.Max(x => x.Id) + 1,
            Name = request.Name,
            Amount = request.Amount,
            Date = DateTime.UtcNow.ToString("yyyy-MM-dd")
        };

        _investments.Add(newInvestment);
        return Task.FromResult(newInvestment);
    }

    public override Task<InvestmentsResponse> GetInvestments(GetInvestmentsRequest request, ServerCallContext context)
    {
        var response = new InvestmentsResponse();
        response.Investments.AddRange(_investments);
        return Task.FromResult(response);
    }

    public override Task<InvestmentResponse> UpdateInvestment(UpdateInvestmentRequest request, ServerCallContext context)
    {
        var investment = _investments.FirstOrDefault(x => x.Id == request.Id);
        if (investment == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Investment not found"));
        }

        investment.Name = request.Name;
        investment.Amount = request.Amount;
        investment.Date = DateTime.UtcNow.ToString("yyyy-MM-dd");

        return Task.FromResult(investment);
    }

    public override Task<DeleteInvestmentResponse> DeleteInvestment(DeleteInvestmentRequest request, ServerCallContext context)
    {
        var investment = _investments.FirstOrDefault(x => x.Id == request.Id);
        if (investment == null)
        {
            return Task.FromResult(new DeleteInvestmentResponse { Success = false });
        }

        _investments.Remove(investment);
        return Task.FromResult(new DeleteInvestmentResponse { Success = true });
    }
}
