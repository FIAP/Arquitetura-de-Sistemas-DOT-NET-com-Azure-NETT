var titulo = new Titulo("Título Y", 10000, 7);
var acao = new Acao("Empresa X", 5000, 100, 50, 5);
var imovel = new Imovel("Apartamento Z", 300000, 1500);

var investimentoUseCase = new InvestimentoCalculaRetornoAnualUseCase();

Console.WriteLine($"Retorno anual Acao: R$ {investimentoUseCase.CalcularRetornoAnualAcao(acao)}");
Console.WriteLine($"Retorno anual Titulo: R$ {investimentoUseCase.CalcularRetornoAnualAcao(titulo)}");
Console.WriteLine($"Retorno anual Imovel: R$ {investimentoUseCase.CalcularRetornoAnualAcao(imovel)}");
Console.WriteLine($"Retorno depreciacao: R$ {investimentoUseCase.CalcularDepreciacao(imovel)}");