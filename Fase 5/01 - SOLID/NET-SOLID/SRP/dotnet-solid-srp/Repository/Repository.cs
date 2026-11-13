using System.Text.Json;

public class Repository<T> : IRepository<T>
{
    public void Save(T investimento)
    {
        Console.WriteLine($"Saving investment: {JsonSerializer.Serialize(investimento)}");
    }
}