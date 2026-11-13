namespace dotnet_solid.Models;

public class Investment(string name, decimal amount)
{
    public string Name { get; private set; } = name;
    public decimal Amount { get; private set; } = amount;
}
