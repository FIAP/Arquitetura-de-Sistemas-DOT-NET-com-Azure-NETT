using Microsoft.Extensions.DependencyInjection;

var serviceProvider = ServiceConfiguration.ConfigureServices();


var acao = new Acao("Empresa X", 5000, 1000, 500, 5);
var acaoRetornoService = serviceProvider.GetService<AcaoRetornoService>();
var acaoDescricaoService = serviceProvider.GetService<AcaoDescricaoService>();
var repository = serviceProvider.GetService<IRepository<Acao>>();

Console.WriteLine(acaoDescricaoService.Descrever(acao));
Console.WriteLine($"Retorno anual: R$ {acaoRetornoService.CalcularRetornoAnual(acao)}");
repository.Save(acao);