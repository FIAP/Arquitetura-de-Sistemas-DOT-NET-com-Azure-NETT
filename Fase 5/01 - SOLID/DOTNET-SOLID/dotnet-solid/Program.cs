using dotnet_solid.Models;

var fixedIncomeCalculator = new FixedIncomeCalculator(0.05m);
var stockInvestmentCalculator = new StockInvestmentCalculator(0.10m);

var portfolio = new InvestmentPortfolio(new SimpleInvestmentReport());
portfolio.AddInvestment(new Investment("Fixed Income Bond", 1000));
portfolio.AddInvestment(new Investment("Tech Stock", 2000));

portfolio.GenerateReport();

decimal fixedIncomeReturn = portfolio.CalculateTotalReturn(fixedIncomeCalculator);
decimal stockInvestmentReturn = portfolio.CalculateTotalReturn(stockInvestmentCalculator);

Console.WriteLine($"Total Fixed Income Return: {fixedIncomeReturn:C}");
Console.WriteLine($"Total Stock Investment Return: {stockInvestmentReturn:C}");
Console.ReadLine();