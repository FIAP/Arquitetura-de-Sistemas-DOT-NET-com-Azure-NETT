using Dapper;
using dotnet_dapper.Entities;

namespace dotnet_dapper.Infra.Config;

public class QuoteRepository
{
    private readonly DbConnectionProvider _dbProvider;
    public QuoteRepository(DbConnectionProvider dbProvider) =>
                _dbProvider = dbProvider;


    public async Task<IEnumerable<Quote>> GetAllQuote()
    {
        using var connection = _dbProvider.GetConnection();
        var query = QuoteQuery.ListAll;
        var result = await connection.QueryAsync<Quote>(query);
        return result;
    }
}

