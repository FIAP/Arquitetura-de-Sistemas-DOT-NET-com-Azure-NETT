using Microsoft.Extensions.DependencyInjection;

var serviceProvider = ServiceConfiguration.ConfigureServices();


var acao = new Acao("Empresa X", 5000, 1000, 50, 5);
var titulo = new Titulo("Título Y", 1000, 7);
var imovel = new Imovel("Apartamento Z", 30000, 1500);

var acaoService = new AcaoService();
var tituloService = new TituloService();
var imovelService = new ImovelService();

Console.WriteLine($"Retorno anual: R$ {acaoService.CalcularRetornoAnual(acao)}");

Console.WriteLine($"Retorno anual: R$ {tituloService.CalcularRetornoAnual(titulo)}");

Console.WriteLine($"Retorno anual: R$ {imovelService.CalcularRetornoAnual(imovel)}");
